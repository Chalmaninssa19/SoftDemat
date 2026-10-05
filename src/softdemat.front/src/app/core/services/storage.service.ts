import { Injectable, inject } from '@angular/core';

const REFRESH_KEY = 'softdemat.refresh';

@Injectable({ providedIn: 'root' })
export class StorageService {
  private readonly storage = inject(BrowserStorage);

  getRefreshToken(): string | null {
    return this.storage.get(REFRESH_KEY);
  }

  setRefreshToken(token: string): void {
    this.storage.set(REFRESH_KEY, token);
  }

  clear(): void {
    this.storage.remove(REFRESH_KEY);
  }
}

@Injectable({ providedIn: 'root' })
export class BrowserStorage {
  get(key: string): string | null {
    return sessionStorage.getItem(key);
  }

  set(key: string, value: string): void {
    sessionStorage.setItem(key, value);
  }

  remove(key: string): void {
    sessionStorage.removeItem(key);
  }
}
