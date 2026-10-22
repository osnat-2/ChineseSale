import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { TranslatePipe } from '../../i18n/translate.pipe';
import { UserService } from '../../services/userService/user-service';
import { HttpService } from '../../services/httpService/http-service';
import { AuthService } from '../../services/authService/auth-service';

@Component({
  selector: 'app-login',
  imports: [CommonModule, RouterLink, ReactiveFormsModule, TranslatePipe],
  templateUrl: './login.html',
  styleUrl: './login.scss',
})
export class Login {
  httpService: HttpService = inject(HttpService);
  userService: UserService = inject(UserService);
  router: Router = inject(Router);
  route: ActivatedRoute = inject(ActivatedRoute);
  fb: FormBuilder = inject(FormBuilder);
  authService: AuthService = inject(AuthService);

  loginForm!: FormGroup;
  errorMessage: string = '';
  isLoading: boolean = false;

  ngOnInit(): void {
    this.initializeForm();
  }

  initializeForm(): void {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(4)]],
    });
  }

  get returnUrl(): string | null {
    return this.route.snapshot.queryParamMap.get('returnUrl');
  }

  login(): void {
    console.log("Login function");

    if (this.loginForm.invalid) {
      this.errorMessage = 'auth.invalidForm';
      return;
    }

    this.errorMessage = '';
    this.isLoading = true;

    const { email, password } = this.loginForm.value;
    this.userService.login({ email, password }).subscribe({
      next: (response) => {
        this.isLoading = false;
        if (response.success !== true || typeof response.message !== 'string') {
          this.authService.clearToken();
          this.errorMessage = 'auth.loginFailed';
          return;
        }
        this.authService.setToken(response.message);
        if (!this.authService.isAuthenticated()) {
          this.authService.clearToken();
          this.errorMessage = 'auth.invalidToken';
          return;
        }
        const defaultUrl = this.getDefaultUrl();
        if (!defaultUrl) {
          return;
        }
        const returnUrl = this.getSafeReturnUrl(this.returnUrl);
        void this.router.navigateByUrl(returnUrl ?? defaultUrl);
      },
      error: (error) => {
        this.isLoading = false;
        this.authService.clearToken();
        this.errorMessage =
          error?.status === 401 ? 'auth.invalidCredentials' : 'auth.loginTryAgain';
      },
    });
  }

  private getDefaultUrl(): string | null {
    if (this.authService.isAdmin()) {
      return '/admin';
    }
    if (this.authService.hasRole('Donor')) {
      return '/personal-area';
    }
    if (this.authService.hasRole('User')) {
      return '/catalog';
    }

    this.authService.clearToken();
    this.errorMessage = 'auth.unsupportedRole';
    return null;
  }

  private getSafeReturnUrl(returnUrl: string | null): string | null {
    if (!returnUrl || !returnUrl.startsWith('/') || returnUrl.startsWith('//') || returnUrl.includes('\\')) {
      return null;
    }

    try {
      const urlTree = this.router.parseUrl(returnUrl);
      const segments = urlTree.root.children['primary']?.segments.map((segment) => segment.path) ?? [];
      const firstSegment = segments[0]?.toLowerCase();
      const requiresAdmin =
        firstSegment === 'admin' ||
        firstSegment === 'present' ||
        firstSegment === 'donor' ||
        firstSegment === 'winner';

      if (
        firstSegment === 'login' ||
        firstSegment === 'register' ||
        (requiresAdmin && !this.authService.isAdmin())
      ) {
        return null;
      }

      return returnUrl;
    } catch {
      return null;
    }
  }
}