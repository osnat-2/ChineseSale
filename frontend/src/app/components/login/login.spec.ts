import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpErrorResponse, provideHttpClient } from '@angular/common/http';
import { ActivatedRoute, provideRouter, Router } from '@angular/router';
import { of, throwError } from 'rxjs';

import { Login } from './login';
import { AuthService } from '../../services/authService/auth-service';
import { UserService } from '../../services/userService/user-service';

function createToken(role: string): string {
  const encode = (value: string) =>
    btoa(value).replace(/=/g, '').replace(/\+/g, '-').replace(/\//g, '_');
  return `${encode('{"alg":"none"}')}.${encode(
    JSON.stringify({ sub: '7', role, exp: Math.floor(Date.now() / 1000) + 300 }),
  )}.signature`;
}

describe('Login', () => {
  let component: Login;
  let fixture: ComponentFixture<Login>;
  let userService: jasmine.SpyObj<UserService>;
  let authService: AuthService;
  let router: Router;
  let returnUrl: string | null;

  beforeEach(async () => {
    localStorage.clear();
    userService = jasmine.createSpyObj<UserService>('UserService', ['login']);
    returnUrl = null;

    await TestBed.configureTestingModule({
      imports: [Login],
      providers: [
        provideHttpClient(),
        provideRouter([]),
        { provide: UserService, useValue: userService },
        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: {
              queryParamMap: {
                get: (key: string) => (key === 'returnUrl' ? returnUrl : null),
              },
            },
          },
        },
      ],
    })
    .compileComponents();

    fixture = TestBed.createComponent(Login);
    component = fixture.componentInstance;
    authService = TestBed.inject(AuthService);
    router = TestBed.inject(Router);
    spyOn(router, 'navigateByUrl').and.resolveTo(true);
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  for (const { role, expectedUrl } of [
    { role: 'User', expectedUrl: '/catalog' },
    { role: 'Donor', expectedUrl: '/personal-area' },
    { role: 'Admin', expectedUrl: '/admin' },
  ]) {
    it(`routes ${role} users to their default destination`, () => {
      completeLoginWithRole(role);

      expect(router.navigateByUrl).toHaveBeenCalledWith(expectedUrl);
    });
  }

  it('returns to a requested authenticated route after login', () => {
    returnUrl = '/personal-area?tab=history';
    completeLoginWithRole('User');

    expect(router.navigateByUrl).toHaveBeenCalledWith(returnUrl);
  });

  it('makes the registration link carry the interrupted task forward', () => {
    returnUrl = '/payment';
    fixture.detectChanges();

    const registerLink = fixture.nativeElement.querySelector('.auth-footer a') as HTMLAnchorElement;
    expect(registerLink.getAttribute('href')).toContain('/register?returnUrl=%2Fpayment');
  });

  it('does not return a non-admin user to an admin-only route', () => {
    returnUrl = '/admin/presents';
    completeLoginWithRole('User');

    expect(router.navigateByUrl).toHaveBeenCalledWith('/catalog');
  });

  it('does not navigate to an external return URL', () => {
    returnUrl = '//example.com/path';
    completeLoginWithRole('User');

    expect(router.navigateByUrl).toHaveBeenCalledWith('/catalog');
  });

  it('clears a token with an unsupported role without navigating to a protected page', () => {
    userService.login.and.returnValue(
      of({ success: true, message: createToken('Other'), data: null }),
    );
    setValidCredentials();

    component.login();

    expect(authService.getToken()).toBeNull();
    expect(router.navigateByUrl).not.toHaveBeenCalled();
    expect(component.errorMessage).toBe('auth.unsupportedRole');
  });

  it('clears the existing token and shows a friendly message for unauthorized login', () => {
    authService.setToken(createToken('User'));
    localStorage.setItem('unrelatedSessionValue', 'preserve-me');
    userService.login.and.returnValue(
      throwError(() => new HttpErrorResponse({
        status: 401,
        statusText: 'Unauthorized',
        error: { message: 'Raw backend detail' },
      })),
    );
    setValidCredentials();

    component.login();

    expect(authService.getToken()).toBeNull();
    expect(localStorage.getItem('authToken')).toBeNull();
    expect(localStorage.getItem('unrelatedSessionValue')).toBe('preserve-me');
    expect(component.errorMessage).toBe('auth.invalidCredentials');
  });

  it('clears the existing token when the login response reports failure', () => {
    authService.setToken(createToken('User'));
    userService.login.and.returnValue(
      of({ success: false, message: 'Raw backend detail', data: null }),
    );
    setValidCredentials();

    component.login();

    expect(authService.getToken()).toBeNull();
    expect(localStorage.getItem('authToken')).toBeNull();
    expect(component.errorMessage).toBe('auth.loginFailed');
  });

  function completeLoginWithRole(role: string): void {
    userService.login.and.returnValue(
      of({ success: true, message: createToken(role), data: null }),
    );
    setValidCredentials();
    component.login();
  }

  function setValidCredentials(): void {
    component.loginForm.setValue({ email: 'user@example.com', password: 'secret123' });
  }
});
