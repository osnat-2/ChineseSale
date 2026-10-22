import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';

import { routes } from '../../app.routes';
import { Admin } from './admin';

describe('Admin', () => {
  let component: Admin;
  let fixture: ComponentFixture<Admin>;

  beforeEach(async () => {
    window.localStorage.setItem('chinese-sale-language', 'en');
    await TestBed.configureTestingModule({
      imports: [Admin],
      providers: [provideRouter([])]
    })
    .compileComponents();

    fixture = TestBed.createComponent(Admin);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  afterEach(() => window.localStorage.removeItem('chinese-sale-language'));

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should render the admin dashboard sections', () => {
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('Admin dashboard');
    expect(compiled.textContent).toContain('Present management');
    expect(compiled.textContent).toContain('Donor management');
  });

  it('offers lottery management and combines card and purchase management', () => {
    const links = Array.from(fixture.nativeElement.querySelectorAll('.admin-card')) as HTMLAnchorElement[];
    const lotteryLink = links.find((link) => link.textContent?.includes('Lottery management'));
    const purchasesLink = links.find((link) => link.textContent?.includes('Cards & purchases management'));

    expect(lotteryLink?.getAttribute('href')).toBe('/admin/winners');
    expect(purchasesLink?.getAttribute('href')).toBe('/admin/purchases');
    expect(links.some((link) => link.textContent?.includes('Card management'))).toBeFalse();
    expect(routes.find((route) => route.path === 'admin/cards')?.redirectTo).toBe('admin/purchases');
    expect(routes.find((route) => route.path === 'winner')?.redirectTo).toBe('admin/winners');
  });
});
