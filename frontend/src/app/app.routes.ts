import { Routes } from '@angular/router';
import { Home } from './components/home/home';
import { Present } from './components/present/present';
import { Donor } from './components/donor/donor';
import { Card } from './components/card/card';
import { Payment } from './components/payment/payment';
import { Winner } from './components/winner/winner';
import { Login } from './components/login/login';
import { Register } from './components/register/register';
import { Admin } from './components/admin/admin';
import { DonorManagement } from './components/management/donor-management/donor-management';
import { PresentManagement } from './components/management/present-management/present-management';
import { CardManagement } from './components/management/card-management/card-management';
import { WinnerManagement } from './components/management/winner-management/winner-management';
import { PurchasesManagement } from './components/management/purchases-management/purchases-management';
import { AdminGuard } from './guards/admin.guard';
import { AuthGuard } from './guards/auth.guard';
import { Catalog } from './components/catalog/catalog';
import { CatalogDetail } from './components/catalog-detail/catalog-detail';
import { PersonalArea } from './components/personal-area/personal-area';

export const routes: Routes = [
  { path: '', redirectTo: 'home', pathMatch: 'full' },
  { path: 'home', component: Home },
  { path: 'catalog', component: Catalog },
  { path: 'catalog/:id', component: CatalogDetail },
  { path: 'present', component: Present, canActivate: [AdminGuard] },
  { path: 'donor', component: Donor, canActivate: [AdminGuard] },
  { path: 'card', component: Card, canActivate: [AuthGuard] },
  { path: 'payment', component: Payment, canActivate: [AuthGuard] },
  { path: 'personal-area', component: PersonalArea, canActivate: [AuthGuard] },
  { path: 'winner', component: Winner, canActivate: [AdminGuard] },
  { path: 'admin', component: Admin, canActivate: [AdminGuard] },
  { path: 'admin/donors', component: DonorManagement, canActivate: [AdminGuard] },
  { path: 'admin/presents', component: PresentManagement, canActivate: [AdminGuard] },
  { path: 'admin/cards', component: CardManagement, canActivate: [AdminGuard] },
  { path: 'admin/winners', component: WinnerManagement, canActivate: [AdminGuard] },
  { path: 'admin/purchases', component: PurchasesManagement, canActivate: [AdminGuard] },
  { path: 'login', component: Login },
  { path: 'register', component: Register }
];