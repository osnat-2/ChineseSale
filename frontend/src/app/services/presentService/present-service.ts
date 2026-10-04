import { inject, Injectable } from '@angular/core';
import { presentModel } from '../../models/present';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { HttpService } from '../httpService/http-service';
import { Result } from '../../models/result';

export interface PresentQuery {
  search?: string;
  categoryId?: number;
  sortBy?: 'name' | 'price';
  sortDirection?: 'asc' | 'desc';
}

@Injectable({
  providedIn: 'root',
})
export class PresentService {
  private readonly httpClient = inject(HttpClient);
  private readonly http = inject(HttpService);
  private readonly url = `${this.http.url}present`;

  getAllPresents(query: PresentQuery = {}): Observable<Result<presentModel>> {
    const params: Record<string, string> = {
      onlyActive: 'true',
      search: query.search ?? '',
      categoryId: query.categoryId?.toString() ?? '',
      sortBy: query.sortBy ?? 'name',
      sortDirection: query.sortDirection ?? 'asc'
    };

    return this.httpClient.get<Result<presentModel>>(`${this.url}/getAllPresents`, { params });
  }

  getPresentById(id: number): Observable<Result<presentModel>> {
    return this.httpClient.get<Result<presentModel>>(`${this.url}/${id}`);
  }
}