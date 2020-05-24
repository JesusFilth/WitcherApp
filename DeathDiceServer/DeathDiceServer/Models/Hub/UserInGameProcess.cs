using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeathDiceServer.Models.Hub
{
    public class UserInGameProcess
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Gold { get; set; }
        public bool Step { get; set; }
        public int WinCount { get; set; }
        public StateGame stateGame { get; set; }
        public int[] cubes { get; set; }
        public int RollRaund { get; set; }
    }
}
