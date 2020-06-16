using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeathDiceServer.Models
{
    public class UserClient
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int WinCount { get; set; }
        public int Gold { get; set; }
        public string ImgAvatarHref { get; set; }
        public int Rank { get; set; }
        public int Stars { get; set; }
        public List<Friend> Friends { get; set; }

        //public int UserId { get; set; }
        //public User User { get; set; }
    }
}
