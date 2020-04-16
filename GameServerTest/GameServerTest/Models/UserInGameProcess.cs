using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GameServerTest.Models
{
    public class UserInGameProcess
    {
        public string Id { get; set; }
        public bool Step { get; set; }
        public int WinCount { get; set; }
        public StateGame stateGame { get; set; }
        public int[] cubes { get; set; }
        public int RollInRaund { get; set; }
    }
}
