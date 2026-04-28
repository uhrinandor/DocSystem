import { CommonModule } from '@angular/common';
import { Component, computed, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

import { AuthSessionService } from '../auth/auth-session.service';

@Component({
  selector: 'app-dashboard-page',
  imports: [CommonModule, RouterLink, RouterLinkActive, RouterOutlet],
  templateUrl: './dashboard-page.component.html',
  styleUrl: './dashboard-page.component.css',
})
export class DashboardPageComponent {
  private readonly authSession = inject(AuthSessionService);
  private readonly router = inject(Router);

  protected readonly navigationItems = [
    { label: 'Iratok', route: '/iratok' },
    { label: 'Ügyiratok', route: '/ugyiratok' },
    { label: 'Főszámos iktatás', route: '/foszamos-iktatas' },
    { label: 'Alszámos iktatás', route: '/alszamos-iktatas' },
    { label: 'Iktatókönyvek', route: '/iktatokonyvek' },
    { label: 'Felhasználók', route: '/felhasznalok' },
  ];
  protected readonly currentUser = computed(() => this.authSession.user());
  protected readonly expiresAt = computed(() => this.authSession.expiresAt());

  protected logout(): void {
    this.authSession.clearSession();
    void this.router.navigate(['/login']);
  }
}
