import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { TranslatePipe } from '../../i18n/translate.pipe';
import { UserService } from '../../services/userService/user-service';
import { HttpService } from '../../services/httpService/http-service';
import { AuthService } from '../../services/authService/auth-service';

@Component({
  selector: 'app-login',
  imports: [CommonModule, ReactiveFormsModule, RouterLink, TranslatePipe],
  templateUrl: './login.html',
  styleUrl: './login.scss',
})
export class Login implements OnInit {
  httpService: HttpService = inject(HttpService);
  userService: UserService = inject(UserService);
  router: Router = inject(Router);
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

  login(): void {
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
          this.errorMessage = response.message || 'auth.loginFailed';
          return;
        }
        this.authService.setToken(response.message);
        if (!this.authService.isAuthenticated()) {
          this.errorMessage = 'auth.invalidToken';
          return;
        }
        if (this.authService.hasRole('User'))
          this.router.navigate(['/catalog']);
        if (this.authService.hasRole('Admin'))
          this.router.navigate(['/admin']);
      },
      error: (error) => {
        this.isLoading = false;
        this.errorMessage =
          error?.error?.message || error?.message || 'auth.loginTryAgain';
      },
    });
  }
}