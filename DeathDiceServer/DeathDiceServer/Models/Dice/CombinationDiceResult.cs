using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeathDiceServer.Models.Dice
{
    class CombinationDiceResult
    {
        public int Sum { get; set; }
        public string Message { get; set; }
        public bool FullHouse { get; set; }
        public int Pair { get; set; }
        public int Three { get; set; }

        public int ToCompare(CombinationDiceResult obj)
        {
            if (this.FullHouse && obj.FullHouse)//если фул-хаус
            {
                if (this.Three == obj.Three)
                {
                    if (this.Pair == obj.Pair)
                        return 0;
                    if (this.Pair > obj.Pair)
                        return 1;
                    else
                        return -1;
                }
                if (this.Three > obj.Three)
                    return 1;
                else
                    return -1;
            }

            if (this.Sum == obj.Sum)
                return 0;
            if (this.Sum > obj.Sum)
                return 1;
            else
                return -1;

        }
    }
}
