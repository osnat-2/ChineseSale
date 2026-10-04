import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { inject } from '@angular/core';
import { Observable } from 'rxjs';
import { Result } from '../../models/result';
import { winnerModel } from '../../models/winner';
import { HttpService } from '../httpService/http-service';

@Injectable({
  providedIn: 'root',
})
export class LotteryService {
  private readonly httpClient = inject(HttpClient);
  private readonly http = inject(HttpService);
  private readonly url = `${this.http.url}lottery`;

  drawWinner(presentId: number, lotteryId?: number): Observable<Result<winnerModel>> {
    const suffix = lotteryId === undefined ? '' : `?lotteryId=${lotteryId}`;
    return this.httpClient.post<Result<winnerModel>>(`${this.url}/draw/${presentId}${suffix}`, {});
  }
}
