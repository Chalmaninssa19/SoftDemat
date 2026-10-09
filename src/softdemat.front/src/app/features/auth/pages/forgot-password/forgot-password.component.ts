import { ChangeDetectionStrategy, Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { AuthService } from '../../../../core/services/auth.service';
import { FieldErrorComponent } from '../../../../shared/components/field-error/field-error.component';

@Component({
  selector: 'app-forgot-password',
  imports: [ReactiveFormsModule, RouterLink, FieldErrorComponent],
  templateUrl: './forgot-password.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ForgotPasswordComponent {
  private readonly auth = inject(AuthService);
  private readonly destroyRef = inject(DestroyRef);
  readonly submitting = signal(false);
  readonly submitted = signal(false);
  readonly successMessage = signal<string | null>(null);
  readonly errorMessage = signal<string | null>(null);
  readonly form = inject(FormBuilder).nonNullable.group({
    email: ['', [Validators.required, Validators.email, Validators.maxLength(254)]],
  });

  submit(): void {
    if (this.submitting()) {
      return;
    }

    this.submitted.set(true);
    this.successMessage.set(null);
    this.errorMessage.set(null);
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.submitting.set(false);
      return;
    }

    this.submitting.set(true);
    this.auth.requestPasswordReset(this.form.controls.email.value)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.submitting.set(false)),
      )
      .subscribe({
        next: (message) => {
          this.successMessage.set(message);
        },
        error: (error: unknown) => {
          this.errorMessage.set(this.auth.messageOf(error));
        },
      });
  }

  fieldError(): string | null {
    const email = this.form.controls.email;
    if (!this.submitted() || !email.invalid) {
      return null;
    }
    if (email.hasError('required')) {
      return "L'adresse e-mail est obligatoire.";
    }
    return "L'adresse e-mail n'est pas valide.";
  }
}
