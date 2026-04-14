import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { NotificationDto, NotificationService } from '../../services/notification';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-notification-component',
  imports: [CommonModule, FormsModule],
  templateUrl: './notification-component.html',
  styleUrl: './notification-component.scss',
})
export class NotificationComponent implements OnInit {
  notification: NotificationDto[] = [];
  isLoading: boolean = true;

  constructor(
    private notificationService: NotificationService,
    private router: Router,
    private cdr: ChangeDetectorRef,
  ) {}

  ngOnInit(): void {
    this.loadNotification();
    this.cdr.detectChanges();
  }

  loadNotification() {
    this.isLoading = true;

    this.notificationService.getAllNotifications().subscribe({
      next: (data) => {
        this.notification = data || [];
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Bildirimler alınırken hata', err);
        this.isLoading = false;
      },
    });
  }
  markAsRead(id: string) {
    this.notificationService.markAsRead(id).subscribe({
      next: () => {
        this.notification = this.notification.filter((n) => n.id !== id);
      },
      error: (err) => console.error('Okundu işaretlenemedi:', err),
    });
  }

  goToProduct(productId: string) {
    if (productId) {
      this.router.navigate(['/product/detay', productId]);
    }
  }

  goToOffers() {
    this.router.navigate(['/offers']);
  }
}
