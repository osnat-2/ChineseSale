import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';

import { WinnerService } from './winner-service';

describe('WinnerService', () => {
  let service: WinnerService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
    service = TestBed.inject(WinnerService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('marks a successful winner draw locally', () => {
    const winner = { id: 1, presentId: 12, cardId: 8, lotteryId: 4 };
    service.markDrawn(12, winner);
    expect(service.isDrawn(12)).toBeTrue();
    expect(service.getLatestWinner()).toEqual(winner);
  });
});
