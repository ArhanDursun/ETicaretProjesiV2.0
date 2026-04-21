import {
  ChangeDetectorRef,
  Component,
  OnDestroy,
  OnInit,
  ViewChild,
  ElementRef,
  AfterViewChecked,
} from '@angular/core';
import { Observable } from 'rxjs';
import { MessageDto, Signalr } from '../../core/signalr/signalr';
import { ActivatedRoute } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { form } from '@angular/forms/signals';

@Component({
  selector: 'app-ticket-chat',
  imports: [CommonModule, FormsModule],
  templateUrl: './ticket-chat.html',
  styleUrl: './ticket-chat.scss',
})
export class TicketChat implements OnInit, OnDestroy, AfterViewChecked {
  @ViewChild('scrollMe') private myScrollContainer!: ElementRef;
  ticketId: string = '';
  messageText: string = '';
  messages$: any;
  currentUserId: string = '';
  ticketStatus: number = 0;
  @ViewChild('fileInput') fileInput!: ElementRef;
  constructor(
    private signalRService: Signalr,
    private route: ActivatedRoute,
    private http: HttpClient,
    private cdr: ChangeDetectorRef,
  ) {
    this.messages$ = this.signalRService.messages$;
  }

  ngOnInit(): void {
    this.ticketId = this.route.snapshot.paramMap.get('id') || '';
    this.currentUserId = this.getMyIdFromToken() || '';

    if (this.ticketId) {
      this.http.get<any>(`https://localhost:7185/api/support/${this.ticketId}/messages`).subscribe({
        next: (res) => {
          this.ticketStatus = res.status;

          const mesajlar = res.messages || [];

          this.signalRService.loadInitialMessage(mesajlar);
          this.signalRService.startConnection(this.ticketId);

          this.cdr.detectChanges();
        },
        error: (err) => {
          console.error('Veriler çekilemedi', err);
        },
      });
    }
    this.cdr.detectChanges();
  }

  sendMessage() {
    if (!this.messageText.trim()) return;

    this.signalRService.sendMessage(this.ticketId, this.messageText, 'text');
    this.cdr.detectChanges();
    this.messageText = '';
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
  onFilesSelected(event: any) {
    const files: FileList = event.target.files;
    if (files.length === 0) return;

    const formData = new FormData();

    for (let i = 0; i < files.length; i++) {
      formData.append('files', files[i]);
    }

    this.http
      .post<{ urls: string[] }>(`https://localhost:7185/api/support/upload-support-file`, formData)
      .subscribe({
        next: (res) => {
          res.urls.forEach((url) => {
            this.signalRService.sendMessage(this.ticketId, url, 'image');
          });
          event.target.value = '';
          this.cdr.detectChanges();
        },
        error: (err) => {
          console.error('Dosya yüklenemedi', err);
          alert('resimler yüklenirken bir hata oluştu. Lütfen tekrar deneyin.');
        },
      });
  }
}
