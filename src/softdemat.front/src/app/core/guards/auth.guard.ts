import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const authGuard: CanActivateFn = (_route, state) => {
  const auth = inject(AuthService);
  const router = inject(Router);
  if (!auth.accessToken()) {
    return router.createUrlTree(['/auth/connexion']);
  }
  if (auth.mustChangePassword() && !state.url.startsWith('/profil')) {
    return router.createUrlTree(['/profil']);
  }
  return true;
};

export const adminGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);
  if (auth.session()?.role === 'Administrateur') {
    return true;
  }
  return router.createUrlTree(['/']);
};

export const guestGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);
  if (auth.accessToken()) {
    return router.createUrlTree(['/']);
  }
  return true;
};
