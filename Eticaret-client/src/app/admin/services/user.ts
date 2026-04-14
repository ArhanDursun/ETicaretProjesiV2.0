import { HttpClient,HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class UserService {
  private apiUrl = 'https://localhost:7185/api/Auth'

  constructor(private http:HttpClient) {}

  getAllUsers():Observable<any>{
    

    return this.http.get(`${this.apiUrl}/get-all-users`);
  }
  deleteUser(id:string):Observable<any>{
    
    return this.http.delete(`${this.apiUrl}/delete-user/${id}`);
  }
}
