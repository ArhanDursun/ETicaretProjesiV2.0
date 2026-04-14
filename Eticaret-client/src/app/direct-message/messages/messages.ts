import { ChangeDetectorRef, Component, OnDestroy, OnInit } from '@angular/core';
import { DirectMessageService } from '../direct-message-service';
import { Auth } from '../../auth/services/auth';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-messages',
  imports: [CommonModule, FormsModule],
  templateUrl: './messages.html',
  styleUrl: './messages.scss',
})
export class Messages implements OnInit, OnDestroy {
  recentChats: any[] = [];
  messageThread: any[] = [];
  activeUserId: string | null = null;
  activeUserName: string = '';

  messageText: string = '';
  myId: string = '';

  isNewChatModalOpen: boolean = false;
  availableUsers: any[] = [];
  filteredUsers: any[] = [];
  searchUserText: string = '';
  constructor(
    private dmService: DirectMessageService,
    private authService: Auth,
    private cdr: ChangeDetectorRef,
  ) {
    const token = localStorage.getItem('token') || sessionStorage.getItem('token');
    if (token) {
      const decoded = this.authService.getDecodedToken(token);
      this.myId =
        decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] ||
        decoded['nameid'];
    }
  }
  ngOnInit(): void {
    this.dmService.recentChats$.subscribe((chats) => {
      this.recentChats = chats;
      this.cdr.detectChanges();
    });

    this.dmService.messageThread$.subscribe((messages) => {
      this.messageThread = messages;
      this.cdr.detectChanges();
      this.scrollToBottom();
    });
  }

  selectChat(userId: string, userName: string) {
    this.activeUserId = userId;
    this.activeUserName = userName;
    this.dmService.activeChatUserId = userId;
    this.cdr.detectChanges();

    this.dmService.loadChatHistory(userId).subscribe(() => {
      this.cdr.detectChanges();
      this.scrollToBottom();
    });
    this.dmService.markAsRead(userId).subscribe(() => {
      this.dmService.loadRecentChats();
    });
  }

  sendMessage() {
    if (!this.messageText.trim() || !this.activeUserId) return;

    this.dmService.sendMessage(this.activeUserId, this.messageText).subscribe({
      next: (res) => {
        this.messageThread.push(res);
        this.messageText = '';
        this.cdr.detectChanges();
        this.scrollToBottom();
        this.dmService.loadRecentChats();
      },
      error: (err) => alert('Mesaj gönderilemedi: ' + err.message),
    });
  }

  scrollToBottom() {
    setTimeout(() => {
      const container = document.getElementById('chat-scroll-area');
      if (container) {
        container.scrollTop = container.scrollHeight;
      }
    }, 50);
  }
  openNewChatModal() {
    this.isNewChatModalOpen = true;
    this.searchUserText = '';
    this.cdr.detectChanges();
    this.dmService.getAvailableUsers().subscribe((users) => {
      this.availableUsers = users;
      this.filteredUsers = users;
      this.cdr.detectChanges();
    });
  }

  closeNewChatModal() {
    this.isNewChatModalOpen = false;
    this.cdr.detectChanges();
  }

  filterUsers() {
    if (!this.searchUserText) {
      this.filteredUsers = this.availableUsers;
    } else {
      this.filteredUsers = this.availableUsers.filter((u) =>
        u.fullName.toLowerCase().includes(this.searchUserText.toLowerCase()),
      );
    }
    this.cdr.detectChanges();
  }

  startNewChatWith(userId: string, userName: string) {
    this.closeNewChatModal();

    this.activeUserId = userId;
    this.activeUserName = userName;
    this.dmService.activeChatUserId = userId;
    this.cdr.detectChanges();

    this.dmService.loadChatHistory(userId).subscribe(() => {
      this.cdr.detectChanges();
      this.scrollToBottom();
    });
    this.dmService.markAsRead(userId).subscribe(() => {
      this.dmService.loadRecentChats();
    });
  }
  ngOnDestroy(): void {
    this.activeUserId = null;
    this.dmService.activeChatUserId = null;
  }
}
