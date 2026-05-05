import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { Walletservice } from '../../services/walletservice';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';

import { PaymentService } from '../../../core/services/payment.service';
import { PaymentRequest } from '../../../core/models/payment.model';
@Component({
  selector: 'app-my-wallet',
  imports: [CommonModule, FormsModule, TranslateModule],
  templateUrl: './my-wallet.html',
  styleUrls: ['./my-wallet.scss'],
})
export class MyWallet implements OnInit {
  currentBalance: number = 0;
  transactions: any[] = [];
  isLoading: boolean = true;

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
  isPaymentLoading: boolean = false;
  paymentMessage: string = '';
  isSuccess: boolean = false;
  constructor(
    private walletService: Walletservice,
    private paymentService: PaymentService,
    private cdr: ChangeDetectorRef,
  ) {}

  ngOnInit(): void {
    this.loadWalletData();
  }

  formatCardNumber(value: string) {
    if (!value) {
      this.paymentData.cardNumber = '';
      return;
    }
    let val = value.replace(/\D/g, '');

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
      error: (err) => console.error('Hata' + err),
    });
    this.walletService.getTransactions().subscribe({
      next: (res) => {
        this.transactions = res;
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Hata Dekontlar Getirilemedi' + err);
        this.isLoading = false;
        this.cdr.detectChanges();
      },
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
        this.paymentMessage = `Ödeme Başarılı! Fiş No: ${res.transactionId}`;
        this.isPaymentLoading = false;
        this.loadWalletData();
      },
      error: (err) => {
        this.isSuccess = false;
        this.paymentMessage = `Hata: ${err.error?.message || 'Sistemsel bir sorun oluştu'}`;
        this.isPaymentLoading = false;
        this.cdr.detectChanges();
      },
    });
  }
}
