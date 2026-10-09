import { ChangeDetectionStrategy, Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { AuthService } from '../../../../core/services/auth.service';
import { FieldErrorComponent } from '../../../../shared/components/field-error/field-error.component';

@Component({
  selector: 'app-reset-password',
  imports: [ReactiveFormsModule, RouterLink, FieldErrorComponent],
  templateUrl: './reset-password.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ResetPasswordComponent {
  private readonly auth = inject(AuthService);
  private readonly destroyRef = inject(DestroyRef);
  readonly token = inject(ActivatedRoute).snapshot.queryParamMap.get('token') ?? '';
  readonly submitting = signal(false);
  readonly submitted = signal(false);
  readonly success = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly form = inject(FormBuilder).nonNullable.group({
    newPassword: ['', [Validators.required, Validators.minLength(12)]],
    confirmation: ['', Validators.required],
  });

  submit(): void {
    if (this.submitting()) {
      return;
    }

    this.submitted.set(true);
    this.errorMessage.set(null);
    if (!this.token) {
      this.errorMessage.set('Le lien de réinitialisation est invalide ou expiré.');
      this.submitting.set(false);
      return;
    }
    if (this.form.invalid || this.form.controls.newPassword.value !== this.form.controls.confirmation.value) {
      this.form.markAllAsTouched();
      this.submitting.set(false);
      return;
    }

    this.submitting.set(true);
    const { newPassword, confirmation } = this.form.getRawValue();
    this.auth.resetPassword(this.token, newPassword, confirmation)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.submitting.set(false)),
      )
      .subscribe({
        next: () => {
          this.success.set(true);
        },
        error: (error: unknown) => {
          this.errorMessage.set(this.auth.messageOf(error));
        },
      });
  }

  fieldError(name: 'newPassword' | 'confirmation'): string | null {
    const control = this.form.controls[name];
    if (!this.submitted() || !control.invalid) {
      return null;
    }
    if (control.hasError('required')) {
      return name === 'newPassword' ? 'Le mot de passe est obligatoire.' : 'La confirmation est obligatoire.';
    }
    if (control.hasError('minlength')) {
      return 'Le mot de passe doit contenir au moins 12 caractères.';
    }
    return null;
  }

  passwordsDiffer(): boolean {
    return this.submitted()
      && this.form.controls.confirmation.touched
      && this.form.controls.newPassword.value !== this.form.controls.confirmation.value;
  }
}
