import { CommonModule } from '@angular/common';
import { TranslatePipe } from '../../../i18n/translate.pipe';
import { Component, OnInit } from '@angular/core';
import { winnerModel } from '../../../models/winner';
import { WinnerService } from '../../../services/winnerService/winner-service';

@Component({
  selector: 'app-winner-management',
  imports: [CommonModule, TranslatePipe],
  templateUrl: './winner-management.html',
  styleUrl: './winner-management.scss',
})
export class WinnerManagement {
  latestWinner: winnerModel | null = null;

  constructor(private readonly winnerService: WinnerService) {}

  ngOnInit(): void {
    this.latestWinner = this.winnerService.getLatestWinner();
  }
}
