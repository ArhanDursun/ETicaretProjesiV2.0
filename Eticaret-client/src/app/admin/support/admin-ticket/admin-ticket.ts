import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-admin-ticket',
  imports: [CommonModule, RouterModule],
  templateUrl: './admin-ticket.html',
  styleUrl: './admin-ticket.scss',
})
export class AdminTicket implements OnInit {
  pendingTickets: any[] = [];
  myActiveTickets: any[] = [];
  isLoading: boolean = true;

  constructor(
    private http: HttpClient,
    private cdr: ChangeDetectorRef,
  ) {}

  ngOnInit(): void {
    this.loadTickets();
  }

  loadTickets() {
    this.isLoading = true;

    this.http.get<any[]>('https://localhost:7185/api/support/pending').subscribe((res) => {
      this.pendingTickets = res || [];

      this.http.get<any[]>('https://localhost:7185/api/support/my-active').subscribe((res2) => {
        this.myActiveTickets = res2 || [];
        this.isLoading = false;
        this.cdr.detectChanges();
      });
      this.cdr.detectChanges();
    });
  }

  assignTicket(ticketId: string) {
    this.http.post(`https://localhost:7185/api/support/${ticketId}/assign`, {}).subscribe({
      next: () => {
        alert('Bilet başarıyla üzerinize alındı!');
        this.loadTickets();
      },
      error: (err) => console.error('Hata Oluştu', err),
    });
  }

  closeTicket(ticketId: string) {
    if (!confirm('Bu talebi kapatmak istediğinize emin misiniz?')) return;

    this.http.put(`https://localhost:7185/api/support/${ticketId}/status?status=4`, {}).subscribe({
      next: () => {
        alert('Bilet kapatıldı.');
        this.loadTickets();
      },
      error: (err) => console.error('Kapatılamadı', err),
    });
  }
  rejectTicket(ticketId: string) {
    if (!confirm('Bu talebi reddetmek istediğinize emin misiniz?')) return;

    this.http.put(`https://localhost:7185/api/support/${ticketId}/status?status=3`, {}).subscribe({
      next: () => {
        alert('Talep reddedildi ve arşive kaldırıldı');
        this.loadTickets();
      },
      error: (err) => console.error('Reddedilemedi', err),
    });
  }
}
