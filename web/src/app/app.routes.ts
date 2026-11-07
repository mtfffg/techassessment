import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./presentation/home/home.component').then(module => module.HomeComponent),
    title: 'Bookstore',
  },
  { path: '**', redirectTo: '' },
];
