import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { DialogService } from './dialog.service';
import { ModalHeaderComponent } from './modal-header.component';
import { ModalShellComponent } from './modal-shell.component';

/**
 * Hote global des dialogues centralises (confirmation, succes, erreur, info, avertissement).
 * Monte une seule fois dans `App` ; affiche `DialogService.current`.
 * Accessibilite : dialogue modal, fermeture Echap/fond, focus initial sur l'action la moins destructive.
 */
@Component({
  selector: 'app-dialog-host',
  imports: [ModalHeaderComponent, ModalShellComponent],
  template: `
    @if (dialog()) {
      <app-modal-shell labelledBy="softdemat-dialog-title" (closed)="onDismiss()">
        <app-modal-header />
        <div class="mt-6 flex items-start gap-3">
          <span
            class="flex h-10 w-10 shrink-0 items-center justify-center rounded-full"
            [class]="iconClass()"
            aria-hidden="true"
          >
            @switch (dialog()?.kind) {
              @case ('success') {
                <svg class="h-5 w-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" aria-hidden="true">
                  <path stroke-linecap="round" stroke-linejoin="round" d="M5 13l4 4L19 7" />
                </svg>
              }
              @case ('error') {
                <svg class="h-5 w-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" aria-hidden="true">
                  <path stroke-linecap="round" stroke-linejoin="round" d="M12 8v5m0 3.5v.5M10.3 3.8 2.6 17a2 2 0 0 0 1.7 3h15.4a2 2 0 0 0 1.7-3L13.7 3.8a2 2 0 0 0-3.4 0Z" />
                </svg>
              }
              @case ('warning') {
                <svg class="h-5 w-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" aria-hidden="true">
                  <path stroke-linecap="round" stroke-linejoin="round" d="M12 8v5m0 3.5v.5M10.3 3.8 2.6 17a2 2 0 0 0 1.7 3h15.4a2 2 0 0 0 1.7-3L13.7 3.8a2 2 0 0 0-3.4 0Z" />
                </svg>
              }
              @case ('info') {
                <svg class="h-5 w-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" aria-hidden="true">
                  <path stroke-linecap="round" stroke-linejoin="round" d="M12 11v6m0-10v.5M12 22a10 10 0 1 0 0-20 10 10 0 0 0 0 20Z" />
                </svg>
              }
              @default {
                <svg class="h-5 w-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" aria-hidden="true">
                  <path stroke-linecap="round" stroke-linejoin="round" d="M4 7h16M9 7V5a1 1 0 0 1 1-1h4a1 1 0 0 1 1 1v2m-9 0 1 13a1 1 0 0 0 1 1h8a1 1 0 0 0 1-1l1-13" />
                </svg>
              }
            }
          </span>
          <div class="min-w-0">
            <h2 id="softdemat-dialog-title" class="text-lg font-semibold">{{ dialog()?.title }}</h2>
            <p class="mt-2 text-sm opacity-80">{{ dialog()?.message }}</p>
          </div>
        </div>
        <div class="mt-6 flex flex-col-reverse justify-end gap-3 sm:flex-row">
          @if (dialog()?.cancelText) {
            <button
              type="button"
              class="ui-btn-ghost"
              autofocus
              (click)="onDismiss()"
            >
              {{ dialog()?.cancelText }}
            </button>
            <button
              type="button"
              [class]="confirmClass()"
              (click)="onConfirm()"
            >
              {{ dialog()?.confirmText }}
            </button>
          } @else {
            <button
              type="button"
              class="ui-btn"
              autofocus
              (click)="onConfirm()"
            >
              {{ dialog()?.confirmText }}
            </button>
          }
        </div>
        <p class="mt-6 border-t border-black/10 pt-4 text-center text-xs opacity-60">Softwell · SOFT-DEMAT</p>
      </app-modal-shell>
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DialogHostComponent {
  private readonly dialogs = inject(DialogService);

  readonly dialog = this.dialogs.current;

  readonly iconClass = computed(() => {
    switch (this.dialog()?.kind) {
      case 'success':
        return 'bg-green-500/15 text-green-600 dark:text-green-300';
      case 'error':
        return 'bg-red-500/15 text-red-600 dark:text-red-300';
      case 'warning':
        return 'bg-amber-500/15 text-amber-600 dark:text-amber-300';
      case 'info':
        return 'bg-navy/10 text-navy dark:text-accent';
      default:
        return 'bg-red-500/10 text-red-500';
    }
  });

  readonly confirmClass = computed(() =>
    this.dialog()?.danger ? 'ui-btn-danger' : 'ui-btn',
  );

  onConfirm(): void {
    this.dialogs.close(true);
  }

  onDismiss(): void {
    this.dialogs.close(false);
  }
}
