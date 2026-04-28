import { provideHttpClient, withInterceptors } from '@angular/common/http';
import {
  ApplicationConfig,
  provideBrowserGlobalErrorListeners,
} from '@angular/core';
import { provideRouter } from '@angular/router';

import { apiInterceptor } from './api/api.interceptor';
import { API_ROOT_URL } from './api/api-root.token';
import { authTokenInterceptor } from './auth/auth-token.interceptor';
import { routes } from './app.routes';
import { environment } from '../environments/environment';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideHttpClient(withInterceptors([apiInterceptor, authTokenInterceptor])),
    { provide: API_ROOT_URL, useValue: environment.apiRootUrl },
  ],
};
