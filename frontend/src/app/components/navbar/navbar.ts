import { Component, computed, ElementRef, inject, ViewChild } from '@angular/core';
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
  readonly isUser = computed(() => this.auth.isUser());
  readonly isDonor = computed(() => this.auth.isDonor());
  menuOpen = false;

  @ViewChild('menuToggle') private menuToggle?: ElementRef<HTMLButtonElement>;

  toggleMenu(): void {
    this.menuOpen = !this.menuOpen;
  }

  closeMenu(): void {
    if (!this.menuOpen) {
      return;
    }
    this.menuOpen = false;
    queueMicrotask(() => this.menuToggle?.nativeElement.focus());
  }

  setLanguage(language: Language): void {
    this.language.setLanguage(language);
  }

  logout(): void {
    this.menuOpen = false;
    this.auth.logout();
    void this.router.navigate(['/home']);
  }
}