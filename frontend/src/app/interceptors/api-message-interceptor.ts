import { HttpInterceptorFn, HttpResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, tap, throwError } from 'rxjs';
import { readApiResponseMessage, readHttpErrorMessage } from '../models/api-message';
import { ApiMessageService } from '../services/apiMessageService/api-message-service';
import { HttpService } from '../services/httpService/http-service';
import { isConfiguredApiRequest } from './api-request';

export const apiMessageInterceptor: HttpInterceptorFn = (request, next) => {
  const apiBaseUrl = inject(HttpService).url;
  if (!isConfiguredApiRequest(request.url, apiBaseUrl)) {
    return next(request);
  }

  const messageService = inject(ApiMessageService);
  const isLoginRequest =
    /\/auth\/login\/?$/i.test(new URL(request.url, apiBaseUrl).pathname);

  return next(request).pipe(
    tap((event) => {
      if (!(event instanceof HttpResponse)) {
        return;
      }

      const response = readApiResponseMessage(event.body);
      const isReadSuccess = request.method === 'GET' && response?.success;
      if (response && !isReadSuccess && !(isLoginRequest && response.success)) {
        messageService.show(response.message, response.success);
      }
    }),
    catchError((error: unknown) => {
      const message = readHttpErrorMessage(error);
      if (message) {
        messageService.show(message, false);
      }
      return throwError(() => error);
    })
  );
};
