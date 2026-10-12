import { ChangeDetectionStrategy, Component, HostListener, input, output } from '@angular/core';

/**
 * Coquille commune des modales SOFT-DEMAT.
 * Fond assombri, carte « login-card » (claire/sombre), responsive,
 * fermeture sur Echap ou clic du fond sauf pendant une action sensible.
 */
@Component({
  selector: 'app-modal-shell',
  template: `
    <div
      class="sd-modal-backdrop fixed inset-0 z-50 flex items-center justify-center bg-black/60 p-4"
      (click)="onBackdrop()"
    >
      <section
        class="login-card sd-modal-card max-h-[90vh] w-full overflow-y-auto rounded-2xl p-6 shadow-2xl sm:p-8"
        [class.max-w-md]="!wide()"
        [class.max-w-lg]="wide()"
        role="dialog"
        aria-modal="true"
        [attr.aria-labelledby]="labelledBy()"
        (click)="$event.stopPropagation()"
      >
        <ng-content />
      </section>
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ModalShellComponent {
  readonly labelledBy = input<string | null>(null);
  /** `true` pour les formulaires larges (creation/edition). */
  readonly wide = input(false);
  /** `true` pendant une soumission : bloque la fermeture accidentelle. */
  readonly busy = input(false);

  readonly closed = output<void>();

  @HostListener('document:keydown.escape')
  onEscape(): void {
    if (!this.busy()) {
      this.closed.emit();
    }
  }

  onBackdrop(): void {
    if (!this.busy()) {
      this.closed.emit();
    }
  }
}
