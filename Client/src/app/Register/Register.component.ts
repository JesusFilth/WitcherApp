import {Component} from '@angular/core';
import {Router} from '@angular/router';
import { DataServer } from '../data.server';
import { Login } from 'src/models/Login';

@Component({
    selector:'app-register',
    templateUrl:'./Register.component.html',
    styleUrls:['./login.style.css'],
    providers:[DataServer]
})
export class RegisterComponent{

    loginReg: Login = new Login();
    constructor(private router:Router, private server:DataServer){}

    goToCharecterManager(){ 
        this.server.registration(this.loginReg).subscribe(()=>{
            this.router.navigate(['/Login']);
        },error=>{console.log(error.error)})  
    }
}