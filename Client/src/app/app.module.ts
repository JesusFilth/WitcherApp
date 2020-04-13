import { NgModule }      from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule }   from '@angular/forms';
import {Routes, RouterModule} from '@angular/router';
import {HttpClientModule} from '@angular/common/http';

import {AppComponent} from './app.component';
import {LoginComponent} from './Register/login.component';
import {CharacterManager} from './CharacterManager/characterManager.component';
import {RegisterComponent} from './Register/Register.component';
import {GameProcessComponent} from './Game/gameProcess.component';

import {AppChatComponent} from './chat/app.component';
import {ChatService} from './chat/chat.service'

const appRoutes: Routes = [
    {path:'', component:LoginComponent},
    {path:'characterManager',component:CharacterManager},
    {path:'Register',component:RegisterComponent},
    {path:'GameProcess',component:GameProcessComponent},
    {path:'chat', component:AppChatComponent}
];
@NgModule({
    imports:      [ BrowserModule, FormsModule, HttpClientModule, RouterModule.forRoot(appRoutes) ],
    declarations: [ AppComponent,AppChatComponent, LoginComponent, CharacterManager, RegisterComponent, GameProcessComponent],
    providers:[ChatService],
    bootstrap:    [ AppComponent ]
})
export class AppModule { }