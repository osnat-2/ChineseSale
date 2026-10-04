import { CommonModule } from '@angular/common';
import { TranslatePipe } from '../../../i18n/translate.pipe';
import { Component } from '@angular/core';

@Component({
  selector: 'app-donor-management',
  imports: [CommonModule, TranslatePipe],
  templateUrl: './donor-management.html',
  styleUrl: './donor-management.scss',
})
export class DonorManagement {
  readonly donors: Array<{ id: number; name: string; email: string; phone: string; isActive: boolean }> = [];
}
