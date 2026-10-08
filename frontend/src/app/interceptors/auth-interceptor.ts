import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '../services/authService/auth-service';
import { HttpService } from '../services/httpService/http-service';

export const authInterceptor: HttpInterceptorFn = (request, next) => {
  const authService = inject(AuthService);
  const apiBaseUrl = inject(HttpService).url;
  const router = inject(Router);
  const token = authService.getToken();
  const apiBase = new URL(apiBaseUrl);
  const requestUrl = new URL(request.url, apiBase);
  const apiPath = apiBase.pathname.endsWith('/') ? apiBase.pathname : `${apiBase.pathname}/`;
  const requestPath = requestUrl.pathname;
  const isApiRequest =
    requestUrl.origin === apiBase.origin &&
    (requestPath === apiBase.pathname || requestPath.startsWith(apiPath));
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