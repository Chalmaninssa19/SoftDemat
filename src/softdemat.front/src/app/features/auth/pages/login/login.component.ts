import { DestroyRef, ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { ThemeToggleComponent } from '../../../../core/layout/theme-toggle.component';
import { AuthService } from '../../../../core/services/auth.service';
import { FieldErrorComponent } from '../../../../shared/components/field-error/field-error.component';

@Component({
  selector: 'app-login',
  imports: [ReactiveFormsModule, FieldErrorComponent, ThemeToggleComponent],
  templateUrl: './login.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoginComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly formBuilder = inject(FormBuilder);
  private readonly destroyRef = inject(DestroyRef);
  readonly submitting = signal(false);
  readonly submitted = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly showPassword = signal(false);
  readonly form = this.formBuilder.nonNullable.group({
    username: ['', Validators.required],
    password: ['', Validators.required],
  });

  submit(): void {
    this.submitted.set(true);
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.submitting.set(true);
    this.errorMessage.set(null);
    const value = this.form.getRawValue();
    this.auth.login(value.username, value.password).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (result) => {
        this.submitting.set(false);
        void this.router.navigate(result.mustChangePassword ? ['/profil'] : ['/dematerialisation']);
      },
      error: (error: unknown) => {
        this.submitting.set(false);
        this.errorMessage.set(this.auth.messageOf(error));
      },
    });
  }

  togglePassword(): void {
    this.showPassword.update((visible) => !visible);
  }

  fieldError(name: 'username' | 'password'): string | null {
    const control = this.form.controls[name];
    if (this.submitted() && control.invalid) {
      return 'Veuillez remplir les champs.';
    }
    return null;
  }
}
