import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class Auth {
  private apiUrl = 'https://localhost:7185/api/Auth';
  private loggedIn = new BehaviorSubject<boolean>(this.hasToken());

  public isLoggedIn$ = this.loggedIn.asObservable();

  constructor(private http: HttpClient) {}

  login(dto: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/login`, dto);
  }
  registerRequest(dto: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/register-request`, dto);
  }
  verifyRegister(dto: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/register-verification`, dto);
  }
  getDecodedToken(token: string): any {
    try {
      const payload = token.split('.')[1];
      const decodedJson = atob(payload);
      return JSON.parse(decodedJson);
    } catch (error) {
      console.error('Token Çözülemedi', error);
      return null;
    }
  }
  resendVerificationCode(email: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/resend-verification-code`, { email });
  }
  forgotPassword(email: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/forgot-password`, { email });
  }
  resetPassword(resetDto: {
    email: string;
    token: string;
    newPassword: string;
    confirmNewPassword: string;
  }) {
    return this.http.post(`${this.apiUrl}/reset-password`, resetDto);
  }
  private hasToken() {
    if (typeof window !== 'undefined') {
      const token = localStorage.getItem('token') || sessionStorage.getItem('token');
      return !!token;
    }
    return false;
  }
  setLoginStatus(status: boolean) {
    this.loggedIn.next(status);
  }
  isAuthenticated(): boolean {
    return this.hasToken();
  }
  logout(): void {
    if (typeof window !== 'undefined') {
      localStorage.removeItem('token');
      sessionStorage.removeItem('token');
    }
    this.loggedIn.next(false);
  }
  getUserProfile() {
    return this.http.get(`${this.apiUrl}/getProfile`);
  }

  getMyProfile(): Observable<any> {
    return this.http.get(`${this.apiUrl}/profile`);
  }
  updateMyProfile(profileData: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/profile`, profileData);
  }
  changeMyPassword(passwordData: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/change-password`, passwordData);
  }
  uploadProfileImage(file: File): Observable<any> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post(`${this.apiUrl}/upload-profile-image`, formData);
  }
  getPublicProfile(sellerId: string): Observable<any> {
    return this.http.get(`${this.apiUrl}/public-profile/${sellerId}`);
  }

  searchUsers(keyword: any): Observable<any> {
    return this.http.get(`${this.apiUrl}/search?q=${keyword}`);
  }
}
