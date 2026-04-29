import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

export interface AdminProduct {
  id: string;
  name: string;
  sellerName: string;
  price: number;
  stock: number;
  status: string;
  images: string[];
  discountPercentage?: number;
  discountEndDate?: string;
  discountedPrice?: number;
}

export interface UserDetailDto {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  balance: number;
  role: string;
  joinDate: Date;
}
@Injectable({
  providedIn: 'root',
})
export class Admin {
  private apiUrl = 'https://localhost:7185/api/Admin';

  constructor(private http: HttpClient) {}
  getAllProducts(): Observable<AdminProduct[]> {
    return this.http.get<AdminProduct[]>(`${this.apiUrl}/products`);
  }
  deleteProductWithReason(productId: string, reason: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/products/delete-with-reason`, { productId, reason });
  }
  getUserInfo(userId: string): Observable<UserDetailDto> {
    return this.http.get<UserDetailDto>(`${this.apiUrl}/users/${userId}`);
  }

  getUserProducts(userId: string): Observable<AdminProduct[]> {
    return this.http.get<AdminProduct[]>(`${this.apiUrl}/users/${userId}/products`);
  }
  generateSalesReport() {
    return this.http.post(`${this.apiUrl}/generate-sales-report`, {});
  }
}
