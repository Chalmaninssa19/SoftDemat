import { Routes } from '@angular/router';
import { adminGuard, authGuard } from './core/guards/auth.guard';
import { ShellComponent } from './core/layout/shell.component';

export const routes: Routes = [
  {
    path: 'auth',
    loadChildren: () => import('./features/auth/auth.routes').then((module) => module.AUTH_ROUTES),
  },
  {
    path: '',
    component: ShellComponent,
    canActivate: [authGuard],
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'dematerialisation' },
      {
        path: 'profil',
        loadChildren: () => import('./features/profile/profile.routes').then((module) => module.PROFILE_ROUTES),
      },
      {
        path: 'utilisateurs',
        canActivate: [adminGuard],
        loadChildren: () => import('./features/users/users.routes').then((module) => module.USERS_ROUTES),
      },
      {
        path: 'parametrage',
        canActivate: [adminGuard],
        loadChildren: () => import('./features/settings/settings.routes').then((module) => module.SETTINGS_ROUTES),
      },
      {
        path: 'dematerialisation',
        loadChildren: () =>
          import('./features/dematerialization/dematerialization.routes').then((module) => module.DEMAT_ROUTES),
      },
      {
        path: 'historique',
        loadChildren: () => import('./features/history/history.routes').then((module) => module.HISTORY_ROUTES),
      },
    ],
  },
  { path: '**', redirectTo: '/dematerialisation' },
];
