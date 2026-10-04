import { Pipe, PipeTransform, inject } from '@angular/core';
import { LanguageService } from './language.service';

@Pipe({
  name: 'localizedCurrency',
  standalone: true,
  pure: false,
})
export class LocalizedCurrencyPipe implements PipeTransform {
  private readonly language = inject(LanguageService);

  transform(value: number | string | null | undefined, currencyCode = 'ILS'): string {
    if (value === null || value === undefined || value === '') {
      return '';
    }

    const amount = Number(value);
    if (!Number.isFinite(amount)) {
      return '';
    }

    const locale = this.language.currentLanguage() === 'he' ? 'he-IL' : 'en-US';
    return new Intl.NumberFormat(locale, { style: 'currency', currency: currencyCode }).format(amount);
  }
}
