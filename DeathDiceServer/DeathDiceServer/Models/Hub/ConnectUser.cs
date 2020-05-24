using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeathDiceServer.Models.Hub
{
    public class ConnectUser
    {
        public Guid Id { get; set; }
        public string ConnectId { get; set; }
        public string Name { get; set; }
        public int Gold { get; set; }
    }
}
