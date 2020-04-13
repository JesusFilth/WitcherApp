import {Component} from '@angular/core';
import {Router} from '@angular/router';

import {User} from '../../models/User';
import {Enemy} from '../../models/Enemy';
import {Opponents} from '../../models/Opponents';

import {DataServer} from '../data.server';

@Component({
    selector:'app-person',
    templateUrl:'./characterManager.component.html',
    styleUrls:['./characterManager.component.css'],
    providers:[DataServer]
})
export class CharacterManager{

    pathAvatarImg: any = require("../../assets/images/avatar.jpg");
    pathFriendImg: any = require("../../assets/images/goblin.jpg");
 ///test

    user:User = new User();
    enemy:Enemy = new Enemy();
    opponents:Opponents = new Opponents();

    constructor(private router:Router, private dataServer:DataServer){
        this.user = this.router.getCurrentNavigation().extras.state;
    }
    ToGameSearch(){
        this.dataServer.gameSearch(this.user.name).subscribe((data:Enemy)=>{
            this.enemy = data;
            this.opponents.user = this.user;
            this.opponents.enemy = this.enemy;
            this.router.navigate(['/GameProcess'],{
                state:this.opponents
            });
        }, error=>{console.log("error -> character Manager -> to game search")});
    }

}