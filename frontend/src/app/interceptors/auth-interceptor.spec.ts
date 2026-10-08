import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { AuthService } from '../services/authService/auth-service';
import { authInterceptor } from './auth-interceptor';

function createToken(): string {
  const encode = (value: string) =>
    btoa(value).replace(/=/g, '').replace(/\+/g, '-').replace(/\//g, '_');
  return `${encode('{"alg":"none"}')}.${encode(
    JSON.stringify({ sub: '7', role: 'Admin', exp: Math.floor(Date.now() / 1000) + 300 }),
  )}.signature`;
}

describe('authInterceptor', () => {
  let httpClient: HttpClient;
  let httpTestingController: HttpTestingController;
  let authService: AuthService;

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({
      providers: [
        provideRouter([]),
        provideHttpClient(withInterceptors([authInterceptor])),
        provideHttpClientTesting(),
      ],
    });
    httpClient = TestBed.inject(HttpClient);
    httpTestingController = TestBed.inject(HttpTestingController);
    authService = TestBed.inject(AuthService);
    authService.setToken(createToken());
  });

  afterEach(() => httpTestingController.verify());

  it('attaches the token to authenticated API requests', () => {
    httpClient.get('https://localhost:7142/api/present/getAllPresents').subscribe();

    const request = httpTestingController.expectOne(
      'https://localhost:7142/api/present/getAllPresents',
    );
    expect(request.request.headers.get('Authorization')).toBe(`Bearer ${authService.getToken()}`);
    request.flush({});
  });

  it('does not attach the token to login and registration requests', () => {
    httpClient.post('https://localhost:7142/api/auth/login', {}).subscribe();
    httpClient.post('https://localhost:7142/api/auth/register', {}).subscribe();

    for (const url of [
      'https://localhost:7142/api/auth/login',
      'https://localhost:7142/api/auth/register',
    ]) {
      const request = httpTestingController.expectOne(url);
      expect(request.request.headers.has('Authorization')).toBeFalse();
      request.flush({});
    }
  });

  it('does not send the token to non-API requests', () => {
    httpClient.get('https://example.com/api/data').subscribe();

    const request = httpTestingController.expectOne('https://example.com/api/data');
    expect(request.request.headers.has('Authorization')).toBeFalse();
    request.flush({});
  });

  it('clears the token and redirects after an unauthorized authenticated API request', () => {
    const router = TestBed.inject(Router);
    const navigateSpy = spyOn(router, 'navigate').and.resolveTo(true);
    httpClient.get('https://localhost:7142/api/present/getAllPresents').subscribe({
      error: () => undefined,
    });

    httpTestingController
      .expectOne('https://localhost:7142/api/present/getAllPresents')
      .flush({}, { status: 401, statusText: 'Unauthorized' });

    expect(authService.getToken()).toBeNull();
    expect(navigateSpy).toHaveBeenCalledWith(['/login']);
  });
});
