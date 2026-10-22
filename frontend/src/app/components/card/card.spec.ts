import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';

import { Card } from './card';
import { CardService } from '../../services/cardService/card-service';

describe('Card', () => {
  let component: Card;
  let fixture: ComponentFixture<Card>;
  const cardService = {
    getMyCards: jasmine.createSpy().and.returnValue(of({ success: true, data: [] })),
    removeCard: jasmine.createSpy()
  };

  beforeEach(async () => {
    cardService.getMyCards.and.returnValue(of({ success: true, data: [] }));
    await TestBed.configureTestingModule({
      imports: [Card],
      providers: [
        provideRouter([]),
        { provide: CardService, useValue: cardService }
      ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(Card);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('groups unpaid cards by present and shows names and counts without identifiers', () => {
    cardService.getMyCards.and.returnValue(of({
      success: true,
      data: [
        { id: 12, presentId: 4, present: { id: 4, name: 'Laptop', categoryName: 'Electronics' }, isPaid: false },
        { id: 13, presentId: 4, present: { id: 4, name: 'Laptop', categoryName: 'Electronics' }, isPaid: false },
        { id: 15, presentId: 7, present: { id: 7, name: 'Gift basket', categoryName: 'Home goods' }, isPaid: false }
      ]
    }));
    component.loadCards();
    fixture.detectChanges();

    const text = fixture.nativeElement.textContent as string;
    const rows = Array.from<Element>(fixture.nativeElement.querySelectorAll('ul li'))
      .map((row) => (row.textContent as string).trim());

    expect(rows.length).toBe(2);
    expect(rows[0]).toContain('Laptop');
    expect(rows[0]).toContain('Electronics');
    expect(rows[0]).toContain('Number of cards: 2');
    expect(rows[1]).toContain('Gift basket');
    expect(rows[1]).toContain('Home goods');
    expect(rows[1]).toContain('Number of cards: 1');
    expect(text).not.toContain('Card #');
    expect(text).not.toContain('present #');
    expect(cardService.getMyCards).toHaveBeenCalledWith(false);
  });

  it('shows a catalogue path rather than requiring customers to know a present ID', () => {
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('input[type="number"]')).toBeNull();
    const links = Array.from(fixture.nativeElement.querySelectorAll('a[routerLink]')) as HTMLAnchorElement[];
    expect(links.some((link) => link.getAttribute('routerLink') === '/catalog')).toBeTrue();
  });

  it('preserves cards and offers retry when refreshing fails', () => {
    cardService.getMyCards.and.returnValue(of({
      success: true,
      data: [{ id: 12, presentId: 4, present: { id: 4, name: 'Laptop' }, isPaid: false }]
    }));
    component.loadCards();
    fixture.detectChanges();

    cardService.getMyCards.and.returnValue(throwError(() => new Error('offline')));
    component.loadCards();
    fixture.detectChanges();

    expect(component.cardGroups.length).toBe(1);
    expect(fixture.nativeElement.querySelector('.card-review-list li')).toBeTruthy();
    expect(fixture.nativeElement.textContent).toContain('Your previous card list is still shown.');
    expect(fixture.nativeElement.querySelector('.purchase-error button')?.textContent).toContain('Retry');
  });

  it('removes one card from a present group without displaying its ID', () => {
    cardService.getMyCards.and.returnValue(of({
      success: true,
      data: [
        { id: 12, presentId: 4, present: { id: 4, name: 'Laptop' }, isPaid: false },
        { id: 13, presentId: 4, present: { id: 4, name: 'Laptop' }, isPaid: false }
      ]
    }));
    cardService.removeCard.and.returnValue(of('Card removed successfully.'));
    component.loadCards();
    fixture.detectChanges();

    const removeButton = fixture.nativeElement.querySelector('button[type="button"]') as HTMLButtonElement;
    removeButton.click();
    fixture.detectChanges();

    expect(cardService.removeCard).toHaveBeenCalledWith(12);
    expect(fixture.nativeElement.querySelectorAll('ul li').length).toBe(1);
    expect(fixture.nativeElement.querySelector('ul li').textContent).toContain('Number of cards: 1');
    expect(fixture.nativeElement.textContent).not.toContain('Card #12');
    expect(component.successMessage).toBe('');
  });

  it('shows the local removal fallback when the endpoint returns no message', () => {
    cardService.removeCard.and.returnValue(of(''));
    const group = {
      presentId: 4,
      presentName: 'Laptop',
      categoryName: '',
      cards: [{ id: 12, presentId: 4, isPaid: false, userId: 3, createdAt: new Date() }],
    };

    component.removeCard(group);

    expect(component.successMessage).toBe('card.removeSuccess');
  });
});
