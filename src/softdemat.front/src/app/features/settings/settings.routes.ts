import { Routes } from '@angular/router';
import { MailSettingsComponent } from './pages/mail-settings/mail-settings.component';
import { SageSettingsComponent } from './pages/sage-settings/sage-settings.component';

export const SETTINGS_ROUTES: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'sage' },
  { path: 'sage', component: SageSettingsComponent, title: 'Connexion Sage' },
  { path: 'envoi', component: MailSettingsComponent, title: 'Paramètres d’envoi' },
];
