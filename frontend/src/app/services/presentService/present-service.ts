import { inject, Injectable } from '@angular/core';
import { presentModel } from '../../models/present';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { HttpService } from '../httpService/http-service';

@Injectable({
  providedIn: 'root',
})
export class PresentService {
    http : HttpService = inject(HttpService);
    url : string = this.http.url + "present";
}