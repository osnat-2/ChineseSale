import { inject, Injectable } from '@angular/core';
import { cardModel } from '../../models/card';
import { cardDtoModel } from '../../models/ModelsDto/cardDto';
import { Result } from '../../models/result';
import { Observable } from 'rxjs';
import { HttpService } from '../httpService/http-service';

@Injectable({
  providedIn: 'root',
})
export class CardService {
  http : HttpService = inject(HttpService);
  url: string = this.http.url + 'card';

  addCard(card: cardDtoModel): Observable<Result<cardModel>> {
    return this.http.httpClient.post<Result<cardModel>>(this.url, card);
  }

  getMyCards(paid?: boolean): Observable<Result<cardModel>> {
    const query = paid === undefined ? '' : `?paid=${paid}`;
    return this.http.httpClient.get<Result<cardModel>>(`${this.url}/my${query}`);
  }

  removeCard(id: number): Observable<Result<cardModel>> {
    return this.http.httpClient.delete<Result<cardModel>>(`${this.url}/${id}`);
  }

  processPayment(): Observable<Result<cardModel>> {
    return this.http.httpClient.post<Result<cardModel>>(`${this.url}/payment`, {});
  }
}