import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class Categoryservice {
  private apiUrl = 'https://localhost:7185/api/Category';

  constructor(private http: HttpClient) {}

  getAllCategories(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/get-all`);
  }

  createCategory(category: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/create-category`, category);
  }
  updateCategory(id: string, category: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/update/${id}`, category);
  }
  deleteCategory(id: string): Observable<any> {
    return this.http.delete(`${this.apiUrl}/delete/${id}`);
  }
}
