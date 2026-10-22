import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { Navbar } from './navbar';
import { LanguageService } from '../../i18n/language.service';
import { AuthService } from '../../services/authService/auth-service';

function createToken(claims: Record<string, unknown>): string {
  const encode = (value: string) => btoa(String.fromCharCode(...new TextEncoder().encode(value)))
    .replace(/=/g, '').replace(/\+/g, '-').replace(/\//g, '_');
  return `${encode('{"alg":"none"}')}.${encode(JSON.stringify(claims))}.signature`;
}

describe('Navbar', () => {
  let component: Navbar;
  let fixture: ComponentFixture<Navbar>;

  beforeEach(async () => {
    localStorage.removeItem('authToken');
    await TestBed.configureTestingModule({
      imports: [Navbar],
      providers: [provideRouter([])],
    })
    .compileComponents();

    fixture = TestBed.createComponent(Navbar);
    component = fixture.componentInstance;
    TestBed.inject(LanguageService).setLanguage('en');
    fixture.detectChanges();
  });

  afterEach(() => localStorage.removeItem('authToken'));

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('replaces the separate cards and purchases links with one Personal Area link', () => {
    TestBed.inject(AuthService).setToken(createToken({
      role: 'User',
      exp: Math.floor(Date.now() / 1000) + 300
    }));
    fixture.detectChanges();

    const links = fixture.nativeElement.querySelectorAll('.nav-links a[routerLink]');
    expect(fixture.nativeElement.querySelector('.nav-links a[routerLink="/personal-area"]')).toBeTruthy();
    expect(fixture.nativeElement.querySelector('.nav-links a[routerLink="/card"]')).toBeNull();
    expect(links.length).toBe(3);
  });

  it('shows a localized Personal Area hover title and accessible description', () => {
    TestBed.inject(AuthService).setToken(createToken({
      role: 'User',
      exp: Math.floor(Date.now() / 1000) + 300
    }));
    fixture.detectChanges();

    const link = fixture.nativeElement.querySelector('.nav-links a[routerLink="/personal-area"]') as HTMLAnchorElement;
    const descriptionId = link.getAttribute('aria-describedby');
    const description = fixture.nativeElement.querySelector(`#${descriptionId}`) as HTMLElement;

    expect(link.getAttribute('title')).toBe('View your profile, cards, and checkout');
    expect(description.textContent).toContain('View your profile, cards, and checkout');
    expect(link.getAttribute('routerLink')).toBe('/personal-area');

    TestBed.inject(LanguageService).setLanguage('he');
    fixture.detectChanges();

    expect(link.getAttribute('title')).toBe('לצפייה בפרופיל, בכרטיסים ובתהליך התשלום');
    expect(description.textContent).toContain('לצפייה בפרופיל, בכרטיסים ובתהליך התשלום');
  });

  it('exposes the mobile navigation disclosure state accessibly', () => {
    const toggle = fixture.nativeElement.querySelector('.nav-menu-toggle') as HTMLButtonElement;

    expect(toggle.getAttribute('aria-expanded')).toBe('false');
    component.toggleMenu();
    fixture.detectChanges();

    expect(toggle.getAttribute('aria-expanded')).toBe('true');
    expect(fixture.nativeElement.querySelector('.nav-content').classList).toContain('is-open');
  });
});
