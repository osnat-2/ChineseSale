import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { loginDtoModel } from '../../models/ModelsDto/loginDto';
import { userDtoModel } from '../../models/ModelsDto/userDto';
import { Result } from '../../models/result';
import { HttpService } from '../httpService/http-service';
import { AuthService } from '../authService/auth-service';

@Injectable({
  providedIn: 'root',
})
export class UserService {
  http: HttpService = inject(HttpService);
  auth: AuthService = inject(AuthService);
  url: string = this.http.url + 'auth';

  login(credentials: loginDtoModel): Observable<Result<string>> {
    return this.http.httpClient.post<Result<string>>(`${this.url}/login`, credentials);
  }

  register(user: userDtoModel): Observable<Result<unknown>> {
    return this.http.httpClient.post<Result<unknown>>(`${this.url}/register`, user);
  }

  logout(): void {
    this.auth.logout();
  }
}