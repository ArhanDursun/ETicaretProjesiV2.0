import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { tick } from '@angular/core/testing';
import { Observable } from 'rxjs';
export interface NotificationDto {
  id: string;
  message: string;
  title: string;
  createdDate: Date;
  type: 'OfferAccepted' | 'OfferRejected' | 'CounterOffer' | 'StockAlert' | 'System';
  relatedId: string;
  isRead: boolean;
}
@Injectable({
  providedIn: 'root',
})
export class NotificationService {
  private apiUrl = 'https://localhost:7185/api/Notifications';

  constructor(private http: HttpClient) {}

  getUnreadNotifications(): Observable<NotificationDto[]> {
    return this.http.get<NotificationDto[]>(`${this.apiUrl}/unread`);
  }

  markAsRead(id: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/mark-read/${id}`, {});
  }

  getAllNotifications() {
    return this.http.get<NotificationDto[]>(`${this.apiUrl}/all`);
  }
}
