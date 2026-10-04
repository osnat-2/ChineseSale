import { CommonModule } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TranslatePipe } from '../../i18n/translate.pipe';
import { cardModel } from '../../models/card';
import { CardService } from '../../services/cardService/card-service';

@Component({
  selector: 'app-personal-area',
  imports: [CommonModule, RouterLink, TranslatePipe],
  templateUrl: './personal-area.html',
  styleUrl: './personal-area.scss',
})
export class PersonalArea {
  cards: cardModel[] = [];
  loading = true;
  errorMessage = '';
  private readonly cardService = inject(CardService);

  ngOnInit(): void {
    this.cardService.getMyCards().subscribe({
      next: (result) => {
        this.cards = result.success ? result.data ?? [] : [];
        this.errorMessage = result.success ? '' : result.message || 'personal.loadFailed';
        this.loading = false;
      },
      error: () => {
        this.errorMessage = 'personal.loadFailed';
        this.loading = false;
      }
    });
  }

}
