import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root'
})
export class HttpService {
  httpClient: HttpClient = inject(HttpClient);
  port: number = 7142;
  // port: number = 5023;
  environment: string = 'development';
  a: string = 'http';
  url: string = `${this.a}://localhost:${this.port}/api/`;
  router = inject(Router);

  // constructor() {
  //   const configuredUrl = localStorage.getItem('apiBaseUrl');
  //   // if (!configuredUrl) {
  //   //   localStorage.setItem('apiBaseUrl', this.url);
  //   // }
  //   if (configuredUrl) {
  //     this.url = configuredUrl.endsWith('/') ? configuredUrl : `${configuredUrl}/`;
  //   }
  // }
}