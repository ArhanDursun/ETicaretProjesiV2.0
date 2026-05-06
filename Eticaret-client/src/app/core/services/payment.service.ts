import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { PaymentRequest, PaymentResponse } from '../models/payment.model';

@Injectable({
  providedIn: 'root',
})
export class PaymentService {
  private apiUrl = 'https://localhost:7185/api/Payments';

  constructor(private http: HttpClient) {}

  directCheckout(request: PaymentRequest): Observable<PaymentResponse> {
    return this.http.post<PaymentResponse>(`${this.apiUrl}/direct-checkout`, request);
  }

  topUpWallet(request: PaymentRequest): Observable<PaymentResponse> {
    return this.http.post<PaymentResponse>(`${this.apiUrl}/top-up`, request);
  }
}
