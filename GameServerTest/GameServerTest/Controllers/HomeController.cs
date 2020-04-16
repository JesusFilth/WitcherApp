using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using GameServerTest.Models;
using System.IO;
using Microsoft.AspNetCore.Authorization;

namespace GameServerTest.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("ok");
        }
        [HttpPost]
        public IActionResult Login([FromBody]Login mess)
        {
            User user = new User();
            user.Name = "Bender";
            user.Rank = 20;
            user.Gold = 250;
            user.ImgAvatarHref = "www";
            user.WinCount = 3;

            Friend[] friends = {
            new Friend(){Name ="Li", Rank=20, ImgHref ="www", LastTimeOnline= "был онлайн вчера" },
            new Friend(){Name ="Valera", Rank=20, ImgHref ="www", LastTimeOnline= "был онлайн 3 часа назад" },
            new Friend(){Name ="Tomas", Rank=20, ImgHref ="www", LastTimeOnline= "был онлайн неделю назад" },
            new Friend(){Name ="Berejok NV", Rank=18, ImgHref ="www", LastTimeOnline= "онлайн" }};

            user.Friends = friends;


            return Ok(user);
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public IActionResult GameSearch(string name) {

            Enemy enemy = new Enemy() { Name = "Hrushev", Rank = 20, ImgAvatarHref = "www" };
            return Ok(enemy);
        }
        public IActionResult RollCubes()
        {
            Random random = new Random();
            int[] cubes = new int[5];
            for (int i = 0; i < cubes.Length; i++)
            {
                cubes[i] = random.Next(1,7);
            }
            return Ok(cubes);
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
