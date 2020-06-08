using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeathDiceServer.Models.Hub
{
    public class GameManager
    {
        public int[] CubesEnemy { get; set; }
        public int UserGold { get; set; }
        public int EnemyGold { get; set; }
        public int UserWin { get; set; }
        public int EnemyWin { get; set; }
        public int AllBet { get; set; }
        public int LastBet { get; set; }
        public string Message { get; set; }
        public int RollRaund { get; set; }
        public Raund SemiRaund { get; set; }
        public Raund Raund { get; set; }
        public bool BargainEnd { get; set; }
        public bool AllIn { get; set; }
        public bool OpenEnemyDice { get; set; }
    }
}
