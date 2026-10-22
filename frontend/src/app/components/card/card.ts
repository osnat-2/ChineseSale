import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslatePipe } from '../../i18n/translate.pipe';
import { cardModel } from '../../models/card';
import { CardService } from '../../services/cardService/card-service';
import { RouterLink } from '@angular/router';
import { hasApiResponseMessage } from '../../models/api-message';

interface CardGroup {
  presentId: number;
  presentName: string;
  categoryName: string;
  cards: cardModel[];
}

// Gift Purchase Component - Display gifts for selection and purchase
@Component({
  selector: 'app-card',
  imports: [CommonModule, RouterLink, TranslatePipe],
  templateUrl: './card.html',
  styleUrl: './card.scss',
})
export class Card implements OnInit {
  cardGroups: CardGroup[] = [];
  loading = true;
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
        if (result.success) {
          this.cardGroups = this.groupCards(result.data ?? []);
        } else {
          this.errorMessage = this.cardGroups.length ? 'card.refreshFailed' : 'card.loadFailed';
        }
        this.loading = false;
      },
      error: () => {
        this.errorMessage = this.cardGroups.length ? 'card.refreshFailed' : 'card.loadFailed';
        this.loading = false;
      }
    });
  }

  get totalCardCount(): number {
    return this.cardGroups.reduce((total, group) => total + group.cards.length, 0);
  }

  removeCard(group: CardGroup): void {
    const card = group.cards[0];
    if (this.submitting || !card) return;
    this.submitting = true;
    this.errorMessage = '';
    this.successMessage = '';
    this.cardService.removeCard(card.id).subscribe({
      next: (response) => {
        this.cardGroups = this.cardGroups.flatMap((currentGroup) => {
          if (currentGroup.presentId !== group.presentId) return [currentGroup];
          const remainingCards = currentGroup.cards.filter((item) => item.id !== card.id);
          return remainingCards.length ? [{ ...currentGroup, cards: remainingCards }] : [];
        });
        this.successMessage = hasApiResponseMessage(response) ? '' : 'card.removeSuccess';
        this.submitting = false;
      },
      error: () => {
        this.errorMessage = 'card.removeFailed';
        this.submitting = false;
      }
    });
  }

  private groupCards(cards: cardModel[]): CardGroup[] {
    const groups = new Map<number, CardGroup>();

    for (const card of cards) {
      const group = groups.get(card.presentId);
      if (group) {
        group.cards.push(card);
        continue;
      }

      const presentName = typeof card.present?.name === 'string' ? card.present.name.trim() : '';
      const categoryName = typeof card.present?.categoryName === 'string'
        ? card.present.categoryName.trim()
        : '';
      groups.set(card.presentId, { presentId: card.presentId, presentName, categoryName, cards: [card] });
    }

    return Array.from(groups.values());
  }
}