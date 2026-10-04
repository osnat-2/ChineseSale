import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { TranslatePipe } from '../../i18n/translate.pipe';

@Component({
  selector: 'app-admin',
  imports: [CommonModule, RouterLink, TranslatePipe],
  templateUrl: './admin.html',
  styleUrl: './admin.scss',
})
export class Admin {
  readonly sections = [
    { title: 'admin.presentTitle', path: '/admin/presents', description: 'admin.presentDescription' },
    { title: 'admin.donorTitle', path: '/admin/donors', description: 'admin.donorDescription' },
    { title: 'admin.cardTitle', path: '/admin/cards', description: 'admin.cardDescription' },
    { title: 'admin.winnerTitle', path: '/admin/winners', description: 'admin.winnerDescription' },
    { title: 'admin.purchaseTitle', path: '/admin/purchases', description: 'admin.purchaseDescription' }
  ];
}
