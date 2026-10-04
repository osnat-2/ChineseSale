import { Pipe, PipeTransform, inject } from '@angular/core';
import { LanguageService } from './language.service';

@Pipe({
  name: 'localizedDate',
  standalone: true,
  pure: false,
})
export class LocalizedDatePipe implements PipeTransform {
  private readonly language = inject(LanguageService);

  transform(
    value: Date | string | number | null | undefined,
    options: Intl.DateTimeFormatOptions = { dateStyle: 'medium' },
  ): string {
    if (value === null || value === undefined || value === '') {
      return '';
    }

    const date = value instanceof Date ? value : new Date(value);
    if (!Number.isFinite(date.getTime())) {
      return '';
    }

    const locale = this.language.currentLanguage() === 'he' ? 'he-IL' : 'en-US';
    return new Intl.DateTimeFormat(locale, options).format(date);
  }
}
