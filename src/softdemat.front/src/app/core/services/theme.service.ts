import { Injectable, signal } from '@angular/core';

export type ThemeMode = 'light' | 'dark';

const STORAGE_KEY = 'softdemat.theme';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  readonly mode = signal<ThemeMode>(this.read());

  constructor() {
    this.apply(this.mode());
  }

  toggle(): void {
    this.applyStored(this.mode() === 'dark' ? 'light' : 'dark');
  }

  private applyStored(mode: ThemeMode): void {
    localStorage.setItem(STORAGE_KEY, mode);
    this.mode.set(mode);
    this.apply(mode);
  }

  private read(): ThemeMode {
    return localStorage.getItem(STORAGE_KEY) === 'light' ? 'light' : 'dark';
  }

  private apply(mode: ThemeMode): void {
    document.documentElement.classList.toggle('dark', mode === 'dark');
    document.documentElement.classList.toggle('light', mode === 'light');
    document.documentElement.style.colorScheme = mode;
  }
}
