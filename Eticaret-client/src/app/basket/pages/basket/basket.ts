import { ChangeDetectorRef, Component, CUSTOM_ELEMENTS_SCHEMA, OnInit } from '@angular/core';
import { BasketService } from '../../services/basket';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { OrderService } from '../../../order/services/order';
import { UserService } from '../../../admin/services/user';
import { Auth } from '../../../auth/services/auth';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-basket',
  templateUrl: './basket.html',
  standalone: true,
  imports: [CommonModule, RouterModule, TranslateModule],
  styleUrl: './basket.scss',
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
})
export class Basket implements OnInit {
  basket: any = null;
  isLoading: boolean = true;

  showCheckOutModal: boolean = false;
  userBalance: number = 0;
  isSubmitting: boolean = false;
  constructor(
    private basketService: BasketService,
    private cdr: ChangeDetectorRef,
    private orderService: OrderService,
    private router: Router,
    private authService: Auth,
  ) {}

  ngOnInit(): void {
    this.loadBasket();

    this.loadUserBalance();
  }

  loadBasket() {
    this.isLoading = true;

    this.basketService.getBasket().subscribe({
      next: (res: any) => {
        if (res && res.items) {
          res.items = res.items.map((item: any) => {
            let processedImages: string[] = [];

            if (item.images && item.images.length > 0) {
              processedImages = item.images.map((img: string) =>
                img.includes('http') ? img : `https://localhost:7185${img}`,
              );
            } else if (item.imageUrl) {
              processedImages = [
                item.imageUrl.includes('http')
                  ? item.imageUrl
                  : `https://localhost:7185${item.imageUrl}`,
              ];
            }

            return {
              ...item,
              images: processedImages,
            };
          });
        }

        this.basket = res;
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('HATA GELDİ:', err);
        this.isLoading = false;
        this.cdr.detectChanges();
      },
    });
  }
  loadUserBalance() {
    this.authService.getUserProfile().subscribe({
      next: (res: any) => {
        this.userBalance = res.balance || 0;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Hata', err);
      },
    });
  }
  removeItem(productId: string) {
    if (confirm('Bu ürünü sepetten çıkarmak istediğinize emin misiniz ?')) {
      this.basketService.removeItemFromBasket(productId).subscribe({
        next: () => {
          this.loadBasket();
          this.basketService.updateCartCount();
        },
        error: (err) => {
          alert('Ürün silinirken hata oluştu');
          console.error(err);
        },
      });
    }
  }
  clearBasket() {
    if (confirm('Sepetteki Tüm Ürünleri Silmek İstediğinize Emin Misiniz?')) {
      this.basketService.clearBasket().subscribe({
        next: () => {
          this.loadBasket();
          this.basketService.updateCartCount();
        },
        error: (err) => alert('Sepet boşaltılırken hata oluştu'),
      });
    }
  }

  openCheckOutModal() {
    if (!this.basket || this.basket.items.length === 0) {
      alert('Sepetiniz boş');
      return;
    }
    this.showCheckOutModal = true;
  }
  closeCheckOutModal() {
    this.showCheckOutModal = false;
  }

  confirmCheckOut() {
    if (this.userBalance < this.basket.totalPrices) {
      alert('Bakiyeniz Yetersiz');
      return;
    }

    this.isSubmitting = true;

    const dto = {};

    this.orderService.createOrder(dto).subscribe({
      next: (res) => {
        this.isSubmitting = false;
        this.showCheckOutModal = false;
        alert('Siparişiniz başarıyla alındı');

        this.basket = null;
        this.basketService.updateCartCount();
        this.cdr.detectChanges();

        this.router.navigate(['/my-orders']);
      },
      error: (err) => {
        this.isSubmitting = false;
        const errorMsg = err.error?.message || 'Ödeme Sırasında Hata Oluştu';

        alert('İşlem başarısız:' + errorMsg);
        this.cdr.detectChanges();
      },
    });
  }
}
