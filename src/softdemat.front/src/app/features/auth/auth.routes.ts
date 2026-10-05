import { Routes } from '@angular/router';
import { guestGuard } from '../../core/guards/auth.guard';
import { LoginComponent } from './pages/login/login.component';

export const AUTH_ROUTES: Routes = [
  { path: 'connexion', component: LoginComponent, title: 'Connexion', canActivate: [guestGuard] },
  { path: '', pathMatch: 'full', redirectTo: 'connexion' },
];
