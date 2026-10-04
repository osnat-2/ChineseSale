import { CommonModule } from '@angular/common';
import { TranslatePipe } from '../../../i18n/translate.pipe';
import { Component } from '@angular/core';
import { LocalizedCurrencyPipe } from '../../../i18n/localized-currency.pipe';

@Component({
  selector: 'app-present-management',
  imports: [CommonModule, TranslatePipe, LocalizedCurrencyPipe],
  templateUrl: './present-management.html',
  styleUrl: './present-management.scss',
})
export class PresentManagement {
  readonly presents: Array<{ id: number; name: string; category: string; price: number; active: boolean }> = [];
}
