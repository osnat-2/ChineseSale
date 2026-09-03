import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { presentModel } from '../../models/present';
import { PresentService } from '../../services/presentService/present-service'
// import { PresentService } from '../../services/presentService/present-service';
import { RouterLink, Router } from '@angular/router';

// Gift Purchase Component - Display gifts for selection and purchase
@Component({
  selector: 'app-card',
  imports: [CommonModule, RouterLink],
  templateUrl: './card.html',
  styleUrl: './card.scss',
})
export class Card {
  
}