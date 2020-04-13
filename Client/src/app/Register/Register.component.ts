import {Component} from '@angular/core';
import {Router} from '@angular/router';

@Component({
    selector:'app-register',
    templateUrl:'./Register.component.html',
    styleUrls:['./login.style.css']
})
export class RegisterComponent{

    constructor(private router:Router){}

    goToCharecterManager(){
        this.router.navigate(['/characterManager']);
    }
}