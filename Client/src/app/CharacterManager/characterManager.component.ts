import {Component, NgZone} from '@angular/core';
import {Router} from '@angular/router';

import {UserClient} from '../../models/UserClient';
import {Enemy} from '../../models/Enemy';
import {Opponents} from '../../models/Opponents';
import {SearchResult} from '../../models/SearchResult';

import {DataServer} from '../data.server';
import {GameSearchService} from './gameSearchManager.component';

@Component({
    selector:'app-person',
    templateUrl:'./characterManager.component.html',
    styleUrls:['./characterManager.component.css'],
    providers:[DataServer]
})
export class CharacterManager{

    pathAvatarImg: any = require("../../assets/images/avatar.jpg");
    pathFriendImg: any = require("../../assets/images/goblin.jpg");
    gameSearch:boolean = false;
 ///test

    user:UserClient = new UserClient();
    enemy:Enemy = new Enemy();
    opponents:Opponents = new Opponents();

    constructor(private router:Router, 
        private dataServer:DataServer,
        private gameSearchService:GameSearchService,
        private _ngZone: NgZone){
        this.user = this.router.getCurrentNavigation().extras.state;
        console.log(this.user);
        //сохронять данные пользователя при перезагурзке страницы
        if(this.user!=null){
            localStorage.setItem("user", JSON.stringify(this.user));
        }
        else{
            let temp = JSON.parse(localStorage.getItem("user"));
            this.user = temp;
        } 
    }
    ToGameConnection(){
        this.gameSearchService.connection(); 
        this.subscribeToEvents();
    }
    ToGameSearch(){
        this.gameSearch = true;
        this.gameSearchService.Send_GameSearch(this.user.name, this.user.rank);
    }
    exitSearchGame(){
        this.gameSearch = false;
        this.gameSearchService.Send_AbortGameSearch();
    }
    ToAbortionSearch(){

    }
    //service
    subscribeToEvents(){
        this.gameSearchService.messageConnection.subscribe((data: any) => {
            this._ngZone.run(() => {
             console.log(data);
             //регестрируем игру
             this.ToGameSearch();
            });
          });
          this.gameSearchService.messageGameSearch.subscribe((data: SearchResult) => {
            this._ngZone.run(() => {
             //игра нашлась
             //отправляет на поле боя
             console.log(data);      
             
             this.user.id = data.userId;//id подключения к лобби
             this.enemy = data.enemy;

             this.opponents.user = this.user;
             this.opponents.enemy = this.enemy;
             ///redirect
             this.router.navigate(['/GameProcess'],{
                state:this.opponents
            });
            });
          });
          this.gameSearchService.disconectGameSearch.subscribe((data: any) => {
            this._ngZone.run(() => {
             //дисконект
             console.log(data);
            
            });
          });
    }
}