import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';

import { LotteryService } from './lottery-service';

describe('LotteryService', () => {
  let service: LotteryService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
    service = TestBed.inject(LotteryService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('posts a draw request for a present', () => {
    service.drawWinner(12).subscribe();
    const request = http.expectOne('https://localhost:7142/api/lottery/draw/12');
    expect(request.request.method).toBe('POST');
    request.flush({ success: true, data: [] });
  });
});
