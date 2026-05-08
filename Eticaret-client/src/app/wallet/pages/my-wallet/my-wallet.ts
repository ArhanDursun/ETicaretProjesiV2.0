import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { Walletservice } from '../../services/walletservice';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { PaymentService } from '../../../core/services/payment.service';
import { PaymentRequest } from '../../../core/models/payment.model';
import { Auth } from '../../../auth/services/auth';
import { CreditCardComponent } from '../../../shared/components/credit-card/credit-card.component';
import { CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-my-wallet',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslateModule, CreditCardComponent],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  templateUrl: './my-wallet.html',
  styleUrls: ['./my-wallet.scss'],
})
export class MyWallet implements OnInit {
  currentBalance: number = 0;
  transactions: any[] = [];
  isLoading: boolean = true;
  cardLogo: string = 'fa-solid fa-credit-card text-secondary';
  isAgreed: boolean = false;
  months: string[] = ['01', '02', '03', '04', '05', '06', '07', '08', '09', '10', '11', '12'];
  years: string[] = [];
  selectedCountryCode: string = '+90';
  phoneNumber: string = '';
  paymentData: PaymentRequest = {
    cardHolderName: '',
    cardNumber: '',
    expireMonth: '',
    expireYear: '',
    cvc: '',
    price: 0,
    paymentType: 2,
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
  isMockupActive: boolean = false;
  isCvcFocused: boolean = false;
  isPaymentLoading: boolean = false;
  paymentMessage: string = '';
  isSuccess: boolean = false;

  constructor(
    private walletService: Walletservice,
    private paymentService: PaymentService,
    private cdr: ChangeDetectorRef,
    private authService: Auth,
    private translate: TranslateService
  ) {}

  ngOnInit(): void {
    this.loadWalletData();
    this.loadUserProfile();
    const currentYear = new Date().getFullYear();
    for (let i = 0; i < 15; i++) {
      this.years.push((currentYear + i).toString());
    }
  }

  formatCardNumber(value: string) {
    if (!value) {
      this.paymentData.cardNumber = '';
      this.cardLogo = 'fa-credit-card text-secondary';
      return;
    }
    let val = value.replace(/\D/g, '');
    if (val.startsWith('4')) this.cardLogo = 'fa-brands fa-cc-visa text-primary';
    else if (val.startsWith('5')) this.cardLogo = 'fa-brands fa-cc-mastercard text-warning';
    else if (val.startsWith('9')) this.cardLogo = 'fa-solid fa-credit-card text-success';
    else this.cardLogo = 'fa-solid fa-credit-card text-secondary';
    if (val.length > 16) {
      val = val.substring(0, 16);
    }
    let formatted = val.match(/.{1,4}/g)?.join(' ') || '';
    this.paymentData.cardNumber = formatted;
  }

  loadWalletData() {
    this.isLoading = true;
    this.walletService.getBalance().subscribe({
      next: (res) => {
        this.currentBalance = res.currentBalance;
        this.cdr.detectChanges();
      },
      error: (err) => {},
    });
    this.walletService.getTransactions().subscribe({
      next: (res) => {
        this.transactions = res;
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.isLoading = false;
        this.cdr.detectChanges();
      },
    });
  }

  loadUserProfile() {
    this.authService.getMyProfile().subscribe({
      next: (res) => {
        this.paymentData.buyerName = res.firstName || res.name || '';
        this.paymentData.buyerSurname = res.lastName || res.surname || '';
        this.paymentData.buyerEmail = res.email;
        if (res.phoneNumber) {
          if (res.phoneNumber.startsWith('+90')) {
            this.selectedCountryCode = '+90';
            this.phoneNumber = res.phoneNumber.replace('+90', '').trim();
          } else if (res.phoneNumber.startsWith('+1')) {
            this.selectedCountryCode = '+1';
            this.phoneNumber = res.phoneNumber.replace('+90', '').trim();
          } else if (res.phoneNumber.startsWith('+44')) {
            this.selectedCountryCode = '+44';
            this.phoneNumber = res.phoneNumber.replace('+44', '').trim();
          } else if (res.phoneNumber.startsWith('+49')) {
            this.selectedCountryCode = '+49';
            this.phoneNumber = res.phoneNumber.replace('+49', '').trim();
          } else {
            this.phoneNumber = res.phoneNumber;
          }
        }
        this.cdr.detectChanges;
      },
      error: (err) => {},
    });
  }

  submitPayment() {
    this.isPaymentLoading = true;
    this.paymentMessage = '';
    this.paymentData.buyerGsmNumber = this.selectedCountryCode + this.phoneNumber;
    const safeRequest: PaymentRequest = {
      ...this.paymentData,
      cardNumber: this.paymentData.cardNumber.replace(/\s+/g, ''),
    };
    this.paymentService.topUpWallet(safeRequest).subscribe({
      next: (res) => {
        this.isSuccess = true;
        this.paymentMessage = `${this.translate.instant('WALLET.TOPUP.MESSAGES.SUCCESS_PREFIX')} ${res.transactionId}`;
        this.isPaymentLoading = false;
        this.loadWalletData();
      },
      error: (err) => {
        this.isSuccess = false;
        const errMsg = err.error?.message || this.translate.instant('WALLET.TOPUP.MESSAGES.SYSTEM_ERROR');
        this.paymentMessage = `${this.translate.instant('WALLET.TOPUP.MESSAGES.ERROR_PREFIX')} ${errMsg}`;
        this.isPaymentLoading = false;
        this.cdr.detectChanges();
      },
    });
  }

  formatPhone(value: string) {
    if (!value) return;
    let digits = value.replace(/\D/g, '');
    if (digits.length > 10) digits = digits.substring(0, 10);
    let formatted = '';
    if (digits.length > 0) formatted += digits.substring(0, 3);
    if (digits.length > 3) formatted += ' ' + digits.substring(3, 6);
    if (digits.length > 6) formatted += ' ' + digits.substring(6, 8);
    if (digits.length > 8) formatted += ' ' + digits.substring(8, 10);
    this.phoneNumber = formatted;
  }
}
