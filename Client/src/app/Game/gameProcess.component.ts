import {Component, NgZone} from '@angular/core';
import {Router} from '@angular/router';

import {Opponents} from '../../models/Opponents';
import {DataServer} from '../data.server';
import { ActionGameService } from './actionGameService.service';
import { GameManager} from "../../models/GameManager";
import {Cube} from '../../models/Cube';

@Component({
    selector:'app-game',
    templateUrl:'./gameProcess.component.html',
    styleUrls:['./gameProcess.component.css'],
    providers:[DataServer]
})
export class GameProcessComponent{

    cubesImage: Array<any> = new Array<any>();
    cubesEnemy: Array<any> = new Array<any>();

    cubesInfo:Array<Cube> = new Array<Cube>();
    cubes:number[];//change this
    gameManager:GameManager = new GameManager();

    upBetString:string = '';
    rollCount:number = 0;
    rollButtonText:string = 'Бросить кости';

    opponents:Opponents = new Opponents();
    constructor(private router:Router, 
        private dataServer:DataServer, 
        private actionGameService:ActionGameService,
        private _ngZone:NgZone){
        this.opponents = router.getCurrentNavigation().extras.state;
        this.cubeInfoStart();
        this.subscribeToEvents();

        this.gameManager.message = '';
    }
    choseChangeCubes(data:number){
        if(this.rollCount==1)
            this.cubesInfo[data].change = !this.cubesInfo[data].change;
    }
    ChangeCubesInfo(data:number[]){
        for(var i=0;i<data.length;i++){
            if(this.cubesInfo[i].change){
                this.cubesInfo[i].value = data[i];
                //this.cubesInfo[i].img = require("../../assets/images/cubes/cube_"+data[i]+".png");
                this.cubesInfo[i].change = false;
            }
        }
        this.rollCount++;
    }
    rollCubes(){
        if(this.rollCount==0){
            this.actionGameService.Send_rollCubes(5);
            this.rollButtonText = 'Перебросить кости';
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
                this.actionGameService.Send_rollCubes(count,index);
                this.rollButtonText = 'Бросить кости';
                this.rollCount++;
            }
            else{
                this.gameManager.message = "Выберите кости которые хотите перебросить";
            }
        }
    }
    cubeInfoStart(){     
        for(var i=0;i<5;i++){
            this.cubesInfo.push(new Cube());
            this.cubesInfo[i].value = 1;
           this.cubesInfo[i].change = true;
           this.cubesInfo[i].indexCubeOnTable = i;
           //this.cubesInfo[i].img = require("../../assets/images/cubes/cube_"+this.cubesInfo[i].value+".png");

           //this.cubesEnemy.push(require("../../assets/images/cubes/cube_enemy_private.png"));
        }
        console.log(this.cubesInfo);
    }
    viewImageCubes(){
        for(var i=0;i<this.cubes.length;i++){
            //this.cubesImage[i] = require("../../assets/images/cubes/cube_"+this.cubes[i]+".png");
        }
    }
    viewImageEnemyCubes(){
        for(var i=0;i<this.gameManager.cubesEnemy.length;i++){
            //this.cubesEnemy[i] = require("../../assets/images/cubes/cube_"+this.gameManager.cubesEnemy[i]+".png");
        }
    }
    upBet(){
        this.actionGameService.Send_upBet(this.upBetString);
    }
    acceptBet(){
        this.actionGameService.Send_acceptBet();
    }
    ///servece
    private subscribeToEvents(): void {
        this.actionGameService.messageReceived.subscribe((data: GameManager) => {
          this._ngZone.run(() => {
           this.gameManager = data;
           if(this.gameManager.cubesEnemy!=null){
            this.viewImageEnemyCubes();
           }
          });
        });
        //roll cubes
        this.actionGameService.rollCubesReceived.subscribe((data: any) => {
            this._ngZone.run(() => {
                this.ChangeCubesInfo(data);
            });
          });
      }
}