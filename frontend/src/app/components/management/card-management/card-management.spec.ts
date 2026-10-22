import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';

import { CardManagement } from './card-management';

describe('CardManagement', () => {
  let component: CardManagement;
  let fixture: ComponentFixture<CardManagement>;

  beforeEach(async () => {
    window.localStorage.setItem('chinese-sale-language', 'en');
    await TestBed.configureTestingModule({
      imports: [CardManagement],
      providers: [provideRouter([])],
    }).compileComponents();

    fixture = TestBed.createComponent(CardManagement);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  afterEach(() => window.localStorage.removeItem('chinese-sale-language'));

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('links back to the management menu', () => {
    const link = fixture.nativeElement.querySelector('.management-back-link') as HTMLAnchorElement;

    expect(link.getAttribute('href')).toBe('/admin');
    expect(link.textContent).toContain('Back to management menu');
  });
});
