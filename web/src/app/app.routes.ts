import { Routes } from '@angular/router';
import { authGuard } from './auth/auth.guard';
import { loginGuard } from './auth/login.guard';
import { DashboardPageComponent } from './pages/dashboard-page.component';
import { IratokPageComponent } from './pages/iratok-page.component';
import { LoginPageComponent } from './pages/login-page.component';

export const routes: Routes = [
  {
    path: 'login',
    component: LoginPageComponent,
    canActivate: [loginGuard],
  },
  {
    path: '',
    component: DashboardPageComponent,
    canActivate: [authGuard],
    children: [
      {
        path: '',
        pathMatch: 'full',
        redirectTo: 'iratok',
      },
      {
        path: 'iratok',
        component: IratokPageComponent,
      },
    ],
  },
  {
    path: '**',
    redirectTo: '',
  },
];
