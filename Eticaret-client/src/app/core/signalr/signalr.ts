import { Injectable, NgZone } from '@angular/core';
import { tick } from '@angular/core/testing';
import * as signalR from '@microsoft/signalr';
import { BehaviorSubject } from 'rxjs';
export interface MessageDto {
  id?: string;
  ticketId: string;
  senderId: string;
  senderName: string;
  messageBody: string;
  sentAt: Date;
  isAdmin: boolean;
}
@Injectable({
  providedIn: 'root',
})
export class Signalr {
  private hubConnection: signalR.HubConnection | undefined;
  private messageSource = new BehaviorSubject<MessageDto[]>([]);
  public messages$ = this.messageSource.asObservable();

  constructor(private zone: NgZone) {}

  public startConnection(ticketId: string) {
    const token = localStorage.getItem('token') || sessionStorage.getItem('token');
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl('https://localhost:7185/supporthub', {
        accessTokenFactory: () => token || '',
      })
      .withAutomaticReconnect()
      .build();
    this.hubConnection
      .start()
      .then(() => {
        this.joinTicketGroup(ticketId);
        this.addRecieveMessageListener();
      })
      .catch((err) => console.error('Signalr bağlantı hatası', err));
  }

  private joinTicketGroup(ticketId: string) {
    this.hubConnection
      ?.invoke('JoinTicketGroup', ticketId)
      .catch((err) => console.error('Odaya girilemedi', err));
  }

  public stopConnection(ticketId: string) {
    this.hubConnection
      ?.invoke('LeaveTicketGroup', ticketId)
      .then(() => {
        this.hubConnection?.stop();
        this.messageSource.next([]);
      })
      .catch((err) => console.error('Odadan Çıkılamadı', err));
  }

  public sendMessage(ticketId: string, messageBody: string, messageType: string = 'text') {
    this.hubConnection
      ?.invoke('SendMessage', ticketId, messageBody, messageType)
      .catch((err) => console.error('Mesaj Gönderilemedi', err));
  }

  private addRecieveMessageListener() {
    this.hubConnection?.on('RecieveMessage', (message: MessageDto) => {
      this.zone.run(() => {
        const currentMessages = this.messageSource.value;
        this.messageSource.next([...currentMessages, message]);
      });
    });

    this.hubConnection?.on('ErrorMessage', (errorMsg: string) => {
      this.zone.run(() => {
        alert('Hata: ' + errorMsg);
      });
    });
  }

  public loadInitialMessage(messages: MessageDto[]) {
    this.messageSource.next(messages);
  }
}
