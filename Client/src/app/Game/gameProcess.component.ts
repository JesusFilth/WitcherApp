import {Component, NgZone} from '@angular/core';
import {Router} from '@angular/router';

import {Opponents} from '../../models/Opponents';
import {DataServer} from '../data.server';
import { ActionGameService } from './actionGameService.service';
import { GameManager} from "../../models/GameManager";
import {Cube} from '../../models/Cube';
import {EnemyDice} from '../../models/EnemyDice';

@Component({
    selector:'app-game',
    templateUrl:'./gameProcess.component.html',
    styleUrls:['./gameProcess.component.css'],
    providers:[DataServer]
})
export class GameProcessComponent{

    cubesEnemy: Array<Cube>;
    cubesInfo: Array<Cube>;

    gameManager:GameManager = new GameManager();

    upBetString:number;
    infoText:string = '';
    raundImg:any;

    opponents:Opponents = new Opponents();
    constructor(private router:Router, 
        private dataServer:DataServer, 
        private actionGameService:ActionGameService,
        private _ngZone:NgZone){
        this.opponents = router.getCurrentNavigation().extras.state;
        this.cubeInfoStart();
        this.subscribeToEvents();

        //сохронять данные пользователя при перезагурзке страницы
        if(this.opponents!=null){
            localStorage.setItem("opponents", JSON.stringify(this.opponents));
        }
        else{
            let temp = JSON.parse(localStorage.getItem("opponents"));
            this.opponents = temp;
        } 
    }
    choseChangeCubes(data:number){
        if(this.gameManager.rollRaund==1&&this.gameManager.bargainEnd)
           this.cubesInfo[data].change = !this.cubesInfo[data].change;
        else
           this.gameManager.message = "Сперва сторгуйтесь";
        
    }
    ChangeCubesInfo(data:number[]){
        for(var i=0;i<data.length;i++){
            if(this.cubesInfo[i].change){
                this.cubesInfo[i].value = data[i];
                this.cubesInfo[i].img = require("../../assets/images/cubes/dice_"+this.cubesInfo[i].value+".png");
                this.cubesInfo[i].change = false;
            }
        }
        //this.gameManager.rollRaund++;
    }
    rollCubes(){
        if(this.gameManager.rollRaund==0){
            if(this.gameManager.raund>1)
            {
                console.log("roll");
                this.cubeInfoStart();
                this.infoText = "Раунд "+this.gameManager.raund;
            }
            this.actionGameService.Send_rollCubes(this.opponents.user.id, 5);
        }
        else{
            let count=0;
            let index: Array<number> = new Array<number>();
                 
            this.cubesInfo.forEach(element => {
                if(element.change){
                    count++;
                    index.push(element.indexCubeOnTable);
                }        
            });
            if(count>0)
            {
                this.actionGameService.Send_rollCubes(this.opponents.user.id,count,index);
                this.gameManager.rollRaund++;
            }
            else{
                this.gameManager.message = "Выберите кости которые хотите перебросить";
            }
        }
    }
    cubeInfoStart(){     
        this.cubesInfo = new Array<Cube>();
        this.cubesEnemy = new Array<Cube>();
        for(var i=0;i<5;i++){
            this.cubesInfo.push(new Cube());
            this.cubesInfo[i].value = 1;
           this.cubesInfo[i].change = true;
           this.cubesInfo[i].indexCubeOnTable = i;
           this.cubesInfo[i].img = require("../../assets/images/cubes/dice_null.png");

           this.cubesEnemy.push(new Cube());
           this.cubesEnemy[i].img = require("../../assets/images/cubes/dice_null.png");
           this.cubesEnemy[i].indexCubeOnTable = i;
        }
    }
    viewImageEnemyCubes(cubes:number[]){
        for(var i=0;i<cubes.length;i++){
            this.cubesEnemy[i].img = require("../../assets/images/cubes/dice_"+cubes[i]+".png");
            this.cubesEnemy[i].indexCubeOnTable = i;
        }
    }
    upBet(){
        if(this.checkBetInputText()){
            this.actionGameService.Send_upBet(this.opponents.user.id, this.upBetString);
            this.upBetString = null;
        }

    }
    acceptBet(){
        this.actionGameService.Send_acceptBet(this.opponents.user.id);
    }
    passBet(){
        this.actionGameService.Send_passBet(this.opponents.user.id);
    }
    allInBet(){
        this.actionGameService.Send_allInBet(this.opponents.user.id);
    }
    infoTextUpdate(str:string){
        if(this.infoText == '')
            this.infoText = str;
        else{
            this.infoText+="\n ------------\n";
            this.infoText+=str;
        }
    }
    checkBetInputText(){
        if(this.upBetString<=0){
            this.infoTextUpdate("Недопустимо маленькая сумма");
            return false;
        }
        if(this.upBetString>this.gameManager.userGold){
            this.infoTextUpdate("Нужна больше золота!");
            return false;
        }
        return true;
    }
    checkRaund(data: GameManager){
        switch(data.raund){//обновляем img раунда
            case 1:this.raundImg = require("../../assets/images/one_raund.png"); break;
            case 2:this.raundImg = require("../../assets/images/two_raund.png"); break;
            case 3:this.raundImg = require("../../assets/images/tree_raund.png"); break;
        }
        this.infoTextUpdate(data.message);//обновляем инфу
        if(data.raund>this.gameManager.raund){//если новый раунд
            if(data.cubesEnemy!=null){//если нужно вскрыть оппонента
                this.viewImageEnemyCubes(data.cubesEnemy);
                //timer maby?
            }
            else{//если нет, то просто обнуляем кости
                this.cubeInfoStart();//обнуляем
            }
        }
        return data;
    }
    ///servece
    private subscribeToEvents(): void {
        //приходящие сообщение о статусе игры
        this.actionGameService.messageReceived.subscribe((data: GameManager) => {
          this._ngZone.run(() => {
           this.gameManager = this.checkRaund(data);
           console.log(this.gameManager);
          });
        });
        //roll cubes
        this.actionGameService.rollCubesReceived.subscribe((data: any) => {
            this._ngZone.run(() => {
                this.ChangeCubesInfo(data);
            });
          });
          this.actionGameService.connectionMessage.subscribe((data: any) => {
            this._ngZone.run(() => {
                //подключение установлено
                //создаем лобби
                this.actionGameService.Send_CreateRoom(this.opponents.user.name, this.opponents.user.gold, this.opponents.user.id);
            });
          });
          this.actionGameService.gameOverReceived.subscribe((data: any) => {
            this._ngZone.run(() => {
                //окончание игры
                console.log("Game Over");
                console.log(data);
                if(data.winCount>this.opponents.user.winCount){
                    alert("Вы победили");
                }
                else{
                    alert("Вы Проиграли");
                }
                console.log("gp");
                this.router.navigate(['/characterManager'],{
                    state:data
                });
            });
          });
      }
}