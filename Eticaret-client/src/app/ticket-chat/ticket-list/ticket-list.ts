import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';

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
  imports: [CommonModule, RouterModule, FormsModule, TranslateModule],
  templateUrl: './ticket-list.html',
  styleUrls: ['./ticket-list.scss'],
})
export class TicketList implements OnInit {
  tickets: any[] = [];
  isLoading: boolean = true;
  showCreateModal: boolean = false;
  isSubmitting: boolean = false;

  newTicket = {
    categoryId: 1,
    subject: '',
    message: '',
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

  createTicket() {
    if (!this.newTicket.subject || !this.newTicket.message) {
        alert('Lütfen tüm alanları doldurunuz.');
        return;
    }
    this.isSubmitting = true;
    const payload = {
      category: Number(this.newTicket.categoryId),
      subject: this.newTicket.subject,
      initialMessage: this.newTicket.message,
    };
    this.http.post('https://localhost:7185/api/support/create', payload).subscribe({
      next: () => {
        this.showCreateModal = false;
        this.isSubmitting = false;
        this.newTicket = { categoryId: 1, subject: '', message: '' };
        this.loadMyTickets();
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Talep oluşturulamadı. Detay:', err.error);
        alert('Hata: ' + (err.error?.message || 'Talep oluşturulurken bir hata oluştu.'));
        this.isSubmitting = false;
        this.cdr.detectChanges();
      },
    });
  }
}
