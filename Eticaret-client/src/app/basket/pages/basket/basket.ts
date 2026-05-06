import { ChangeDetectorRef, Component, CUSTOM_ELEMENTS_SCHEMA, OnInit } from '@angular/core';
import { BasketService } from '../../services/basket';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { OrderService } from '../../../order/services/order';
import { UserService } from '../../../admin/services/user';
import { Auth } from '../../../auth/services/auth';
import { TranslateModule } from '@ngx-translate/core';
import { FormsModule } from '@angular/forms';

import { PaymentService } from '../../../core/services/payment.service';
import { PaymentRequest } from '../../../core/models/payment.model';
@Component({
  selector: 'app-basket',
  templateUrl: './basket.html',
  standalone: true,
  imports: [CommonModule, RouterModule, TranslateModule, FormsModule],
  styleUrl: './basket.scss',
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
})
export class Basket implements OnInit {
  basket: any = null;
  isLoading: boolean = true;

  showCheckOutModal: boolean = false;
  userBalance: number = 0;
  isSubmitting: boolean = false;

  paymentMethod: 'wallet' | 'card' = 'wallet';

  selectedCountryCode: string = '+90';
  phoneNumber: string = '';

  paymentData: PaymentRequest = {
    cardHolderName: '',
    cardNumber: '',
    expireMonth: '',
    expireYear: '',
    cvc: '',
    price: 0,
    paymentType: 1,
    buyerName: '',
    buyerSurname: '',
    buyerEmail: '',
    buyerGsmNumber: '',
    buyerIdentityNumber: '',
    city: '',
    country: '',
    addressDescription: '',
    zipCode: '',
  };
  constructor(
    private basketService: BasketService,
    private cdr: ChangeDetectorRef,
    private orderService: OrderService,
    private router: Router,
    private authService: Auth,
    private paymentService: PaymentService,
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

        this.paymentData.buyerEmail = res.email || '';
        this.paymentData.buyerName = res.name || '';
        this.paymentData.buyerSurname = res.surname || '';

        if (res.phoneNumber) {
          if (res.phoneNumber.startsWith('+90')) {
            this.selectedCountryCode = '+90';
            this.phoneNumber = res.phoneNumber.replace('+90', '').trim();
          } else if (res.phoneNumber.startsWith('+1')) {
            this.selectedCountryCode = '+1';
            this.phoneNumber = res.phoneNumber.replace('+1', '').trim();
          } else {
            this.phoneNumber = res.phoneNumber;
          }
        }
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
    if (this.userBalance < this.basket.totalBasketPrices) {
      this.paymentMethod = 'card';
    } else {
      this.paymentMethod = 'wallet';
    }
    this.showCheckOutModal = true;
  }

  closeCheckOutModal() {
    this.showCheckOutModal = false;
  }

  setPaymentMethod(method: 'wallet' | 'card') {
    if (method === 'wallet' && this.userBalance < this.basket.totalBasketPrices) {
      return;
    }
    this.paymentMethod = method;
  }

  formatCardNumber(value: string) {
    if (!value) {
      this.paymentData.cardNumber = '';
      return;
    }
    let val = value.replace(/\D/g, '');
    if (val.length > 16) val = val.substring(0, 16);
    let formatted = val.match(/.{1,4}/g)?.join(' ') || '';
    this.paymentData.cardNumber = formatted;
  }

  confirmCheckOut() {
    this.isSubmitting = true;

    if (this.paymentMethod === 'wallet') {
      const dto = {};
      this.orderService.createOrder(dto).subscribe({
        next: (res) => this.handleSuccessfulOrder(),
        error: (err) => this.handleErrorOrder(err),
      });
    } else if (this.paymentMethod === 'card') {
      this.paymentData.price = this.basket.totalBasketPrices;
      this.paymentData.buyerGsmNumber = this.selectedCountryCode + this.phoneNumber;

      const safeRequest: PaymentRequest = {
        ...this.paymentData,
        cardNumber: this.paymentData.cardNumber.replace(/\s+/g, ''),
      };

      this.paymentService.directCheckout(safeRequest).subscribe({
        next: (res) => {
          this.handleSuccessfulOrder();
        },
        error: (err) => this.handleErrorOrder(err),
      });
    }
  }
  private handleSuccessfulOrder() {
    this.isSubmitting = false;
    this.showCheckOutModal = false;
    alert('Siparişiniz başarıyla alındı');
    this.basket = null;
    this.basketService.updateCartCount();
    this.cdr.detectChanges();
    this.router.navigate(['/my-orders']);
  }

  private handleErrorOrder(err: any) {
    this.isSubmitting = false;
    const errorMsg = err.error?.message || 'Ödeme Sırasında Hata Oluştu';
    alert('İşlem başarısız: ' + errorMsg);
    this.cdr.detectChanges();
  }
}
