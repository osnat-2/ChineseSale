import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { presentModel } from '../../../models/present';
import { Result } from '../../../models/result';
import { winnerModel } from '../../../models/winner';
import { TranslatePipe } from '../../../i18n/translate.pipe';
import { LotteryService } from '../../../services/lotteryService/lottery-service';
import { PresentService } from '../../../services/presentService/present-service';

@Component({
  selector: 'app-winner-management',
  imports: [CommonModule, FormsModule, TranslatePipe],
  templateUrl: './winner-management.html',
  styleUrl: './winner-management.scss',
})
export class WinnerManagement implements OnInit {
  presents: presentModel[] = [];
  selectedPresentId: number | null = null;
  latestWinner: winnerModel | null = null;
  readonly completedPresentIds = new Set<number>();
  loading = false;
  drawing = false;
  loadError = '';
  drawError = '';
  successMessage = '';

  constructor(
    private readonly presentService: PresentService,
    private readonly lotteryService: LotteryService
  ) {}

  ngOnInit(): void {
    this.loadPresents();
  }

  createLottery(): void {
    const presentId = this.selectedPresentId;
    if (presentId === null || this.drawing || this.completedPresentIds.has(presentId)) {
      return;
    }

    this.drawing = true;
    this.drawError = '';
    this.successMessage = '';
    this.lotteryService.drawWinner(presentId).subscribe({
      next: (response: Result<winnerModel>) => {
        const winner = response.data?.[0];
        if (!response.success || !winner) {
          this.drawError = this.readDrawError(response.message);
          this.drawing = false;
          return;
        }

        this.latestWinner = winner;
        this.completedPresentIds.add(presentId);
        this.successMessage = 'winner.drawSuccess';
        this.drawing = false;
      },
      error: (error: unknown) => {
        this.drawError = this.readDrawError(this.readServerMessage(error));
        this.drawing = false;
      },
    });
  }

  isSelectedPresentDrawn(): boolean {
    return this.selectedPresentId !== null && this.completedPresentIds.has(this.selectedPresentId);
  }

  private loadPresents(): void {
    this.loading = true;
    this.loadError = '';
    this.presentService.getAllPresents().subscribe({
      next: (response) => {
        if (!response.success) {
          this.loadError = response.message || 'winner.loadFailed';
          this.presents = [];
        } else {
          this.presents = response.data ?? [];
        }
        this.loading = false;
      },
      error: (error: unknown) => {
        this.loadError = this.readServerMessage(error) || 'winner.loadFailed';
        this.loading = false;
      },
    });
  }

  private readDrawError(message?: string | null): string {
    const normalized = message?.toLowerCase() ?? '';
    if (normalized.includes('no paid cards')) {
      return 'winner.noPaidCards';
    }
    if (
      normalized.includes('already') ||
      normalized.includes('made out') ||
      normalized.includes('drawn') ||
      normalized.includes('completed')
    ) {
      return 'winner.alreadyDrawn';
    }
    return message || 'winner.drawFailed';
  }

  private readServerMessage(error: unknown): string | null {
    if (typeof error !== 'object' || error === null || !('error' in error)) {
      return null;
    }

    const body = error.error;
    return typeof body === 'object' && body !== null && 'message' in body &&
      typeof body.message === 'string'
      ? body.message
      : null;
  }
}
