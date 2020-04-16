using GameServerTest.Models;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ChatApp.Hubs
{
    public class MessageHub : Hub
    {
        static int allBet = 100;
        static int lastBet = 0;

        static GameManager gameManager = new GameManager();
        static List<UserInGameProcess> usersInGameProcess = new List<UserInGameProcess>();

        public async Task UpBet(string gold)
        {
            UserInGameProcess user = usersInGameProcess.Find(x=>x.Id==Context.ConnectionId);
            if (user.Step&&user.RollInRaund>0)
            {
                UserInGameProcess enemy = usersInGameProcess.Find(x => x.Id != Context.ConnectionId);
                if(enemy.stateGame == StateGame.none)//если вы делаете ставку первым
                {
                    user.Step = false;
                    enemy.Step = true;
                    user.stateGame = StateGame.upBet;
                    lastBet = int.Parse(gold);
                    allBet += lastBet;

                    await Clients.AllExcept(Context.ConnectionId).SendAsync("MessageReceived", new GameManager()
                    {
                        AllBet = allBet,
                        Message = string.Format("Противник поднял ставку на {0}", gold.ToString())
                    });

                    await Clients.Caller.SendAsync("MessageReceived", new GameManager()
                    {
                        AllBet = allBet,
                        Message = string.Format("ВЫ подняли ставку на {0}", gold.ToString())
                    });
                }
                else if(enemy.stateGame == StateGame.upBet)//если оппонент уже сделал ставку
                {
                    if (lastBet == int.Parse(gold))//сравнял ставку, вскрывают кости
                    {
                        allBet += int.Parse(gold);
                        user.Step = enemy.Step = true;
                        //тут вскрываем кости !!!!!
                        await OpenHends(user, enemy);
                        
                    }
                    else if (lastBet > int.Parse(gold))//ставка не принята, слишком мало дал, ептать
                    {
                        await Clients.Caller.SendAsync("MessageReceived", new GameManager()
                        {
                            AllBet = allBet,
                            Message = string.Format("Вы минимум должны сравнять сумму")
                        });
                    }
                    else if (lastBet < int.Parse(gold))//увеличил ставку
                    {
                        user.Step = false;
                        enemy.Step = true;
                        user.stateGame = StateGame.upBet;
                        enemy.stateGame = StateGame.none;

                        lastBet = int.Parse(gold) - lastBet;
                        allBet += lastBet;

                        await Clients.AllExcept(Context.ConnectionId).SendAsync("MessageReceived", new GameManager()
                        {
                            AllBet = allBet,
                            Message = string.Format("Противник поднял ставку еще на {0}", lastBet.ToString())
                        });

                        await Clients.Caller.SendAsync("MessageReceived", new GameManager()
                        {
                            AllBet = allBet,
                            Message = string.Format("ВЫ подняли ставку еще на {0}", lastBet.ToString())
                        });
                    }
                }  
            }
            else
            {
                await Clients.Caller.SendAsync("MessageReceived", new GameManager()
                {
                    AllBet = allBet,
                    Message = user.RollInRaund > 0 ? "Сейчас ходит оппонент" : "Сперва бросте кости"
                });
            }
            
        }
        public async Task OpenHends(UserInGameProcess user, UserInGameProcess enemy )
        {
            enemy.Step = user.Step = true;
            enemy.stateGame = user.stateGame = StateGame.none;
            enemy.RollInRaund = user.RollInRaund = 0;
            //условие победы

            //
            await Clients.Caller.SendAsync("MessageReceived", new GameManager()
            {
                AllBet = allBet,
                Message = "Вскрываем кости",
                CubesEnemy = enemy.cubes
            });
            await Clients.AllExcept(Context.ConnectionId).SendAsync("MessageReceived", new GameManager()
            {
                AllBet = allBet,
                Message = "Вскрываем кости",
                CubesEnemy = user.cubes
            });
        }
        public async Task RollCubes(int countCubes, int[] index = null)
        {
            UserInGameProcess user = usersInGameProcess.Find(x => x.Id == Context.ConnectionId);
            Random random = new Random();
            int[] cubes = new int[countCubes];
            for (int i = 0; i < cubes.Length; i++)
            {
                cubes[i] = random.Next(1, 7);
            }
            if (user.RollInRaund == 0)
            {
                user.cubes = cubes;
            }
            else
            {
                for (int i = 0; i < cubes.Length; i++)
                {
                    user.cubes[index[i]] = cubes[i];
                }
            }
            
            user.RollInRaund++;

            await Clients.Caller.SendAsync("RollCubesReceived",cubes);
        }
        public async Task AcceptBet()
        {
            UserInGameProcess user = usersInGameProcess.Find(x => x.Id == Context.ConnectionId);
            if (user.Step&&user.RollInRaund>0)
            {
                UserInGameProcess enemy = usersInGameProcess.Find(x => x.Id != Context.ConnectionId);
                if(enemy.stateGame == StateGame.upBet)
                {
                    allBet += lastBet;//бабло должно браться из профиля юзера
                   // await OpenHends(user, enemy);
                }
            }
            else
            {
                await Clients.Caller.SendAsync("MessageReceived", new GameManager()
                {
                    AllBet = allBet,
                    Message = user.RollInRaund > 0 ? "Сейчас ходит оппонент" : "Сперва бросте кости"
                });
            }
        }
        public override async Task OnConnectedAsync()
        {
            if (usersInGameProcess.Count <= 1)
            {
                if(!usersInGameProcess.Any(x=>x.Id == Context.ConnectionId))
                    usersInGameProcess.Add(new UserInGameProcess() { Id = Context.ConnectionId, Step = true, WinCount = 0, stateGame = StateGame.none, RollInRaund=0 });
            }
            if (usersInGameProcess.Count==2)
            {
                await Clients.All.SendAsync("MessageReceived", new GameManager()
                {
                    AllBet = allBet,
                    Message = "Игра началась"
                });
            }
        }
    }
}