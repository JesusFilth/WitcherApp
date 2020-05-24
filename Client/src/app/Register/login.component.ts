import {Component} from '@angular/core';
import {Router} from '@angular/router';

import {DataServer} from '../data.server';
import {Login} from '../../models/Login';
import {UserClient} from '../../models/UserClient';

@Component ({
    selector: 'app-login',
    templateUrl:'./login.component.html',
    styleUrls:['./login.style.css'],
    providers:[DataServer]
})
export class LoginComponent{

    login:Login = new Login();

    imgEnter: any = require("../../assets/images/Enter-button.png");
    imgReg: any = require("../../assets/images/reg-button.png");

    constructor(private router:Router, private dataServer:DataServer){}

    goToPersonManager(){
        this.dataServer.logIn(this.login).subscribe((data:UserClient)=>{
            this.router.navigate(['/characterManager'],{
                state:data
            })
        },error=>{console.log(error.error)});
    }
    goToRegister(){
        this.router.navigate(['/Register']);
        //this.dataServer.timer().subscribe((data:string)=>{console.log(data)})
    }
}