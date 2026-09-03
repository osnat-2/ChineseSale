import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root'
})
export class HttpService {
  httpClient: HttpClient = inject(HttpClient);
  url: string = "https://localhost:7142/api/";
  router = inject(Router);

}