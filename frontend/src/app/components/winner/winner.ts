import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { presentModel } from '../../models/present';
import { winnerModel } from '../../models/winner';
import { PresentService } from '../../services/presentService/present-service';
import { WinnerService } from '../../services/winnerService/winner-service';
import { RouterLink } from '@angular/router';

// Raffle Component - Draw winners and display raffle results
@Component({
  selector: 'app-winner',
  imports: [CommonModule, RouterLink],
  templateUrl: './winner.html',
  styleUrl: './winner.scss',
})
export class Winner {
  
}