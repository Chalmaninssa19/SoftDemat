import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { map, Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { ApiResponse } from '../../../core/models/api-response.model';

export interface SageConnection {
  server: string;
  databaseName: string;
  login: string;
  hasPassword: boolean;
}

export interface SagePayload {
  server: string;
  databaseName: string;
  login: string | null;
  password: string | null;
  windowsAuthentication: boolean;
}

export interface MailTemplate {
  id: number;
  mailType: string;
  mailObject: string;
  mailContent: string;
  mailCode: string;
}

export interface GeneralParameter {
  archiveFolder: string;
  cc: string;
  senderTool: string;
  senderAddress: string;
}

@Injectable({ providedIn: 'root' })
export class SettingsApiService {
  private readonly http = inject(HttpClient);

  sage(): Observable<SageConnection> {
    return this.http
      .get<ApiResponse<SageConnection>>(`${environment.apiUrl}/sage-connections`)
      .pipe(map((response) => response.data));
  }

  saveSage(payload: SagePayload): Observable<SageConnection> {
    return this.http
      .put<ApiResponse<SageConnection>>(`${environment.apiUrl}/sage-connections`, payload)
      .pipe(map((response) => response.data));
  }

  testSage(payload: SagePayload): Observable<string> {
    return this.http
      .post<ApiResponse<string>>(`${environment.apiUrl}/sage-connections/tests`, payload)
      .pipe(map((response) => response.data));
  }

  templates(): Observable<MailTemplate[]> {
    return this.http
      .get<ApiResponse<MailTemplate[]>>(`${environment.apiUrl}/mail-templates`)
      .pipe(map((response) => response.data ?? []));
  }

  saveTemplate(id: number, mailObject: string, mailContent: string): Observable<MailTemplate> {
    return this.http
      .put<ApiResponse<MailTemplate>>(`${environment.apiUrl}/mail-templates/${id}`, { mailObject, mailContent })
      .pipe(map((response) => response.data));
  }

  general(): Observable<GeneralParameter> {
    return this.http
      .get<ApiResponse<GeneralParameter>>(`${environment.apiUrl}/general-parameters`)
      .pipe(map((response) => response.data));
  }

  saveGeneral(payload: GeneralParameter): Observable<GeneralParameter> {
    return this.http
      .put<ApiResponse<GeneralParameter>>(`${environment.apiUrl}/general-parameters`, {
        archiveFolder: payload.archiveFolder,
        cc: payload.cc || null,
        senderTool: payload.senderTool,
        senderAddress: payload.senderAddress || null,
      })
      .pipe(map((response) => response.data));
  }

  browseArchive(): Observable<string> {
    return this.http
      .post<ApiResponse<{ folder: string }>>(`${environment.apiUrl}/general-parameters/archive-browse`, {})
      .pipe(map((response) => response.data.folder ?? ''));
  }

  outlookSender(): Observable<string> {
    return this.http
      .get<ApiResponse<{ email: string }>>(`${environment.apiUrl}/general-parameters/outlook-sender`)
      .pipe(map((response) => response.data.email));
  }
}
