import { HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { userDtoModel } from '../../models/ModelsDto/userDto';
import { HttpService } from '../httpService/http-service';
import { AuthService } from '../authService/auth-service';

@Injectable({
  providedIn: 'root',
})
export class UserService {
  http: HttpService = inject(HttpService);
  auth: AuthService = inject(AuthService);
  url: string = this.http.url + 'auth';

  login(email: string, password: string): Observable<any> {
    const params = new HttpParams()
      .set('email', email)
      .set('password', password);
    return this.http.httpClient.post<any>(`${this.url}/login`, null, { params });
  }

  register(user: userDtoModel): Observable<any> {
    return this.http.httpClient.post<any>(`${this.url}/register`, user);
  }

  logout(): void {
    this.auth.logout();
  }
}