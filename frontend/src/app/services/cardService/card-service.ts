import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { cardModel } from '../../models/card';
import { Observable } from 'rxjs';
// import { HttpConfigInterceptor } from '../http.interceptor';
import { HttpService } from '../httpService/http-service';

@Injectable({
  providedIn: 'root',
})
export class CardService {
  http : HttpService = inject(HttpService);
  url : string = this.http.url + "card";
}