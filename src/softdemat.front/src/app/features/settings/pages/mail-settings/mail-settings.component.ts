import { DestroyRef, ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { NotificationService } from '../../../../core/services/notification.service';
import { MailTemplate, SettingsApiService } from '../../services/settings-api.service';

@Component({
  selector: 'app-mail-settings',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './mail-settings.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MailSettingsComponent {
  private readonly api = inject(SettingsApiService);
  private readonly notifications = inject(NotificationService);
  private readonly destroyRef = inject(DestroyRef);
  readonly templates = signal<MailTemplate[]>([]);
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
    this.api.smtp().pipe(takeUntilDestroyed()).subscribe((setting) => {
      this.smtp.patchValue({
        host: setting.host,
        port: setting.port,
        useSsl: setting.useSsl,
        user: setting.user,
        password: '',
        fromAddress: setting.fromAddress,
      });
    });
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
    this.api.saveSmtp(this.smtp.getRawValue()).pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => {
      this.smtp.controls.password.reset('');
      this.notifications.success('Serveur SMTP enregistré.');
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
