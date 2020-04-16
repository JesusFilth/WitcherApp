using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GameServerTest.Models
{
    public class Friend
    {
        public string Name { get; set; }
        public string LastTimeOnline { get; set; }
        public string ImgHref { get; set; }
        public int Rank { get; set; }
    }
}