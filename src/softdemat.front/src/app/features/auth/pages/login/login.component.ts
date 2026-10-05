import { DestroyRef, ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../../../core/services/auth.service';
import { FieldErrorComponent } from '../../../../shared/components/field-error/field-error.component';

@Component({
  selector: 'app-login',
  imports: [ReactiveFormsModule, FieldErrorComponent],
  templateUrl: './login.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoginComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly formBuilder = inject(FormBuilder);
  private readonly destroyRef = inject(DestroyRef);
  readonly submitting = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly form = this.formBuilder.nonNullable.group({
    username: ['', Validators.required],
    password: ['', Validators.required],
  });

  submit(): void {
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
        void this.router.navigate(result.mustChangePassword ? ['/profil'] : ['/']);
      },
      error: (error: unknown) => {
        this.submitting.set(false);
        this.errorMessage.set(this.auth.messageOf(error));
      },
    });
  }

  fieldError(name: 'username' | 'password'): string | null {
    const control = this.form.controls[name];
    if (control.touched && control.invalid) {
      return 'Veuillez remplir les champs.';
    }
    return null;
  }
}
