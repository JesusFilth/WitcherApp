import {Injectable} from '@angular/core';
import {HttpClient, HttpHeaders} from '@angular/common/http';

import {Login} from '../models/Login'

@Injectable()
export class DataServer{
    private urlToNodeJs ="http://localhost:3000";
    private urlToAsp ="https://localhost:44398/home";

    constructor(private http:HttpClient){}

    logIn(login:Login){
        return this.http.post(this.urlToNodeJs+"/login", login);
    }
    gameSearch(name:string){
        return this.http.post(this.urlToNodeJs+"/GameSearch",name);
    }

    
   // timer(){
    //    return this.http.get(this.url+"/timer");
   // }
}