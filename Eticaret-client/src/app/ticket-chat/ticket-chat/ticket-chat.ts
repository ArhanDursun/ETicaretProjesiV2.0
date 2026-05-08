import {
  ChangeDetectorRef,
  Component,
  OnDestroy,
  OnInit,
  ViewChild,
  ElementRef,
  AfterViewChecked,
} from '@angular/core';
import { Signalr } from '../../core/signalr/signalr';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-ticket-chat',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslateModule, RouterModule],
  templateUrl: './ticket-chat.html',
  styleUrl: './ticket-chat.scss',
})
export class TicketChat implements OnInit, OnDestroy, AfterViewChecked {
  @ViewChild('scrollMe') private myScrollContainer!: ElementRef;
  
  ticket: any = null;
  ticketId: string = '';
  newMessage: string = '';
  messages: any[] = [];
  currentUserId: string = '';
  
  private messagesSubscription: Subscription | null = null;

  constructor(
    private signalRService: Signalr,
    private route: ActivatedRoute,
    private http: HttpClient,
    private cdr: ChangeDetectorRef,
  ) {}

  ngOnInit(): void {
    this.ticketId = this.route.snapshot.paramMap.get('id') || '';
    this.currentUserId = this.getMyIdFromToken() || '';

    if (this.ticketId) {
      // Subscribe to messages from SignalR service
      this.messagesSubscription = this.signalRService.messages$.subscribe(msgs => {
        this.messages = msgs;
        this.cdr.detectChanges();
      });

      this.loadTicketDetails();
    }
  }

  loadTicketDetails() {
    this.http.get<any>(`https://localhost:7185/api/support/${this.ticketId}/messages`).subscribe({
      next: (res) => {
        this.ticket = {
            id: res.id || this.ticketId,
            status: res.status,
            subject: res.subject || 'Destek Talebi'
        };

        const initialMsgs = res.messages || [];
        this.signalRService.loadInitialMessage(initialMsgs);
        this.signalRService.startConnection(this.ticketId);

        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Veriler çekilemedi', err);
      },
    });
  }

  getStatusKey(status: number): string {
    switch (status) {
      case 1: return 'PENDING';
      case 2: return 'CONFIRMED';
      case 3: return 'PROCESSING';
      case 4: return 'DELIVERED';
      case 5: return 'CANCELLED';
      default: return 'UNKNOWN';
    }
  }

  sendMessage() {
    if (!this.newMessage.trim()) return;

    this.signalRService.sendMessage(this.ticketId, this.newMessage, 'text');
    this.newMessage = '';
    this.cdr.detectChanges();
  }

  ngAfterViewChecked() {
    this.scrollToBottom();
  }

  scrollToBottom(): void {
    try {
      this.myScrollContainer.nativeElement.scrollTop =
        this.myScrollContainer.nativeElement.scrollHeight;
    } catch (err) {}
  }

  ngOnDestroy(): void {
    if (this.ticketId) {
      this.signalRService.stopConnection(this.ticketId);
    }
    if (this.messagesSubscription) {
        this.messagesSubscription.unsubscribe();
    }
  }

  getMyIdFromToken(): string | null {
    const token = localStorage.getItem('token');
    if (!token) return null;
    try {
      const payload = token.split('.')[1];
      const decoded = JSON.parse(atob(payload));
      return (
        decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] ||
        decoded['nameid'] ||
        null
      );
    } catch {
      return null;
    }
  }
}
