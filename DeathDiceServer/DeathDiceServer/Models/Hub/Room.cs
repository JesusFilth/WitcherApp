using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeathDiceServer.Models.Hub
{
    public class Room
    {
        public Guid id { get; set; }
        public List<UserInGameProcess> Users { get; set; } 
        public GameManager GameManager { get; set; }
        public void UpBet(int gold, UserInGameProcess userUpBet, UserInGameProcess enemy)
        {
            userUpBet.Step = false;
            enemy.Step = true;
            userUpBet.stateGame = StateGame.upBet;
            enemy.stateGame = StateGame.none;//по идее в любом случае скидывается, но это не точно

            userUpBet.Gold -= gold;//списываем бабло

            GameManager.LastBet = GameManager.LastBet < gold ? (gold - GameManager.LastBet) : gold;//если последняя ставка была меньше чем новая ставка, тогда присваевается разница, если нет, то вся сумма
            GameManager.AllBet += gold;
        }
        public void AcceptBet(int gold, UserInGameProcess userAcceptBet, UserInGameProcess enemy)
        {
            userAcceptBet.Gold -= gold;
            if (userAcceptBet.Gold < 0) userAcceptBet.Gold = 0;//если вы пошли в олл-ин то сумма должна быть максимум ноль

            GameManager.AllBet += gold;
            GameManager.LastBet = 0;

            GameManager.BargainEnd = true;//торг окончен

            userAcceptBet.Step = enemy.Step = true;
            userAcceptBet.stateGame = enemy.stateGame = StateGame.none;
        }
        public void PassBet(UserInGameProcess userPassBet, UserInGameProcess enemy)
        {
            enemy.Gold += GameManager.AllBet;
            GameManager.LastBet = GameManager.AllBet = 0;
            enemy.WinCount++;

            userPassBet.Step = enemy.Step = true;
            userPassBet.stateGame = enemy.stateGame = StateGame.none;
            userPassBet.RollRaund = enemy.RollRaund = 0;
        }
        public void AllInBet(UserInGameProcess userAllInBet, UserInGameProcess enemy)
        {
            GameManager.AllBet += userAllInBet.Gold;
            GameManager.LastBet = userAllInBet.Gold;
            userAllInBet.Gold = 0;
            userAllInBet.stateGame = StateGame.allIn;

            userAllInBet.Step = false;
            enemy.Step = true;
        }

        public void TakeBet(UserInGameProcess winner)
        {
            winner.Gold += GameManager.AllBet;
            winner.WinCount++;
        }
        /// <summary>
        /// Ничья
        /// </summary>
        /// <param name="user"></param>
        /// <param name="enemy"></param>
        public void Draw(UserInGameProcess user, UserInGameProcess enemy)
        {
            user.Gold += GameManager.AllBet / 2;
            enemy.Gold += GameManager.AllBet / 2;
            user.WinCount++;
            enemy.WinCount++;
        }
        /// <summary>
        /// Быстрая проверка на окончание игры
        /// </summary>
        /// <returns></returns>
        public bool GameOver()
        {
            if (Users[0].WinCount == 2 || Users[1].WinCount == 2|| Users[0].Gold==0||Users[1].Gold==0)//если у кого то есть две победы, или кто то слил все бабло
                return true;
            return false;
        }

        public void Winner(UserInGameProcess user, UserInGameProcess enemy, ApplicationDBContext context)
        {

            User userWinnner = context.Users.Include(i=>i.UserClient).Where(w => w.Name == user.Name).FirstOrDefault();
            userWinnner.UserClient.Gold = user.Gold;
            if (user.WinCount == 2)
            {
                userWinnner.UserClient.WinCount++;
                //тут изменить систему рангов, пока что просто --
                userWinnner.UserClient.Rank--;
            }
            //ememy
            User userLose = context.Users.Include(i => i.UserClient).Where(w => w.Name == enemy.Name).FirstOrDefault();
            userLose.UserClient.Gold = enemy.Gold;
            //в теории может быть ничья в финале, так что чекаем и этот момент
            if(enemy.WinCount==2)
            {
                userLose.UserClient.WinCount++;
                userLose.UserClient.Rank--;
            }
            //ранг не меняем
            //сохроняем в базе
            context.SaveChanges();
        }
        /// <summary>
        /// получаем id победителя или null если ничья
        /// </summary>
        /// <returns></returns>
        public string GetWinnerId()
        {
            if (Users[0].WinCount == 2 && Users[1].WinCount == 2)
                return null;//если вдруг ничья
            return Users.Find(f => f.WinCount == 2).Id;
        }
    }
}
