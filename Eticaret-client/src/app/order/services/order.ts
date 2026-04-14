import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class OrderService {
  private apiUrl = 'https://localhost:7185/api/Order';

  constructor(private http: HttpClient) {}

  createOrder(dto: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/createOrder`, dto);
  }
  getMyOrders(): Observable<any> {
    return this.http.get(`${this.apiUrl}/my-orders`);
  }
  getOrderDetails(orderId: string): Observable<any> {
    return this.http.get(`${this.apiUrl}/${orderId}`);
  }
  cancelOrder(orderId: string): Observable<any> {
    return this.http.put(`${this.apiUrl}/${orderId}/cancel`, {});
  }
  getSellerOrders(sellerId: string): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/GetSellersOrders/${sellerId}`);
  }
  updateOrderStatus(orderId: string, newStatus: number) {
    const body = { newStatus: newStatus };
    return this.http.put(`${this.apiUrl}/UpdateStatus/${orderId}`, body);
  }
  checkPurchaseStatus(productId: string): Observable<boolean> {
    return this.http.get<boolean>(`${this.apiUrl}/check-purchase/${productId}`);
  }
}
