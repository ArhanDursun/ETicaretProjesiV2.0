import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-credit-card',
  standalone: true,
  imports: [CommonModule, TranslateModule],
  templateUrl: './credit-card.component.html',
  styleUrls: ['./credit-card.component.scss']
})
export class CreditCardComponent implements OnChanges {
  @Input() cardNumber: string = '';
  @Input() cardHolderName: string = '';
  @Input() expireMonth: string = '';
  @Input() expireYear: string = '';
  @Input() cvc: string = '';
  @Input() isActive: boolean = false;
  @Input('isFlipped') isFlipped: boolean = false;
  @Input() bankName: string = 'Bank Name';

  cardType: string = 'unknown';
  cardLogoClass: string = 'fa-solid fa-credit-card';

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['cardNumber']) {
      this.updateCardType();
    }
  }

  displayBankName: string = 'Bank Name';

  updateCardType(): void {
    const number = this.cardNumber.replace(/\s+/g, '');
    
    if (number.startsWith('4')) {
      this.cardType = 'visa';
      this.cardLogoClass = 'fa-brands fa-cc-visa';
    } else if (/^(5[1-5]|2[2-7])/.test(number)) {
      this.cardType = 'mastercard';
      this.cardLogoClass = 'fa-brands fa-cc-mastercard';
    } else if (/^3[47]/.test(number)) {
      this.cardType = 'amex';
      this.cardLogoClass = 'fa-brands fa-cc-amex';
    } else if (number.startsWith('9792') || number.startsWith('6500') || number.startsWith('6501')) {
      this.cardType = 'troy';
      this.cardLogoClass = 'fa-solid fa-credit-card';
    } else {
      this.cardType = 'unknown';
      this.cardLogoClass = 'fa-solid fa-credit-card';
    }

    const bankMap: { [key: string]: string } = {
      '589004': 'AKBANK',
      '552608': 'AKBANK',
      '979207': 'AKBANK',
      '476662': 'DENİZBANK',
      '460345': 'DENİZBANK',
      '979202': 'QNB',
      '498749': 'QNB',
      '531157': 'QNB',
      '979203': 'QNB',
      '517041': 'GARANTİ BBVA',
      '540036': 'GARANTİ BBVA',
      '374427': 'GARANTİ BBVA',
      '447505': 'HALKBANK',
      '552879': 'HALKBANK',
      '405903': 'HSBC',
      '550472': 'HSBC',
      '589283': 'İŞ BANKASI',
      '454359': 'İŞ BANKASI',
      '491005': 'VAKIFBANK',
      '415792': 'VAKIFBANK',
      '650052': 'VAKIFBANK',
      '650170': 'VAKIFBANK',
      '516888': 'YAPI KREDİ',
      '545103': 'YAPI KREDİ',
      '454360': 'AKBANK',
      '557113': 'AKBANK',
      '540061': 'GARANTİ BBVA',
      '404308': 'ZİRAAT BANKASI'
    };
    
    const foundBankKey = Object.keys(bankMap).find(key => number.startsWith(key));
    
    if (foundBankKey) {
      this.displayBankName = bankMap[foundBankKey];
    } else {
      this.displayBankName = this.bankName !== 'Bank Name' ? this.bankName : 'Bank Name';
    }
  }

  get formattedCardNumber(): string {
    if (!this.cardNumber) return '#### #### #### ####';
    let num = this.cardNumber.replace(/\s+/g, '');
    let res = '';
    for (let i = 0; i < 16; i++) {
      if (i > 0 && i % 4 === 0) res += ' ';
      res += num[i] || '#';
    }
    return res;
  }

  get formattedName(): string {
    return this.cardHolderName ? this.cardHolderName.toUpperCase() : 'NAME SURNAME';
  }

  get formattedExpiry(): string {
    const m = this.expireMonth || 'MM';
    const y = this.expireYear ? this.expireYear.slice(-2) : 'YY';
    return `${m}/${y}`;
  }
}
