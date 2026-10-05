import { DestroyRef, ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { UsersApiService, UserAccount } from '../../services/users-api.service';
import { NotificationService } from '../../../../core/services/notification.service';

@Component({
  selector: 'app-user-list',
  imports: [ReactiveFormsModule],
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
  readonly temporaryPassword = signal<string | null>(null);
  readonly form = inject(FormBuilder).nonNullable.group({
    name: ['', Validators.required],
    username: ['', Validators.required],
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

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const raw = this.form.getRawValue();
    const payload = { ...raw, roleId: Number(raw.roleId) };
    const editing = this.editingId();
    const request = editing ? this.api.update(editing, payload) : this.api.create(payload);
    request.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => {
      this.notifications.success(editing ? 'Utilisateur modifié.' : 'Utilisateur créé.');
      this.form.reset({ name: '', username: '', pc: '', roleId: '0', password: '', passwordConfirmation: '' });
      this.editingId.set(null);
      this.load();
    });
  }

  edit(user: UserAccount): void {
    this.editingId.set(user.id);
    this.form.patchValue({
      name: user.name,
      username: user.username,
      pc: user.pc,
      roleId: user.role === 'Administrateur' ? '1' : '0',
      password: '',
      passwordConfirmation: '',
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
}
