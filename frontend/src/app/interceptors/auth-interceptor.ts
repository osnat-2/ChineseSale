import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '../services/authService/auth-service';

export const authInterceptor: HttpInterceptorFn = (request, next) => {
  const authService = inject(AuthService);
  const router = inject(Router);
  const token = authService.getToken();
  const isCardPaymentRequest = /\/api\/card(?:\/|$)/i.test(request.url);
  const authorizedRequest = token && isCardPaymentRequest
    ? request.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
    : request;

  return next(authorizedRequest).pipe(
    catchError((error) => {
      if (error.status === 401 && isCardPaymentRequest) {
        authService.clearToken();
        void router.navigate(['/login']);
      }
      return throwError(() => error);
    })
  );
};