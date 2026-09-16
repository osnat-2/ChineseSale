import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { presentModel } from '../../models/present';
import { PresentService } from '../../services/presentService/present-service';

@Component({
  selector: 'app-catalog-detail',
  imports: [CommonModule, RouterLink],
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
      this.errorMessage = 'This present could not be found.';
      return;
    }

    this.presentService.getPresentById(id).subscribe({
      next: (result) => {
        this.present = result.success ? result.data?.[0] ?? null : null;
        this.errorMessage = this.present ? '' : result.message || 'This present could not be found.';
        this.loading = false;
      },
      error: () => {
        this.errorMessage = 'Unable to load this present. Please try again.';
        this.loading = false;
      }
    });
  }

  imageFallback(event: Event): void {
    (event.target as HTMLImageElement).src = 'assets/images/present-placeholder.svg';
  }
}