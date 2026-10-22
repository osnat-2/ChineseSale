import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { AuthService } from '../../services/authService/auth-service';

import { Home } from './home';

function createToken(role: string): string {
  const encode = (value: string) => btoa(value).replace(/=/g, '').replace(/\+/g, '-').replace(/\//g, '_');
  return `${encode('{"alg":"none"}')}.${encode(JSON.stringify({ role, exp: Math.floor(Date.now() / 1000) + 300 }))}.signature`;
}

describe('Home', () => {
  let component: Home;
  let fixture: ComponentFixture<Home>;

  beforeEach(async () => {
    localStorage.removeItem('authToken');
    await TestBed.configureTestingModule({
      imports: [Home],
      providers: [provideRouter([])],
    })
    .compileComponents();

    fixture = TestBed.createComponent(Home);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  afterEach(() => localStorage.removeItem('authToken'));

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('shows sign-in and registration actions to an anonymous visitor', () => {
    const links = fixture.nativeElement.querySelectorAll('.home-account-actions a');
    expect(links.length).toBe(2);
    expect(links[0].getAttribute('href')).toBe('/login');
    expect(links[1].getAttribute('href')).toBe('/register');
  });

  it('routes a signed-in customer to the Personal Area', () => {
    TestBed.inject(AuthService).setToken(createToken('User'));
    fixture.detectChanges();

    const links = fixture.nativeElement.querySelectorAll('.home-account-actions a');
    expect(links.length).toBe(1);
    expect(links[0].getAttribute('href')).toBe('/personal-area');
  });

  it('routes an administrator to the management dashboard', () => {
    TestBed.inject(AuthService).setToken(createToken('Admin'));
    fixture.detectChanges();

    const links = fixture.nativeElement.querySelectorAll('.home-account-actions a');
    expect(links.length).toBe(1);
    expect(links[0].getAttribute('href')).toBe('/admin');
  });
});
