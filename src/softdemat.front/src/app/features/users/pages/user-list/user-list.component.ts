import { DestroyRef, ChangeDetectionStrategy, Component, HostListener, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { HttpErrorResponse } from '@angular/common/http';
import { UsersApiService, UserAccount, UserPayload } from '../../services/users-api.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { FieldErrorComponent } from '../../../../shared/components/field-error/field-error.component';

@Component({
  selector: 'app-user-list',
  imports: [ReactiveFormsModule, FieldErrorComponent],
  templateUrl: './user-list.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UserListComponent {
  private readonly api = inject(UsersApiService);
  private readonly notifications = inject(NotificationService);
  private readonly destroyRef = inject(DestroyRef);
  readonly users = signal<UserAccount[]>([]);
  readonly total = signal(0);
  readonly page = signal(1);
  readonly editingId = signal<number | null>(null);
  readonly modalOpen = signal(false);
  readonly submitting = signal(false);
  readonly temporaryPassword = signal<string | null>(null);
  readonly form = inject(FormBuilder).nonNullable.group({
    name: ['', Validators.required],
    username: ['', Validators.required],
    email: ['', [Validators.email, Validators.maxLength(254)]],
    pc: ['', Validators.required],
    roleId: ['0', Validators.required],
    password: [''],
    passwordConfirmation: [''],
  });

  constructor() {
    this.load();
  }

  load(): void {
    this.api.search(this.page(), '').pipe(takeUntilDestroyed(this.destroyRef)).subscribe((result) => {
      this.users.set(result.items);
      this.total.set(result.totalCount);
    });
  }

  openCreate(): void {
    this.editingId.set(null);
    this.form.reset({ name: '', username: '', email: '', pc: '', roleId: '0', password: '', passwordConfirmation: '' });
    this.setPasswordRequired(true);
    this.modalOpen.set(true);
  }

  edit(user: UserAccount): void {
    this.editingId.set(user.id);
    this.form.reset({
      name: user.name,
      username: user.username,
      email: user.email ?? '',
      pc: user.pc,
      roleId: user.role === 'Administrateur' ? '1' : '0',
      password: '',
      passwordConfirmation: '',
    });
    this.setPasswordRequired(false);
    this.modalOpen.set(true);
  }

  closeModal(): void {
    if (this.submitting()) {
      return;
    }
    this.modalOpen.set(false);
    this.editingId.set(null);
  }

  save(): void {
    if (this.form.invalid || this.passwordsDiffer()) {
      this.form.markAllAsTouched();
      return;
    }
    const editing = this.editingId();
    const payload = this.payload();
    this.submitting.set(true);
    const request = editing ? this.api.update(editing, payload) : this.api.create(payload);
    request.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (user) => this.onSaved(user, editing),
      error: (error: unknown) => {
        this.submitting.set(false);
        this.notifications.error(this.messageOf(error));
      },
    });
  }

  remove(user: UserAccount): void {
    if (!confirm(`Supprimer l'utilisateur ${user.username} ?`)) {
      return;
    }
    this.api.delete(user.id).pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => {
      this.notifications.success('Utilisateur supprimé.');
      this.load();
    });
  }

  reset(user: UserAccount): void {
    if (!confirm(`Réinitialiser le mot de passe de ${user.username} ?`)) {
      return;
    }
    this.api.resetPassword(user.id).pipe(takeUntilDestroyed(this.destroyRef)).subscribe((password) => {
      this.temporaryPassword.set(password);
      this.notifications.success('Mot de passe temporaire généré.');
    });
  }

  fieldError(name: 'name' | 'username' | 'email' | 'pc' | 'password' | 'passwordConfirmation'): string | null {
    const control = this.form.controls[name];
    if (!control.touched) {
      return null;
    }
    if (control.hasError('required')) {
      return 'Ce champ est obligatoire.';
    }
    if (name === 'email' && (control.hasError('email') || control.hasError('maxlength'))) {
      return "L'adresse e-mail n'est pas valide.";
    }
    if (name === 'passwordConfirmation' && this.passwordsDiffer()) {
      return 'Vérifiez votre mot de passe.';
    }
    return null;
  }

  @HostListener('document:keydown.escape')
  onEscape(): void {
    if (this.modalOpen()) {
      this.closeModal();
    }
  }

  private onSaved(user: UserAccount, editing: number | null): void {
    this.submitting.set(false);
    if (editing) {
      this.replaceUser(user);
      this.notifications.success('Utilisateur modifié.');
    } else {
      this.insertCreated(user);
      this.notifications.success('Utilisateur créé.');
    }
    this.modalOpen.set(false);
    this.editingId.set(null);
  }

  private insertCreated(user: UserAccount): void {
    let inserted = false;
    this.users.update((items) => {
      inserted = !items.some((item) => item.id === user.id);
      return [user, ...items.filter((item) => item.id !== user.id)].slice(0, 20);
    });
    if (inserted) {
      this.total.update((count) => count + 1);
    }
  }

  private replaceUser(user: UserAccount): void {
    this.users.update((items) => items.map((item) => (item.id === user.id ? user : item)));
  }

  private payload(): UserPayload {
    const raw = this.form.getRawValue();
    return { ...raw, roleId: Number(raw.roleId) };
  }

  private messageOf(error: unknown): string {
    if (error instanceof HttpErrorResponse && typeof error.error?.message === 'string') {
      return error.error.message;
    }
    return 'Enregistrement impossible.';
  }

  private passwordsDiffer(): boolean {
    const value = this.form.getRawValue();
    return value.password !== value.passwordConfirmation;
  }

  private setPasswordRequired(required: boolean): void {
    const passwordValidators = required ? [Validators.required] : [];
    this.form.controls.password.setValidators(passwordValidators);
    this.form.controls.passwordConfirmation.setValidators(required ? [Validators.required] : []);
    this.form.controls.password.updateValueAndValidity();
    this.form.controls.passwordConfirmation.updateValueAndValidity();
  }
}
