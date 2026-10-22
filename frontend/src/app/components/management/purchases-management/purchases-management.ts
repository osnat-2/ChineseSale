import { CommonModule } from '@angular/common';
import { TranslatePipe } from '../../../i18n/translate.pipe';
import { Component } from '@angular/core';
import { LocalizedCurrencyPipe } from '../../../i18n/localized-currency.pipe';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-purchases-management',
  imports: [CommonModule, RouterLink, TranslatePipe, LocalizedCurrencyPipe],
  templateUrl: './purchases-management.html',
  styleUrl: './purchases-management.scss',
})
export class PurchasesManagement {
  readonly cards: Array<{ id: number; user: string; present: string; paid: boolean }> = [];
  readonly purchases: Array<{ customer: string; ticket: string; total: number; status: string }> = [];
}