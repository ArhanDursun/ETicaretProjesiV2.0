import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { OrderService } from '../../services/order';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-seller-orders',
  standalone: true,
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
    private translate: TranslateService
  ) {}

  getSellerIdFromToken(): string | null {
    const token = localStorage.getItem('token') || sessionStorage.getItem('token');
    if (!token) return null;

    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      const nameIdentifierClaim = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier';
      return payload[nameIdentifierClaim] || payload.nameid || payload.sub;
    } catch (error) {
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
      alert(this.translate.instant('ORDERS.MESSAGES.SESSION_EXPIRED'));
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
      error: () => {
        this.isLoading = false;
        this.cdr.detectChanges();
      },
    });
  }

  getStatusText(status: any): string {
    const s = status?.toString().toLowerCase();
    switch (s) {
      case 'pending':
      case '0':
        return this.translate.instant('ORDERS.STATUS.PENDING');
      case 'confirmed':
      case '1':
        return this.translate.instant('ORDERS.STATUS.CONFIRMED');
      case 'processing':
      case '2':
        return this.translate.instant('ORDERS.STATUS.PROCESSING');
      case 'shipped':
      case '3':
        return this.translate.instant('ORDERS.STATUS.SHIPPED');
      case 'delivered':
      case '4':
        return this.translate.instant('ORDERS.STATUS.DELIVERED');
      case 'cancelled':
      case '5':
        return this.translate.instant('ORDERS.STATUS.CANCELLED');
      case 'refunded':
      case '6':
        return this.translate.instant('ORDERS.STATUS.REFUNDED');
      default:
        return this.translate.instant('ORDERS.STATUS.UNKNOWN');
    }
  }

  getStatusColor(status: any): string {
    const s = status?.toString().toLowerCase();
    switch (s) {
      case 'pending':
      case '0':
        return 'status-warning';
      case 'confirmed':
      case '1':
      case 'processing':
      case '2':
      case 'shipped':
      case '3':
        return 'status-info';
      case 'delivered':
      case '4':
        return 'status-success';
      case 'cancelled':
      case '5':
        return 'status-danger';
      case 'refunded':
      case '6':
        return 'status-secondary';
      default:
        return 'status-secondary';
    }
  }

  isActionDisabled(status: any): boolean {
    if (status === null || status === undefined) return false;
    const s = status.toString().toLowerCase();
    return s === 'cancelled' || s === 'refunded' || s === 'delivered' || s === '5' || s === '6' || s === '4';
  }

  openUpdateModal(orderId: string, currentStatus: any) {
    this.selectedOrderId = orderId;
    this.selectedNewStatus = 1;
    this.showUpdateModal = true;
    this.cdr.detectChanges();
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
      next: () => {
        this.isUpdating = false;
        this.closeUpdateModal();
        this.loadSellerOffers();
        this.cdr.detectChanges();
      },
      error: () => {
        this.isUpdating = false;
        this.loadSellerOffers();
      },
    });
  }
}
