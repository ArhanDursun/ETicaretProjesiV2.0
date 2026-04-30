import { Injectable, NgZone } from '@angular/core';
import { tick } from '@angular/core/testing';
import * as signalR from '@microsoft/signalr';
import { BehaviorSubject, Subject } from 'rxjs';
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
  private trafficConnection: signalR.HubConnection | undefined;
  private notificationConnection: signalR.HubConnection | undefined;

  private reportNotificationSource = new Subject<{ message: string; downloadUrl: string }>();
  public reportNotification$ = this.reportNotificationSource.asObservable();

  private messageSource = new BehaviorSubject<MessageDto[]>([]);
  public messages$ = this.messageSource.asObservable();

  private priceAlertSource = new Subject<{ message: string; productId: string }>();
  public priceAlert$ = this.priceAlertSource.asObservable();

  private onlineUsersSource = new BehaviorSubject<string[]>([]);
  public onlineUsers$ = this.onlineUsersSource.asObservable();
  constructor(private zone: NgZone) {
    this.startTrafficConnection();
    this.startNotificationConnection();
  }

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
  private startTrafficConnection() {
    const token = localStorage.getItem('token') || sessionStorage.getItem('token');
    if (!token) {
      return;
    }

    if (
      this.notificationConnection?.state === signalR.HubConnectionState.Connected ||
      this.notificationConnection?.state === signalR.HubConnectionState.Connecting
    ) {
      return;
    }
    this.trafficConnection = new signalR.HubConnectionBuilder()
      .withUrl('https://localhost:7185/traffichub', {
        accessTokenFactory: () => token || '',
      })
      .withAutomaticReconnect()
      .build();

    this.trafficConnection
      .start()
      .then(() => {
        console.log('TrafficHub bağlantısı başarılı');
        this.addTrafficListeners();
      })
      .catch((err) => console.error('TrafficHub Bağlantı Hatası', err));
  }

  private addTrafficListeners() {
    this.trafficConnection?.on('UpdateOnlineStatus', (userIds: string[]) => {
      this.zone.run(() => {
        this.onlineUsersSource.next(userIds);
      });
    });
  }
  public stopTrafficConnection() {
    if (this.trafficConnection) {
      this.trafficConnection
        .stop()
        .then(() => {
          console.log('TrafficHub bağlantısı kesildi.');
          this.onlineUsersSource.next([]);
        })
        .catch((err) => console.error('TrafficHub durdurulurken hata:', err));
    }
  }
  private startNotificationConnection() {
    const token = localStorage.getItem('token') || sessionStorage.getItem('token');
    if (!token) {
      return;
    }

    if (
      this.notificationConnection?.state === signalR.HubConnectionState.Connected ||
      this.notificationConnection?.state === signalR.HubConnectionState.Connecting
    ) {
      return;
    }
    this.notificationConnection = new signalR.HubConnectionBuilder()

      .withUrl('https://localhost:7185/notificationhub', {
        accessTokenFactory: () => token || '',
      })
      .withAutomaticReconnect()
      .build();

    this.notificationConnection
      .start()
      .then(() => {
        console.log('NotificationHub bağlantısı başarılı 🚀');
        this.addNotificationListeners();
      })
      .catch((err) => console.error('NotificationHub Bağlantı Hatası', err));
  }

  private addNotificationListeners() {
    this.notificationConnection?.on('ReceiveReportNotification', (data: any) => {
      this.zone.run(() => {
        this.reportNotificationSource.next({
          message: data.message,
          downloadUrl: data.downloadUrl,
        });
      });
    });
    this.notificationConnection?.on('ReceivePriceAlert', (data: any) => {
      this.zone.run(() => {
        console.log('Sinyal :', data);
        this.priceAlertSource.next({
          message: data.message,
          productId: data.productId,
        });
      });
    });
  }

  public stopNotificationConnection() {
    if (this.notificationConnection) {
      this.notificationConnection
        .stop()
        .then(() => console.log('NotificationHub bağlantısı kesildi.'))
        .catch((err) => console.error('NotificationHub durdurulurken hata:', err));
    }
  }
}
