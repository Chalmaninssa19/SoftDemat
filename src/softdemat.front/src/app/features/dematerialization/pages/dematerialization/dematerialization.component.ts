import { DestroyRef, ChangeDetectionStrategy, Component, computed, ElementRef, inject, signal, viewChild } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { HttpErrorResponse } from '@angular/common/http';
import { catchError, debounceTime, distinctUntilChanged, filter, map, Observable, of, switchMap, tap } from 'rxjs';
import { DispatchApiService, DispatchResult, MailChoice, PayslipFile, PayslipUploadRejection } from '../../../../core/services/dispatch-api.service';
import { Establishment, ReferenceApiService } from '../../../../core/services/reference-api.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { DialogService } from '../../../../shared/components/modal/dialog.service';
import { PaginatedResult } from '../../../../core/models/api-response.model';
import { DateFrPipe } from '../../../../shared/pipes/date-fr.pipe';
import { buildPayslipSearchFilter, PayslipSearchCriterion } from '../../payslip-search';
import { readPayslipFolder } from '../../payslip-folder';

const SEARCH_DEBOUNCE_MS = 300;

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
  private readonly dialogs = inject(DialogService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly searchBox = viewChild<ElementRef<HTMLInputElement>>('searchInput');
  readonly currentFolder = signal('');
  readonly folderName = signal('');
  readonly uploading = signal(false);
  readonly rejections = signal<PayslipUploadRejection[]>([]);
  readonly files = signal<PayslipFile[]>([]);
  readonly total = signal(0);
  readonly page = signal(1);
  readonly selected = signal<string[]>([]);
  readonly establishments = signal<Establishment[]>([]);
  readonly templates = signal<MailChoice[]>([]);
  readonly result = signal<DispatchResult | null>(null);
  readonly sending = signal(false);
  readonly confirmingSend = signal(false);
  readonly filterOpen = signal(true);
  readonly searching = signal(false);
  /** Critere visible : « Par nom » ou « Par matricule » (recherche partielle). */
  readonly criterion = signal<PayslipSearchCriterion>('name');
  readonly searchPlaceholder = computed(() =>
    this.criterion() === 'name' ? 'Rechercher par nom…' : 'Rechercher par matricule…',
  );
  readonly filters = inject(FormBuilder).nonNullable.group({
    establishmentCode: [''],
    search: [''],
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

    // Recherche dynamique : un seul champ, debounce, annule la requete
    // precedente (switchMap) pour ne jamais afficher de resultats obsoletes.
    this.filters.controls.search.valueChanges
      .pipe(
        map((value) => (value ?? '').trim()),
        debounceTime(SEARCH_DEBOUNCE_MS),
        distinctUntilChanged(),
        filter(() => this.currentFolder() !== ''),
        tap(() => {
          this.page.set(1);
          this.searching.set(true);
        }),
        switchMap(() => this.queryFiles().pipe(catchError(() => of(null)))),
        takeUntilDestroyed(),
      )
      .subscribe((result) => {
        this.searching.set(false);
        if (result) {
          this.applyResult(result);
        }
      });

    this.filters.controls.establishmentCode.valueChanges
      .pipe(
        filter(() => this.currentFolder() !== ''),
        takeUntilDestroyed(),
      )
      .subscribe(() => {
        this.page.set(1);
        this.load();
      });
  }

  chooseFolder(input: HTMLInputElement): void {
    input.click();
  }

  onFolderSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const selection = readPayslipFolder(input.files);
    input.value = '';
    this.rejections.set(selection.errors.map((reason) => ({ name: '', reason })));
    if (selection.files.length === 0) {
      this.notifications.error(selection.errors[0] ?? 'Aucun bulletin PDF dans ce dossier.');
      return;
    }

    this.folderName.set(selection.folderName);
    this.uploading.set(true);
    this.api.upload(selection.files).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (result) => this.afterUpload(result.relativeFolder, result.folderName, result.rejected, result.accepted.length),
      error: () => this.uploading.set(false),
    });
  }

  open(folder: string): void {
    this.currentFolder.set(folder);
    this.page.set(1);
    this.load();
  }

  toggleFilter(): void {
    this.filterOpen.update((open) => !open);
    if (!this.filterOpen()) {
      return;
    }
    window.setTimeout(() => this.searchBox()?.nativeElement.focus(), 0);
  }

  selectCriterion(criterion: PayslipSearchCriterion): void {
    if (this.criterion() === criterion) {
      this.searchBox()?.nativeElement.focus();
      return;
    }
    this.criterion.set(criterion);
    window.setTimeout(() => this.searchBox()?.nativeElement.focus(), 0);
    if (!this.currentFolder()) {
      return;
    }
    this.page.set(1);
    this.load();
  }

  clearSearch(): void {
    this.filters.controls.search.setValue('');
    this.searchBox()?.nativeElement.focus();
  }

  resetFilter(): void {
    this.criterion.set('name');
    this.filters.controls.search.setValue('', { emitEvent: false });
    this.filters.controls.establishmentCode.setValue('', { emitEvent: false });
    this.page.set(1);
    if (this.currentFolder()) {
      this.load();
    }
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

  async send(): Promise<void> {
    if (this.sending() || this.confirmingSend()) {
      return;
    }
    const names = this.selected();
    const templateId = Number(this.filters.controls.mailTemplateId.value);
    if (names.length === 0 || templateId < 1) {
      this.notifications.error('Sélectionnez un modèle et au moins un bulletin.');
      return;
    }
    this.confirmingSend.set(true);
    const confirmed = await this.dialogs.confirm({
      title: "Confirmer l'envoi",
      message: `Envoyer ${names.length} bulletin(s) ? Vérifiez la sélection avant de confirmer.`,
      confirmText: 'Envoyer',
      cancelText: 'Annuler',
    });
    this.confirmingSend.set(false);
    if (!confirmed) {
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
      error: (error: unknown) => {
        this.sending.set(false);
        void this.dialogs.error(this.messageOf(error));
      },
    });
  }

  private afterUpload(relativeFolder: string, folderName: string, rejected: PayslipUploadRejection[], accepted: number): void {
    this.uploading.set(false);
    this.rejections.set([...this.rejections(), ...rejected]);
    if (!relativeFolder) {
      this.notifications.error('Aucun bulletin n\'a été retenu.');
      return;
    }

    this.folderName.set(folderName || this.folderName());
    this.notifications.success(`${accepted} bulletin(s) prêt(s). L'envoi reste une action séparée.`);
    this.open(relativeFolder);
  }

  private load(): void {
    if (!this.currentFolder()) {
      return;
    }
    this.queryFiles().pipe(takeUntilDestroyed(this.destroyRef)).subscribe((result) => this.applyResult(result));
  }

  private queryFiles(): Observable<PaginatedResult<PayslipFile>> {
    const text = buildPayslipSearchFilter(this.criterion(), this.filters.controls.search.value);
    return this.api.files({
      relativeFolder: this.currentFolder(),
      establishmentCode: this.filters.controls.establishmentCode.value,
      employeeMatricule: '',
      matricule: text.matricule,
      name: text.name,
      page: this.page(),
    });
  }

  private applyResult(result: PaginatedResult<PayslipFile>): void {
    this.files.set(result.items);
    this.total.set(result.totalCount);
    const defaults = result.items.filter((file) => file.selectedByDefault).map((file) => file.fileName);
    this.selected.set(defaults);
  }

  private messageOf(error: unknown): string {
    if (error instanceof HttpErrorResponse && typeof error.error?.message === 'string') {
      return error.error.message;
    }
    return "L'envoi a échoué. Vérifiez la connexion puis réessayez.";
  }
}
