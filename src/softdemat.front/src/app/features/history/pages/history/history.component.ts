import { DestroyRef, ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { DispatchApiService, HistoryItem } from '../../../../core/services/dispatch-api.service';
import { Employee, Establishment, ReferenceApiService } from '../../../../core/services/reference-api.service';
import { DateFrPipe } from '../../../../shared/pipes/date-fr.pipe';

@Component({
  selector: 'app-history',
  imports: [ReactiveFormsModule, DateFrPipe],
  templateUrl: './history.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HistoryComponent {
  private readonly api = inject(DispatchApiService);
  private readonly references = inject(ReferenceApiService);
  private readonly destroyRef = inject(DestroyRef);
  readonly items = signal<HistoryItem[]>([]);
  readonly total = signal(0);
  readonly page = signal(1);
  readonly establishments = signal<Establishment[]>([]);
  readonly employees = signal<Employee[]>([]);
  readonly filters = inject(FormBuilder).nonNullable.group({
    from: [monthStart()],
    to: [today()],
    establishmentCode: [''],
    employeeMatricule: [''],
    sent: [true],
  });

  constructor() {
    this.references.establishments().pipe(takeUntilDestroyed()).subscribe((list) => this.establishments.set(list));
    this.references.employees().pipe(takeUntilDestroyed()).subscribe((list) => this.employees.set(list));
    this.load();
  }

  apply(): void {
    this.page.set(1);
    this.load();
  }

  changePage(page: number): void {
    this.page.set(page);
    this.load();
  }

  private load(): void {
    const value = this.filters.getRawValue();
    this.api.history({
      from: value.from,
      to: value.to,
      establishmentCode: value.establishmentCode,
      employeeMatricule: value.employeeMatricule,
      sent: value.sent === true || String(value.sent) === 'true',
      page: this.page(),
    }).pipe(takeUntilDestroyed(this.destroyRef)).subscribe((result) => {
      this.items.set(result.items);
      this.total.set(result.totalCount);
    });
  }
}

function today(): string {
  return formatDate(new Date());
}

function monthStart(): string {
  const date = new Date();
  return formatDate(new Date(date.getFullYear(), date.getMonth(), 1));
}

function formatDate(date: Date): string {
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const day = String(date.getDate()).padStart(2, '0');
  return `${date.getFullYear()}-${month}-${day}`;
}
