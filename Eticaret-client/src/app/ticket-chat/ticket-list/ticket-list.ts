import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router'; // routerLink için şart
import { HttpClient } from '@angular/common/http';
import { FormsModule } from '@angular/forms';

export enum TicketStatus {
  Pending = 1,
  Active = 2,
  Rejected = 3,
  Closed = 4,
}

export enum TicketCategory {
  OrderIssue = 1,
  ReturnRequest = 2,
  TechnicalSupport = 3,
  GeneralQuestion = 4,
  Other = 5,
}
@Component({
  selector: 'app-ticket-list',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './ticket-list.html',
  styleUrls: ['./ticket-list.scss'],
})
export class TicketList implements OnInit {
  tickets: any[] = [];
  isLoading: boolean = true;

  showCreateModal: boolean = false;
  isSubmitting: boolean = false;

  categoryOptions = [
    { id: TicketCategory.OrderIssue, name: 'Sipariş Sorunu' },
    { id: TicketCategory.ReturnRequest, name: 'İade Talebi' },
    { id: TicketCategory.TechnicalSupport, name: 'Teknik Destek' },
    { id: TicketCategory.GeneralQuestion, name: 'Genel Soru' },
    { id: TicketCategory.Other, name: 'Diğer' },
  ];
  newTicket = {
    category: TicketCategory.OrderIssue,
    subject: '',
    initialMessage: '',
  };
  constructor(
    private http: HttpClient,
    private cdr: ChangeDetectorRef,
  ) {}

  ngOnInit(): void {
    this.loadMyTickets();
  }

  loadMyTickets() {
    this.isLoading = true;

    this.http.get<any[]>('https://localhost:7185/api/support/my-tickets').subscribe({
      next: (res) => {
        this.tickets = res || [];
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Biletler yüklenirken hata oluştu', err);
        this.isLoading = false;
        this.cdr.detectChanges();
      },
    });
  }

  openNewTicketModal() {
    this.showCreateModal = true;
  }

  createTicket() {
    if (!this.newTicket.subject || !this.newTicket.initialMessage) return;
    this.isSubmitting = true;
    const payload = {
      category: Number(this.newTicket.category),
      subject: this.newTicket.subject,
      initialMessage: this.newTicket.initialMessage,
    };
    this.http.post('https://localhost:7185/api/support/create', payload).subscribe({
      next: () => {
        this.showCreateModal = false;
        this.isSubmitting = false;
        this.newTicket = { category: TicketCategory.OrderIssue, subject: '', initialMessage: '' };
        this.loadMyTickets();
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Talep oluşturulamadı. Detay:', err.error);
        this.isSubmitting = false;
      },
    });
  }
}
