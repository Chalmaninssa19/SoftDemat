import { Routes } from '@angular/router';
import { UserListComponent } from './pages/user-list/user-list.component';

export const USERS_ROUTES: Routes = [
  { path: '', component: UserListComponent, title: 'Utilisateurs' },
];
