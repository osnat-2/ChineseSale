import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { winnerModel } from '../../models/winner';
import { Observable } from 'rxjs';
import { HttpService } from '../httpService/http-service';

@Injectable({
  providedIn: 'root',
})
export class WinnerService {
    http : HttpService = inject(HttpService);
    url : string = this.http.url + "winner";
}