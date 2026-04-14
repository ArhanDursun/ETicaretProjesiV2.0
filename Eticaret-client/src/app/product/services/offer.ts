import { HttpClient, HttpHeaders } from '@angular/common/http';
import { ChangeDetectorRef, Injectable } from '@angular/core';
import { BehaviorSubject, Observable, of } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class OfferService {
  private apiUrl = 'https://localhost:7185/api/Offer';
  private pendingOfferCountScore = new BehaviorSubject<number>(0);
  public pendingOfferCount$ = this.pendingOfferCountScore.asObservable();

  constructor(private http: HttpClient) {}

  makeOffer(productId: string, offeredPrice: number, offerQuantity: number): Observable<any> {
    const payload = {
      productId: productId,
      offeredPrice: offeredPrice,
      offerQuantity: offerQuantity,
    };
    return this.http.post(`${this.apiUrl}/make-offer`, payload);
  }
  getMyOffers(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/my-offers`);
  }
  getReceivedOffers(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/received-offers`);
  }
  respondToOffer(offerId: string, isAccepted: boolean): Observable<any> {
    return this.http.put(`${this.apiUrl}/${offerId}/respond?isAccepted=${isAccepted}`, {});
  }
  updatePendingOfferCount() {
    this.getReceivedOffers().subscribe({
      next: (offer) => {
        const pendingCount = offer.filter((o) => o.status === 0).length;

        this.pendingOfferCountScore.next(pendingCount);
      },
      error: () => this.pendingOfferCountScore.next(0),
    });
  }
  makeCounterOffer(offerId: string, counterPrice: number): Observable<any> {
    return this.http.post(`${this.apiUrl}/counter-offer`, { offerId, counterPrice });
  }
}
