import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { categoryModel } from '../../models/category';
import { Result } from '../../models/result';
import { HttpService } from '../httpService/http-service';

@Injectable({
  providedIn: 'root',
})
export class CategoryService {
  private readonly httpClient = inject(HttpClient);
  private readonly http = inject(HttpService);

  getAllCategories(): Observable<Result<categoryModel>> {
    return this.httpClient.get<Result<categoryModel>>(`${this.http.url}category`);
  }
}
