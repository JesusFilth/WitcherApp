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
    }
}
