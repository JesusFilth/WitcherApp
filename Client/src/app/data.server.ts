import {Injectable} from '@angular/core';
import {HttpClient, HttpHeaders} from '@angular/common/http';

import {Login} from '../models/Login'

@Injectable()
export class DataServer{
    private urlToNodeJs ="http://localhost:3000";
    private urlToAsp ="https://localhost:5001/home";

    constructor(private http:HttpClient){}

    logIn(login:Login){
        return this.http.post(this.urlToAsp+"/login", login);
        //if ok. return User
    }
    registration(login:Login){
        return this.http.post(this.urlToAsp+"/registration", login)
    }
    gameSearch(name:string){
        return this.http.post(this.urlToAsp+"/GameSearch",name);
        //if ok. return Enemy
    }
    rollCubes(){
        return this.http.get(this.urlToAsp+"/rollCubes");
    }
}