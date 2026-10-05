import { DestroyRef, ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../../../core/services/auth.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { FieldErrorComponent } from '../../../../shared/components/field-error/field-error.component';

@Component({
  selector: 'app-profile',
  imports: [ReactiveFormsModule, FieldErrorComponent],
  templateUrl: './profile.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProfileComponent {
  private readonly auth = inject(AuthService);
  private readonly notifications = inject(NotificationService);
  private readonly destroyRef = inject(DestroyRef);
  readonly mustChange = this.auth.mustChangePassword;
  readonly form = inject(FormBuilder).nonNullable.group({
    currentPassword: ['', Validators.required],
    newPassword: ['', Validators.required],
    confirmation: ['', Validators.required],
  });

  submit(): void {
    if (this.form.invalid || !this.passwordsMatch()) {
      this.form.markAllAsTouched();
      return;
    }
    const value = this.form.getRawValue();
    this.auth.changePassword(value.currentPassword, value.newPassword, value.confirmation)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
      next: () => {
        this.notifications.success('Mot de passe modifié. Reconnectez-vous.');
        this.auth.logout();
      },
      error: () => undefined,
    });
  }

  passwordsMatch(): boolean {
    return this.form.controls.newPassword.value === this.form.controls.confirmation.value;
  }

  confirmationError(): string | null {
    if (this.form.controls.confirmation.touched && !this.passwordsMatch()) {
      return 'Vérifiez la confirmation du mot de passe.';
    }
    return null;
  }
}
