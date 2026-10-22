import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { provideRouter } from '@angular/router';

import { PersonalArea } from './personal-area';
import { LanguageService } from '../../i18n/language.service';
import { AuthService } from '../../services/authService/auth-service';
import { CardService } from '../../services/cardService/card-service';
import { PresentService } from '../../services/presentService/present-service';

function createToken(claims: Record<string, unknown>): string {
  const encode = (value: string) => btoa(String.fromCharCode(...new TextEncoder().encode(value)))
    .replace(/=/g, '').replace(/\+/g, '-').replace(/\//g, '_');
  return `${encode('{"alg":"none"}')}.${encode(JSON.stringify(claims))}.signature`;
}

describe('PersonalArea', () => {
  let component: PersonalArea;
  let fixture: ComponentFixture<PersonalArea>;
  const cardService = {
    getMyCards: jasmine.createSpy().and.returnValue(of({ success: true, data: [] }))
  };
  const presentService = {
    getMyDonatedPresents: jasmine.createSpy().and.returnValue(of({ success: true, data: [] }))
  };

  beforeEach(async () => {
    cardService.getMyCards.and.returnValue(of({ success: true, data: [] }));
    presentService.getMyDonatedPresents.and.returnValue(of({ success: true, data: [] }));
    await TestBed.configureTestingModule({
      imports: [PersonalArea],
      providers: [
        provideRouter([]),
        { provide: CardService, useValue: cardService },
        { provide: PresentService, useValue: presentService }
      ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PersonalArea);
    component = fixture.componentInstance;
    TestBed.inject(AuthService).setToken(createToken({
      unique_name: 'Ada Example',
      email: 'ada@example.com',
      phone_number: '+1 555 0100',
      role: 'User',
      exp: Math.floor(Date.now() / 1000) + 300
    }));
    TestBed.inject(LanguageService).setLanguage('en');
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('shows paid and unpaid cards grouped by present without identifiers', () => {
    cardService.getMyCards.and.callFake((paid: boolean) => of({
      success: true,
      data: paid
        ? [
            { id: 14, presentId: 4, present: { id: 4, name: 'Laptop', categoryName: 'Electronics' }, isPaid: true },
            { id: 15, presentId: 7, present: { id: 7, name: 'Gift basket', categoryName: 'Home goods' }, isPaid: true }
          ]
        : [
            { id: 12, presentId: 4, present: { id: 4, name: 'Laptop', categoryName: 'Electronics' }, isPaid: false },
            { id: 13, presentId: 4, present: { id: 4, name: 'Laptop', categoryName: 'Electronics' }, isPaid: false }
          ]
    }));
    fixture.detectChanges();

    const text = fixture.nativeElement.textContent as string;
    const paidRows = Array.from<Element>(fixture.nativeElement.querySelectorAll(
      'section[aria-labelledby="paid-cards-heading"] .personal-card-list li'
    ))
      .map((row) => (row.textContent as string).trim());
    const unpaidRows = Array.from<Element>(fixture.nativeElement.querySelectorAll(
      'section[aria-labelledby="unpaid-cards-heading"] .personal-card-list li'
    ))
      .map((row) => (row.textContent as string).trim());

    expect(text).toContain('Ada Example');
    expect(text).toContain('ada@example.com');
    expect(text).toContain('+1 555 0100');
    expect(text.match(/Ada Example/g)?.length).toBe(1);
    expect(text.match(/ada@example\.com/g)?.length).toBe(1);
    expect(fixture.nativeElement.querySelectorAll('.personal-details > div').length).toBe(2);
    expect(paidRows.length).toBe(2);
    expect(paidRows[0]).toContain('Laptop');
    expect(paidRows[0]).toContain('Electronics');
    expect(paidRows[0]).toContain('Number of cards: 1');
    expect(paidRows[1]).toContain('Gift basket');
    expect(unpaidRows.length).toBe(1);
    expect(unpaidRows[0]).toContain('Laptop');
    expect(unpaidRows[0]).toContain('Electronics');
    expect(unpaidRows[0]).toContain('Number of cards: 2');
    expect(unpaidRows[0]).toContain('Unpaid');
    expect(fixture.nativeElement.querySelector('.profile-avatar')?.textContent).toContain('AE');
    expect(text).not.toContain('Card #');
    expect(text).not.toContain('present #');
    expect(cardService.getMyCards).toHaveBeenCalledWith(true);
    expect(cardService.getMyCards).toHaveBeenCalledWith(false);
  });

  it('keeps card management and checkout reachable from the personal area', () => {
    cardService.getMyCards.and.callFake((paid: boolean) => of({
      success: true,
      data: paid ? [] : [{ id: 12, presentId: 4, present: { id: 4, name: 'Laptop' }, isPaid: false }]
    }));
    fixture.detectChanges();
    const links = Array.from(fixture.nativeElement.querySelectorAll('a[routerLink]')) as HTMLAnchorElement[];

    expect(links.some(link => link.getAttribute('routerLink') === '/card')).toBeTrue();
    expect(links.some(link => link.getAttribute('routerLink') === '/payment')).toBeTrue();
  });

  it('shows an empty state when the user has no cards', () => {
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('You do not have any cards yet.');
  });

  it('shows donated presents, including inactive ones, to donor-role users only', () => {
    TestBed.inject(AuthService).setToken(createToken({
      unique_name: 'Dora Donor',
      role: 'Donor',
      exp: Math.floor(Date.now() / 1000) + 300
    }));
    presentService.getMyDonatedPresents.and.returnValue(of({
      success: true,
      data: [
        { id: 1, name: 'Handmade quilt', description: 'A warm quilt', categoryName: 'Home goods', quantity: 2, isActive: true },
        { id: 2, name: 'Vintage vase', description: '', quantity: 1, isActive: false }
      ]
    }));
    fixture.detectChanges();

    const section = fixture.nativeElement.querySelector(
      'section[aria-labelledby="personal-donated-presents-heading"]'
    ) as HTMLElement;

    expect(section.textContent).toContain('Handmade quilt');
    expect(section.textContent).toContain('Home goods');
    expect(section.textContent).toContain('Quantity donated: 2');
    expect(section.textContent).toContain('Vintage vase');
    expect(section.textContent).toContain('Inactive');
    expect(presentService.getMyDonatedPresents).toHaveBeenCalledTimes(1);

    fixture.destroy();
    fixture = TestBed.createComponent(PersonalArea);
    component = fixture.componentInstance;
    TestBed.inject(AuthService).setToken(createToken({
      unique_name: 'Una Donor',
      role: 'User',
      exp: Math.floor(Date.now() / 1000) + 300
    }));
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector(
      'section[aria-labelledby="personal-donated-presents-heading"]'
    )).toBeNull();
    expect(presentService.getMyDonatedPresents).toHaveBeenCalledTimes(1);
  });

  it('shows a retryable error when the donor presents request fails', () => {
    TestBed.inject(AuthService).setToken(createToken({
      role: 'Donor',
      exp: Math.floor(Date.now() / 1000) + 300
    }));
    presentService.getMyDonatedPresents.and.returnValue(throwError(() => new Error('request failed')));
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('[role="alert"]')?.textContent)
      .toContain('Unable to load your donated presents.');
  });

  it('shows a status-specific empty message when one payment state has no cards', () => {
    cardService.getMyCards.and.callFake((paid: boolean) => of({
      success: true,
      data: paid ? [{ id: 4, presentId: 2, present: { id: 2, name: 'Book' }, isPaid: true }] : []
    }));
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('You do not have any paid cards.');
    expect(fixture.nativeElement.textContent).toContain('You do not have any unpaid cards.');
  });

  it('shows an error when the cards request fails', () => {
    cardService.getMyCards.and.returnValue(throwError(() => new Error('request failed')));
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('[role="alert"]')).toBeTruthy();
    expect(fixture.nativeElement.querySelector('[role="alert"] button')?.textContent).toContain('Retry');
  });

  it('does not offer profile editing when the API only provides read-only identity claims', () => {
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain(
      'Account details are read-only; editing is unavailable.'
    );
    expect(fixture.nativeElement.querySelector('button.edit-profile')).toBeNull();
  });

  it('displays Hebrew identity values from JWT claims correctly', () => {
    TestBed.inject(AuthService).setToken(createToken({
      unique_name: 'נועה',
      email: 'noa@example.com',
      phone_number: '0501234567',
      role: 'User',
      exp: Math.floor(Date.now() / 1000) + 300
    }));
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('נועה');
  });
});
