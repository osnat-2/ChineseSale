import { DOCUMENT } from '@angular/common';
import { computed, inject, Injectable, signal } from '@angular/core';
import { englishTranslations, hebrewTranslations } from './translations';

export type Language = 'en' | 'he';

@Injectable({ providedIn: 'root' })
export class LanguageService {
  private readonly document = inject(DOCUMENT);
  private readonly storageKey = 'chinese-sale-language';
  private readonly dictionaries = {
    en: englishTranslations,
    he: hebrewTranslations,
  };

  readonly currentLanguage = signal<Language>(this.getInitialLanguage());
  readonly direction = computed(() => this.currentLanguage() === 'he' ? 'rtl' : 'ltr');

  constructor() {
    this.activate(this.currentLanguage());
  }

  translate(key: string, params?: Record<string, string | number>): string {
    const translation = key.split('.').reduce<unknown>((value, part) => {
      if (typeof value !== 'object' || value === null) {
        return undefined;
      }
      return (value as Record<string, unknown>)[part];
    }, this.dictionaries[this.currentLanguage()]);

    if (typeof translation !== 'string') {
      return key;
    }

    return translation.replace(/\{\{\s*([\w]+)\s*\}\}/g, (placeholder, name: string) => {
      const value = params?.[name];
      return value === undefined ? placeholder : String(value);
    });
  }

  instant(key: string, params?: Record<string, string | number>): string {
    return this.translate(key, params);
  }

  setLanguage(language: Language): void {
    if (typeof window !== 'undefined') {
      window.localStorage.setItem(this.storageKey, language);
    }
    if (this.currentLanguage() === language) {
      return;
    }

    this.currentLanguage.set(language);
    this.activate(language);
  }

  private activate(language: Language): void {
    this.document.documentElement.lang = language;
    this.document.documentElement.dir = language === 'he' ? 'rtl' : 'ltr';
  }

  private getInitialLanguage(): Language {
    if (typeof window === 'undefined') {
      return 'en';
    }

    const savedLanguage = window.localStorage.getItem(this.storageKey);
    if (savedLanguage === 'en' || savedLanguage === 'he') {
      return savedLanguage;
    }

    return window.navigator.language.toLowerCase().startsWith('he') ? 'he' : 'en';
  }
}
