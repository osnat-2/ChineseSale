import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { of, Subject, throwError } from 'rxjs';
import { CatalogDetail } from './catalog-detail';
import { PresentService } from '../../services/presentService/present-service';
import { LanguageService } from '../../i18n/language.service';
import { AuthService } from '../../services/authService/auth-service';
import { CardService } from '../../services/cardService/card-service';
import { presentModel } from '../../models/present';

describe('CatalogDetail', () => {
  let component: CatalogDetail;
  let fixture: ComponentFixture<CatalogDetail>;
  const isAuthenticated = signal(false);
  const presentService = { getPresentById: jasmine.createSpy() };
  const cardService = { addCard: jasmine.createSpy() };

  const createPresent = (): presentModel => ({
    id: 7,
    name: 'Present 7',
    donorId: 1,
    categoryId: 1,
    categoryName: 'Books',
    quantity: 1,
    price: 10,
    description: 'Test present',
    imageUrl: 'assets/test.jpg',
    isActive: true
  });

  beforeEach(async () => {
    window.localStorage.setItem('chinese-sale-language', 'en');
    isAuthenticated.set(false);
    presentService.getPresentById.and.returnValue(of({ success: true, data: [createPresent()] }));
    cardService.addCard.and.returnValue(of('Card added successfully.'));
    await TestBed.configureTestingModule({
      imports: [CatalogDetail],
      providers: [
        provideRouter([]),
        { provide: PresentService, useValue: presentService },
        { provide: CardService, useValue: cardService },
        { provide: AuthService, useValue: { isAuthenticated } }
      ]
    }).compileComponents();
    fixture = TestBed.createComponent(CatalogDetail);
    component = fixture.componentInstance;
  });

  afterEach(() => window.localStorage.removeItem('chinese-sale-language'));

  it('reports an invalid detail id without requesting the API', () => {
    fixture.detectChanges();
    expect(component.loading).toBe(false);
    expect(component.errorMessage).toBe('detail.notFound');
    expect(fixture.nativeElement.textContent).toContain('This present could not be found.');

    TestBed.inject(LanguageService).setLanguage('he');
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('לא ניתן למצוא את המתנה.');
  });

  it('offers retry after a recoverable detail load failure', () => {
    presentService.getPresentById.and.returnValue(throwError(() => new Error('offline')));
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Unable to load this present. Please try again.');
    expect(fixture.nativeElement.querySelector('.state-error button')?.textContent).toContain('Retry');

    presentService.getPresentById.and.returnValue(of({ success: true, data: [createPresent()] }));
    (fixture.nativeElement.querySelector('.state-error button') as HTMLButtonElement).click();
    fixture.detectChanges();

    expect(component.present?.id).toBe(7);
    expect(fixture.nativeElement.querySelector('.state-error')).toBeNull();
  });

  it('does not repeat the endpoint success message inline after adding from a present detail', () => {
    isAuthenticated.set(true);
    fixture.detectChanges();
    showPresent();
    const quantityInput = fixture.nativeElement.querySelector('input[name="quantity"]') as HTMLInputElement;
    quantityInput.value = '3';
    quantityInput.dispatchEvent(new Event('input'));
    fixture.detectChanges();
    (fixture.nativeElement.querySelector('.detail-purchase button') as HTMLButtonElement).click();
    fixture.detectChanges();

    expect(cardService.addCard).toHaveBeenCalledWith({ presentId: 7, quantity: 3 });
    expect(component.addSuccessMessage).toBe('');
  });

  it('does not show the purchase action to anonymous visitors', () => {
    fixture.detectChanges();
    showPresent();

    expect(fixture.nativeElement.querySelector('.detail-purchase')).toBeNull();
  });

  it('shows the API-provided category name for the present', () => {
    fixture.detectChanges();
    showPresent();

    expect(fixture.nativeElement.querySelector('.present-category')?.textContent).toContain('Books');
  });

  it('uses the available placeholder when a present image fails to load', () => {
    fixture.detectChanges();
    showPresent();

    const image = fixture.nativeElement.querySelector('.detail-layout img') as HTMLImageElement;
    image.dispatchEvent(new Event('error'));

    expect(image.getAttribute('src')).toBe('assets/present-placeholder.svg');
  });

  it('shows a translated fallback when adding from a present detail fails', () => {
    isAuthenticated.set(true);
    cardService.addCard.and.returnValue(
      throwError(() => new HttpErrorResponse({ error: 'This present is no longer available.' }))
    );
    fixture.detectChanges();
    showPresent();
    (fixture.nativeElement.querySelector('.detail-purchase button') as HTMLButtonElement).click();
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('[role="alert"]')?.textContent).toContain(
      'The card could not be added. Please try again.'
    );
  });

  it('disables the detail action while the add request is pending', () => {
    const pendingResponse = new Subject<string>();
    isAuthenticated.set(true);
    cardService.addCard.and.returnValue(pendingResponse);
    fixture.detectChanges();
    showPresent();
    const button = fixture.nativeElement.querySelector('.detail-purchase button') as HTMLButtonElement;
    button.click();
    fixture.detectChanges();

    expect(button.disabled).toBeTrue();

    pendingResponse.next('Cards added successfully.');
    pendingResponse.complete();
    fixture.detectChanges();

    expect(button.disabled).toBeFalse();
  });

  function showPresent(): void {
    component.present = createPresent();
    component.loading = false;
    component.errorMessage = '';
    fixture.detectChanges();
  }
});