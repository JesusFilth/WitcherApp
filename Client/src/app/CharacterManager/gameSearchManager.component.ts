import { EventEmitter, Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder } from '@aspnet/signalr';

@Injectable()
export class GameSearchService {
  messageConnection = new EventEmitter<any>();
  messageGameSearch = new EventEmitter<any>();
  disconectGameSearch = new EventEmitter<any>();

  rollCubesReceived = new EventEmitter<any>();
  connectionEstablished = new EventEmitter<Boolean>();

  private connectionIsEstablished = false;
  private _hubConnection: HubConnection;

  Send_GameSearch(name:string, rank:number){
      this._hubConnection.invoke("GameSearch",name,rank);
  }
  private createConnection() {
    this._hubConnection = new HubConnectionBuilder()
      .withUrl("https://localhost:5001/GameSearch")
      .build();
  }
  connection(){
      this.createConnection();
      this.registerOnServerEvents();
      this.startConnection();
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
    this._hubConnection.on('MessageConnection', (data: any) => {
      this.messageConnection.emit(data);
    });
    this._hubConnection.on('MessageGameSearch', (data: any) => {
        this.messageGameSearch.emit(data);
      });
      this._hubConnection.on('DisconectGameSearch', (data: any) => {
        this.disconectGameSearch.emit(data);
      });
  }
}  
