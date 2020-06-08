using DeathDiceServer.Controllers;
using DeathDiceServer.Models;
using DeathDiceServer.Models.Dice;
using DeathDiceServer.Models.Hub;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeathDiceServer.Hubs
{
    public class GameProcessHub:Hub
    {
        ApplicationDBContext db;
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
                        RollRaund = 0,
                        stateGame = StateGame.none 
                    });
                    //стартовая ставка
                    temp.GameManager.AllBet += startBet;
                }
                connectUsers.RemoveAll(r => r.Id == gId);//удаляем из очереди подключений
                rooms.Add(temp);

                foreach(var us in temp.Users)
                {
                    await Clients.Client(us.Id).SendAsync("MessageReceived", GetGameManager(temp.GameManager,
                        temp.Users.Find(f => f.Id == us.Id),
                        temp.Users.Find(f=>f.Id!=us.Id),
                        "Игра началась"));
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
            UserInGameProcess enemy = room.Users.Find(x => x.Id != Context.ConnectionId);

            if (user.Step && user.RollRaund > 0)//если ваш ход и вы уже бросили кости
            {
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
                                await Clients.Caller.SendAsync("MessageReceived", GetGameManager(room.GameManager, user,enemy, "У противника недостаточно золота что бы ответить на вашу ставку"));
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
                                await Clients.Caller.SendAsync("MessageReceived", GetGameManager(room.GameManager, user, enemy, "Ставка не принята, вы минимум должны сравнять сумму"));
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
                                await Clients.Caller.SendAsync("MessageReceived", GetGameManager(room.GameManager, user, enemy, "сумма должна равняться последней ставки"));
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
                                    await Clients.Caller.SendAsync("MessageReceived", GetGameManager(room.GameManager, user, enemy, "У противника недостаточно золота что бы ответить на вашу ставку"));
                            }
                            else
                                await Clients.Caller.SendAsync("MessageReceived", GetGameManager(room.GameManager, user, enemy, "Противник уже спасовал"));
                        }
                        break;
                }
            }
            else
                await Clients.Caller.SendAsync("MessageReceived", GetGameManager(room.GameManager, user, enemy, (user.RollRaund > 0 ? "Сейчас ходит оппонент" : "Сперва бросте кости")));

        }
        public async Task AcceptBet(string id)
        {
            Room room = rooms.Find(w => w.id == Guid.Parse(id));
            UserInGameProcess user = room.Users.Find(w => w.Id == Context.ConnectionId);
            UserInGameProcess enemy = room.Users.Find(x => x.Id != Context.ConnectionId);

            if (user.Step && user.RollRaund > 0)//если ваш ход и вы уже бросили кости
            {
                if (enemy.stateGame == StateGame.upBet|| enemy.stateGame == StateGame.allIn)
                {
                    await CompareBet(room,room.GameManager.LastBet, user, enemy);
                }
                else
                    await Clients.Caller.SendAsync("MessageReceived", GetGameManager(room.GameManager, user, enemy, "Противник еще не поднимал ставку"));
            }
            else
                await Clients.Caller.SendAsync("MessageReceived", GetGameManager(room.GameManager, user, enemy, (user.RollRaund > 0 ? "Сейчас ходит оппонент" : "Сперва бросте кости")));
        }
        public async Task PassBet(string id)
        {
            Room room = rooms.Find(w => w.id == Guid.Parse(id));
            UserInGameProcess user = room.Users.Find(w => w.Id == Context.ConnectionId);
            UserInGameProcess enemy = room.Users.Find(x => x.Id != Context.ConnectionId);
            if (user.Step && user.RollRaund > 0)
            {
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
                    if (room.GameManager.SemiRaund == Raund.first)
                        room.GameManager.SemiRaund++;
                    else
                    {
                        room.GameManager.SemiRaund = Raund.first;
                    }
                    if (!room.GameOver())
                        room.GameManager.Raund++;//новый раунд
                    //отдать бабло противнику
                    room.PassBet(user, enemy);

                    await ReturnMessage(user.Id, enemy.Id, room,
                    "Вы спасовали, противник выиграл этот раунд.",
                    "Противник спасовал, вы выиграли этот раунд.");
                    //чекаем окончание игры
                    if (room.GameOver())
                    {
                        //user пасанул значит автоматом отдает победу оппоненту
                        room.Winner(enemy, user, db);//обновляем бд
                        //отправляем инфу клиентам
                        await GameOver(room);
                    }
                }
                else if(enemy.stateGame == StateGame.pass)
                {
                    await CompareBet(room, 0, user, enemy);
                }
            }
            else
                await Clients.Caller.SendAsync("MessageReceived", GetGameManager(room.GameManager, user, enemy, (user.RollRaund > 0 ? "Сейчас ходит оппонент" : "Сперва бросте кости")));
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
                await Clients.Caller.SendAsync("MessageReceived", GetGameManager(room.GameManager,user,room.Users.Find(f=>f.Id!=user.Id),"Сперва торг"));
        }
        async Task CompareBet(Room room, int gold, UserInGameProcess user, UserInGameProcess enemy)
        {
            if (room.GameManager.SemiRaund == Raund.first)
            {
                room.GameManager.SemiRaund++;
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
                room.GameManager.SemiRaund = Raund.first;//обновляем и полу раунд
                if(!room.GameOver())
                    room.GameManager.Raund++;//некст раунд ++
                room.AcceptBet(gold, user, enemy);
                user.RollRaund = enemy.RollRaund = 0;
                room.GameManager.OpenEnemyDice = true;//показываем дайсы друг другу

                CombinationDiceResult combinationDiceResultUser = DiceResult(user.cubes);
                CombinationDiceResult combinationDiceResultEnemy = DiceResult(enemy.cubes);

                int result = combinationDiceResultUser.ToCompare(combinationDiceResultEnemy);
                string userMessage = null, enemyMessage = null;
                switch (result)
                {
                    case 0:
                        userMessage = string.Format("Вы приняли ставку, вскрываем кости.\n{0}\nНичья", combinationDiceResultUser.Message);
                        enemyMessage = string.Format("Противник принял ставку, вскрываем кости.\n{0}\nНичья", combinationDiceResultEnemy.Message);
                        room.Draw(user, enemy);//делим бабло поравну
                        break;
                    case 1:
                        userMessage = string.Format("Вы приняли ставку, вскрываем кости.\n{0}\nВы выиграли", combinationDiceResultUser.Message);
                        enemyMessage = string.Format("Противник принял ставку, вскрываем кости.\n{0}\nВы Проиграли", combinationDiceResultEnemy.Message);
                        room.TakeBet(user); //забираем бабло со ставки
                        break;
                    case -1:
                        userMessage = string.Format("Вы приняли ставку, вскрываем кости.\n{0}\nВы Проиграли", combinationDiceResultUser.Message);
                        enemyMessage = string.Format("Противник принял ставку, вскрываем кости.\n{0}\nВы выиграли", combinationDiceResultEnemy.Message);
                        room.TakeBet(enemy);//отдаем бабло со ставки
                        break;
                }
                await ReturnMessage(user.Id, enemy.Id, room, userMessage, enemyMessage);
                room.GameManager.OpenEnemyDice = false;//отменяем показ костей  оппонента до след. раза
                //теперь чекаем окончание игры
                if (room.GameOver())
                {
                    room.Winner(user, enemy, db);
                    //отправляем инфу клиентам
                    await GameOver(room);
                }
            }
        }
        async Task ReturnMessage(string userId, string enemyId, Room room, string userMessage, string enemyMessage)
        {
            await Clients.Client(enemyId).SendAsync("MessageReceived", GetGameManager(room.GameManager,
                room.Users.Find(f=>f.Id== enemyId),
                room.Users.Find(f=>f.Id==userId),
                enemyMessage));

            await Clients.Client(userId).SendAsync("MessageReceived", GetGameManager(room.GameManager,
                room.Users.Find(f => f.Id == userId),
                room.Users.Find(f=>f.Id==enemyId),
                userMessage));
        }
        GameManager GetGameManager(GameManager gameManager, UserInGameProcess user, UserInGameProcess enemy, string message)
        {
            GameManager temp = new GameManager()
            {
                AllBet = gameManager.AllBet,
                BargainEnd = gameManager.BargainEnd,
                LastBet = gameManager.LastBet,
                Raund = gameManager.Raund,
                SemiRaund = gameManager.SemiRaund,
                Message = message,
                UserGold = user.Gold,
                EnemyGold = enemy.Gold,
                UserWin = user.WinCount,
                EnemyWin = enemy.WinCount,
                RollRaund = user.RollRaund
            };
            if (gameManager.OpenEnemyDice)
                temp.CubesEnemy = enemy.cubes;

            return temp;
        }
        CombinationDiceResult DiceResult(int[] cubes)
        {
            Array.Sort(cubes);//сортируем
            List<CombinationDice> combinationDices = new List<CombinationDice>();
            for (int i = 0; i < cubes.Length; i++)
            {
                var temp = cubes.Where(w => w == cubes[i]).ToArray();//ищет совпадения

                i += temp.Length - 1;//сразу увеличивает итерацию

                switch (temp.Length)
                {
                    case 1: combinationDices.Add(new CombinationDice() { CombinationType = CombinationDiceType.None, Value = cubes[i] }); break;
                    case 2: combinationDices.Add(new CombinationDice() { CombinationType = CombinationDiceType.Pair, Value = cubes[i] }); break;
                    case 3: combinationDices.Add(new CombinationDice() { CombinationType = CombinationDiceType.Three, Value = cubes[i] }); break;
                    case 4: combinationDices.Add(new CombinationDice() { CombinationType = CombinationDiceType.Four, Value = cubes[i] }); break;
                    case 5: combinationDices.Add(new CombinationDice() { CombinationType = CombinationDiceType.Flush, Value = cubes[i] }); break;
                }
            }
            int sum = 0;
            ///вычисляем сумму
            ///формула такая: sum = type*10+D; где type это чило комбинации, D - старшая кость
            CombinationDiceResult combinationDiceResult = new CombinationDiceResult();
            if (combinationDices.Count == 5)//то есть если все дайсы оказались разные, то проверка на стрит
            {
                bool flag = true;
                for (int i = 0; i < combinationDices.Count - 1; i++)
                {
                    if ((combinationDices[i].Value + 1) != combinationDices[i + 1].Value)//если это не стрит
                    {
                        flag = false;
                        continue;//прерываем цикл
                    }
                }
                if (flag)//если все же стрит
                {
                    sum += ((int)CombinationDiceType.Straight) * 10 + (combinationDices.Max(m => m.Value));
                    combinationDiceResult.Sum = sum;
                    combinationDiceResult.Message = string.Format("Стрит. Старшая кость: {0}", combinationDices.Max(m => m.Value));
                    return combinationDiceResult;
                }

            }
            foreach (var comb in combinationDices)
            {
                sum += ((int)comb.CombinationType) * 10 + comb.Value;
                switch (comb.CombinationType)
                {
                    case CombinationDiceType.None: combinationDiceResult.Message += string.Format(" *{0}* ", comb.Value); break;
                    case CombinationDiceType.Pair: combinationDiceResult.Message += string.Format(" *Пара из {0}* ", comb.Value); break;
                    case CombinationDiceType.Three: combinationDiceResult.Message += string.Format(" *Сет из {0}* ", comb.Value); break;
                    case CombinationDiceType.Four: combinationDiceResult.Message += string.Format(" *Карэ из {0}* ", comb.Value); break;
                    case CombinationDiceType.Flush: combinationDiceResult.Message += string.Format(" *Флеш из {0}* ", comb.Value); break;
                }
            }
            //если всего две комбинации   и   сдержит пару и сет. то это фулл-хаус, нужно будет отдельное сравнение
            if (combinationDices.Count == 2 && combinationDices.Where(w => w.CombinationType == CombinationDiceType.Pair).FirstOrDefault() != null && combinationDices.Where(w => w.CombinationType == CombinationDiceType.Three).FirstOrDefault() != null)
            {
                sum += 10;
                combinationDiceResult.Message = "Фулл-Хаус.";
                combinationDiceResult.FullHouse = true;
            }
            combinationDiceResult.Sum = sum;

            return combinationDiceResult;
        }
        public async Task GameOver(Room room)
        {
            User[] users = new User[2];
            int count = 0;
            foreach(UserInGameProcess us in room.Users)
            {
                users[count++] = db.Users.Include(i => i.UserClient).Include(i => i.UserClient.Friends).Where(w => w.Name == us.Name).FirstOrDefault();
            }
            UserClient[] userClients = new UserClient[2];//копируем обновленные данные для отправления клиенту
            count = 0;
            foreach(User usc in users)
            {
                userClients[count++] = HomeController.CloneUserClient(usc.UserClient);
            }
            count = 0;
            foreach(var us in room.Users)
            {
                await Clients.Client(us.Id).SendAsync("GameOverReceived", userClients[count++]);
            }
        }
    }
}
