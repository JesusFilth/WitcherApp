using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DeathDiceServer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DeathDiceServer.Controllers
{
    public class HomeController : Controller
    {
        ApplicationDBContext db;

        public HomeController(ApplicationDBContext context)
        {
            db = context;
        }
        public IActionResult Registration([FromBody]Login login)
        {
            if (db.Users.Where(w => w.Name == login.Name).FirstOrDefault() != null)
                return BadRequest("Пользователь с таким ником уже существует");
            if(db.Users.Where(w => w.Mail == login.Mail).FirstOrDefault() != null)
                return BadRequest("Пользователь с таким мылом уже существует");

            db.Users.Add(new User()
            {
                Name = login.Name,
                Mail = login.Mail,
                Password = login.Password,
                UserClient = new UserClient()
                {
                    Gold = 1000,
                    ImgAvatarHref = "",
                    WinCount = 0,
                    Name = login.Name,
                    Rank = 20,
                    Stars = 0
                }
            }) ; ;
            db.SaveChanges();
            ///
            //Friend friend = new Friend()
            //{
            //    ImgHref = "",
            //    Name = "Li",
            //    Online = true,
            //    Rank = 20,
            //    UserClient = db.UserClients.Where(w => w.Id == 1).FirstOrDefault()
            //};
            //db.Friends.Add(friend);
            db.SaveChanges();
            ///
            return Ok();
        }
        public IActionResult Login([FromBody]Login login)
        {
            var user = db.Users.Where(w => w.Password == login.Password && w.Mail == login.Mail).FirstOrDefault();
            
            if (user != null)
            {  
                var client = db.Users.Include(i => i.UserClient).Include(i => i.UserClient.Friends).Where(w => w.Mail==login.Mail).FirstOrDefault();
                return Ok(CloneUserClient(client.UserClient));
            } 
            else
                return BadRequest("Неверный логин или пароль");
        }
        public void Index()
        {
            Console.WriteLine("Start Index View");
        }
        public void Error()
        {
            Console.WriteLine("ERROR BLEAT");
        }
        public static UserClient CloneUserClient(UserClient userClient)
        {
            UserClient temp = new UserClient()
            {
                Gold = userClient.Gold,
                Name = userClient.Name,
                Rank = userClient.Rank,
                Stars = userClient.Stars,
                ImgAvatarHref = userClient.ImgAvatarHref,
                WinCount = userClient.WinCount,
                Friends = new List<Friend>()
            };
            foreach (var fr in userClient.Friends)
            {
                temp.Friends.Add(new Friend()
                {
                    ImgHref = fr.ImgHref,
                    Rank = fr.Rank,
                    Name = fr.Name,
                    Online = fr.Online
                });
            }
            return temp;
        }
    }
}
