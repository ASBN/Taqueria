import type { AuthResponse } from '../models/api';

const storageKey = 'taqueria.session';

export const authStorage = {
  get(): AuthResponse | null {
    const raw = localStorage.getItem(storageKey);
    if (!raw) {
      return null;
    }

    const session = JSON.parse(raw) as AuthResponse;
    if (new Date(session.expiraUtc).getTime() <= Date.now()) {
      localStorage.removeItem(storageKey);
      return null;
    }

    return session;
  },

  set(session: AuthResponse): void {
    localStorage.setItem(storageKey, JSON.stringify(session));
  },

  clear(): void {
    localStorage.removeItem(storageKey);
  }
};
