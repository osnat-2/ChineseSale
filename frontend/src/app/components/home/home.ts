import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { TranslatePipe } from '../../i18n/translate.pipe';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../services/authService/auth-service';

@Component({
  selector: 'app-home',
  imports: [CommonModule, RouterLink, TranslatePipe],
  templateUrl: './home.html',
  styleUrl: './home.scss',
})
export class Home {
  private readonly auth = inject(AuthService);
  readonly isAuthenticated = this.auth.isAuthenticated;

  isAdmin(): boolean {
    return this.auth.isAdmin();
  }
}