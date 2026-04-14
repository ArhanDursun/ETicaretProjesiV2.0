import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class Walletservice {
  private apiUrl = 'https://localhost:7185/api/Wallet';

  constructor(private http: HttpClient) {}

  getBalance(): Observable<{ currentBalance: number }> {
    return this.http.get<{ currentBalance: number }>(`${this.apiUrl}/balance`);
  }
  getTransactions(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/transactions`);
  }
}
