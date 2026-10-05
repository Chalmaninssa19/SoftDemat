import { DestroyRef, ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { NotificationService } from '../../../../core/services/notification.service';
import { SettingsApiService } from '../../services/settings-api.service';

@Component({
  selector: 'app-sage-settings',
  imports: [ReactiveFormsModule],
  templateUrl: './sage-settings.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SageSettingsComponent {
  private readonly api = inject(SettingsApiService);
  private readonly notifications = inject(NotificationService);
  private readonly destroyRef = inject(DestroyRef);
  readonly form = inject(FormBuilder).nonNullable.group({
    server: ['', Validators.required],
    databaseName: ['', Validators.required],
    login: [''],
    password: [''],
    windowsAuthentication: [false],
  });

  constructor() {
    this.api.sage().pipe(takeUntilDestroyed()).subscribe((connection) => {
      this.form.patchValue({
        server: connection.server,
        databaseName: connection.databaseName,
        login: connection.login,
        password: '',
        windowsAuthentication: !connection.login,
      });
    });
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.api.saveSage(this.payload()).pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => {
      this.notifications.success('Connexion Sage enregistrée.');
      this.form.controls.password.reset('');
    });
  }

  test(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.api.testSage(this.payload()).pipe(takeUntilDestroyed(this.destroyRef)).subscribe((message) => {
      this.notifications.success(message);
    });
  }

  private payload() {
    const value = this.form.getRawValue();
    return {
      server: value.server,
      databaseName: value.databaseName,
      login: value.windowsAuthentication ? null : value.login,
      password: value.password || null,
      windowsAuthentication: value.windowsAuthentication,
    };
  }
}
