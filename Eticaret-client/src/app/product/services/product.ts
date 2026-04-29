import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, ObservedValueOf } from 'rxjs';
export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}
export interface ProductListResponseDto {
  id: string;
  name: string;
  price: number;
  stockQuanity: number;
  categoryName: string;
  images: string[];
  averageStar: number;
  commentCount: number;
  discountedPrice?: number;
  discountPercentage?: number;
  discountEndDate?: Date;
}
@Injectable({
  providedIn: 'root',
})
export class Product {
  private apiUrl = 'https://localhost:7185/api/Product';
  private categoryUrl = 'https://localhost:7185/api/Category';
  private interactionsUrl = 'https://localhost:7185/api/ProductInteractions';
  constructor(private http: HttpClient) {}

  addProduct(formData: FormData): Observable<any> {
    return this.http.post(`${this.apiUrl}/add-product`, formData);
  }
  getCategories(): Observable<any[]> {
    return this.http.get<any[]>(`${this.categoryUrl}/get-all`);
  }
  getFilteredProducts(
    filters: any,
    pageNumber: number = 1,
    pageSize: number = 10,
  ): Observable<any[]> {
    let params = new HttpParams();

    if (filters.searchTerm) {
      params = params.set('searchTerm', filters.searchTerm);
    }
    if (filters.categoryId) {
      params = params.set('categoryId', filters.categoryId);
    }
    if (filters.minPrice) {
      params = params.set('minPrice', filters.minPrice.toString());
    }
    if (filters.maxPrice) {
      params = params.set('maxPrice', filters.maxPrice.toString());
    }

    params = params.set('PageNumber', pageNumber.toString()).set('PageSize', pageSize.toString());

    return this.http.get<any[]>(`${this.apiUrl}/filter`, { params });
  }
  getProductById(id: string): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/${id}`);
  }

  getMyProduct(): Observable<any> {
    return this.http.get(`${this.apiUrl}/my-products`);
  }
  getSellerProducts(sellerId: string): Observable<any> {
    return this.http.get(`${this.apiUrl}/seller-products/${sellerId}`);
  }
  getComments(productId: string) {
    return this.http.get(`${this.interactionsUrl}/comment/${productId}`);
  }
  addComment(data: { productId: string; content: string; starCount: number }) {
    return this.http.post(`${this.interactionsUrl}/comment`, data);
  }
  getQuestions(productId: string) {
    return this.http.get(`${this.interactionsUrl}/question/${productId}`);
  }
  addQuestion(data: { productId: string; questionContent: string }) {
    return this.http.post(`${this.interactionsUrl}/question`, data);
  }
  answerQuestion(data: { questionId: string; answerContent: string }) {
    return this.http.put(`${this.interactionsUrl}/question/answer`, data);
  }
  updateProduct(id: string, productData: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}`, productData);
  }
  getShowCaseProducts(pageNumber: number, pageSize: number): Observable<any> {
    let params = new HttpParams()
      .set('PageNumber', pageNumber.toString())
      .set('PageSize', pageSize.toString());
    return this.http.get<PagedResult<ProductListResponseDto>>(`${this.apiUrl}/showcase`, {
      params,
    });
  }
  getTrendingProducts(): Observable<ProductListResponseDto[]> {
    return this.http.get<ProductListResponseDto[]>(`${this.apiUrl}/trending`);
  }
}
