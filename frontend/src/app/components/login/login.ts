import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { UserService } from '../../services/userService/user-service';
import { HttpService } from '../../services/httpService/http-service';
import { AuthService } from '../../services/authService/auth-service';

@Component({
  selector: 'app-login',
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
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
      this.errorMessage = 'Please fill in all required fields correctly.';
      return;
    }

    this.errorMessage = '';
    this.isLoading = true;

    const { email, password } = this.loginForm.value;
    this.userService.login(email, password).subscribe({
      next: (response) => {
        this.isLoading = false;
        if (response?.success !== true || typeof response?.message !== 'string') {
          this.errorMessage = response?.message || 'Login failed.';
          return;
        }
        this.authService.setToken(response.message);
        if (!this.authService.isAuthenticated()) {
          this.errorMessage = 'Login returned an invalid or expired token.';
          return;
        }
        this.router.navigate(['/home']);
      },
      error: (error) => {
        this.isLoading = false;
        this.errorMessage =
          error?.error?.message || error?.message || 'Login failed. Please try again.';
      },
    });
  }
}