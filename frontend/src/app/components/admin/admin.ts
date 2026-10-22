import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslatePipe } from '../../i18n/translate.pipe';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-admin',
  imports: [CommonModule, TranslatePipe, RouterLink],
  templateUrl: './admin.html',
  styleUrl: './admin.scss',
})
export class Admin {
  readonly sections = [
    { title: 'admin.presentTitle', path: '/admin/presents', description: 'admin.presentDescription' },
    // { title: 'admin.donorTitle', path: '/admin/donors', description: 'admin.donorDescription' },
    { title: 'admin.categoryTitle', path: '/admin/categories', description: 'admin.categoryDescription' },
    { title: 'admin.roleTitle', path: '/admin/roles', description: 'admin.roleDescription' },
    { title: 'admin.userTitle', path: '/admin/users', description: 'admin.userDescription' },
    { title: 'admin.lotteryTitle', path: '/admin/winners', description: 'admin.lotteryDescription' },
    { title: 'admin.purchaseTitle', path: '/admin/purchases', description: 'admin.purchaseDescription' }
  ];
}
