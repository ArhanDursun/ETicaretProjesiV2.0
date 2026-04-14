import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { OfferService } from '../../services/offer';

import { BasketService } from '../../../basket/services/basket';
import { TranslateModule } from '@ngx-translate/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

export interface OfferModel {
  id: string;
  productId: string;
  productName: string;
  productImages?: string[];
  price: number;
  offeredPrice: number;
  quantity: number;
  status: number;
  createdAt?: string | Date;
  counterPrice: number;
  buyerName?: string;
  createdTime: string | Date;
}
@Component({
  selector: 'app-offer',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, TranslateModule],
  templateUrl: './offer.html',
  styleUrl: './offer.scss',
})
export class Offer implements OnInit {
  activeTab: 'sent' | 'received' = 'sent';

  sentOffers: OfferModel[] = [];
  receivedOffers: OfferModel[] = [];

  isLoading: boolean = false;
  showCounterModal: boolean = false;
  counterPriceInput: number | null = null;
  selectedOfferIdForCounter: string | null = null;
  isSubmittingCounter: boolean = false;

  constructor(
    private offerService: OfferService,
    private cdr: ChangeDetectorRef,
    private basketService: BasketService,
  ) {}

  ngOnInit(): void {
    this.loadOffers();
  }

  openCounterModal(offerId: string) {
    this.selectedOfferIdForCounter = offerId;
    this.counterPriceInput = null;
    this.showCounterModal = true;
  }

  closeCounterModal() {
    this.showCounterModal = false;
    this.counterPriceInput = null;
    this.selectedOfferIdForCounter = null;
  }

  submitCounterOffer() {
    if (!this.counterPriceInput || this.counterPriceInput <= 0) {
      alert('Geçerli bir fiyat giriniz');
      return;
    }
    if (this.selectedOfferIdForCounter) {
      this.isSubmittingCounter = true;
      this.offerService
        .makeCounterOffer(this.selectedOfferIdForCounter, this.counterPriceInput)
        .subscribe({
          next: () => {
            alert('Karşı Teklifiniz Alıcıya İletildi');
            this.closeCounterModal();
            this.isSubmittingCounter = false;
            this.loadOffers();
            this.cdr.detectChanges();
          },
          error: (err) => {
            alert('Hata Oluşut' + err);
            this.closeCounterModal();
            this.cdr.detectChanges();
          },
        });
    }
    this.cdr.detectChanges();
  }

  loadOffers() {
    this.isLoading = true;
    let pendingRequests = 2;
    const checkLoading = () => {
      pendingRequests--;
      if (pendingRequests === 0) {
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    };

    this.offerService.getMyOffers().subscribe({
      next: (res: any) => {
        this.sentOffers = res || [];
        checkLoading();
      },
      error: (err) => {
        console.error('Yaptığım teklifler çekilirken hata:', err);
        this.sentOffers = [];
        checkLoading();
      },
    });
    this.offerService.getReceivedOffers().subscribe({
      next: (res: any) => {
        this.receivedOffers = res || [];
        checkLoading();
      },
      error: (err) => {
        console.error('Gelen Teklifler çekilirken hata:', err);
        this.receivedOffers = [];
        checkLoading();
      },
    });
  }

  handleOfferResponse(offerId: string, isAccepted: boolean) {
    const action = isAccepted ? 'kabul etmek' : 'reddetmek';
    if (confirm(`Bu teklifi ${action} istediğinize emin misiniz?`)) {
      this.offerService.respondToOffer(offerId, isAccepted).subscribe({
        next: () => {
          alert(`Teklif başarıyla ${isAccepted ? 'onaylandı' : 'reddedildi'}.`);
          this.loadOffers();
          this.offerService.updatePendingOfferCount();

          if (isAccepted) {
            this.basketService.updateCartCount();
          }
        },
        error: (err) => alert('İşlem sırasında bir hata oluştu.'),
      });
    }
  }
  getStatusClass(status: any) {
    switch (status) {
      case 1:
        return 'status-success';
      case 2:
        return 'status-danger';
      case 3:
        return 'status-info';
      default:
        return 'status-warning';
    }
  }
}
