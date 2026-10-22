import { TestBed } from '@angular/core/testing';
import { provideRouter, Router, RouterStateSnapshot } from '@angular/router';
import { AuthGuard } from './auth.guard';
import { AuthService } from '../services/authService/auth-service';

describe('AuthGuard', () => {
  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({
      providers: [provideRouter([])],
    });
  });

  it('preserves the requested URL when redirecting unauthenticated users to login', () => {
    const result = TestBed.runInInjectionContext(() =>
      AuthGuard({} as never, { url: '/personal-area?tab=history' } as RouterStateSnapshot),
    );

    expect(result).toEqual(
      TestBed.inject(Router).parseUrl('/login?returnUrl=%2Fpersonal-area%3Ftab%3Dhistory'),
    );
  });

  it('allows an authenticated user to continue to a protected route', () => {
    const authService = TestBed.inject(AuthService);
    const encode = (value: string) =>
      btoa(value).replace(/=/g, '').replace(/\+/g, '-').replace(/\//g, '_');
    authService.setToken(
      `${encode('{"alg":"none"}')}.${encode(
        JSON.stringify({ sub: '7', role: 'User', exp: Math.floor(Date.now() / 1000) + 300 }),
      )}.signature`,
    );

    const result = TestBed.runInInjectionContext(() =>
      AuthGuard({} as never, { url: '/personal-area' } as RouterStateSnapshot),
    );

    expect(result).toBeTrue();
  });
});
