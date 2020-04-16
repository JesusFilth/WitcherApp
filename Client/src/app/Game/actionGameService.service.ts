import { EventEmitter, Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder } from '@aspnet/signalr';

@Injectable()
export class ActionGameService {
  messageReceived = new EventEmitter<any>();
  rollCubesReceived = new EventEmitter<any>();
  connectionEstablished = new EventEmitter<Boolean>();

  private connectionIsEstablished = false;
  private _hubConnection: HubConnection;

  constructor() {
    this.createConnection();
    this.registerOnServerEvents();
    this.startConnection();
  }

  Send_upBet(gold:string) {
     this._hubConnection.invoke('UpBet', gold);
  }
  Send_rollCubes(count:number, index?:Array<number>){
    this._hubConnection.invoke('RollCubes',count,index);
  }
  Send_acceptBet(){
    this._hubConnection.invoke('AcceptBet');
  }

  private createConnection() {
    this._hubConnection = new HubConnectionBuilder()
      .withUrl("https://localhost:44398/MessageHub")
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
    this._hubConnection.on('RollCubesReceived', (data: any) => {
      this.rollCubesReceived.emit(data);
    });
  }
}  
