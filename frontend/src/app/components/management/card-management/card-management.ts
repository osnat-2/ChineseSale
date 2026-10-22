import { CommonModule } from '@angular/common';
import { TranslatePipe } from '../../../i18n/translate.pipe';
import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CardService } from '../../../services/cardService/card-service';

@Component({
  selector: 'app-card-management',
  imports: [CommonModule, RouterLink, TranslatePipe],
  templateUrl: './card-management.html',
  styleUrl: './card-management.scss',
})
export class CardManagement {
  cardService: CardService = inject(CardService);
  readonly cards: Array<{ id: number; user: string; present: string; paid: boolean }> = [];
  onInit() {
    this.loadCards();
  }
  loadCards() {
    // this.cardService.getCards().subscribe((card) => {
    //   this.cards = card;
    // }
  }
}