import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { presentModel } from '../../../models/present';
import { Result } from '../../../models/result';
import { lotteryModel } from '../../../models/lottery';
import { winnerModel } from '../../../models/winner';
import { TranslatePipe } from '../../../i18n/translate.pipe';
import { LotteryService } from '../../../services/lotteryService/lottery-service';
import { PresentService } from '../../../services/presentService/present-service';
import { readHttpErrorMessage } from '../../../models/api-message';
import { CdkTrapFocus } from '@angular/cdk/a11y';

@Component({
  selector: 'app-winner-management',
  imports: [CommonModule, FormsModule, RouterLink, TranslatePipe, CdkTrapFocus],
  templateUrl: './winner-management.html',
  styleUrl: './winner-management.scss',
})
export class WinnerManagement implements OnInit {
  presents: presentModel[] = [];
  lotteries: lotteryModel[] = [];
  selectedLotteryId: number | null = null;
  winnerSearchTerm = '';
  lotteryDate = '';
  editingLotteryId: number | null = null;
  latestWinners: winnerModel[] = [];
  loading = false;
  saving = false;
  processing = false;
  lotteryFormOpen = false;
  processConfirmationOpen = false;
  loadError = '';
  actionError = '';
  successMessage = '';

  constructor(
    private readonly presentService: PresentService,
    private readonly lotteryService: LotteryService
  ) {}

  ngOnInit(): void {
    this.loadPresents();
    this.loadLotteries();
  }

  selectedLottery(): lotteryModel | undefined {
    return this.lotteries.find((lottery) => lottery.id === this.selectedLotteryId);
  }

  get filteredWinners(): winnerModel[] {
    const query = this.winnerSearchTerm.trim().toLocaleLowerCase();
    return query
      ? this.latestWinners.filter((winner) =>
        [
          winner.presentId.toString(),
          winner.cardId.toString(),
          winner.lotteryId.toString(),
          this.presentFor(winner.presentId)?.name ?? '',
          this.presentFor(winner.presentId)?.categoryName ?? '',
        ].some((value) => value.toLocaleLowerCase().includes(query))
      )
      : this.latestWinners;
  }

  presentFor(presentId: number): presentModel | undefined {
    return this.presents.find((present) => present.id === presentId);
  }

  clearWinnerSearch(): void {
    this.winnerSearchTerm = '';
  }

  openAddLottery(): void {
    this.editingLotteryId = null;
    this.lotteryDate = new Date().toISOString().slice(0, 10);
    this.actionError = '';
    this.lotteryFormOpen = true;
  }

  openEditLottery(): void {
    const lottery = this.selectedLottery();
    if (!lottery || lottery.isMadeOut || this.saving || this.processing) {
      return;
    }
    this.editingLotteryId = lottery.id;
    this.lotteryDate = lottery.time;
    this.actionError = '';
    this.lotteryFormOpen = true;
  }

  closeLotteryForm(): void {
    if (!this.saving) {
      this.lotteryFormOpen = false;
    }
  }

  saveLottery(): void {
    const parsedDate = new Date(`${this.lotteryDate}T00:00:00.000Z`);
    if (
      !this.lotteryDate ||
      this.saving ||
      !/^\d{4}-\d{2}-\d{2}$/.test(this.lotteryDate) ||
      Number.isNaN(parsedDate.getTime()) ||
      parsedDate.toISOString().slice(0, 10) !== this.lotteryDate
    ) {
      this.actionError = 'lottery.dateRequired';
      return;
    }

    this.saving = true;
    this.actionError = '';
    this.successMessage = '';
    const editingId = this.editingLotteryId;
    const request = editingId === null
      ? this.lotteryService.addLottery({ time: this.lotteryDate })
      : this.lotteryService.updateLottery(editingId, { time: this.lotteryDate });

    request.subscribe({
      next: (response: Result<lotteryModel>) => {
        const savedLottery = response.data?.[0];
        if (!response.success || !savedLottery) {
          this.actionError = 'lottery.saveFailed';
          this.saving = false;
          return;
        }

        this.selectedLotteryId = savedLottery.id;
        this.lotteryFormOpen = false;
        this.latestWinners = [];
        this.successMessage = editingId === null ? 'lottery.addSuccess' : 'lottery.updateSuccess';
        this.saving = false;
        this.loadLotteries(savedLottery.id);
      },
      error: (error: unknown) => {
        this.actionError = readHttpErrorMessage(error) ?? 'lottery.saveFailed';
        this.saving = false;
      },
    });
  }

  selectLottery(): void {
    const lottery = this.selectedLottery();
    this.lotteryDate = lottery?.time ?? '';
    this.latestWinners = [];
    this.actionError = '';
    this.successMessage = '';
  }

  openProcessConfirmation(): void {
    const lottery = this.selectedLottery();
    if (!lottery || lottery.isMadeOut || this.processing || this.presents.length === 0) {
      return;
    }
    this.actionError = '';
    this.processConfirmationOpen = true;
  }

  cancelProcessConfirmation(): void {
    if (!this.processing) {
      this.processConfirmationOpen = false;
    }
  }

  processLottery(): void {
    const lottery = this.selectedLottery();
    if (!lottery || lottery.isMadeOut || this.processing) {
      return;
    }

    this.processing = true;
    this.actionError = '';
    this.successMessage = '';
    this.lotteryService.processLottery(lottery.id).subscribe({
      next: (response: Result<winnerModel>) => {
        const winners = response.data ?? [];
        const matchesPresentCounts = this.presents.every((present) =>
          winners.filter((winner) => winner.presentId === present.id).length >= present.quantity
        );
        const matchesAnnualLottery = winners.every((winner) => winner.lotteryId === lottery.id);
        if (!response.success || !matchesPresentCounts || !matchesAnnualLottery) {
          this.actionError = 'lottery.processFailed';
          this.processing = false;
          this.loadLotteries(lottery.id);
          return;
        }

        this.latestWinners = winners;
        this.successMessage = 'lottery.processSuccess';
        this.processConfirmationOpen = false;
        this.processing = false;
        this.loadLotteries(lottery.id);
      },
      error: (error: unknown) => {
        this.actionError = readHttpErrorMessage(error) ?? 'lottery.processFailed';
        this.processing = false;
        this.loadLotteries(lottery.id);
      },
    });
  }

  private loadPresents(): void {
    this.loading = true;
    this.loadError = '';
    this.presentService.getAllPresents().subscribe({
      next: (response) => {
        if (!response.success) {
          this.loadError = 'lottery.loadFailed';
          this.presents = [];
        } else {
          this.presents = response.data ?? [];
        }
        this.loading = false;
      },
      error: () => {
        this.loadError = 'lottery.loadFailed';
        this.loading = false;
      },
    });
  }

  private loadLotteries(preferredId?: number): void {
    this.loading = true;
    this.loadError = '';
    this.lotteryService.getLotteries().subscribe({
      next: (response) => {
        if (!response.success) {
          this.loadError = 'lottery.loadFailed';
          this.lotteries = [];
          this.selectedLotteryId = null;
        } else {
          this.lotteries = response.data ?? [];
          const preferredLottery = this.lotteries.find((lottery) => lottery.id === preferredId);
          const selectedLottery = this.lotteries.find((lottery) => lottery.id === this.selectedLotteryId);
          this.selectedLotteryId = (preferredLottery ?? selectedLottery ?? this.lotteries[0])?.id ?? null;
          this.lotteryDate = this.selectedLottery()?.time ?? '';
        }
        this.loading = false;
      },
      error: () => {
        this.loadError = 'lottery.loadFailed';
        this.lotteries = [];
        this.selectedLotteryId = null;
        this.loading = false;
      },
    });
  }
}
