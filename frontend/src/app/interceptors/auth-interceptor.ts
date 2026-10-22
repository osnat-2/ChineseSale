import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '../services/authService/auth-service';
import { HttpService } from '../services/httpService/http-service';
import { isConfiguredApiRequest } from './api-request';

export const authInterceptor: HttpInterceptorFn = (request, next) => {
  const authService = inject(AuthService);
  const apiBaseUrl = inject(HttpService).url;
  const router = inject(Router);
  const token = authService.getToken();
  const isApiRequest = isConfiguredApiRequest(request.url, apiBaseUrl);
  const requestPath = new URL(request.url, apiBaseUrl).pathname;
  const isAnonymousAuthRequest = /\/auth\/(?:login|register)\/?$/i.test(requestPath);
  const shouldAttachToken = Boolean(token && isApiRequest && !isAnonymousAuthRequest);
  const authorizedRequest = shouldAttachToken
    ? request.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
    : request;

  return next(authorizedRequest).pipe(
    catchError((error) => {
      if (error.status === 401 && shouldAttachToken) {
        authService.clearToken();
        void router.navigate(['/login']);
      }
      return throwError(() => error);
    })
  );
};