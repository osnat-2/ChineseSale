import { TestBed } from '@angular/core/testing';
import { AuthService } from './auth-service';

function createToken(claims: Record<string, unknown>): string {
  const encode = (value: string) => btoa(value).replace(/=/g, '').replace(/\+/g, '-').replace(/\//g, '_');
  return `${encode('{"alg":"none"}')}.${encode(JSON.stringify(claims))}.signature`;
}

describe('AuthService', () => {
  let service: AuthService;

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({});
    service = TestBed.inject(AuthService);
  });

  it('stores a valid token and reads its claims', () => {
    const token = createToken({ sub: '7', role: 'Admin', exp: Math.floor(Date.now() / 1000) + 300 });

    service.setToken(token);

    expect(service.getToken()).toBe(token);
    expect(service.isAuthenticated()).toBeTrue();
    expect(service.isAdmin()).toBeTrue();
  });

  it('rejects expired tokens and clears them on logout', () => {
    const token = createToken({ sub: '7', role: 'User', exp: Math.floor(Date.now() / 1000) - 1 });

    service.setToken(token);

    expect(service.getToken()).toBeNull();
    expect(service.isAuthenticated()).toBeFalse();

    service.logout();
    expect(localStorage.getItem('authToken')).toBeNull();
  });
});