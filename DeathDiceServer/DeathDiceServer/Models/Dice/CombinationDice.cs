using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeathDiceServer.Models.Dice
{
    public class CombinationDice
    {
        public CombinationDiceType CombinationType { get; set; }
        public int Value { get; set; }
    }
}
