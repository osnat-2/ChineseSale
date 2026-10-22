import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, Params, Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { TranslatePipe } from '../../i18n/translate.pipe';
import { presentModel } from '../../models/present';
import { PresentService } from '../../services/presentService/present-service';
import { LocalizedCurrencyPipe } from '../../i18n/localized-currency.pipe';
import { LocalizedNumberPipe } from '../../i18n/localized-number.pipe';
import { AuthService } from '../../services/authService/auth-service';
import { CardService } from '../../services/cardService/card-service';
import { hasApiResponseMessage } from '../../models/api-message';
import { PresentImageUrlPipe } from '../../services/presentService/present-image-url.pipe';

@Component({
  selector: 'app-catalog-detail',
  imports: [CommonModule, FormsModule, RouterLink, TranslatePipe, LocalizedCurrencyPipe, LocalizedNumberPipe, PresentImageUrlPipe],
  templateUrl: './catalog-detail.html',
  styleUrl: './catalog-detail.scss'
})
export class CatalogDetail implements OnInit {
  present: presentModel | null = null;
  loading = true;
  errorMessage = '';
  quantity: number | null = 1;
  submitting = false;
  addSuccessMessage = '';
  addErrorMessage = '';
  readonly maximumQuantity = 100;

  private readonly route = inject(ActivatedRoute);
  readonly router = inject(Router);
  private readonly presentService = inject(PresentService);
  private readonly cardService = inject(CardService);
  readonly isAuthenticated = inject(AuthService).isAuthenticated;

  get isQuantityValid(): boolean {
    return this.isValidQuantity(this.quantity);
  }

  get backQueryParams(): Params {
    return this.route.snapshot.queryParams;
  }

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (!Number.isInteger(id) || id <= 0) {
      this.loading = false;
      this.errorMessage = 'detail.notFound';
      return;
    }

    this.loadPresent(id);
  }

  retryLoad(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (!Number.isInteger(id) || id <= 0) {
      this.loading = false;
      this.errorMessage = 'detail.notFound';
      return;
    }

    this.loadPresent(id);
  }

  private loadPresent(id: number): void {
    this.loading = true;
    this.errorMessage = '';
    this.presentService.getPresentById(id).subscribe({
      next: (result) => {
        this.present = result.success ? result.data?.[0] ?? null : null;
        this.errorMessage = this.present ? '' : 'detail.notFound';
        this.loading = false;
      },
      error: () => {
        this.errorMessage = 'detail.loadFailed';
        this.loading = false;
      }
    });
  }

  imageFallback(event: Event): void {
    (event.target as HTMLImageElement).src = 'assets/present-placeholder.svg';
  }

  addPresentToBasket(): void {
    const quantity = this.quantity;
    if (!this.present || !this.isAuthenticated() || this.submitting || !this.isValidQuantity(quantity)) {
      return;
    }

    this.submitting = true;
    this.addSuccessMessage = '';
    this.addErrorMessage = '';
    this.cardService.addCard({ presentId: this.present.id, quantity }).subscribe({
      next: (response) => {
        this.addSuccessMessage = hasApiResponseMessage(response) ? '' : 'card.addQuantitySuccess';
        this.submitting = false;
      },
      error: () => {
        this.addErrorMessage = 'card.addFailed';
        this.submitting = false;
      }
    });
  }

  private isValidQuantity(quantity: number | null): quantity is number {
    return typeof quantity === 'number' &&
      Number.isInteger(quantity) &&
      quantity >= 1 &&
      quantity <= this.maximumQuantity;
  }
}