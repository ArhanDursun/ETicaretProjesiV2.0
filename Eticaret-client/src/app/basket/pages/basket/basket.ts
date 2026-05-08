import { ChangeDetectorRef, Component, CUSTOM_ELEMENTS_SCHEMA, OnInit } from '@angular/core';
import { BasketService } from '../../services/basket';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { OrderService } from '../../../order/services/order';
import { Auth } from '../../../auth/services/auth';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { FormsModule, ReactiveFormsModule, FormGroup, FormControl, Validators } from '@angular/forms';
import { CreditCardComponent } from '../../../shared/components/credit-card/credit-card.component';
import { PaymentService } from '../../../core/services/payment.service';
import { PaymentRequest } from '../../../core/models/payment.model';

@Component({
  selector: 'app-basket',
  templateUrl: './basket.html',
  standalone: true,
  imports: [CommonModule, RouterModule, TranslateModule, FormsModule, ReactiveFormsModule, CreditCardComponent],
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
  cardLogo: string = 'fa-solid fa-credit-card text-secondary';
  isAgreed: boolean = false;
  months: string[] = ['01', '02', '03', '04', '05', '06', '07', '08', '09', '10', '11', '12'];
  years: string[] = [];
  paymentForm!: FormGroup;
  isMockupActive: boolean = false;
  isCvcFocused: boolean = false;
  isContractRead: boolean = false;
  isInfoRead: boolean = false;
  activeAgreementModal: 'contract' | 'info' | null = null;

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
    private translate: TranslateService
  ) {}

  ngOnInit(): void {
    this.loadBasket();
    this.loadUserBalance();
    const currentYear = new Date().getFullYear();
    for (let i = 0; i < 15; i++) {
      this.years.push((currentYear + i).toString());
    }
    this.initPaymentForm();
  }

  initPaymentForm() {
    this.paymentForm = new FormGroup({
      buyerName: new FormControl(this.paymentData.buyerName, Validators.required),
      buyerSurname: new FormControl(this.paymentData.buyerSurname, Validators.required),
      buyerEmail: new FormControl(this.paymentData.buyerEmail, [Validators.required, Validators.email]),
      phoneNumber: new FormControl('', Validators.required),
      buyerIdentityNumber: new FormControl('', Validators.required),
      city: new FormControl('', Validators.required),
      country: new FormControl('', Validators.required),
      addressDescription: new FormControl('', Validators.required),
      zipCode: new FormControl('', Validators.required),
      cardHolderName: new FormControl('', Validators.required),
      cardNumber: new FormControl('', [Validators.required, Validators.maxLength(19)]),
      expireMonth: new FormControl('', Validators.required),
      expireYear: new FormControl('', Validators.required),
      cvc: new FormControl('', [Validators.required, Validators.maxLength(3)]),
    });

    this.paymentForm.valueChanges.subscribe(val => {
      this.paymentData = { ...this.paymentData, ...val };
    });
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
            return { ...item, images: processedImages };
          });
        }
        this.basket = res;
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.isLoading = false;
        this.cdr.detectChanges();
      },
    });
  }

  loadUserBalance() {
    this.authService.getUserProfile().subscribe({
      next: (res: any) => {
        this.userBalance = res.balance || 0;
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
          this.paymentForm.patchValue({ 
            buyerEmail: res.email || '',
            buyerName: res.name || '',
            buyerSurname: res.surname || '',
            phoneNumber: this.phoneNumber 
          });
        }
        this.cdr.detectChanges();
      },
      error: (err) => {},
    });
  }

  removeItem(productId: string) {
    if (confirm(this.translate.instant('BASKET.MESSAGES.REMOVE_CONFIRM'))) {
      this.basketService.removeItemFromBasket(productId).subscribe({
        next: () => {
          this.loadBasket();
          this.basketService.updateCartCount();
        },
        error: (err) => {
          alert(this.translate.instant('BASKET.MESSAGES.REMOVE_ERROR'));
        },
      });
    }
  }

  clearBasket() {
    if (confirm(this.translate.instant('BASKET.MESSAGES.CLEAR_CONFIRM'))) {
      this.basketService.clearBasket().subscribe({
        next: () => {
          this.loadBasket();
          this.basketService.updateCartCount();
        },
        error: (err) => alert(this.translate.instant('BASKET.MESSAGES.CLEAR_ERROR')),
      });
    }
  }

  openCheckOutModal() {
    if (!this.basket || this.basket.items.length === 0) {
      alert(this.translate.instant('BASKET.MESSAGES.EMPTY_CART'));
      return;
    }
    this.paymentMethod = this.userBalance < this.basket.totalBasketPrices ? 'card' : 'wallet';
    this.showCheckOutModal = true;
    this.cdr.detectChanges();
  }

  closeCheckOutModal() {
    this.showCheckOutModal = false;
    this.isAgreed = false;
    this.isContractRead = false;
    this.isInfoRead = false;
    this.cdr.detectChanges();
  }

  openAgreement(type: 'contract' | 'info') {
    this.activeAgreementModal = type;
  }

  closeAgreement() {
    this.activeAgreementModal = null;
  }

  onScrollAgreement(event: any, type: 'contract' | 'info') {
    const element = event.target;
    if (element.scrollHeight - element.scrollTop <= element.clientHeight + 10) {
      if (type === 'contract') this.isContractRead = true;
      if (type === 'info') this.isInfoRead = true;
      if (this.isContractRead && this.isInfoRead) {
        this.isAgreed = true;
      }
    }
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
      this.cardLogo = 'fa-solid fa-credit-card text-secondary';
      return;
    }
    let val = value.replace(/\D/g, '');
    if (val.startsWith('4')) this.cardLogo = 'fa-brands fa-cc-visa text-primary';
    else if (val.startsWith('5')) this.cardLogo = 'fa-brands fa-cc-mastercard text-warning';
    else if (val.startsWith('34') || val.startsWith('37'))
      this.cardLogo = 'fa-brands fa-cc-amex text-info';
    else if (val.startsWith('9')) this.cardLogo = 'fa-solid fa-credit-card text-success';
    else this.cardLogo = 'fa-solid fa-credit-card text-secondary';
    if (val.length > 16) val = val.substring(0, 16);
    let formatted = val.match(/.{1,4}/g)?.join(' ') || '';
    this.paymentForm.patchValue({ cardNumber: formatted }, { emitEvent: true });
    this.paymentData.cardNumber = formatted;
  }

  confirmCheckOut() {
    this.isSubmitting = true;
    if (this.paymentMethod === 'wallet') {
      this.orderService.createOrder({}).subscribe({
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
        next: (res) => this.handleSuccessfulOrder(),
        error: (err) => this.handleErrorOrder(err),
      });
    }
  }

  private handleSuccessfulOrder() {
    this.isSubmitting = false;
    this.showCheckOutModal = false;
    alert(this.translate.instant('BASKET.MESSAGES.ORDER_SUCCESS'));
    this.basket = null;
    this.basketService.updateCartCount();
    this.cdr.detectChanges();
    this.router.navigate(['/my-orders']);
  }

  private handleErrorOrder(err: any) {
    this.isSubmitting = false;
    const errorMsg = err.error?.message || this.translate.instant('BASKET.MESSAGES.ORDER_ERROR');
    alert(this.translate.instant('BASKET.MESSAGES.ORDER_ERROR_PREFIX') + ': ' + errorMsg);
    this.cdr.detectChanges();
  }
}
