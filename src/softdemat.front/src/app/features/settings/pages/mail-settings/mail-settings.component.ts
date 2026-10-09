import { DestroyRef, ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { NotificationService } from '../../../../core/services/notification.service';
import { MailTemplate, SettingsApiService, SmtpSetting } from '../../services/settings-api.service';

@Component({
  selector: 'app-mail-settings',
  imports: [ReactiveFormsModule],
  templateUrl: './mail-settings.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MailSettingsComponent {
  private readonly api = inject(SettingsApiService);
  private readonly notifications = inject(NotificationService);
  private readonly destroyRef = inject(DestroyRef);
  readonly templates = signal<MailTemplate[]>([]);
  readonly smtpSettings = signal<SmtpSetting[]>([]);
  readonly selectedSmtpId = signal<number | null>(null);
  readonly outlookAddress = signal('');
  readonly browsing = signal(false);
  readonly general = inject(FormBuilder).nonNullable.group({
    archiveFolder: ['', Validators.required],
    sageFolder: [''],
    senderEmail: [''],
    cc: [''],
    senderTool: ['Outlook', Validators.required],
  });
  readonly smtp = inject(FormBuilder).nonNullable.group({
    name: ['', Validators.required],
    host: ['', Validators.required],
    port: [587, [Validators.required, Validators.min(1), Validators.max(65535)]],
    useSsl: [true],
    user: [''],
    password: [''],
    fromAddress: ['', [Validators.required, Validators.email]],
  });
  readonly mail = inject(FormBuilder).nonNullable.group({
    id: [0],
    mailType: ['', Validators.required],
    mailCode: ['', Validators.required],
    mailObject: ['', Validators.required],
    mailContent: ['', Validators.required],
  });

  constructor() {
    this.api.general().pipe(takeUntilDestroyed()).subscribe((parameter) => {
      this.general.patchValue({
        archiveFolder: parameter.archiveFolder,
        sageFolder: parameter.sageFolder ?? '',
        senderEmail: parameter.senderEmail ?? '',
        cc: parameter.cc,
        senderTool: parameter.senderTool,
      });
    });
    this.refreshSmtp();
    this.api.templates().pipe(takeUntilDestroyed()).subscribe((templates) => {
      this.templates.set(templates);
      if (templates[0]) {
        this.select(templates[0].id);
      }
    });
  }

  select(id: number): void {
    const template = this.templates().find((item) => item.id === id);
    if (!template) {
      return;
    }
    this.mail.patchValue({
      id: template.id,
      mailType: template.mailType,
      mailCode: template.mailCode,
      mailObject: template.mailObject,
      mailContent: template.mailContent,
    });
  }

  saveGeneral(): void {
    if (this.general.invalid) {
      this.general.markAllAsTouched();
      return;
    }
    const value = this.general.getRawValue();
    this.api.saveGeneral(value).pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => {
      this.notifications.success('Paramètres d’envoi enregistrés.');
    });
  }

  saveSmtp(): void {
    if (this.smtp.invalid) {
      this.smtp.markAllAsTouched();
      return;
    }
    const value = this.smtp.getRawValue();
    const selectedId = this.selectedSmtpId();
    const request = selectedId === null
      ? this.api.createSmtp(value)
      : this.api.updateSmtp(selectedId, value);
    request.pipe(takeUntilDestroyed(this.destroyRef)).subscribe((saved) => {
      this.smtpSettings.update((items) => selectedId === null
        ? [...items, saved]
        : items.map((item) => item.id === saved.id ? saved : item));
      this.selectSmtp(saved.id);
      this.notifications.success('Serveur SMTP enregistré.');
    });
  }

  selectSmtp(id: number): void {
    const setting = this.smtpSettings().find((item) => item.id === id);
    if (!setting) {
      return;
    }

    this.selectedSmtpId.set(setting.id);
    this.smtp.patchValue({
      name: setting.name,
      host: setting.host,
      port: setting.port,
      useSsl: setting.useSsl,
      user: setting.user,
      password: '',
      fromAddress: setting.fromAddress,
    });
  }

  createSmtp(): void {
    this.selectedSmtpId.set(null);
    this.smtp.reset({
      name: '',
      host: '',
      port: 587,
      useSsl: true,
      user: '',
      password: '',
      fromAddress: '',
    });
  }

  activateSmtp(id: number): void {
    this.api.activateSmtp(id).pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => {
      this.smtpSettings.update((items) => items.map((item) => ({ ...item, isActive: item.id === id })));
      this.selectSmtp(id);
      this.notifications.success('Serveur SMTP activé.');
    });
  }

  removeSmtp(id: number): void {
    this.api.deleteSmtp(id).pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => {
      this.refreshSmtp();
      this.notifications.success('Serveur SMTP supprimé.');
    });
  }

  private refreshSmtp(): void {
    this.api.smtp().pipe(takeUntilDestroyed(this.destroyRef)).subscribe((settings) => {
      this.smtpSettings.set(settings);
      const selection = settings.find((setting) => setting.isActive) ?? settings[0];
      if (selection) {
        this.selectSmtp(selection.id);
      } else {
        this.createSmtp();
      }
    });
  }

  browse(): void {
    this.browsing.set(true);
    this.api.browseArchive().pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (folder) => {
        this.browsing.set(false);
        if (folder) {
          this.general.controls.archiveFolder.setValue(folder);
        }
      },
      error: () => this.browsing.set(false),
    });
  }

  detectOutlook(): void {
    this.api.outlookSender().pipe(takeUntilDestroyed(this.destroyRef)).subscribe((email) => {
      this.outlookAddress.set(email);
      this.notifications.success('Adresse Outlook lue sur ce poste.');
    });
  }

  saveMail(): void {
    if (this.mail.invalid) {
      this.mail.markAllAsTouched();
      return;
    }
    const value = this.mail.getRawValue();
    const request = value.id > 0 ? this.api.saveTemplate(value) : this.api.createTemplate(value);
    request.pipe(takeUntilDestroyed(this.destroyRef)).subscribe((saved) => {
      this.templates.update((items) => value.id > 0
        ? items.map((item) => (item.id === saved.id ? saved : item))
        : [...items, saved]);
      this.select(saved.id);
      this.notifications.success('Modèle de mail enregistré.');
    });
  }

  createMail(): void {
    this.mail.reset({ id: 0, mailType: '', mailCode: '', mailObject: '', mailContent: '' });
  }

  removeMail(): void {
    const id = this.mail.controls.id.value;
    if (id < 1) {
      return;
    }
    this.api.deleteTemplate(id).pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => {
      const remaining = this.templates().filter((item) => item.id !== id);
      this.templates.set(remaining);
      if (remaining[0]) {
        this.select(remaining[0].id);
      } else {
        this.createMail();
      }
      this.notifications.success('Modèle de mail supprimé.');
    });
  }
}
