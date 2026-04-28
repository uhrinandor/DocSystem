import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';

import { AuthSessionService } from './auth-session.service';

const LOGIN_ENDPOINT = '/api/Auth/login';

export const authTokenInterceptor: HttpInterceptorFn = (req, next) => {
  const authSession = inject(AuthSessionService);
  const router = inject(Router);
  const token = authSession.token();

  const request =
    token && !req.url.includes(LOGIN_ENDPOINT)
      ? req.clone({
          setHeaders: {
            Authorization: `Bearer ${token}`,
          },
        })
      : req;

  return next(request).pipe(
    catchError((error: unknown) => {
      if (error instanceof HttpErrorResponse && error.status === 401) {
        authSession.clearSession();
        void router.navigate(['/login']);
      }

      return throwError(() => error);
    }),
  );
};
