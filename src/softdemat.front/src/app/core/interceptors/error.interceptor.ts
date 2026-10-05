import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { NotificationService } from '../services/notification.service';
import { AuthService } from '../services/auth.service';

export const errorInterceptor: HttpInterceptorFn = (request, next) => {
  const notifications = inject(NotificationService);
  const auth = inject(AuthService);
  return next(request).pipe(
    catchError((error: unknown) => {
      const loginRequest = request.url.includes('/auth/login');
      if (error instanceof HttpErrorResponse && error.status !== 401 && !loginRequest) {
        if (error.status === 403) {
          notifications.error(auth.messageOf(error) || 'Accès interdit.');
        } else if (error.status >= 500) {
          notifications.error('Une erreur interne est survenue.');
        } else {
          notifications.error(auth.messageOf(error));
        }
      }
      return throwError(() => error);
    }),
  );
};
