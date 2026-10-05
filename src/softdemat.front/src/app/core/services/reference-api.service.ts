import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { map, Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse, PaginatedResult } from '../models/api-response.model';

export interface Establishment {
  code: string;
  name: string;
}

export interface Employee {
  matricule: string;
  lastName: string;
  firstName: string;
  fullName: string;
  email: string;
  establishmentCode: string;
  establishmentName: string;
}

@Injectable({ providedIn: 'root' })
export class ReferenceApiService {
  private readonly http = inject(HttpClient);

  establishments(): Observable<Establishment[]> {
    const params = new HttpParams().set('page', 1).set('size', 100);
    return this.http
      .get<ApiResponse<PaginatedResult<Establishment>>>(`${environment.apiUrl}/establishments`, { params })
      .pipe(map((response) => response.data.items));
  }

  employees(search = ''): Observable<Employee[]> {
    let params = new HttpParams().set('page', 1).set('size', 100);
    if (search) {
      params = params.set('search', search);
    }
    return this.http
      .get<ApiResponse<PaginatedResult<Employee>>>(`${environment.apiUrl}/employees`, { params })
      .pipe(map((response) => response.data.items));
  }
}
