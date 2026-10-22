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

  it('loads annual lotteries', () => {
    service.getLotteries().subscribe();
    const request = http.expectOne('https://localhost:7142/api/lottery');
    expect(request.request.method).toBe('GET');
    request.flush({ success: true, data: [] });
  });

  it('creates and updates an annual lottery using the DateOnly time field', () => {
    service.addLottery({ time: '2026-11-01' }).subscribe();
    const createRequest = http.expectOne('https://localhost:7142/api/lottery');
    expect(createRequest.request.method).toBe('POST');
    expect(createRequest.request.body).toEqual({ time: '2026-11-01' });
    createRequest.flush({ success: true, data: [{ id: 5, time: '2026-11-01', isMadeOut: false }] });

    service.updateLottery(5, { time: '2026-11-02' }).subscribe();
    const updateRequest = http.expectOne('https://localhost:7142/api/lottery/5');
    expect(updateRequest.request.method).toBe('PUT');
    expect(updateRequest.request.body).toEqual({ time: '2026-11-02' });
    updateRequest.flush({ success: true, data: [{ id: 5, time: '2026-11-02', isMadeOut: false }] });
  });

  it('processes all presents for an annual lottery', () => {
    service.processLottery(5).subscribe();
    const request = http.expectOne('https://localhost:7142/api/lottery/5/process');
    expect(request.request.method).toBe('POST');
    request.flush({ success: true, data: [] });
  });

  it('posts a compatibility draw request for a present and selected lottery', () => {
    service.drawWinner(12, 5).subscribe();
    const request = http.expectOne('https://localhost:7142/api/lottery/draw/12?lotteryId=5');
    expect(request.request.method).toBe('POST');
    request.flush({ success: true, data: [] });
  });
});
