import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';

import { CardService } from './card-service';

describe('CardService', () => {
  let service: CardService;
  let httpTestingController: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
    service = TestBed.inject(CardService);
    httpTestingController = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpTestingController.verify());

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('posts the selected present and quantity and reads the message response as text', () => {
    service.addCard({ presentId: 7, quantity: 3 }).subscribe((message) => {
      expect(message).toBe('Card added successfully.');
    });

    const request = httpTestingController.expectOne(service.url);
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({ presentId: 7, quantity: 3 });
    expect(request.request.responseType).toBe('text');
    request.flush('Card added successfully.');
  });

  it('defaults existing single-card calls to quantity one', () => {
    service.addCard({ presentId: 7 }).subscribe();

    const request = httpTestingController.expectOne(service.url);
    expect(request.request.body).toEqual({ presentId: 7, quantity: 1 });
    request.flush('Card added successfully.');
  });

  it('reads the card removal message as text', () => {
    service.removeCard(12).subscribe((message) => {
      expect(message).toBe('Card removed successfully.');
    });

    const request = httpTestingController.expectOne(`${service.url}/12`);
    expect(request.request.method).toBe('DELETE');
    expect(request.request.responseType).toBe('text');
    request.flush('Card removed successfully.');
  });

  it('posts payment details and the expected total to the development-only endpoint', () => {
    const paymentRequest = {
      amount: 27,
      cardholderName: 'Test Customer',
      cardNumber: '4111111111111111',
      expirationMonth: 8,
      expirationYear: 2030,
      securityCode: '123',
    };
    service.processDevelopmentPayment(paymentRequest).subscribe();

    const request = httpTestingController.expectOne(`${service.url}/development-payment`);
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual(paymentRequest);
    request.flush({ success: true, message: null, data: [] });
  });
});
