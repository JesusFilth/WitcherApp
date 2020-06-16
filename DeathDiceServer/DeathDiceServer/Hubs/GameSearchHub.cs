using DeathDiceServer.Models;
using DeathDiceServer.Models.Hub;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeathDiceServer.Hubs
{
    public class GameSearchHub:Hub
    {
        static List<GameSearchModel> gameSearchModels = new List<GameSearchModel>();

        async Task SearchCheck(GameSearchModel user)
        {
            if (gameSearchModels.Count > 1)
            {
                int plus = user.Rank;
                int minus = user.Rank-1;

                GameSearchModel opponent;
                for (int i = 0; i < gameSearchModels.Count; i++)
                {
                    opponent = gameSearchModels.Find(f => f.Rank == plus);
                    if (opponent != null)
                    {
                        await ReturnToGame(user, opponent);
                        continue;
                    }
                    opponent = gameSearchModels.Find(f => f.Rank == minus);
                    if (opponent != null)
                    {
                        await ReturnToGame(user, opponent);
                        continue;
                    }
                    plus++;
                    minus--;
                }
            }
        }
        async Task ReturnToGame(GameSearchModel user, GameSearchModel enemy)
        {
            Guid id = Guid.NewGuid();
            SearchResult searchResult = new SearchResult()
            {
                UserId = id,
                Enemy = new Enemy()
                {
                    Name = enemy.Name,
                    ImgAvatarHref = "",
                    Rank = enemy.Rank
                }
            };
            await Clients.Client(user.Id).SendAsync("MessageGameSearch", searchResult);//to user

            searchResult.Enemy.Name = user.Name;

            await Clients.Client(enemy.Id).SendAsync("MessageGameSearch", searchResult);//to enemy
            //дисконект
        }
        public async Task GameSearch(string name, int rank)
        {
            //проверка на дубликат регистрации одного и того же камрада
            if (gameSearchModels.Where(w => w.Name == name).FirstOrDefault() == null)
            {
                gameSearchModels.Add(new GameSearchModel()
                {
                    Name = name,
                    Rank = rank,
                    Id = Context.ConnectionId
                });
                await SearchCheck(gameSearchModels[gameSearchModels.Count-1]);
            }
        }
        public void AbortGameSearch()
        {
            GameSearchModel user = gameSearchModels.Where(w => w.Id == Context.ConnectionId).FirstOrDefault();
            if (user != null)
                gameSearchModels.Remove(user);
        }
        public override async Task OnConnectedAsync()
        {
            await Clients.Caller.SendAsync("MessageConnection","Подключение установлено"); 
        }
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            //при дисконекте убирает из очереди 
            gameSearchModels.Remove(gameSearchModels.Where(w=>w.Id==Context.ConnectionId).FirstOrDefault());
            await Clients.Caller.SendAsync("DisconectGameSearch", "Discon");
        }
    }
}
