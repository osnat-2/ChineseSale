import { Routes } from '@angular/router';
import { Home } from './components/home/home';
import { Login } from './components/login/login';
import { Register } from './components/register/register';
import { AdminGuard } from './guards/admin.guard';
import { AuthGuard } from './guards/auth.guard';
import { Catalog } from './components/catalog/catalog';

export const routes: Routes = [
  { path: '', redirectTo: 'home', pathMatch: 'full' },
  { path: 'home', component: Home },
  { path: 'catalog', component: Catalog },
  { path: 'catalog/:id', loadComponent: () => import('./components/catalog-detail/catalog-detail').then((module) => module.CatalogDetail) },
  { path: 'present', loadComponent: () => import('./components/present/present').then((module) => module.Present), canActivate: [AdminGuard] },
  { path: 'donor', loadComponent: () => import('./components/donor/donor').then((module) => module.Donor), canActivate: [AdminGuard] },
  { path: 'card', loadComponent: () => import('./components/card/card').then((module) => module.Card), canActivate: [AuthGuard] },
  { path: 'payment', loadComponent: () => import('./components/payment/payment').then((module) => module.Payment), canActivate: [AuthGuard] },
  { path: 'personal-area', loadComponent: () => import('./components/personal-area/personal-area').then((module) => module.PersonalArea), canActivate: [AuthGuard] },
  { path: 'winner', redirectTo: 'admin/winners', pathMatch: 'full' },
  { path: 'admin', loadComponent: () => import('./components/admin/admin').then((module) => module.Admin), canActivate: [AdminGuard] },
  { path: 'admin/donors', loadComponent: () => import('./components/management/donor-management/donor-management').then((module) => module.DonorManagement), canActivate: [AdminGuard] },
  { path: 'admin/presents', loadComponent: () => import('./components/management/present-management/present-management').then((module) => module.PresentManagement), canActivate: [AdminGuard] },
  { path: 'admin/cards', redirectTo: 'admin/purchases', pathMatch: 'full' },
  { path: 'admin/winners', loadComponent: () => import('./components/management/winner-management/winner-management').then((module) => module.WinnerManagement), canActivate: [AdminGuard] },
  { path: 'admin/purchases', loadComponent: () => import('./components/management/purchases-management/purchases-management').then((module) => module.PurchasesManagement), canActivate: [AdminGuard] },
  { path: 'admin/categories', loadComponent: () => import('./components/management/category-management/category-management').then((module) => module.CategoryManagement), canActivate: [AdminGuard] },
  { path: 'admin/roles', loadComponent: () => import('./components/management/role-management/role-management').then((module) => module.RoleManagement), canActivate: [AdminGuard] },
  { path: 'admin/users', loadComponent: () => import('./components/management/user-management/user-management').then((module) => module.UserManagement), canActivate: [AdminGuard] },
  { path: 'login', component: Login },
  { path: 'register', component: Register }
];