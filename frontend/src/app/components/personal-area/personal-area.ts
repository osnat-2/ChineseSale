import { CommonModule } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { TranslatePipe } from '../../i18n/translate.pipe';
import { cardModel } from '../../models/card';
import { presentModel } from '../../models/present';
import { AuthService } from '../../services/authService/auth-service';
import { CardService } from '../../services/cardService/card-service';
import { PresentService } from '../../services/presentService/present-service';

interface PersonalCardSummary {
  presentName: string;
  categoryName: string;
  count: number;
}

@Component({
  selector: 'app-personal-area',
  imports: [CommonModule, RouterLink, TranslatePipe],
  templateUrl: './personal-area.html',
  styleUrl: './personal-area.scss',
})
export class PersonalArea implements OnInit {
  profile = { name: '', email: '', phone: '' };
  paidCardSummaries: PersonalCardSummary[] = [];
  unpaidCardSummaries: PersonalCardSummary[] = [];
  donatedPresents: presentModel[] = [];
  loadingDonatedPresents = false;
  donatedPresentError = '';
  loading = true;
  errorMessage = '';
  private readonly authService = inject(AuthService);
  private readonly cardService = inject(CardService);
  private readonly presentService = inject(PresentService);

  ngOnInit(): void {
    const claims = this.authService.getClaims();
    this.profile = {
      name: this.readClaim(claims, ['unique_name', 'name', 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name']),
      email: this.readClaim(claims, ['email', 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress']),
      phone: this.readClaim(claims, ['phone_number', 'mobile_phone', 'phone', 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/mobilephone'])
    };

    this.loadCards();
    if (this.isDonor) {
      this.loadDonatedPresents();
    }
  }

  get isDonor(): boolean {
    return this.authService.hasRole('Donor');
  }

  get profileInitials(): string {
    return this.profile.name
      .trim() // remove leading/trailing whitespace
      .split(/\s+/) // split by whitespace
      .slice(0, 1) // take the first part
      .map((part) => Array.from(part)[0] ?? '') // take the first character of each part
      .join('') // join them together
      .toLocaleUpperCase(); //
  }

  loadCards(): void {
    this.loading = true;
    this.errorMessage = '';
    forkJoin({
      paid: this.cardService.getMyCards(true),
      unpaid: this.cardService.getMyCards(false),
    }).subscribe({
      next: ({ paid, unpaid }) => {
        if (!paid.success || !unpaid.success) {
          this.errorMessage = this.hasCardData ? 'personal.refreshFailed' : 'personal.loadFailed';
          this.loading = false;
          return;
        }

        this.paidCardSummaries = this.summarizeCards(paid.data ?? []);
        this.unpaidCardSummaries = this.summarizeCards(unpaid.data ?? []);
        this.errorMessage = '';
        this.loading = false;
      },
      error: () => {
        this.errorMessage = this.hasCardData ? 'personal.refreshFailed' : 'personal.loadFailed';
        this.loading = false;
      }
    });
  }

  loadDonatedPresents(): void {
    this.loadingDonatedPresents = true;
    this.donatedPresentError = '';
    this.presentService.getMyDonatedPresents().subscribe({
      next: (result) => {
        if (!result.success || !Array.isArray(result.data)) {
          this.donatedPresentError = this.donatedPresents.length
            ? 'personal.donatedRefreshFailed'
            : 'personal.donatedLoadFailed';
          this.loadingDonatedPresents = false;
          return;
        }

        this.donatedPresents = result.data;
        this.donatedPresentError = '';
        this.loadingDonatedPresents = false;
      },
      error: () => {
        this.donatedPresentError = this.donatedPresents.length
          ? 'personal.donatedRefreshFailed'
          : 'personal.donatedLoadFailed';
        this.loadingDonatedPresents = false;
      }
    });
  }

  get hasCardData(): boolean {
    return this.paidCardSummaries.length > 0 || this.unpaidCardSummaries.length > 0;
  }
  hasPaidCards(): boolean {
    return this.paidCardSummaries.length > 0;
  }

  hasUnpaidCards(): boolean {
    return this.unpaidCardSummaries.length > 0;
  }

  private summarizeCards(cards: cardModel[]): PersonalCardSummary[] {
    const summaries = new Map<number, PersonalCardSummary>();

    for (const card of cards) {
      const summary = summaries.get(card.presentId);
      if (summary) {
        summary.count++;
        continue;
      }

      const presentName = typeof card.present?.name === 'string' ? card.present.name.trim() : '';
      const categoryName = typeof card.present?.categoryName === 'string'
        ? card.present.categoryName.trim()
        : '';
      summaries.set(card.presentId, { presentName, categoryName, count: 1 });
    }

    return Array.from(summaries.values());
  }

  private readClaim(claims: ReturnType<AuthService['getClaims']>, names: readonly string[]): string {
    for (const name of names) {
      const value = claims?.[name];
      if (typeof value === 'string' && value.trim()) {
        return value.trim();
      }
    }
    return '';
  }
}