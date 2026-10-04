import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { TranslatePipe } from '../../i18n/translate.pipe';
import { presentModel } from '../../models/present';
import { PresentService } from '../../services/presentService/present-service';
import { LocalizedCurrencyPipe } from '../../i18n/localized-currency.pipe';
import { LocalizedNumberPipe } from '../../i18n/localized-number.pipe';

@Component({
  selector: 'app-catalog-detail',
  imports: [CommonModule, RouterLink, TranslatePipe, LocalizedCurrencyPipe, LocalizedNumberPipe],
  templateUrl: './catalog-detail.html',
  styleUrl: './catalog-detail.scss'
})
export class CatalogDetail implements OnInit {
  present: presentModel | null = null;
  loading = true;
  errorMessage = '';

  private readonly route = inject(ActivatedRoute);
  private readonly presentService = inject(PresentService);

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (!Number.isInteger(id) || id <= 0) {
      this.loading = false;
      this.errorMessage = 'detail.notFound';
      return;
    }

    this.presentService.getPresentById(id).subscribe({
      next: (result) => {
        this.present = result.success ? result.data?.[0] ?? null : null;
        this.errorMessage = this.present ? '' : result.message || 'detail.notFound';
        this.loading = false;
      },
      error: () => {
        this.errorMessage = 'detail.loadFailed';
        this.loading = false;
      }
    });
  }

  imageFallback(event: Event): void {
    (event.target as HTMLImageElement).src = 'assets/images/present-placeholder.svg';
  }
}