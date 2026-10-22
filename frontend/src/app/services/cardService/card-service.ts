import { inject, Injectable } from '@angular/core';
import { throwError } from 'rxjs';
import { cardModel } from '../../models/card';
import { cardDtoModel } from '../../models/ModelsDto/cardDto';
import { Result } from '../../models/result';
import { Observable } from 'rxjs';
import { HttpService } from '../httpService/http-service';
import { environment } from '../../../environments/environment';

export interface DevelopmentPaymentRequest {
  amount: number;
  cardholderName: string;
  cardNumber: string;
  expirationMonth: number;
  expirationYear: number;
  securityCode: string;
}

@Injectable({
  providedIn: 'root',
})
export class CardService {
  http : HttpService = inject(HttpService);
  url: string = this.http.url + 'card';

  addCard(card: cardDtoModel): Observable<string> {
    return this.http.httpClient.post(
      this.url,
      { presentId: card.presentId, quantity: card.quantity ?? 1 },
      { responseType: 'text' }
    );
  }

  getMyCards(paid?: boolean): Observable<Result<cardModel>> {
    const query = paid === undefined ? '' : `?paid=${paid}`;
    return this.http.httpClient.get<Result<cardModel>>(`${this.url}/my${query}`);
  }

  removeCard(id: number): Observable<string> {
    return this.http.httpClient.delete(`${this.url}/${id}`, { responseType: 'text' });
  }

  processDevelopmentPayment(request: DevelopmentPaymentRequest): Observable<Result<cardModel>> {
    if (!environment.developmentPaymentsEnabled) {
      return throwError(() => new Error('Development payments are disabled.'));
    }
    return this.http.httpClient.post<Result<cardModel>>(
      `${this.url}/development-payment`,
      request
    );
  }
}