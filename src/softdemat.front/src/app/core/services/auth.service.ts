import { HttpBackend, HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, catchError, firstValueFrom, map, of, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse, LoginResponse, Session } from '../models/api-response.model';
import { StorageService } from './storage.service';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly raw = new HttpClient(inject(HttpBackend));
  private readonly storage = inject(StorageService);
  private readonly router = inject(Router);

  readonly accessToken = signal<string | null>(null);
  readonly session = signal<Session | null>(null);
  readonly mustChangePassword = signal(false);

  login(username: string, password: string): Observable<LoginResponse> {
    return this.http
      .post<ApiResponse<LoginResponse>>(`${environment.apiUrl}/auth/login`, { username, password })
      .pipe(
        map((response) => response.data),
        tap((data) => this.apply(data)),
      );
  }

  refresh(): Observable<LoginResponse | null> {
    const refreshToken = this.storage.getRefreshToken();
    if (!refreshToken) {
      return of(null);
    }

    return this.raw
      .post<ApiResponse<LoginResponse>>(`${environment.apiUrl}/auth/refresh`, { refreshToken })
      .pipe(
        map((response) => response.data),
        tap((data) => this.apply(data)),
        catchError(() => {
          this.clear();
          return of(null);
        }),
      );
  }

  storageRefreshAvailable(): boolean {
    return this.storage.getRefreshToken() !== null;
  }

  logout(): void {
    const refreshToken = this.storage.getRefreshToken();
    const token = this.accessToken();
    this.clear();
    if (refreshToken && token) {
      void firstValueFrom(
        this.raw.post(`${environment.apiUrl}/auth/logout`, { refreshToken }, {
          headers: { Authorization: `Bearer ${token}` },
        }),
      ).catch(() => undefined);
    }
    void this.router.navigate(['/auth/connexion']);
  }

  restore(): Promise<void> {
    return new Promise((resolve) => {
      this.refresh().subscribe({
        next: () => resolve(),
        error: () => resolve(),
      });
    });
  }

  changePassword(currentPassword: string, newPassword: string, confirmation: string): Observable<string> {
    return this.http
      .put<ApiResponse<string>>(`${environment.apiUrl}/auth/password`, {
        currentPassword,
        newPassword,
        confirmation,
      })
      .pipe(map((response) => response.message || response.data));
  }

  messageOf(error: unknown): string {
    if (error instanceof HttpErrorResponse) {
      const body = error.error as ApiResponse<unknown> | undefined;
      if (body?.message) {
        return body.message;
      }
      if (error.status === 0) {
        return 'Le serveur est injoignable.';
      }
    }
    return 'Une erreur est survenue.';
  }

  private apply(data: LoginResponse): void {
    this.accessToken.set(data.accessToken);
    this.session.set(data.session);
    this.mustChangePassword.set(data.mustChangePassword);
    this.storage.setRefreshToken(data.refreshToken);
  }

  private clear(): void {
    this.accessToken.set(null);
    this.session.set(null);
    this.mustChangePassword.set(false);
    this.storage.clear();
  }
}
