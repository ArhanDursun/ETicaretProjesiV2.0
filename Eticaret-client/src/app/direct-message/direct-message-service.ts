import { HttpClient } from '@angular/common/http';
import { Injectable, NgZone } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { toDegrees } from 'chart.js/helpers';
import { BehaviorSubject, tap } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class DirectMessageService {
  private apiUrl = 'https://localhost:7185/api/DirectMessage';
  private hubConnection!: signalR.HubConnection;

  private messageThreadSource = new BehaviorSubject<any[]>([]);
  messageThread$ = this.messageThreadSource.asObservable();

  private unreadTotalSource = new BehaviorSubject<number>(0);
  unreadTotal$ = this.unreadTotalSource.asObservable();

  private recentChatsSource = new BehaviorSubject<any[]>([]);
  recentChats$ = this.recentChatsSource.asObservable();

  public activeChatUserId: string | null = null;
  constructor(
    private http: HttpClient,
    private zone: NgZone,
  ) {}

  createHubConnection(token: string) {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl('https://localhost:7185/chathub', {
        accessTokenFactory: () => token,
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection
      .start()
      .then()
      .catch((err) => console.error('chathub Hatası', err));

    this.hubConnection.on('ReceiveDirectMessage', (message) => {
      this.zone.run(() => {
        const incomingSender = (message.senderId || message.SenderId)?.toLowerCase();
        const incomingReceiver = (message.receiverId || message.ReceiverId)?.toLowerCase();
        const activeUser = this.activeChatUserId?.toLowerCase();

        if (activeUser && (incomingSender === activeUser || incomingReceiver === activeUser)) {
          const currentMessages = this.messageThreadSource.value;

          const isDuplicate = currentMessages.some(
            (m) => (m.id || m.Id) === (message.id || message.Id),
          );

          if (!isDuplicate) {
            this.messageThreadSource.next([...currentMessages, message]);
          }
        } else {
          console.log('EŞLEŞMEDİ! Arka planda bildirim yakılıyor...');
          this.unreadTotalSource.next(this.unreadTotalSource.value + 1);
        }

        this.loadRecentChats();
      });
    });
  }

  loadRecentChats() {
    this.http.get<any[]>(`${this.apiUrl}/recent-chats`).subscribe({
      next: (chats) => {
        this.recentChatsSource.next(chats);
        const totalUnread = chats.reduce((sum, chat) => sum + chat.unreadCount, 0);
        this.unreadTotalSource.next(totalUnread);
      },
      error: (err) => {
        console.error('🚨 BACKEND PATLAMA SEBEBİ:', err.error);
      },
    });
  }

  loadChatHistory(otherUserId: string) {
    return this.http.get<any[]>(`${this.apiUrl}/history/${otherUserId}`).pipe(
      tap((messages) => {
        this.messageThreadSource.next(messages);
      }),
    );
  }

  sendMessage(receiverId: string, content: string, messageType: string = 'text') {
    return this.http.post(`${this.apiUrl}/send`, { receiverId, content, messageType });
  }

  stopHubConnection() {
    if (this.hubConnection) {
      this.hubConnection.stop();
    }
  }
  getAvailableUsers() {
    return this.http.get<any[]>(`${this.apiUrl}/available-users`);
  }

  markAsRead(otherUserId: string) {
    return this.http.post(`${this.apiUrl}/mark-read/${otherUserId}`, {});
  }

  uploadChatFiles(files: FileList) {
    const formdata = new FormData();
    for (let i = 0; i < files.length; i++) {
      formdata.append('files', files[i]);
    }
    return this.http.post<any>(`${this.apiUrl}/upload-chat-files`, formdata);
  }
}
