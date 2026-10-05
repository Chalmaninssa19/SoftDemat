import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';

export const authInterceptor: HttpInterceptorFn = (request, next) => {
  const auth = inject(AuthService);
  const token = auth.accessToken();
  const authed = token ? request.clone({ setHeaders: { Authorization: `Bearer ${token}` } }) : request;
  return next(authed).pipe(
    catchError((error: unknown) => {
      const status = (error as { status?: number }).status;
      const refreshing = request.url.includes('/auth/login') || request.url.includes('/auth/refresh');
      if (status !== 401 || refreshing || !auth.storageRefreshAvailable()) {
        return throwError(() => error);
      }

      return auth.refresh().pipe(
        switchMap((data) => {
          if (!data) {
            return throwError(() => error);
          }
          return next(request.clone({ setHeaders: { Authorization: `Bearer ${data.accessToken}` } }));
        }),
      );
    }),
  );
};
