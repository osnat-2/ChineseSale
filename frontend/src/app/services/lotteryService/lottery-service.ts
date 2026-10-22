import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { inject } from '@angular/core';
import { Observable } from 'rxjs';
import { Result } from '../../models/result';
import { lotteryModel } from '../../models/lottery';
import { winnerModel } from '../../models/winner';
import { HttpService } from '../httpService/http-service';

export interface LotteryInput {
  time: string;
}

@Injectable({
  providedIn: 'root',
})
export class LotteryService {
  private readonly httpClient = inject(HttpClient);
  private readonly http = inject(HttpService);
  private readonly url = `${this.http.url}lottery`;

  getLotteries(): Observable<Result<lotteryModel>> {
    return this.httpClient.get<Result<lotteryModel>>(this.url);
  }

  addLottery(lottery: LotteryInput): Observable<Result<lotteryModel>> {
    return this.httpClient.post<Result<lotteryModel>>(this.url, lottery);
  }

  updateLottery(id: number, lottery: LotteryInput): Observable<Result<lotteryModel>> {
    return this.httpClient.put<Result<lotteryModel>>(`${this.url}/${id}`, lottery);
  }

  processLottery(lotteryId: number): Observable<Result<winnerModel>> {
    return this.httpClient.post<Result<winnerModel>>(`${this.url}/${lotteryId}/process`, {});
  }

  drawWinner(presentId: number, lotteryId?: number): Observable<Result<winnerModel>> {
    const suffix = lotteryId === undefined ? '' : `?lotteryId=${lotteryId}`;
    return this.httpClient.post<Result<winnerModel>>(`${this.url}/draw/${presentId}${suffix}`, {});
  }
}
