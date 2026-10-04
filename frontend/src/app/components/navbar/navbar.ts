import { Component, computed, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { CommonModule } from '@angular/common';
import { TranslatePipe } from '../../i18n/translate.pipe';
import { AuthService } from '../../services/authService/auth-service';
import { Language, LanguageService } from '../../i18n/language.service';

@Component({
  selector: 'app-navbar',
  imports: [RouterLink, RouterLinkActive, CommonModule, TranslatePipe],
  templateUrl: './navbar.html',
  styleUrl: './navbar.scss',
})
export class Navbar {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  readonly language = inject(LanguageService);
  readonly isAuthenticated = this.auth.isAuthenticated;
  readonly isAdmin = computed(() => this.auth.isAdmin());

  setLanguage(language: Language): void {
    this.language.setLanguage(language);
  }

  logout(): void {
    this.auth.logout();
    void this.router.navigate(['/home']);
  }
}