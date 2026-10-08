import { Routes } from '@angular/router';
import { guestGuard } from '../../core/guards/auth.guard';
import { ForgotPasswordComponent } from './pages/forgot-password/forgot-password.component';
import { LoginComponent } from './pages/login/login.component';
import { ResetPasswordComponent } from './pages/reset-password/reset-password.component';

export const AUTH_ROUTES: Routes = [
  { path: 'connexion', component: LoginComponent, title: 'Connexion', canActivate: [guestGuard] },
  { path: 'mot-de-passe-oublie', component: ForgotPasswordComponent, title: 'Mot de passe oublié', canActivate: [guestGuard] },
  { path: 'reinitialiser', component: ResetPasswordComponent, title: 'Réinitialiser le mot de passe', canActivate: [guestGuard] },
  { path: '', pathMatch: 'full', redirectTo: 'connexion' },
];
