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

  private readonly url = `${this.http.url}category`;

  getAllCategories(includeInactive = false): Observable<Result<categoryModel>> {
    return includeInactive
      ? this.httpClient.get<Result<categoryModel>>(this.url, { params: { includeInactive: 'true' } })
      : this.httpClient.get<Result<categoryModel>>(this.url);
  }

  createCategory(category: Pick<categoryModel, 'name'> & { isActive?: boolean }): Observable<Result<categoryModel>> {
    return this.httpClient.post<Result<categoryModel>>(this.url, category);
  }

  updateCategory(
    id: number,
    category: Pick<categoryModel, 'name'> & { isActive?: boolean }
  ): Observable<Result<categoryModel>> {
    return this.httpClient.put<Result<categoryModel>>(`${this.url}/${id}`, category);
  }

  deleteCategory(id: number): Observable<Result<categoryModel>> {
    return this.httpClient.delete<Result<categoryModel>>(`${this.url}/${id}`);
  }
}
