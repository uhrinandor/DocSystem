import { computed, Injectable, signal } from '@angular/core';

import type { LoginResultDto } from '../api/generated/model/loginResultDto';
import type { User } from '../api/generated/model/user';

const AUTH_SESSION_STORAGE_KEY = 'docsystem.auth.session';

interface AuthSessionState {
  token: string;
  expiresAt: string | null;
  user: User | null;
}

@Injectable({ providedIn: 'root' })
export class AuthSessionService {
  private readonly sessionState = signal<AuthSessionState | null>(this.loadSession());

  readonly session = computed(() => this.sessionState());
  readonly token = computed(() => (this.isAuthenticated() ? this.sessionState()?.token ?? null : null));
  readonly user = computed(() => (this.isAuthenticated() ? this.sessionState()?.user ?? null : null));
  readonly expiresAt = computed(() =>
    this.isAuthenticated() ? this.sessionState()?.expiresAt ?? null : null,
  );
  readonly isAuthenticated = computed(() => this.hasValidSession(this.sessionState()));

  setSession(result: LoginResultDto): void {
    if (!result.token) {
      this.clearSession();
      return;
    }

    const session: AuthSessionState = {
      token: result.token,
      expiresAt: result.expiresAt ?? null,
      user: result.user ?? null,
    };

    this.sessionState.set(session);
    localStorage.setItem(AUTH_SESSION_STORAGE_KEY, JSON.stringify(session));
  }

  clearSession(): void {
    this.sessionState.set(null);
    localStorage.removeItem(AUTH_SESSION_STORAGE_KEY);
  }

  private loadSession(): AuthSessionState | null {
    const rawSession = localStorage.getItem(AUTH_SESSION_STORAGE_KEY);

    if (!rawSession) {
      return null;
    }

    try {
      const parsed = JSON.parse(rawSession) as AuthSessionState;

      if (!this.hasValidSession(parsed)) {
        localStorage.removeItem(AUTH_SESSION_STORAGE_KEY);
        return null;
      }

      return parsed;
    } catch {
      localStorage.removeItem(AUTH_SESSION_STORAGE_KEY);
      return null;
    }
  }

  private hasValidSession(session: AuthSessionState | null): boolean {
    if (!session?.token) {
      return false;
    }

    if (!session.expiresAt) {
      return true;
    }

    const expiresAtMs = Date.parse(session.expiresAt);

    if (Number.isNaN(expiresAtMs)) {
      return true;
    }

    return expiresAtMs > Date.now();
  }
}
