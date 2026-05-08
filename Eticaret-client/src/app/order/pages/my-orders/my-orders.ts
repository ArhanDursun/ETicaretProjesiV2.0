import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { OrderService } from '../../services/order';
import { CommonModule } from '@angular/common';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-my-orders',
  standalone: true,
  imports: [CommonModule, TranslateModule],
  templateUrl: './my-orders.html',
  styleUrl: './my-orders.scss',
})
export class MyOrders implements OnInit {
  orders: any[] = [];
  isLoading: boolean = true;
  selectedOrderDetails: any = null;
  showDetailsModal: boolean = false;
  isDetailsLoading: boolean = false;

  constructor(
    private orderService: OrderService,
    private cdr: ChangeDetectorRef,
    public translate: TranslateService,
  ) {}

  ngOnInit(): void {
    this.loadMyOrders();
  }

  getStatusText(status: any): string {
    let key = 'STATUS.UNKNOWN';
    if (typeof status === 'string') {
      switch (status) {
        case 'Pending': key = 'STATUS.PENDING'; break;
        case 'Confirmed': key = 'STATUS.CONFIRMED'; break;
        case 'Processing': key = 'STATUS.PROCESSING'; break;
        case 'Shipped': key = 'STATUS.SHIPPED'; break;
        case 'Delivered': key = 'STATUS.DELIVERED'; break;
        case 'Cancelled': key = 'STATUS.CANCELLED'; break;
        case 'Refunded': key = 'STATUS.REFUNDED'; break;
      }
    } else {
      switch (status) {
        case 0: key = 'STATUS.PENDING'; break;
        case 1: key = 'STATUS.CONFIRMED'; break;
        case 2: key = 'STATUS.PROCESSING'; break;
        case 3: key = 'STATUS.SHIPPED'; break;
        case 4: key = 'STATUS.DELIVERED'; break;
        case 5: key = 'STATUS.CANCELLED'; break;
        case 6: key = 'STATUS.REFUNDED'; break;
      }
    }
    return key;
  }

  getStatusColor(status: number): string {
    if (typeof status === 'string') {
      switch (status) {
        case 'Pending': return 'status-warning';
        case 'Confirmed':
        case 'Processing':
        case 'Shipped': return 'status-info';
        case 'Delivered': return 'status-success';
        case 'Cancelled': return 'status-danger';
        case 'Refunded': return 'status-secondary';
        default: return 'status-secondary';
      }
    } else {
      switch (status) {
        case 0: return 'status-warning';
        case 1:
        case 2:
        case 3: return 'status-info';
        case 4: return 'status-success';
        case 5: return 'status-danger';
        case 6: return 'status-secondary';
        default: return 'status-secondary';
      }
    }
  }

  loadMyOrders() {
    this.isLoading = true;
    this.orderService.getMyOrders().subscribe({
      next: (res: any) => {
        this.orders = res || [];
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.isLoading = false;
        this.cdr.detectChanges();
      },
    });
  }

  cancelOrder(orderId: string) {
    const isConfirmed = confirm(this.translate.instant('ORDERS.MESSAGES.CANCEL_CONFIRM'));
    if (isConfirmed) {
      this.orderService.cancelOrder(orderId).subscribe({
        next: (res) => {
          alert(this.translate.instant('ORDERS.MESSAGES.CANCEL_SUCCESS'));
          this.loadMyOrders();
        },
        error: (err) => alert(this.translate.instant('ORDERS.MESSAGES.CANCEL_ERROR')),
      });
    }
  }

  viewOrderDetails(orderId: string) {
    this.showDetailsModal = true;
    this.isDetailsLoading = true;
    this.cdr.detectChanges();
    this.orderService.getOrderDetails(orderId).subscribe({
      next: (res) => {
        this.selectedOrderDetails = res;
        this.isDetailsLoading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.isLoading = false;
        this.showDetailsModal = false;
        alert(this.translate.instant('ORDERS.MESSAGES.DETAILS_ERROR'));
        this.cdr.detectChanges();
      },
    });
  }

  closeDetailsModal() {
    this.showDetailsModal = false;
    this.selectedOrderDetails = null;
    this.cdr.detectChanges();
  }

  isCancelDisabled(status: any): boolean {
    if (status === null || status === undefined) return true;
    const s = status.toString().toLowerCase();
    return (
      s === 'shipped' || s === '3' ||
      s === 'delivered' || s === '4' ||
      s === 'cancelled' || s === '5' ||
      s === 'refunded' || s === '6'
    );
  }

  getImageUrl(images: any[]): string {
    if (!images || images.length === 0) return '';
    const firstImage = images[0];
    if (firstImage.startsWith('http')) {
      return firstImage;
    }
    const slash = firstImage.startsWith('/') ? '' : '/';
    return 'https://localhost:7185' + slash + firstImage;
  }
}
