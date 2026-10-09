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
  senderAddress?: string;
  sageFolder: string;
  senderEmail: string;
}

export interface SmtpSetting {
  id: number;
  name: string;
  isActive: boolean;
  host: string;
  port: number;
  useSsl: boolean;
  user: string;
  hasPassword: boolean;
  fromAddress: string;
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

  saveTemplate(template: MailTemplate): Observable<MailTemplate> {
    return this.http
      .put<ApiResponse<MailTemplate>>(`${environment.apiUrl}/mail-templates/${template.id}`, this.templateBody(template))
      .pipe(map((response) => response.data));
  }

  createTemplate(template: MailTemplate): Observable<MailTemplate> {
    return this.http
      .post<ApiResponse<MailTemplate>>(`${environment.apiUrl}/mail-templates`, this.templateBody(template))
      .pipe(map((response) => response.data));
  }

  deleteTemplate(id: number): Observable<void> {
    return this.http.delete<void>(`${environment.apiUrl}/mail-templates/${id}`);
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
        sageFolder: payload.sageFolder || null,
        senderEmail: payload.senderEmail || null,
      })
      .pipe(map((response) => response.data));
  }

  smtp(): Observable<SmtpSetting[]> {
    return this.http
      .get<ApiResponse<SmtpSetting[]>>(`${environment.apiUrl}/smtp-settings`)
      .pipe(map((response) => response.data ?? []));
  }

  createSmtp(payload: {
    name: string;
    host: string;
    port: number;
    useSsl: boolean;
    user: string;
    password: string;
    fromAddress: string;
  }): Observable<SmtpSetting> {
    return this.http
      .post<ApiResponse<SmtpSetting>>(`${environment.apiUrl}/smtp-settings`, this.smtpBody(payload))
      .pipe(map((response) => response.data));
  }

  updateSmtp(id: number, payload: {
    name: string;
    host: string;
    port: number;
    useSsl: boolean;
    user: string;
    password: string;
    fromAddress: string;
  }): Observable<SmtpSetting> {
    return this.http
      .put<ApiResponse<SmtpSetting>>(`${environment.apiUrl}/smtp-settings/${id}`, this.smtpBody(payload))
      .pipe(map((response) => response.data));
  }

  activateSmtp(id: number): Observable<void> {
    return this.http.put<void>(`${environment.apiUrl}/smtp-settings/${id}/activation`, {});
  }

  deleteSmtp(id: number): Observable<void> {
    return this.http.delete<void>(`${environment.apiUrl}/smtp-settings/${id}`);
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

  private templateBody(template: MailTemplate) {
    return {
      mailType: template.mailType,
      mailObject: template.mailObject,
      mailContent: template.mailContent,
      mailCode: template.mailCode,
    };
  }

  private smtpBody(payload: {
    name: string;
    host: string;
    port: number;
    useSsl: boolean;
    user: string;
    password: string;
    fromAddress: string;
  }) {
    return {
      name: payload.name,
      host: payload.host,
      port: payload.port,
      useSsl: payload.useSsl,
      user: payload.user || null,
      password: payload.password || null,
      fromAddress: payload.fromAddress,
    };
  }
}
