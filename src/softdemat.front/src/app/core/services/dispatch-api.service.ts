import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { map, Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse, PaginatedResult } from '../models/api-response.model';

export interface PayslipFile {
  fileName: string;
  matricule: string;
  lastName: string;
  firstName: string;
  fullName: string;
  email: string;
  establishmentCode: string;
  establishmentName: string;
  payDate: string;
  alreadySent: boolean;
  selectedByDefault: boolean;
}

export interface PayslipFolder {
  relativeFolder: string;
  children: string[];
}

export interface DispatchItem {
  matricule: string;
  fileName: string;
  sent: boolean;
  detail: string;
}

export interface DispatchResult {
  sentCount: number;
  notSentCount: number;
  items: DispatchItem[];
}

export interface HistoryItem {
  id: number;
  matricule: string;
  name: string;
  email: string;
  sentAt: string;
  payDate: string;
  fileName: string;
  sent: boolean;
}

export interface PayslipQuery {
  relativeFolder: string;
  establishmentCode: string;
  employeeMatricule: string;
  matricule: string;
  name: string;
  page: number;
}

export interface MailChoice {
  id: number;
  mailType: string;
}

export interface HistoryQuery {
  from: string;
  to: string;
  establishmentCode: string;
  employeeMatricule: string;
  sent: boolean;
  page: number;
}

@Injectable({ providedIn: 'root' })
export class DispatchApiService {
  private readonly http = inject(HttpClient);

  folders(relativeFolder: string): Observable<PayslipFolder> {
    const params = new HttpParams().set('relativeFolder', relativeFolder);
    return this.http
      .get<ApiResponse<PayslipFolder>>(`${environment.apiUrl}/payslip-files/folders`, { params })
      .pipe(map((response) => response.data));
  }

  files(query: PayslipQuery): Observable<PaginatedResult<PayslipFile>> {
    let params = new HttpParams()
      .set('relativeFolder', query.relativeFolder)
      .set('page', query.page)
      .set('size', 20);
    params = setIfPresent(params, 'establishmentCode', query.establishmentCode);
    params = setIfPresent(params, 'employeeMatricule', query.employeeMatricule);
    params = setIfPresent(params, 'matricule', query.matricule);
    params = setIfPresent(params, 'name', query.name);
    return this.http
      .get<ApiResponse<PaginatedResult<PayslipFile>>>(`${environment.apiUrl}/payslip-files`, { params })
      .pipe(map((response) => response.data));
  }

  templates(): Observable<MailChoice[]> {
    return this.http
      .get<ApiResponse<MailChoice[]>>(`${environment.apiUrl}/mail-templates`)
      .pipe(map((response) => response.data ?? []));
  }

  send(relativeFolder: string, mailTemplateId: number, fileNames: string[]): Observable<DispatchResult> {
    return this.http
      .post<ApiResponse<DispatchResult>>(`${environment.apiUrl}/dispatches`, { relativeFolder, mailTemplateId, fileNames })
      .pipe(map((response) => response.data));
  }

  history(query: HistoryQuery): Observable<PaginatedResult<HistoryItem>> {
    let params = new HttpParams()
      .set('from', query.from)
      .set('to', query.to)
      .set('sent', query.sent)
      .set('page', query.page)
      .set('size', 20)
      .set('sortDirection', 'desc');
    params = setIfPresent(params, 'establishmentCode', query.establishmentCode);
    params = setIfPresent(params, 'employeeMatricule', query.employeeMatricule);
    return this.http
      .get<ApiResponse<PaginatedResult<HistoryItem>>>(`${environment.apiUrl}/dispatches`, { params })
      .pipe(map((response) => response.data));
  }
}

function setIfPresent(params: HttpParams, key: string, value: string): HttpParams {
  return value.trim() ? params.set(key, value.trim()) : params;
}
