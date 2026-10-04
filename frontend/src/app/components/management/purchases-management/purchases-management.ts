import { CommonModule } from '@angular/common';
import { TranslatePipe } from '../../../i18n/translate.pipe';
import { Component } from '@angular/core';
import { LocalizedCurrencyPipe } from '../../../i18n/localized-currency.pipe';

@Component({
  selector: 'app-purchases-management',
  imports: [CommonModule, TranslatePipe, LocalizedCurrencyPipe],
  templateUrl: './purchases-management.html',
  styleUrl: './purchases-management.scss',
})
export class PurchasesManagement {
  readonly purchases: Array<{ customer: string; ticket: string; total: number; status: string }> = [];
}