using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GameServerTest.Models
{
    public class User
    {
        public string Name { get; set; }
        public int WinCount { get; set; }
        public int Gold { get; set; }
        public string ImgAvatarHref { get; set; }
        public int Rank { get; set; }
        public Friend[] Friends { get; set; }
    }
}