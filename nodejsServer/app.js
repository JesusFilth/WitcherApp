const express = require("express");
   
const app = express();
   
// создаем парсер для данных в формате json
const jsonParser = express.json();
 
// настройка CORS
app.use(function(req, res, next) {
   res.header("Access-Control-Allow-Origin", "*");
   res.header("Access-Control-Allow-Headers", "Origin, X-Requested-With, Content-Type, Accept");
   res.header("Access-Control-Allow-Methods", "GET, PATCH, PUT, POST, DELETE, OPTIONS");
   next();  // передаем обработку запроса следующему методу в конвейере
 });
  
 app.get("/timer", function(request, response){
  setInterval(function() {
    console.log("tik-tak");
    response.send("ok");
  }, 2000);
  
});
app.post("/GameSearch", jsonParser,function(request,response){
  // если не переданы данные, возвращаем ошибку
  if(!request.body) return response.sendStatus(400);
  // отправка данных обратно клиенту
  response.json({
    "name":"Boris",
    "imgAvatarHref":"../../assets/images/goblin.jpg",
    "rank":19
  });
});
// обработчик по маршруту localhost:3000/postuser
app.post("/login", jsonParser, function (request, response) {
    // если не переданы данные, возвращаем ошибку
    if(!request.body) return response.sendStatus(400);
    // отправка данных обратно клиенту
    response.json({
      "name":"Bender",
      "winCount":11,
      "gold":1660,
      "imgAvatarHref":"../../assets/images/avatar.jpg",
      "pank":20,
      "friends":[
        {
          "name":"Valera",
          "lastTimeOnline":"Был онлайн вчера",
          "imgHref":"../../assets/images/goblin.jpg",
          "pank":20,
        },
        {
          "name":"Li",
          "lastTimeOnline":"Был онлайн 3 часа назад",
          "imgHref":"../../assets/images/goblin.jpg",
          "pank":20,
        },
        {
          "name":"Tomas",
          "lastTimeOnline":"Онлайн",
          "imgHref":"../../assets/images/goblin.jpg",
          "pank":20,
        },
        {
          "name":"Berejok HB",
          "lastTimeOnline":"Онлайн",
          "imgHref":"../../assets/images/goblin.jpg",
          "pank":19,
        }
      ]
    });
});
  
app.listen(3000);

