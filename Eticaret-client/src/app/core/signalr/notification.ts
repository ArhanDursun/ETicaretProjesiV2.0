import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { Subject } from 'rxjs';
@Injectable({
  providedIn: 'root',
})
export class NotificationSignalR {
  private hubConnection!: signalR.HubConnection;
  public trendUpdate$ = new Subject<string>();

  constructor() {}

  public startConnection = () => {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl('https://localhost:7185/notificationhub')
      .withAutomaticReconnect()
      .build();

    this.hubConnection
      .start()
      .then(() => {
        console.log('NotificationHub bağlantısı kuruldu');
        this.addTrendUpdateListener();
      })
      .catch((err) => console.error('Bağlantı Hatası', err));
  };

  private addTrendUpdateListener = () => {
    this.hubConnection.on('ReceiveTrendUpdate', (message: string) => {
      console.log('Redisten gelen sinyal', message);
      this.trendUpdate$.next(message);
    });
  };
}
