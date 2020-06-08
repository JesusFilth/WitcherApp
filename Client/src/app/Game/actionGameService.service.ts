import { EventEmitter, Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder } from '@aspnet/signalr';

@Injectable()
export class ActionGameService {
  messageReceived = new EventEmitter<any>();
  rollCubesReceived = new EventEmitter<any>();
  connectionMessage = new EventEmitter<any>();
  gameOverReceived = new EventEmitter<any>();

  connectionEstablished = new EventEmitter<Boolean>();

  private connectionIsEstablished = false;
  private _hubConnection: HubConnection;

  constructor() {
    this.createConnection();
    this.registerOnServerEvents();
    this.startConnection();
  }

  Send_upBet(id:string, gold:number) {
     this._hubConnection.invoke('UpBet',id, gold);
  }
  Send_rollCubes(id:string,count:number, index?:Array<number>){
    this._hubConnection.invoke('RollCubes',id,count,index);
  }
  Send_passBet(id:string){
    this._hubConnection.invoke('PassBet',id);
  }
  Send_acceptBet(id:string){
    this._hubConnection.invoke('AcceptBet',id);
  }
  Send_allInBet(id:string){
    this._hubConnection.invoke('AllInBet',id);
  }
  Send_CreateRoom(name:string, gold:number, id:string){
    this._hubConnection.invoke('CreateRoom',name, gold, id);
  }

  private createConnection() {
    this._hubConnection = new HubConnectionBuilder()
      .withUrl("https://localhost:5001/GameProcess")
      .build();
  }

  private startConnection(): void {
    this._hubConnection
      .start()
      .then(() => {
        this.connectionIsEstablished = true;
        console.log('Hub connection started');
        this.connectionEstablished.emit(true);
      })
      .catch(err => {
        console.log('Error while establishing connection, retrying...');
        setTimeout(function () { this.startConnection(); }, 20000);
      });
  }

  private registerOnServerEvents(): void {
    this._hubConnection.on('MessageReceived', (data: any) => {
      this.messageReceived.emit(data);
    });
    this._hubConnection.on('GameOverReceived', (data: any) => {
      this.gameOverReceived.emit(data);
    });
    this._hubConnection.on('RollCubesReceived', (data: any) => {
      this.rollCubesReceived.emit(data);
    });
    this._hubConnection.on('ConnectionMessage', (data: any) => {
      this.connectionMessage.emit(data);
    });
  }
}  
