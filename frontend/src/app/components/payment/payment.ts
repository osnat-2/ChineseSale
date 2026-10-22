import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  AbstractControl,
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  ValidationErrors,
  Validators,
} from '@angular/forms';
import { finalize } from 'rxjs';
import { CardService } from '../../services/cardService/card-service';
import { cardModel } from '../../models/card';
import { RouterLink } from '@angular/router';
import { TranslatePipe } from '../../i18n/translate.pipe';
import { LocalizedCurrencyPipe } from '../../i18n/localized-currency.pipe';
import { environment } from '../../../environments/environment';
import { hasApiResponseMessage } from '../../models/api-message';

interface CheckoutGroup {
  presentId: number;
  presentName: string;
  categoryName: string;
  quantity: number;
  subtotal: number | null;
}

@Component({
  selector: 'app-payment',
  imports: [CommonModule, ReactiveFormsModule, RouterLink, TranslatePipe, LocalizedCurrencyPipe],
  templateUrl: './payment.html',
  styleUrl: './payment.scss',
})
export class Payment implements OnInit {
  cards: cardModel[] = [];
  readonly developmentPaymentsEnabled = environment.developmentPaymentsEnabled;
  loading = true;
  paying = false;
  errorMessage = '';
  successMessage = '';
  readonly paymentForm = new FormGroup({
    cardholderName: new FormControl('', {
      nonNullable: true,
      validators: [
        Validators.required,
        Validators.pattern(/\S/),
        Validators.minLength(2),
        Validators.maxLength(100),
      ],
    }),
    cardNumber: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, validateCardNumber],
    }),
    expiration: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, validateExpiration],
    }),
    securityCode: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.pattern(/^\d{3,4}$/)],
    }),
  });
  private readonly cardService = inject(CardService);

  ngOnInit(): void {
    this.loadUnpaidCards();
  }

  get amountDue(): number {
    return this.cards.reduce((total, card) => total + (card.present?.price ?? 0), 0);
  }

  get canPay(): boolean {
    return this.cards.length > 0 &&
      this.cards.every((card) =>
        card.present !== undefined &&
        Number.isSafeInteger(card.present.price) &&
        card.present.price >= 0
      ) &&
      Number.isSafeInteger(this.amountDue);
  }

  get checkoutGroups(): CheckoutGroup[] {
    const groups = new Map<number, CheckoutGroup>();

    for (const card of this.cards) {
      const price = card.present?.price;
      const validPrice = typeof price === 'number' && Number.isSafeInteger(price) && price >= 0;
      const group = groups.get(card.presentId);

      if (group) {
        group.quantity++;
        if (group.subtotal !== null && validPrice) {
          const subtotal = group.subtotal + price;
          group.subtotal = Number.isSafeInteger(subtotal) ? subtotal : null;
        } else {
          group.subtotal = null;
        }
      } else {
        const presentName = typeof card.present?.name === 'string' ? card.present.name.trim() : '';
        const categoryName = typeof card.present?.categoryName === 'string'
          ? card.present.categoryName.trim()
          : '';
        groups.set(card.presentId, {
          presentId: card.presentId,
          presentName,
          categoryName,
          quantity: 1,
          subtotal: validPrice ? price : null,
        });
      }
    }

    return Array.from(groups.values());
  }

  pay(): void {
    if (!this.developmentPaymentsEnabled || !this.canPay || this.paying) return;
    this.paymentForm.markAllAsTouched();
    if (this.paymentForm.invalid) return;

    const form = this.paymentForm.getRawValue();
    const [year, month] = form.expiration.split('-').map(Number);
    this.paying = true;
    this.errorMessage = '';
    this.successMessage = '';
    this.cardService.processDevelopmentPayment({
      amount: this.amountDue,
      cardholderName: form.cardholderName.trim(),
      cardNumber: form.cardNumber.replace(/[\s-]/g, ''),
      expirationMonth: month,
      expirationYear: year,
      securityCode: form.securityCode,
    }).pipe(
      finalize(() => {
        this.paying = false;
        this.paymentForm.reset();
      })
    ).subscribe({
      next: (result) => {
        if (!result.success) {
          this.errorMessage = 'payment.failed';
          return;
        }
        this.successMessage = hasApiResponseMessage(result) ? '' : 'payment.success';
        this.cards = (result.data ?? []).filter((card) => !card.isPaid);
      },
      error: (error: unknown) => {
        this.errorMessage = error instanceof HttpErrorResponse && error.status === 400
          ? 'payment.validationFailed'
          : 'payment.failed';
      }
    });
  }

  loadUnpaidCards(): void {
    this.loading = true;
    this.errorMessage = '';
    this.cardService.getMyCards(false).subscribe({
      next: (result) => {
        if (result.success) {
          this.cards = result.data ?? [];
        } else {
          this.errorMessage = this.cards.length ? 'payment.refreshFailed' : 'payment.loadFailed';
        }
        this.loading = false;
      },
      error: () => {
        this.errorMessage = this.cards.length ? 'payment.refreshFailed' : 'payment.loadFailed';
        this.loading = false;
      }
    });
  }
}

function validateCardNumber(control: AbstractControl<string>): ValidationErrors | null {
  const number = control.value.replace(/[\s-]/g, '');
  if (!number) return null;
  if (!/^\d{16,19}$/.test(number)) return { cardNumber: true };

  let sum = 0;
  let doubleDigit = false;
  for (let index = number.length - 1; index >= 0; index--) {
    let digit = Number(number[index]);
    if (doubleDigit) {
      digit *= 2;
      if (digit > 9) digit -= 9;
    }
    sum += digit;
    doubleDigit = !doubleDigit;
  }

  return sum % 10 === 0 ? null : { cardNumber: true };
}

function validateExpiration(control: AbstractControl<string>): ValidationErrors | null {
  const match = /^(\d{4})-(\d{2})$/.exec(control.value);
  if (!match) return control.value ? { expiration: true } : null;

  const year = Number(match[1]);
  const month = Number(match[2]);
  const now = new Date();
  if (
    month < 1 ||
    month > 12 ||
    year < now.getFullYear() ||
    (year === now.getFullYear() && month < now.getMonth() + 1)
  ) {
    return { expiration: true };
  }

  return null;
}