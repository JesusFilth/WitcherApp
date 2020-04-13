import { Component, NgZone } from '@angular/core';
import { Message } from './message';
import { ChatService } from './chat.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppChatComponent {

  title = 'ClientApp';
  txtMessage: string = '';
  uniqueID: string = new Date().getTime().toString();
  messages = new Array<Message>();
  message = new Message();

timeLeft: number = 60;
  interval; 
  timer:string = 'tic';
  constructor(
    private chatService: ChatService,
    private _ngZone: NgZone
  ) {
    this.subscribeToEvents();
    this.startTimer();
  }
  sendMessage(): void {
    if (this.txtMessage) {
      this.message = new Message();
      this.message.clientuniqueid = this.uniqueID;
      this.message.type = "sent";
      this.message.message = this.txtMessage;
      this.message.date = new Date();
      this.messages.push(this.message);
      //this.chatService.sendMessage(this.message);
      this.txtMessage = '';
    }
  }
  private subscribeToEvents(): void {

    this.chatService.messageReceived.subscribe((message: string) => {
      this._ngZone.run(() => {
       this.timer = message;
      });
    });
  }
  startTimer() {
    this.interval = setInterval(() => {
      if(this.timeLeft > 0) {
        this.timeLeft--;
        this.chatService.sendMessage(this.timeLeft.toString());
      } else {
        this.timeLeft = 60;
      }
    },1000)
  }
}
