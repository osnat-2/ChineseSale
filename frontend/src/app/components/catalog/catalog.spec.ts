import { ComponentFixture, fakeAsync, TestBed, tick } from '@angular/core/testing';
import { signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import { of, Subject, throwError } from 'rxjs';
import { Catalog } from './catalog';
import { AuthService } from '../../services/authService/auth-service';
import { CardService } from '../../services/cardService/card-service';
import { PresentService } from '../../services/presentService/present-service';
import { CategoryService } from '../../services/categoryService/category-service';
import { LanguageService } from '../../i18n/language.service';
import { presentModel } from '../../models/present';

describe('Catalog', () => {
  let component: Catalog;
  let fixture: ComponentFixture<Catalog>;
  const presentService = { getAllPresents: jasmine.createSpy().and.returnValue(of({ success: true, data: [] })) };
  const categoryService = { getAllCategories: jasmine.createSpy().and.returnValue(of({ success: true, data: [] })) };
  const cardService = { addCard: jasmine.createSpy().and.returnValue(of('Card added successfully.')) };
  const isAuthenticated = signal(false);

  const createPresent = (id: number): presentModel => ({
    id,
    name: `Present ${id}`,
    donorId: 1,
    categoryId: 1,
    quantity: 1,
    price: 10,
    description: 'Test present',
    imageUrl: 'data:image/gif;base64,R0lGODlhAQABAAD/ACwAAAAAAQABAAACADs=',
    isActive: true
  });

  beforeEach(async () => {
    window.localStorage.setItem('chinese-sale-language', 'en');
    presentService.getAllPresents.and.returnValue(of({ success: true, data: [] }));
    categoryService.getAllCategories.and.returnValue(of({ success: true, data: [] }));
    cardService.addCard.and.returnValue(of('Card added successfully.'));
    isAuthenticated.set(false);
    await TestBed.configureTestingModule({
      imports: [Catalog],
      providers: [
        { provide: PresentService, useValue: presentService },
        { provide: CategoryService, useValue: categoryService },
        { provide: CardService, useValue: cardService },
        { provide: AuthService, useValue: { isAuthenticated } },
        provideRouter([])
      ]
    }).compileComponents();
    fixture = TestBed.createComponent(Catalog);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  afterEach(() => window.localStorage.removeItem('chinese-sale-language'));

  it('shows the empty state when the API returns no active presents', () => {
    expect(component.presents).toEqual([]);
    expect(fixture.nativeElement.textContent).toContain('There are no active presents right now.');
  });

  it('shows the API category name or a clear fallback on present cards', () => {
    component.presents = [{ ...createPresent(1), categoryName: 'Books' }, createPresent(2)];
    fixture.detectChanges();

    const cards = fixture.nativeElement.querySelectorAll('.present-card');
    expect(cards[0].querySelector('.present-category')?.textContent).toContain('Books');
    expect(cards[1].querySelector('.present-category')?.textContent).toContain('Category unavailable');
  });

  it('uses the available placeholder when a present image fails to load', () => {
    component.presents = [createPresent(1)];
    fixture.detectChanges();

    const image = fixture.nativeElement.querySelector('.present-card img') as HTMLImageElement;
    image.dispatchEvent(new Event('error'));

    expect(image.getAttribute('src')).toBe('assets/present-placeholder.svg');
  });

  it('keeps the previous results visible and offers retry when refreshing fails', () => {
    const present = createPresent(1);
    presentService.getAllPresents.and.returnValue(of({ success: true, data: [present] }));
    component.loadPresents();
    fixture.detectChanges();

    presentService.getAllPresents.and.returnValue(throwError(() => new Error('offline')));
    component.loadPresents();
    fixture.detectChanges();

    expect(component.presents).toEqual([present]);
    expect(fixture.nativeElement.querySelector('.present-card')).toBeTruthy();
    expect(fixture.nativeElement.textContent).toContain(
      'Unable to update the catalog. Your previous results are still shown.'
    );
    expect(fixture.nativeElement.querySelector('.state-message button')?.textContent).toContain('Retry');
  });

  it('distinguishes an empty catalog from filters with no matches and can clear filters', () => {
    component.search = 'missing';
    component.loadPresents();
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('No presents match these filters.');

    component.clearFilters();
    fixture.detectChanges();

    expect(component.search).toBe('');
    expect(component.categoryId).toBe('');
    expect(fixture.nativeElement.textContent).toContain('There are no active presents right now.');
  });

  it('keeps category and sort filters on the API request while search stays client-side', () => {
    component.search = '  handmade  ';
    component.categoryId = '2';
    component.sortBy = 'price';
    component.sortDirection = 'desc';

    component.loadPresents();

    expect(presentService.getAllPresents).toHaveBeenCalledWith({
      categoryId: 2,
      sortBy: 'price',
      sortDirection: 'desc',
    });
  });

  it('matches catalogue search against present names, descriptions, and category names', () => {
    const present = createPresent(1);
    const otherPresent = { ...createPresent(2), categoryId: 2, categoryName: 'Home goods' };
    presentService.getAllPresents.and.returnValue(of({
      success: true,
      data: [present, otherPresent],
    }));
    component.categories = [];
    component.search = 'HOME';

    component.loadPresents();
    fixture.detectChanges();

    expect(component.filteredPresents).toEqual([otherPresent]);
    expect(fixture.nativeElement.querySelectorAll('.present-card').length).toBe(1);
    expect(fixture.nativeElement.textContent).toContain('Present 2');
    expect(fixture.nativeElement.textContent).toContain('Home goods');
    expect(presentService.getAllPresents).toHaveBeenCalledWith({
      sortBy: 'name',
      sortDirection: 'asc',
    });
  });

  it('clears only the search query and reapplies the other selected filters', () => {
    component.search = 'handmade';
    component.categoryId = '2';
    component.sortBy = 'price';
    component.sortDirection = 'desc';
    fixture.detectChanges();

    const clearSearchButton = fixture.nativeElement.querySelector(
      '.catalog-search-control button'
    ) as HTMLButtonElement;
    expect(clearSearchButton.textContent).toContain('Clear search');
    expect(fixture.nativeElement.querySelector('.catalog-search-field label')?.textContent)
      .toContain('Search presents');
    clearSearchButton.click();
    fixture.detectChanges();

    expect(component.search).toBe('');
    expect(component.categoryId).toBe('2');
    expect(component.sortBy).toBe('price');
    expect(component.sortDirection).toBe('desc');
    expect(presentService.getAllPresents).toHaveBeenCalledWith({
      categoryId: 2,
      sortBy: 'price',
      sortDirection: 'desc',
    });
  });

  it('shows category loading feedback and allows categories to be retried', () => {
    categoryService.getAllCategories.and.returnValue(throwError(() => new Error('offline')));
    component.ngOnInit();
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain(
      'Categories could not be loaded. You can still browse all presents.'
    );

    categoryService.getAllCategories.and.returnValue(of({ success: true, data: [] }));
    (fixture.nativeElement.querySelector('.state-warning button') as HTMLButtonElement).click();
    fixture.detectChanges();

    expect(component.categoryErrorMessage).toBe('');
    expect(fixture.nativeElement.querySelector('.state-warning')).toBeNull();
  });

  it('shows an error state when loading fails', () => {
    presentService.getAllPresents.and.returnValue(throwError(() => new Error('offline')));
    component.loadPresents();
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Unable to load the catalogue.');

    TestBed.inject(LanguageService).setLanguage('he');
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('לא ניתן לטעון את הקטלוג. נסו שוב.');
  });

  it('does not show add-to-basket buttons to anonymous visitors', () => {
    presentService.getAllPresents.and.returnValue(of({ success: true, data: [createPresent(1)] }));
    component.loadPresents();
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('.add-to-basket')).toBeNull();
  });

  it('shows one add-to-basket button per present to authenticated users', () => {
    isAuthenticated.set(true);
    presentService.getAllPresents.and.returnValue(of({
      success: true,
      data: [createPresent(1), createPresent(2)]
    }));
    component.loadPresents();
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelectorAll('.add-to-basket').length).toBe(2);
  });

  it('places the add button beside the present name below the image', () => {
    isAuthenticated.set(true);
    presentService.getAllPresents.and.returnValue(of({ success: true, data: [createPresent(1)] }));
    component.loadPresents();
    fixture.detectChanges();

    const card = fixture.nativeElement.querySelector('.present-card') as HTMLElement;
    const image = card.querySelector('img') as HTMLImageElement;
    const title = card.querySelector('.present-title-row h2') as HTMLHeadingElement;
    const button = card.querySelector('.present-title-row .add-to-basket') as HTMLButtonElement;

    expect(image.compareDocumentPosition(title) & Node.DOCUMENT_POSITION_FOLLOWING).toBeTruthy();
    expect(title.parentElement).toBe(button.parentElement);
    expect(card.querySelector('.purchase-controls input')).toBeNull();
  });

  it('opens the quantity dialog without adding the present yet', () => {
    isAuthenticated.set(true);
    presentService.getAllPresents.and.returnValue(of({ success: true, data: [createPresent(7)] }));
    component.loadPresents();
    fixture.detectChanges();

    (fixture.nativeElement.querySelector('.add-to-basket') as HTMLButtonElement).click();
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('[role="dialog"]')).toBeTruthy();
    expect(fixture.nativeElement.querySelector('[role="dialog"] input[type="number"]')).toBeTruthy();
    expect(document.activeElement).toBe(fixture.nativeElement.querySelector('[role="dialog"] input[type="number"]'));
    expect(cardService.addCard).not.toHaveBeenCalled();
  });

  it('does not repeat the endpoint success message inline after adding from the dialog', fakeAsync(() => {
    isAuthenticated.set(true);
    presentService.getAllPresents.and.returnValue(of({ success: true, data: [createPresent(7)] }));
    component.loadPresents();
    fixture.detectChanges();
    (fixture.nativeElement.querySelector('.present-title-row .add-to-basket') as HTMLButtonElement).click();
    fixture.detectChanges();
    (fixture.nativeElement.querySelector('.catalog-dialog-add') as HTMLButtonElement).click();
    fixture.detectChanges();
    tick();

    expect(cardService.addCard).toHaveBeenCalledWith({ presentId: 7, quantity: 1 });
    expect(component.addCardSuccessMessage).toBe('');
    expect(fixture.nativeElement.querySelector('[role="dialog"]')).toBeNull();
    expect(document.activeElement).toBe(fixture.nativeElement.querySelector('.present-title-row .add-to-basket'));
  }));

  it('adds the chosen quantity from a catalogue card', () => {
    isAuthenticated.set(true);
    presentService.getAllPresents.and.returnValue(of({ success: true, data: [createPresent(7)] }));
    component.loadPresents();
    fixture.detectChanges();

    (fixture.nativeElement.querySelector('.present-title-row .add-to-basket') as HTMLButtonElement).click();
    fixture.detectChanges();
    const quantityInput = fixture.nativeElement.querySelector('[role="dialog"] .quantity-control input') as HTMLInputElement;
    quantityInput.value = '4';
    quantityInput.dispatchEvent(new Event('input'));
    fixture.detectChanges();
    (fixture.nativeElement.querySelector('.catalog-dialog-add') as HTMLButtonElement).click();

    expect(cardService.addCard).toHaveBeenCalledWith({ presentId: 7, quantity: 4 });
  });

  it('disables adding when the quantity is outside the allowed range', () => {
    isAuthenticated.set(true);
    presentService.getAllPresents.and.returnValue(of({ success: true, data: [createPresent(7)] }));
    component.loadPresents();
    fixture.detectChanges();

    (fixture.nativeElement.querySelector('.present-title-row .add-to-basket') as HTMLButtonElement).click();
    fixture.detectChanges();
    const quantityInput = fixture.nativeElement.querySelector('[role="dialog"] .quantity-control input') as HTMLInputElement;
    quantityInput.value = '101';
    quantityInput.dispatchEvent(new Event('input'));
    fixture.detectChanges();

    expect((fixture.nativeElement.querySelector('.catalog-dialog-add') as HTMLButtonElement).disabled).toBeTrue();
    expect(fixture.nativeElement.querySelector('[role="dialog"] .quantity-error')?.textContent).toContain(
      'Choose a quantity from 1 to 100.'
    );
  });

  it('disables basket actions while an add request is pending', () => {
    const pendingResponse = new Subject<string>();
    isAuthenticated.set(true);
    cardService.addCard.and.returnValue(pendingResponse);
    presentService.getAllPresents.and.returnValue(of({ success: true, data: [createPresent(1)] }));
    component.loadPresents();
    fixture.detectChanges();

    (fixture.nativeElement.querySelector('.present-title-row .add-to-basket') as HTMLButtonElement).click();
    fixture.detectChanges();
    const button = fixture.nativeElement.querySelector('.catalog-dialog-add') as HTMLButtonElement;
    button.click();
    fixture.detectChanges();

    expect(button.disabled).toBeTrue();
    expect(button.getAttribute('aria-busy')).toBe('true');

    pendingResponse.next('Card added successfully.');
    pendingResponse.complete();
    fixture.detectChanges();

    expect(button.disabled).toBeFalse();
  });

  it('keeps the dialog open and shows the translated error when adding fails', () => {
    isAuthenticated.set(true);
    cardService.addCard.and.returnValue(
      throwError(() => new HttpErrorResponse({ error: 'This present is no longer available.' }))
    );
    presentService.getAllPresents.and.returnValue(of({ success: true, data: [createPresent(1)] }));
    component.loadPresents();
    fixture.detectChanges();

    (fixture.nativeElement.querySelector('.present-title-row .add-to-basket') as HTMLButtonElement).click();
    fixture.detectChanges();
    (fixture.nativeElement.querySelector('.catalog-dialog-add') as HTMLButtonElement).click();
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('[role="dialog"] [role="alert"]')?.textContent).toContain(
      'The card could not be added. Please try again.'
    );
    expect(fixture.nativeElement.querySelector('[role="dialog"]')).toBeTruthy();
  });

  it('closes the dialog using the cancel button without adding a card', () => {
    isAuthenticated.set(true);
    presentService.getAllPresents.and.returnValue(of({ success: true, data: [createPresent(1)] }));
    component.loadPresents();
    fixture.detectChanges();
    (fixture.nativeElement.querySelector('.present-title-row .add-to-basket') as HTMLButtonElement).click();
    fixture.detectChanges();
    (fixture.nativeElement.querySelector('.catalog-dialog-cancel') as HTMLButtonElement).click();
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('[role="dialog"]')).toBeNull();
    expect(cardService.addCard).not.toHaveBeenCalled();
    expect(document.activeElement).toBe(fixture.nativeElement.querySelector('.present-title-row .add-to-basket'));
  });

  it('closes the dialog with Escape', () => {
    isAuthenticated.set(true);
    presentService.getAllPresents.and.returnValue(of({ success: true, data: [createPresent(1)] }));
    component.loadPresents();
    fixture.detectChanges();
    (fixture.nativeElement.querySelector('.present-title-row .add-to-basket') as HTMLButtonElement).click();
    fixture.detectChanges();
    fixture.nativeElement.querySelector('[role="dialog"]').dispatchEvent(
      new KeyboardEvent('keydown', { key: 'Escape', bubbles: true })
    );
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('[role="dialog"]')).toBeNull();
  });
});