import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { winnerModel } from '../../models/winner';
import { Observable } from 'rxjs';
import { HttpService } from '../httpService/http-service';
import { Result } from '../../models/result';

@Injectable({
  providedIn: 'root',
})
export class WinnerService {
    private readonly httpClient = inject(HttpClient);
    private readonly http = inject(HttpService);
    private readonly url = `${this.http.url}winner`;
    private readonly drawnPresentIds = new Set<number>();
    private latestWinner: winnerModel | null = null;

    drawWinner(presentId: number, lotteryId?: number): Observable<Result<winnerModel>> {
      const suffix = lotteryId === undefined ? '' : `?lotteryId=${lotteryId}`;
      return this.httpClient.post<Result<winnerModel>>(`${this.url}/draw/${presentId}${suffix}`, {});
    }

    markDrawn(presentId: number, winner: winnerModel): void {
      this.drawnPresentIds.add(presentId);
      this.latestWinner = winner;
    }

    isDrawn(presentId: number): boolean {
      return this.drawnPresentIds.has(presentId);
    }

    getLatestWinner(): winnerModel | null {
      return this.latestWinner;
    }
}