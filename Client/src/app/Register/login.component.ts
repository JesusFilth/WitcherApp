import {Component} from '@angular/core';
import {Router} from '@angular/router';

import {DataServer} from '../data.server';
import {Login} from '../../models/Login';
import {User} from '../../models/User';

@Component ({
    selector: 'app-login',
    templateUrl:'./login.component.html',
    styleUrls:['./login.style.css'],
    providers:[DataServer]
})
export class LoginComponent{

    login:Login = new Login();
    user:User = new User();

    imgEnter: any = require("../../assets/images/Enter-button.png");
    imgReg: any = require("../../assets/images/reg-button.png");

    constructor(private router:Router, private dataServer:DataServer){}

    goToPersonManager(){
        this.dataServer.logIn(this.login).subscribe((data:User)=>{
            this.user=data;
            this.router.navigate(['/characterManager'],{
                state:this.user
            })
        },error=>{console.log('error -> login -> go to person manager')});
    }
    goToRegister(){
        this.router.navigate(['/Register']);
        //this.dataServer.timer().subscribe((data:string)=>{console.log(data)})
    }
}