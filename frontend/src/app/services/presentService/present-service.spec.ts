import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';

import { PresentService } from '../presentService/present-service';

describe('PresentService', () => {
  let service: PresentService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
    service = TestBed.inject(PresentService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('requests active presents with the supported filters', () => {
    service.getAllPresents({ search: 'tea', categoryId: 4, sortBy: 'price', sortDirection: 'desc' }).subscribe();

    const request = http.expectOne((item) => item.url.endsWith('/present/getAllPresents'));
    expect(request.request.params.get('onlyActive')).toBe('true');
    expect(request.request.params.get('search')).toBe('tea');
    expect(request.request.params.get('categoryId')).toBe('4');
    expect(request.request.params.get('sortBy')).toBe('price');
    expect(request.request.params.get('sortDirection')).toBe('desc');
    request.flush({ success: true, data: [] });
  });

  it('requests a present by id', () => {
    service.getPresentById(9).subscribe();
    const request = http.expectOne('https://localhost:7142/api/present/9');
    expect(request.request.method).toBe('GET');
    request.flush({ success: true, data: [] });
  });
});
