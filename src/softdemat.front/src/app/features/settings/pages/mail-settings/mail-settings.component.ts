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
    cc: [''],
    senderTool: ['Outlook', Validators.required],
    senderAddress: [''],
  });
  readonly mail = inject(FormBuilder).nonNullable.group({
    id: [0, Validators.min(1)],
    mailObject: ['', Validators.required],
    mailContent: ['', Validators.required],
  });

  constructor() {
    this.api.general().pipe(takeUntilDestroyed()).subscribe((parameter) => this.general.patchValue(parameter));
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
    if (value.senderTool === 'Address' && !value.senderAddress.trim()) {
      this.notifications.error('Indiquez l’adresse de l’expéditeur.');
      return;
    }
    this.api.saveGeneral(value).pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => {
      this.notifications.success('Paramètres d’envoi enregistrés.');
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
    this.api.saveTemplate(value.id, value.mailObject, value.mailContent)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((saved) => {
        this.templates.update((items) => items.map((item) => (item.id === saved.id ? saved : item)));
        this.notifications.success('Modèle de mail enregistré.');
      });
  }
}
