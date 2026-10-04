import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardService } from '../../services/cardService/card-service';
import { cardModel } from '../../models/card';
import { RouterLink } from '@angular/router';
import { TranslatePipe } from '../../i18n/translate.pipe';

// Payment Component - Process ticket purchases and buyer information
@Component({
  selector: 'app-payment',
  imports: [CommonModule, RouterLink, TranslatePipe],
  templateUrl: './payment.html',
  styleUrl: './payment.scss',
})
export class Payment{
  cards: cardModel[] = [];
  loading = true;
  paying = false;
  errorMessage = '';
  successMessage = '';
  private readonly cardService = inject(CardService);

  ngOnInit(): void {
    this.cardService.getMyCards(false).subscribe({
      next: (result) => {
        this.cards = result.success ? result.data ?? [] : [];
        this.errorMessage = result.success ? '' : result.message || 'payment.loadFailed';
        this.loading = false;
      },
      error: () => {
        this.errorMessage = 'payment.loadFailed';
        this.loading = false;
      }
    });
  }

  pay(): void {
    if (this.paying || this.cards.length === 0) return;
    this.paying = true;
    this.errorMessage = '';
    this.cardService.processPayment().subscribe({
      next: (result) => {
        if (result.success) {
          this.successMessage = 'payment.success';
          this.cards = [];
        } else {
          this.errorMessage = result.message || 'payment.failed';
        }
        this.paying = false;
      },
      error: () => {
        this.errorMessage = 'payment.failed';
        this.paying = false;
      }
    });
  }
}