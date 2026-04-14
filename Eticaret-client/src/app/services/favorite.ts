import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';

import { Observable } from 'rxjs';

export interface FavoriteToggleResponse {
  isFavorited: boolean;
  message: string;
}
@Injectable({
  providedIn: 'root',
})
export class Favorite {
  private apiUrl = 'https://localhost:7185/api/Favorite';
  constructor(private http: HttpClient) {}

  toggleFavorite(productId: string): Observable<FavoriteToggleResponse> {
    return this.http.post<FavoriteToggleResponse>(`${this.apiUrl}/toggle/${productId}`, {});
  }
  checkFavoriteStatus(productId: string): Observable<boolean> {
    return this.http.get<boolean>(`${this.apiUrl}/check/${productId}`);
  }
}
