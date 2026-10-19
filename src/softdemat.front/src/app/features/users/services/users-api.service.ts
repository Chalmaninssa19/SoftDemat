import { HttpClient, HttpContext, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { map, Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { ApiResponse, PaginatedResult } from '../../../core/models/api-response.model';
import { SKIP_ERROR_TOAST } from '../../../core/interceptors/http-context';

export interface UserAccount {
  id: number;
  name: string;
  username: string;
  email: string | null;
  role: string;
  pc: string;
}

export interface UserPayload {
  name: string;
  username: string;
  email: string;
  pc: string;
  roleId: number;
  password?: string;
  passwordConfirmation?: string;
}

@Injectable({ providedIn: 'root' })
export class UsersApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/users`;

  search(page: number, search: string): Observable<PaginatedResult<UserAccount>> {
    const params = new HttpParams().set('page', page).set('size', 20).set('search', search).set('sortBy', 'name');
    return this.http.get<ApiResponse<PaginatedResult<UserAccount>>>(this.baseUrl, { params }).pipe(map((response) => response.data));
  }

  create(payload: UserPayload): Observable<UserAccount> {
    return this.http.post<ApiResponse<UserAccount>>(this.baseUrl, payload).pipe(map((response) => response.data));
  }

  update(id: number, payload: UserPayload): Observable<UserAccount> {
    return this.http.put<ApiResponse<UserAccount>>(`${this.baseUrl}/${id}`, payload).pipe(map((response) => response.data));
  }

  delete(id: number): Observable<void> {
    const context = new HttpContext().set(SKIP_ERROR_TOAST, true);
    return this.http.delete<void>(`${this.baseUrl}/${id}`, { context });
  }

  resetPassword(id: number): Observable<string> {
    const context = new HttpContext().set(SKIP_ERROR_TOAST, true);
    return this.http
      .post<ApiResponse<{ temporaryPassword: string }>>(`${this.baseUrl}/${id}/password-resets`, {}, { context })
      .pipe(map((response) => response.data.temporaryPassword));
  }
}
