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

        async Task SearchCheck()
        {
            //алгоритм поиска
            //пока тестовый метод
            if (gameSearchModels.Count >= 2)
            {
                Guid id = Guid.NewGuid();
                SearchResult searchResult = new SearchResult()
                {
                    UserId = id,
                    Enemy = new Enemy()
                    {
                         Name = gameSearchModels[1].Name,
                         ImgAvatarHref = "", 
                         Rank = 20
                    }
                };
                await Clients.Client(gameSearchModels[0].Id).SendAsync("MessageGameSearch", searchResult);//to ben

                searchResult.Enemy.Name = gameSearchModels[0].Name;

                await Clients.Client(gameSearchModels[1].Id).SendAsync("MessageGameSearch", searchResult);//to jesus
                //дисконект
            }
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
                //await Clients.Client(Context.ConnectionId).SendAsync("MessageGameSearch", "Поиск игры");
                await SearchCheck();
            }
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
