import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class BasketService {
  private apiUrl = 'https://localhost:7185/api/Basket';

  private cartItemCount = new BehaviorSubject<number>(0);
  public cartItemCount$ = this.cartItemCount.asObservable();

  constructor(private http: HttpClient) {}

  getBasket(): Observable<any> {
    return this.http.get(this.apiUrl);
  }

  addItemToBasket(dto: {
    productId: string;
    quantity: number;
    unitPrice: number;
  }): Observable<any> {
    return this.http.post(`${this.apiUrl}/add`, dto);
  }

  removeItemFromBasket(productId: string): Observable<any> {
    return this.http.delete(`${this.apiUrl}/remove/${productId}`);
  }
  clearBasket(): Observable<any> {
    return this.http.delete(`${this.apiUrl}/clear`);
  }
  updateCartCount(): void {
    this.getBasket().subscribe({
      next: (basket) => {
        if (!basket || !basket.items) {
          this.cartItemCount.next(0);
          return;
        }
        const totalCount = basket.items.reduce((sum: number, item: any) => sum + item.quantity, 0);

        this.cartItemCount.next(totalCount);
      },
      error: (err) => {
        console.error('Sepet Çekilemedi', err);
        this.cartItemCount.next(0);
      },
    });
  }
}
