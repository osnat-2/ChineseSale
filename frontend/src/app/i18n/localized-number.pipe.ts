import { Pipe, PipeTransform, inject } from '@angular/core';
import { LanguageService } from './language.service';

@Pipe({
  name: 'localizedNumber',
  standalone: true,
  pure: false,
})
export class LocalizedNumberPipe implements PipeTransform {
  private readonly language = inject(LanguageService);

  transform(value: number | string | null | undefined): string {
    if (value === null || value === undefined || value === '') {
      return '';
    }

    const number = Number(value);
    if (!Number.isFinite(number)) {
      return '';
    }

    const locale = this.language.currentLanguage() === 'he' ? 'he-IL' : 'en-US';
    return new Intl.NumberFormat(locale).format(number);
  }
}
