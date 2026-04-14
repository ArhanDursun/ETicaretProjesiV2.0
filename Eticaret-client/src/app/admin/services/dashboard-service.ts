import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

export interface DashboardStats {
  totalPlatformVolume: number;
  totalCommissionEarned: number;
  activeSellers: number;
  totalProducts: number;
}

export interface RecentOrder {
  id: string;
  seller: string;
  customer: string;
  amount: number;
  commission: number;
  date: Date;
  status: string;
  sellerId: string;
}
@Injectable({
  providedIn: 'root',
})
export class DashboardService {
  private apiUrl = 'https://localhost:7185/api/Admin/dashboard';

  constructor(private http: HttpClient) {}

  getStats(): Observable<DashboardStats> {
    return this.http.get<DashboardStats>(`${this.apiUrl}/stats`);
  }
  getRecentOrders(): Observable<RecentOrder[]> {
    return this.http.get<RecentOrder[]>(`${this.apiUrl}/recent-orders`);
  }
  getDailyComission(timeRange: string): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/daily-commission?timeRange=${timeRange}`);
  }
}
