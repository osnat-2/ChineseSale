import { TestBed } from '@angular/core/testing';
import { LanguageService } from './language.service';
import { TranslatePipe } from './translate.pipe';
import { LocalizedCurrencyPipe } from './localized-currency.pipe';

describe('LanguageService', () => {
  const storageKey = 'chinese-sale-language';
  let language: LanguageService;

  beforeEach(() => {
    window.localStorage.removeItem(storageKey);
    TestBed.configureTestingModule({});
    language = TestBed.inject(LanguageService);
  });

  afterEach(() => {
    window.localStorage.removeItem(storageKey);
    TestBed.resetTestingModule();
  });

  it('uses Hebrew for a Hebrew browser locale and English otherwise', () => {
    const expectedLanguage = window.navigator.language.toLowerCase().startsWith('he') ? 'he' : 'en';
    expect(language.currentLanguage()).toBe(expectedLanguage);
  });

  it('persists the selection and updates document direction and translations', () => {
    language.setLanguage('he');

    expect(window.localStorage.getItem(storageKey)).toBe('he');
    expect(document.documentElement.lang).toBe('he');
    expect(document.documentElement.dir).toBe('rtl');
    expect(language.translate('nav.catalog')).toBe('קטלוג');

    language.setLanguage('en');
    expect(window.localStorage.getItem(storageKey)).toBe('en');
    expect(document.documentElement.lang).toBe('en');
    expect(document.documentElement.dir).toBe('ltr');
    expect(language.translate('nav.catalog')).toBe('Catalogue');
  });

  it('restores an explicitly saved language on a new service instance', () => {
    language.setLanguage('he');
    TestBed.resetTestingModule();
    TestBed.configureTestingModule({});
    language = TestBed.inject(LanguageService);

    expect(language.currentLanguage()).toBe('he');
    expect(document.documentElement.dir).toBe('rtl');
  });

  it('formats currency for the active locale', () => {
    const currency = TestBed.runInInjectionContext(() => new LocalizedCurrencyPipe());
    language.setLanguage('he');

    expect(currency.transform(10, 'ILS')).toContain('10');
    expect(currency.transform(Number.NaN)).toBe('');
  });

  it('translates interpolation parameters without losing mixed-script identifiers', () => {
    const pipe = TestBed.runInInjectionContext(() => new TranslatePipe());
    language.setLanguage('he');

    expect(pipe.transform('card.reference', { cardId: 12, presentId: 4 }))
      .toBe('כרטיס מס׳ 12 עבור מתנה מס׳ 4');
  });
});
