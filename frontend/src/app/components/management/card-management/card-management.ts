import { CommonModule } from '@angular/common';
import { TranslatePipe } from '../../../i18n/translate.pipe';
import { Component } from '@angular/core';

@Component({
  selector: 'app-card-management',
  imports: [CommonModule, TranslatePipe],
  templateUrl: './card-management.html',
  styleUrl: './card-management.scss',
})
export class CardManagement {
  readonly cards: Array<{ id: number; user: string; present: string; paid: boolean }> = [];
}
