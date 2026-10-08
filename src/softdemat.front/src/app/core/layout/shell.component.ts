import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { ChangeDetectionStrategy, Component, HostListener, computed, inject, signal } from '@angular/core';
import { NavigationEnd, Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { filter, map, startWith } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { ThemeToggleComponent } from './theme-toggle.component';

@Component({
  selector: 'app-shell',
  imports: [RouterOutlet, RouterLink, RouterLinkActive, ThemeToggleComponent],
  templateUrl: './shell.component.html',
  styleUrl: './shell.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ShellComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  readonly session = this.auth.session;
  readonly isAdmin = computed(() => this.session()?.role === 'Administrateur');
  readonly menuOpen = signal(false);
  readonly settingsOpen = signal(this.router.url.startsWith('/parametrage'));
  readonly logoutConfirmationOpen = signal(false);
  private readonly url = toSignal(
    this.router.events.pipe(
      filter((event): event is NavigationEnd => event instanceof NavigationEnd),
      map((event) => event.urlAfterRedirects),
      startWith(this.router.url),
    ),
    { initialValue: this.router.url },
  );
  readonly settingsActive = computed(() => (this.url() ?? '').split('?')[0].startsWith('/parametrage'));
  readonly pageTitle = computed(() => this.titleOf(this.url() ?? '/'));
  readonly initials = computed(() => this.letters(this.session()?.name ?? ''));

  constructor() {
    this.router.events.pipe(
      filter((event): event is NavigationEnd => event instanceof NavigationEnd),
      takeUntilDestroyed(),
    ).subscribe((event) => {
      if (event.urlAfterRedirects.startsWith('/parametrage')) {
        this.settingsOpen.set(true);
      }
    });
  }

  logout(): void {
    this.auth.logout();
  }

  openLogoutConfirmation(): void {
    this.logoutConfirmationOpen.set(true);
  }

  closeLogoutConfirmation(): void {
    this.logoutConfirmationOpen.set(false);
  }

  confirmLogout(): void {
    this.logoutConfirmationOpen.set(false);
    this.logout();
  }

  @HostListener('document:keydown.escape')
  onEscape(): void {
    this.closeLogoutConfirmation();
  }

  toggleMenu(): void {
    this.menuOpen.update((open) => !open);
  }

  closeMenu(): void {
    this.menuOpen.set(false);
  }

  toggleSettings(): void {
    this.settingsOpen.update((open) => !open);
  }

  private titleOf(url: string): string {
    const path = url.split('?')[0];
    if (path.startsWith('/dematerialisation')) {
      return 'Dématérialisation';
    }
    if (path.startsWith('/historique')) {
      return 'Historique';
    }
    if (path.startsWith('/parametrage/envoi')) {
      return 'Envoi et archivage';
    }
    if (path.startsWith('/parametrage')) {
      return 'Connexion Sage';
    }
    if (path.startsWith('/utilisateurs')) {
      return 'Utilisateurs';
    }
    if (path.startsWith('/profil')) {
      return 'Profil';
    }
    return 'Dématérialisation';
  }

  private letters(name: string): string {
    const parts = name.trim().split(/\s+/).filter(Boolean);
    const value = parts.slice(0, 2).map((part) => part[0]?.toUpperCase() ?? '').join('');
    return value || 'SD';
  }
}
