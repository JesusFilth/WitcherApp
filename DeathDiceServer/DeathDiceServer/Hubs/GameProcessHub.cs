using DeathDiceServer.Models;
using DeathDiceServer.Models.Hub;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeathDiceServer.Hubs
{
    public class GameProcessHub:Hub
    {
        static ApplicationDBContext db;
        static List<ConnectUser> connectUsers = new List<ConnectUser>();
        static List<Room> rooms = new List<Room>();
        public GameProcessHub(ApplicationDBContext context)
        {
            if(db==null)
                db = context;
        }
        public async Task CreateRoom(string name, int gold, string id)
        {
            int startBet = 50;
            Guid gId = Guid.Parse(id);
            connectUsers.Add(new ConnectUser() { Name = name, Gold = gold-=startBet, Id = gId, ConnectId = Context.ConnectionId });//добавляем в список подключения
            //проверяем в списке двоих с одинаковым id
            var userConnect = connectUsers.Where(w => w.Id == gId).ToList();
            if (userConnect.Count == 2)
            {
                //создаем комнату для двоих, удаляем из очереди и выводим сообщенее что игра началась
                Room temp = new Room()
                {
                    id = gId,
                    Users = new List<UserInGameProcess>(),
                    GameManager = new GameManager() { AllBet = 0, SemiRaund = Raund.first, Raund = Raund.first}
                };
                foreach(var us in userConnect)
                {
                    temp.Users.Add(new UserInGameProcess() { Id = us.ConnectId, 
                        Name = us.Name, 
                        Gold = us.Gold, 
                        Step = true, 
                        WinCount = 0, 
                        RollRaund = 0,//на время 
                        stateGame = StateGame.none 
                    });
                    //стартовая ставка
                    temp.GameManager.AllBet += startBet;
                }
                connectUsers.RemoveAll(r => r.Id == gId);//удаляем из очереди подключений
                rooms.Add(temp);

                foreach(var us in temp.Users)
                {
                    await Clients.Client(us.Id).SendAsync("MessageReceived", new GameManager()
                    {
                        AllBet = temp.GameManager.AllBet,
                        Message = "Игра началась"
                    });
                }
            }
        }
        public override async Task OnConnectedAsync()
        {
            await Clients.Caller.SendAsync("ConnectionMessage", "подключение с игрой установлено");
        }
        public async Task UpBet(string id,int gold)
        {
            Room room = rooms.Find(w => w.id == Guid.Parse(id));
            UserInGameProcess user = room.Users.Find(w => w.Id == Context.ConnectionId);

            if (user.Step && user.RollRaund > 0)//если ваш ход и вы уже бросили кости
            {
                UserInGameProcess enemy = room.Users.Find(x => x.Id != Context.ConnectionId);
                switch (enemy.stateGame)
                {
                    case StateGame.none://если вы делаете ставку первым
                        {
                            if (gold <= enemy.Gold)//если у противника хватает золота что бы ответить на вашу ставку
                            {
                                room.UpBet(gold, user, enemy);
                                if (user.Gold == 0)//если вы отдает последнее золото, то автоматом идете в олл-ин
                                    user.stateGame = StateGame.allIn;

                                await ReturnMessage(user.Id, enemy.Id, room, 
                                    string.Format("Вы подняли ставку на {0}", gold.ToString()),
                                    string.Format("Противник поднял ставку на {0}", gold.ToString()));
                            }
                            else
                                await Clients.Caller.SendAsync("MessageReceived", GetGameManager(room.GameManager, user, "У противника недостаточно золота что бы ответить на вашу ставку"));
                        }
                        break;
                    case StateGame.upBet://если оппонент уже сделал ставку
                        {
                            if (room.GameManager.LastBet == gold)//сравнял ставку
                            {
                                await CompareBet(room, gold, user, enemy);
                            }
                            else if (room.GameManager.LastBet > gold)//ставка не принята, слишком мало дал, ептать
                            {
                                await Clients.Caller.SendAsync("MessageReceived", GetGameManager(room.GameManager, user, "Ставка не принята, вы минимум должны сравнять сумму"));
                            }
                            else if (room.GameManager.LastBet < gold)//увеличил ставку
                            {
                                int tempGold = gold - room.GameManager.LastBet;
                                room.UpBet(gold, user, enemy);

                                await ReturnMessage(user.Id, enemy.Id, room,
                                    string.Format("Вы подняли ставку на {0}", tempGold.ToString()),
                                    string.Format("Противник поднял ставку на {0}", tempGold.ToString()));
                            }
                        }
                        break;
                    case StateGame.allIn://если противник ушел в олл-ин, то вы либо пасуете, либо принимаете
                        {
                            if (gold == room.GameManager.LastBet)//если хватает голды
                            {
                                await CompareBet(room, gold, user, enemy);
                            }
                            else
                                await Clients.Caller.SendAsync("MessageReceived", GetGameManager(room.GameManager, user, "сумма должна равняться последней ставки"));
                        }
                        break;
                    case StateGame.pass://если противник спасовал
                        {
                            if (room.GameManager.LastBet == 0)//если противник спасовал, но ставок еще нет
                            {
                                if (gold <= enemy.Gold)//если у противника хватает золота что бы ответить на вашу ставку
                                {
                                    room.UpBet(gold, user, enemy);
                                    if (user.Gold == 0)//если вы отдает последнее золото, то автоматом идете в олл-ин
                                        user.stateGame = StateGame.allIn;

                                    await ReturnMessage(user.Id, enemy.Id, room,
                                        string.Format("Вы подняли ставку на {0}", gold.ToString()),
                                        string.Format("Противник поднял ставку на {0}", gold.ToString()));
                                }
                                else
                                    await Clients.Caller.SendAsync("MessageReceived", GetGameManager(room.GameManager, user, "У противника недостаточно золота что бы ответить на вашу ставку"));
                            }
                            else
                                await Clients.Caller.SendAsync("MessageReceived", GetGameManager(room.GameManager, user, "Противник уже спасовал"));
                        }
                        break;
                }
            }
            else
                await Clients.Caller.SendAsync("MessageReceived", GetGameManager(room.GameManager, user, (user.RollRaund > 0 ? "Сейчас ходит оппонент" : "Сперва бросте кости")));

        }
        public async Task AcceptBet(string id)
        {
            Room room = rooms.Find(w => w.id == Guid.Parse(id));
            UserInGameProcess user = room.Users.Find(w => w.Id == Context.ConnectionId);

            if (user.Step && user.RollRaund > 0)//если ваш ход и вы уже бросили кости
            {
                UserInGameProcess enemy = room.Users.Find(x => x.Id != Context.ConnectionId);
                if (enemy.stateGame == StateGame.upBet|| enemy.stateGame == StateGame.allIn)
                {
                    await CompareBet(room,room.GameManager.LastBet, user, enemy);
                }
                else
                    await Clients.Caller.SendAsync("MessageReceived", GetGameManager(room.GameManager, user, "Противник еще не поднимал ставку"));
            }
            else
                await Clients.Caller.SendAsync("MessageReceived", GetGameManager(room.GameManager, user, (user.RollRaund > 0 ? "Сейчас ходит оппонент" : "Сперва бросте кости")));
        }
        public async Task PassBet(string id)
        {
            Room room = rooms.Find(w => w.id == Guid.Parse(id));
            UserInGameProcess user = room.Users.Find(w => w.Id == Context.ConnectionId);
            if (user.Step && user.RollRaund > 0)
            {
                UserInGameProcess enemy = room.Users.Find(x => x.Id != Context.ConnectionId);
                if (enemy.stateGame == StateGame.none)//если это первый ход и противник еще не ходил 
                {
                    user.stateGame = StateGame.pass;
                    user.Step = false;
                    await ReturnMessage(user.Id, enemy.Id, room,
                    "Вы спасовали",
                    "Противник спасовал");
                }
                else if (enemy.stateGame == StateGame.upBet || enemy.stateGame == StateGame.allIn)//если противник уже поднял ставку, то вы сливаете раунд 
                {
                    //отдать бабло противнику
                    if (room.GameManager.Raund == Raund.first)//если раунд первый, то отдаем бабло и переходим на следующий
                    {
                        room.PassBet(user, enemy);
                        room.GameManager.Raund = Raund.second;//новый раунд

                        await ReturnMessage(user.Id, enemy.Id, room,
                        "Вы спасовали, противник выйграл этот раунд.",
                        "Противник спасовал, вы выйграли этот раунд.");
                    }
                    else
                    {
                        room.PassBet(user, enemy);
                        if (enemy.WinCount == 2)//если у противника уже вторая победа, то вы слили игру
                        {
                            //метод окончания игры
                        }
                        else
                        {
                            room.GameManager.Raund = Raund.third;
                            await ReturnMessage(user.Id, enemy.Id, room,
                        "Вы спасовали, противник выйграл этот раунд.",
                        "Противник спасовал, вы выйграли этот раунд.");
                        }   
                    }
                }
                else if(enemy.stateGame == StateGame.pass)
                {
                    await CompareBet(room, 0, user, enemy);
                }
            }
            else
                await Clients.Caller.SendAsync("MessageReceived", GetGameManager(room.GameManager, user, (user.RollRaund > 0 ? "Сейчас ходит оппонент" : "Сперва бросте кости")));
        }
        public async Task AllInBet(string id)
        {
            Room room = rooms.Find(w => w.id == Guid.Parse(id));
            UserInGameProcess user = room.Users.Find(w => w.Id == Context.ConnectionId);
            if (user.Step && user.RollRaund > 0)
            {
                UserInGameProcess enemy = room.Users.Find(x => x.Id != Context.ConnectionId);
                if (enemy.stateGame == StateGame.none)
                {
                    room.AllInBet(user, enemy);
                    await ReturnMessage(user.Id, enemy.Id, room,
                        string.Format("Вы пошли в Олл-Ин. Подняв ставку на {0} голды",room.GameManager.LastBet),
                        string.Format("Противник пошел в Олл-Ин в Олл-Ин. Подняв ставку на {0} голды", room.GameManager.LastBet));
                }
                else if(enemy.stateGame == StateGame.allIn)//если противник тоже пошел в олл-ин
                {
                    await CompareBet(room,user.Gold, user, enemy);
                }
                else if(enemy.stateGame == StateGame.upBet)
                {
                    int tempGold = user.Gold - room.GameManager.LastBet;//разница
                    room.AllInBet(user, enemy);
                    await ReturnMessage(user.Id, enemy.Id, room,
                        string.Format("Вы пошли в Олл-Ин. Подняв ставку на {0} голды", tempGold),
                        string.Format("Противник пошел в Олл-Ин в Олл-Ин. Подняв ставку на {0} голды", tempGold));
                }
            }
        }

        public async Task RollCubes(string id, int countCubes, int[] index = null)
        {
            Room room = rooms.Find(w => w.id == Guid.Parse(id));
            UserInGameProcess user = room.Users.Find(w => w.Id == Context.ConnectionId);

            Random random = new Random();
            int[] cubes = new int[countCubes];
            for (int i = 0; i < cubes.Length; i++)//рандом 6и интов
            {
                cubes[i] = random.Next(1, 7);
            }

            if (user.RollRaund == 0 && room.GameManager.SemiRaund == Raund.first)//если еще не кидал кости и если это первый полу-раунд
            {
                user.cubes = cubes;
                user.RollRaund++;
                await Clients.Caller.SendAsync("RollCubesReceived", user.cubes);
            }
            else if (user.RollRaund == 1 && room.GameManager.SemiRaund == Raund.second)
            {
                for (int i = 0; i < cubes.Length; i++)
                {
                    user.cubes[index[i]] = cubes[i];
                }
                await Clients.Caller.SendAsync("RollCubesReceived", user.cubes);
            }
            else
                await Clients.Caller.SendAsync("MessageReceived", GetGameManager(room.GameManager,user,"Сперва торг"));
        }

        async Task CompareBet(Room room, int gold, UserInGameProcess user, UserInGameProcess enemy)
        {
            if (room.GameManager.SemiRaund == Raund.first)
            {
                room.GameManager.SemiRaund = Raund.second;
                room.AcceptBet(gold, user, enemy);//торг окончен
                if (user.Gold == 0)
                {
                    user.stateGame = StateGame.allIn;//идем в олл-ин если отдаем последнее золото
                    room.GameManager.AllIn = true;//отмечаем что при следующем перебросе костей, торга не будет. Сразу вскрываются кости. 
                }
                await ReturnMessage(user.Id, enemy.Id, room,
                    "Вы приняли ставку, можете выбрать и перебросить кости",
                    "Противник принял ставку, можете выбрать и перебросить кости");
            }
            else if (room.GameManager.SemiRaund == Raund.second)
            {
                if (room.GameManager.Raund == Raund.first) //чекаем кости, объявляем победителя раунда
                {
                    //дополнить++++++++++++++++++
                    await ReturnMessage(user.Id, enemy.Id, room,
                    "Вы приняли ставку, вскрываем кости.",
                    "Противник принял ставку, вскрываем кости.");
                }
                else if (room.GameManager.Raund == Raund.second)//чекаем кости, объявляем победителя игры
                {
                    //дополнить+++++++++++++++++++
                    await ReturnMessage(user.Id, enemy.Id, room,
                    "Вы приняли ставку, вскрываем кости.",
                    "Противник принял ставку, вскрываем кости.");
                }
            }
        }
        async Task ReturnMessage(string userId, string enemyId, Room room, string userMessage, string enemyMessage)
        {
            await Clients.Client(enemyId).SendAsync("MessageReceived", GetGameManager(room.GameManager,
                room.Users.Find(f=>f.Id== enemyId),
                enemyMessage));

            await Clients.Client(userId).SendAsync("MessageReceived", GetGameManager(room.GameManager,
                room.Users.Find(f => f.Id == userId),
                userMessage));
        }
        GameManager GetGameManager(GameManager gameManager, UserInGameProcess user, string message)
        {
            GameManager temp = new GameManager()
            {
                AllBet = gameManager.AllBet,
                BargainEnd = gameManager.BargainEnd,
                LastBet = gameManager.LastBet,
                Raund = gameManager.Raund,
                SemiRaund = gameManager.SemiRaund,
                Message = message,
                UserGold = user.Gold
            };
            return temp;
        }
    }
}
