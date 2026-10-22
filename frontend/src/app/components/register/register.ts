import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { TranslatePipe } from '../../i18n/translate.pipe';
import { UserService } from '../../services/userService/user-service';
import { userDtoModel } from '../../models/ModelsDto/userDto';
import { hasApiResponseMessage } from '../../models/api-message';

@Component({
  selector: 'app-register',
  imports: [CommonModule, ReactiveFormsModule, RouterLink, TranslatePipe],
  templateUrl: './register.html',
  styleUrl: './register.scss',
})
export class Register implements OnInit {
  userService: UserService = inject(UserService);
  router: Router = inject(Router);
  route: ActivatedRoute = inject(ActivatedRoute);
  fb: FormBuilder = inject(FormBuilder);

  registerForm!: FormGroup;
  errorMessage: string = '';
  successMessage: string = '';
  isSubmitting: boolean = false;

  ngOnInit(): void {
    this.initializeForm();
  }

  initializeForm(): void {
    this.registerForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(2)]],
      email: ['', [Validators.required, Validators.email]],
      phone: ['', [Validators.required, Validators.minLength(10)]],
      password: ['', [Validators.required, Validators.minLength(4)]],
    });
  }

  register(): void {
    if (this.registerForm.invalid) {
      this.errorMessage = 'auth.invalidForm';
      return;
    }

    this.errorMessage = '';
    this.successMessage = '';
    this.isSubmitting = true;

    const user: userDtoModel = this.registerForm.value;
    this.userService.register(user).subscribe({
      next: (response) => {
        this.isSubmitting = false;
        if (response.success !== true) {
          this.errorMessage = 'auth.registerFailed';
          return;
        }
        this.successMessage = hasApiResponseMessage(response) ? '' : 'auth.registerSuccess';
        setTimeout(() => {
          void this.router.navigate(['/login'], {
            queryParams: this.returnUrl ? { returnUrl: this.returnUrl } : undefined,
          });
        }, 2000);
      },
      error: () => {
        this.isSubmitting = false;
        this.errorMessage = 'auth.registerTryAgain';
      },
    });
  }

  get returnUrl(): string | null {
    return this.route.snapshot.queryParamMap.get('returnUrl');
  }
}