import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { OrderService } from '../../services/order';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-seller-orders',
  imports: [CommonModule, FormsModule, TranslateModule],
  templateUrl: './seller-orders.html',
  styleUrl: './seller-orders.scss',
})
export class SellerOrders implements OnInit {
  incomingOrders: any[] = [];
  isLoading: boolean = true;
  showUpdateModal: boolean = false;
  selectedOrderId: string = '';
  selectedNewStatus: number = 0;
  isUpdating: boolean = false;

  constructor(
    private orderService: OrderService,
    private cdr: ChangeDetectorRef,
  ) {}

  getSellerIdFromToken(): string | null {
    const token = localStorage.getItem('token') || sessionStorage.getItem('token');
    if (!token) {
      return null;
    }

    try {
      const payload = JSON.parse(atob(token.split('.')[1]));

      const nameIdentifierClaim =
        'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier';

      const userId = payload[nameIdentifierClaim] || payload.nameid || payload.sub;

      return userId;
    } catch (error) {
      console.error('Token Parçalanırken Hata Oluştu' + error);
      return null;
    }
  }
  ngOnInit(): void {
    this.loadSellerOffers();
  }

  loadSellerOffers() {
    this.isLoading = false;
    const currentSellerId = this.getSellerIdFromToken();

    if (!currentSellerId) {
      alert('Oturum süreniz dolmuş veya giriş yapmamışsınız! Lütfen tekrar giriş yapın.');
      this.isLoading = false;
      this.cdr.detectChanges();
      return;
    }

    this.orderService.getSellerOrders(currentSellerId).subscribe({
      next: (res) => {
        this.incomingOrders = res;
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Veriler Çekilirken Bir Hata Oluştu' + err);
        this.isLoading = false;
        this.cdr.detectChanges();
      },
    });
  }
  getStatusText(status: any): string {
    if (typeof status === 'string') {
      switch (status) {
        case 'Pending':
          return 'Beklemede';
        case 'Confirmed':
          return 'Onaylandı';
        case 'Processing':
          return 'Hazırlanıyor';
        case 'Shipped':
          return 'Kargolandı';
        case 'Delivered':
          return 'Teslim Edildi';
        case 'Cancelled':
          return 'İptal Edildi';
        case 'Refunded':
          return 'İade Edildi';
        default:
          return 'Bilinmiyor';
      }
    } else {
      switch (status) {
        case 0:
          return 'Beklemede';
        case 1:
          return 'Onaylandı';
        case 2:
          return 'Hazırlanıyor';
        case 3:
          return 'Kargolandı';
        case 4:
          return 'Teslim Edildi';
        case 5:
          return 'İptal Edildi';
        case 6:
          return 'İade Edildi';
        default:
          return 'Bilinmiyor';
      }
    }
  }

  getStatusColor(status: number): string {
    if (typeof status === 'string') {
      switch (status) {
        case 'Pending':
          return 'status-warning';
        case 'Confirmed':
        case 'Processing':
        case 'Shipped':
          return 'status-info';
        case 'Delivered':
          return 'status-success';
        case 'Cancelled':
          return 'status-danger';
        case 'Refunded':
          return 'status-secondary';
        default:
          return 'status-secondary';
      }
    } else {
      switch (status) {
        case 0:
          return 'status-warning';
        case 1:
        case 2:
        case 3:
          return 'status-info';
        case 4:
          return 'status-success';
        case 5:
          return 'status-danger';
        case 6:
          return 'status-secondary';
        default:
          return 'status-secondary';
      }
    }
  }

  isActionDisabled(status: any): boolean {
    if (status === null || status === undefined) return false;

    const s = status.toString().toLowerCase();

    return (
      s === 'cancelled' ||
      s === 'refunded' ||
      s === 'delivered' ||
      s === '5' ||
      s === '6' ||
      s === '4'
    );
  }

  openUpdateModal(orderId: string, currentStatus: any) {
    this.selectedOrderId = orderId;

    this.selectedNewStatus = 1;
    this.showUpdateModal = true;
    setTimeout(() => {
      this.cdr.detectChanges();
    }, 0);
  }
  closeUpdateModal() {
    this.showUpdateModal = false;
    this.selectedOrderId = '';
    this.isUpdating = false;
    this.cdr.detectChanges();
  }

  updateOrderStatus() {
    if (!this.selectedOrderId) return;

    this.isUpdating = true;

    this.orderService.updateOrderStatus(this.selectedOrderId, this.selectedNewStatus).subscribe({
      next: (res) => {
        this.isUpdating = false;
        this.closeUpdateModal();
        this.loadSellerOffers();
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Hata' + err);
        this.isUpdating = false;
        this.loadSellerOffers();
      },
    });
  }
}
