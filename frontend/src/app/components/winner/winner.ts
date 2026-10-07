import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslatePipe } from '../../i18n/translate.pipe';
import { presentModel } from '../../models/present';
import { winnerModel } from '../../models/winner';
import { PresentService } from '../../services/presentService/present-service';
import { WinnerService } from '../../services/winnerService/winner-service';
import { Result } from '../../models/result';

// Raffle Component - Draw winners and display raffle results
@Component({
  selector: 'app-winner',
  imports: [CommonModule, TranslatePipe],
  templateUrl: './winner.html',
  styleUrl: './winner.scss',
})
export class Winner {
  presents: presentModel[] = [];
  selectedPresentId: number | null = null;
  latestWinner: winnerModel | null = null;
  loading = false;
  drawing = false;
  errorMessage = '';
  successMessage = '';

  constructor(
    private readonly presentService: PresentService,
    private readonly winnerService: WinnerService
  ) {}

  ngOnInit(): void {
    this.loadPresents();
  }

  loadPresents(): void {
    this.loading = true;
    this.errorMessage = '';
    this.presentService.getAllPresents().subscribe({
      next: (response) => {
        this.presents = response.data ?? [];
        this.loading = false;
      },
      error: () => {
        this.errorMessage = 'winner.loadFailed';
        this.loading = false;
      }
    });
  }

  isDrawn(presentId: number): boolean {
    return this.winnerService.isDrawn(presentId);
  }

  drawSelected(): void {
    if (this.selectedPresentId === null || this.drawing || this.isDrawn(this.selectedPresentId)) {
      return;
    }

    this.drawing = true;
    this.errorMessage = '';
    this.successMessage = '';
    this.winnerService.drawWinner(this.selectedPresentId).subscribe({
      next: (response: Result<winnerModel>) => {
        const winner = response.data?.[0];
        if (!response.success || !winner) {
          this.errorMessage = this.readDrawError(response.message);
          this.drawing = false;
          return;
        }

        this.winnerService.markDrawn(this.selectedPresentId!, winner);
        this.latestWinner = winner;
        this.successMessage = 'winner.drawSuccess';
        this.drawing = false;
      },
      error: (error) => {
        this.errorMessage = this.readDrawError(error?.error?.message);
        this.drawing = false;
      }
    });
  }

  readDrawError(message?: string | null): string {
    const normalized = message?.toLowerCase() ?? '';
    if (normalized.includes('no paid cards')) {
      return 'winner.noPaidCards';
    }
    if (normalized.includes('already') || normalized.includes('made out') || normalized.includes('drawn')) {
      return 'winner.alreadyDrawn';
    }
    return message || 'winner.drawFailed';
  }
}