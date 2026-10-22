import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpErrorResponse } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { cardModel } from '../../models/card';
import { presentModel } from '../../models/present';
import { CardService } from '../../services/cardService/card-service';

import { Payment } from './payment';

describe('Payment', () => {
  let component: Payment;
  let fixture: ComponentFixture<Payment>;
  let cardService: jasmine.SpyObj<CardService>;

  beforeEach(async () => {
    cardService = jasmine.createSpyObj<CardService>(
      'CardService',
      ['getMyCards', 'processDevelopmentPayment']
    );
    cardService.getMyCards.and.returnValue(of({ success: true, message: null, data: [] }));
    cardService.processDevelopmentPayment.and.returnValue(
      of({ success: true, message: null, data: [] })
    );

    await TestBed.configureTestingModule({
      imports: [Payment],
      providers: [
        provideRouter([]),
        { provide: CardService, useValue: cardService }
      ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(Payment);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('submits validated card details with the displayed total and clears them afterward', () => {
    component.cards = [makeCard(1, 25), makeCard(1, 25), makeCard(2, 10)];
    component.cards[0].present!.categoryName = 'Books';
    fillValidPaymentDetails(component);

    expect(component.amountDue).toBe(60);
    expect(component.checkoutGroups[0].quantity).toBe(2);
    expect(component.checkoutGroups[0].subtotal).toBe(50);
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Books');
    component.pay();

    expect(cardService.processDevelopmentPayment).toHaveBeenCalledWith({
      amount: 60,
      cardholderName: 'Test Customer',
      cardNumber: '4111111111111111',
      expirationMonth: new Date().getMonth() + 1,
      expirationYear: new Date().getFullYear() + 1,
      securityCode: '123',
    });
    expect(component.paymentForm.getRawValue()).toEqual({
      cardholderName: '',
      cardNumber: '',
      expiration: '',
      securityCode: '',
    });
  });

  it('does not submit when card details fail client-side validation', () => {
    component.cards = [makeCard(1, 25)];
    component.paymentForm.setValue({
      cardholderName: 'Test Customer',
      cardNumber: '4111111111111112',
      expiration: `${new Date().getFullYear() + 1}-12`,
      securityCode: '123',
    });

    component.pay();

    expect(component.paymentForm.controls.cardNumber.invalid).toBeTrue();
    expect(cardService.processDevelopmentPayment).not.toHaveBeenCalled();
  });

  it('clears card details and reports a backend validation failure', () => {
    component.cards = [makeCard(1, 25)];
    fillValidPaymentDetails(component);
    cardService.processDevelopmentPayment.and.returnValue(
      throwError(() => new HttpErrorResponse({ status: 400 }))
    );

    component.pay();

    expect(component.errorMessage).toBe('payment.validationFailed');
    expect(component.paymentForm.getRawValue().cardNumber).toBe('');
    expect(component.paymentForm.getRawValue().securityCode).toBe('');
  });

  it('does not show an unverified subtotal when a card has no verified price', () => {
    const card = makeCard(1, 0);
    card.present = undefined;
    component.cards = [card];

    expect(component.canPay).toBeFalse();
    expect(component.checkoutGroups[0].subtotal).toBeNull();
  });

  it('preserves checkout items and offers retry after a refresh error', () => {
    component.cards = [makeCard(1, 25)];
    cardService.getMyCards.and.returnValue(throwError(() => new Error('offline')));
    component.loadUnpaidCards();
    fixture.detectChanges();

    expect(component.cards.length).toBe(1);
    expect(component.errorMessage).toBe('payment.refreshFailed');
    expect(fixture.nativeElement.querySelector('.purchase-error button')?.textContent).toContain('Retry');
  });
});

function fillValidPaymentDetails(component: Payment): void {
  const month = String(new Date().getMonth() + 1).padStart(2, '0');
  const year = new Date().getFullYear() + 1;
  component.paymentForm.setValue({
    cardholderName: ' Test Customer ',
    cardNumber: '4111 1111 1111 1111',
    expiration: `${year}-${month}`,
    securityCode: '123',
  });
}

function makeCard(id: number, price: number): cardModel {
  const present = new presentModel();
  present.price = price;
  present.categoryName = 'Books';
  const card = new cardModel();
  card.id = id;
  card.presentId = id;
  card.present = present;
  card.isPaid = false;
  return card;
}
