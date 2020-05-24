using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeathDiceServer.Models
{
    public class Friend
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool Online { get; set; }
        public string ImgHref { get; set; }
        public int Rank { get; set; }

        public int UserClientId { get; set; }
        public UserClient UserClient { get; set; }
    }
}
