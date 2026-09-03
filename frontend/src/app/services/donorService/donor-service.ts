import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { donorModel } from '../../models/donor';
import { Observable } from 'rxjs';
import { HttpService } from '../httpService/http-service';

@Injectable({
  providedIn: 'root',
})
export class DonorService {
    http : HttpService = inject(HttpService);
    url : string = this.http.url + "donor";
}