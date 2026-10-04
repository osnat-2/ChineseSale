import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { TranslatePipe } from '../../i18n/translate.pipe';
import { cardModel } from '../../models/card';
import { CardService } from '../../services/cardService/card-service';

// Gift Purchase Component - Display gifts for selection and purchase
@Component({
  selector: 'app-card',
  imports: [CommonModule, FormsModule, RouterLink, TranslatePipe],
  templateUrl: './card.html',
  styleUrl: './card.scss',
})
export class Card {
  cards: cardModel[] = [];
  presentId: number | null = null;
  loading = false;
  submitting = false;
  errorMessage = '';
  successMessage = '';

  private readonly cardService = inject(CardService);

  ngOnInit(): void {
    this.loadCards();
  }

  loadCards(): void {
    this.loading = true;
    this.errorMessage = '';
    this.cardService.getMyCards(false).subscribe({
      next: (result) => {
        this.cards = result.success ? result.data ?? [] : [];
        this.errorMessage = result.success ? '' : result.message || 'card.loadFailed';
        this.loading = false;
      },
      error: () => {
        this.errorMessage = 'card.loadFailed';
        this.loading = false;
      }
    });
  }

  addCard(): void {
    if (!this.presentId || this.submitting) return;
    this.submitting = true;
    this.errorMessage = '';
    this.successMessage = '';
    this.cardService.addCard({ presentId: this.presentId }).subscribe({
      next: (result) => {
        if (result.success) {
          this.successMessage = 'card.addSuccess';
          this.presentId = null;
          this.loadCards();
        } else {
          this.errorMessage = result.message || 'card.addFailed';
        }
        this.submitting = false;
      },
      error: () => {
        this.errorMessage = 'card.addFailed';
        this.submitting = false;
      }
    });
  }

  removeCard(card: cardModel): void {
    if (this.submitting) return;
    this.submitting = true;
    this.errorMessage = '';
    this.cardService.removeCard(card.id).subscribe({
      next: (result) => {
        if (result.success) this.cards = this.cards.filter(item => item.id !== card.id);
        else this.errorMessage = result.message || 'card.removeFailed';
        this.submitting = false;
      },
      error: () => {
        this.errorMessage = 'card.removeFailed';
        this.submitting = false;
      }
    });
  }
}