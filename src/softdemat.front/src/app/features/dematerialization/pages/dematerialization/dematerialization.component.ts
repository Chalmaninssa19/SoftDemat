import { DestroyRef, ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { DispatchApiService, DispatchResult, MailChoice, PayslipFile } from '../../../../core/services/dispatch-api.service';
import { Establishment, ReferenceApiService } from '../../../../core/services/reference-api.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { DateFrPipe } from '../../../../shared/pipes/date-fr.pipe';

@Component({
  selector: 'app-dematerialization',
  imports: [ReactiveFormsModule, DateFrPipe],
  templateUrl: './dematerialization.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DematerializationComponent {
  private readonly api = inject(DispatchApiService);
  private readonly references = inject(ReferenceApiService);
  private readonly notifications = inject(NotificationService);
  private readonly destroyRef = inject(DestroyRef);
  readonly folders = signal<string[]>([]);
  readonly currentFolder = signal('');
  readonly files = signal<PayslipFile[]>([]);
  readonly total = signal(0);
  readonly page = signal(1);
  readonly selected = signal<string[]>([]);
  readonly establishments = signal<Establishment[]>([]);
  readonly templates = signal<MailChoice[]>([]);
  readonly result = signal<DispatchResult | null>(null);
  readonly sending = signal(false);
  readonly filters = inject(FormBuilder).nonNullable.group({
    establishmentCode: [''],
    employeeMatricule: [''],
    matricule: [''],
    name: [''],
    mailTemplateId: [0],
  });

  constructor() {
    this.references.establishments().pipe(takeUntilDestroyed()).subscribe((items) => this.establishments.set(items));
    this.api.templates().pipe(takeUntilDestroyed()).subscribe((items) => {
      this.templates.set(items);
      if (items[0]) {
        this.filters.controls.mailTemplateId.setValue(items[0].id);
      }
    });
    this.open('');
  }

  open(folder: string): void {
    this.currentFolder.set(folder);
    this.api.folders(folder).pipe(takeUntilDestroyed(this.destroyRef)).subscribe((listing) => {
      this.folders.set(listing.children);
      this.currentFolder.set(listing.relativeFolder);
    });
    this.page.set(1);
    this.load();
  }

  parentFolder(): string {
    const parts = this.currentFolder().split('/').filter(Boolean);
    parts.pop();
    return parts.join('/');
  }

  applyFilters(): void {
    this.page.set(1);
    this.load();
  }

  changePage(page: number): void {
    this.page.set(page);
    this.load();
  }

  toggle(fileName: string, event: Event): void {
    const checked = (event.target as HTMLInputElement).checked;
    this.selected.update((names) => checked ? [...names, fileName] : names.filter((name) => name !== fileName));
  }

  isSelected(fileName: string): boolean {
    return this.selected().includes(fileName);
  }

  send(): void {
    const names = this.selected();
    const templateId = Number(this.filters.controls.mailTemplateId.value);
    if (names.length === 0 || templateId < 1) {
      this.notifications.error('Sélectionnez un modèle et au moins un bulletin.');
      return;
    }
    if (!confirm(`Envoyer ${names.length} bulletin(s) ?`)) {
      return;
    }
    this.sending.set(true);
    this.api.send(this.currentFolder(), templateId, names).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (result) => {
        this.sending.set(false);
        this.result.set(result);
        this.notifications.success(`${result.sentCount} envoyé(s), ${result.notSentCount} non envoyé(s).`);
        this.load();
      },
      error: () => this.sending.set(false),
    });
  }

  private load(): void {
    const filters = this.filters.getRawValue();
    this.api.files({
      relativeFolder: this.currentFolder(),
      establishmentCode: filters.establishmentCode,
      employeeMatricule: filters.employeeMatricule,
      matricule: filters.matricule,
      name: filters.name,
      page: this.page(),
    }).pipe(takeUntilDestroyed(this.destroyRef)).subscribe((result) => {
      this.files.set(result.items);
      this.total.set(result.totalCount);
      const defaults = result.items.filter((file) => file.selectedByDefault).map((file) => file.fileName);
      this.selected.set(defaults);
    });
  }
}
