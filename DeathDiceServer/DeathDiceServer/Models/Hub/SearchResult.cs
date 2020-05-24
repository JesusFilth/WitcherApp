using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeathDiceServer.Models
{
    public class SearchResult
    {
        public Guid UserId { get; set; }
        public Enemy Enemy { get; set; }
    }
}
