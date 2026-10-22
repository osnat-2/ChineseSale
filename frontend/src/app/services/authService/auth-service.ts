import { computed, Injectable, signal } from '@angular/core';

export interface AuthTokenClaims {
  sub?: string;
  name?: string;
  unique_name?: string;
  email?: string;
  phone_number?: string;
  mobile_phone?: string;
  phone?: string;
  role?: string | string[];
  exp?: number;
  [claim: string]: unknown;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly tokenKey = 'authToken';
  private readonly tokenSignal = signal<string>(this.readToken());
  readonly isAuthenticated = computed(() => this.getClaims() !== null);

  getToken(): string {
    const token = this.tokenSignal();
    return this.isValidToken(token) ? token : '';
  }

  setToken(token: string): void {
    if (!this.isValidToken(token)) {
      this.clearToken();
      return;
    }
    this.writeToken(token);
    this.tokenSignal.set(token);
  }

  clearToken(): void {
    if (this.isBrowser()) {
      localStorage.removeItem(this.tokenKey);
    }
    this.tokenSignal.set('');
  }

  logout(): void {
    this.clearToken();
  }

  getClaims(): AuthTokenClaims | null {
    const token = this.tokenSignal();
    if (!this.isValidToken(token)) {
      if (token) {
        this.clearToken();
      }
      return null;
    }
    try {
      return JSON.parse(this.decodeBase64Url(token!.split('.')[1])) as AuthTokenClaims;
    } catch {
      this.clearToken();
      return null;
    }
  }

  hasRole(role: string): boolean {
    const claimRole = this.getClaims()?.role;
    return Array.isArray(claimRole)
      ? claimRole.some((value) => value.toLowerCase() === role.toLowerCase())
      : typeof claimRole === 'string' && claimRole.toLowerCase() === role.toLowerCase();
  }

  isAdmin(): boolean {
    return this.hasRole('Admin');
  }

  isUser(): boolean {
    return this.hasRole('User');
  }
  
  isDonor(): boolean {
    return this.hasRole('Donor');
  }

  private isValidToken(token: string | null): boolean {
    if (!token) {
      return false;
    }
    try {
      const claims = JSON.parse(this.decodeBase64Url(token.split('.')[1])) as AuthTokenClaims;
      return typeof claims.exp !== 'number' || claims.exp * 1000 > Date.now();
    } catch {
      return false;
    }
  }

  private readToken(): string {
    return this.isBrowser() ? localStorage.getItem(this.tokenKey) || '' : '';
  }

  private writeToken(token: string): void {
    if (this.isBrowser()) {
      localStorage.setItem(this.tokenKey, token);
    }
  }

  private isBrowser(): boolean {
    return typeof window !== 'undefined' && typeof localStorage !== 'undefined';
  }

  private decodeBase64Url(value: string): string {
    const base64 = value.replace(/-/g, '+').replace(/_/g, '/');
    const padded = base64.padEnd(Math.ceil(base64.length / 4) * 4, '=');
    const binary = atob(padded);
    const bytes = Uint8Array.from(binary, character => character.charCodeAt(0));
    return new TextDecoder('utf-8', { fatal: true }).decode(bytes);
  }
}