import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';

export type IconButtonIcon =
  | 'edit'
  | 'delete'
  | 'reset'
  | 'check'
  | 'test'
  | 'filter'
  | 'search'
  | 'clear'
  | 'plus'
  | 'eye'
  | 'download'
  | 'close';

export type IconButtonVariant = 'default' | 'danger' | 'primary';

/**
 * Bouton d'action compact en icone (crayon, corbeille, oeil, ...).
 * Icônes SVG inline (aucune dependance), infobulle `title` + `aria-label` obligatoires,
 * zone tactile 40px, coherent clair/sombre via `.ui-icon-btn`.
 * Les actions principales des formulaires gardent leurs libelles textuels.
 */
@Component({
  selector: 'app-icon-button',
  template: `
    <button
      type="button"
      class="ui-icon-btn"
      [class.ui-icon-btn-danger]="variant() === 'danger'"
      [class.ui-icon-btn-primary]="variant() === 'primary'"
      [attr.aria-label]="ariaLabel()"
      [attr.title]="label()"
      [disabled]="disabled() || loading()"
      [attr.aria-busy]="loading()"
    >
      @if (loading()) {
        <svg class="h-5 w-5 animate-spin" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" aria-hidden="true">
          <path stroke-linecap="round" d="M12 3a9 9 0 1 0 9 9" />
        </svg>
      } @else {
        @switch (icon()) {
          @case ('edit') {
            <svg class="h-5 w-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" aria-hidden="true">
              <path stroke-linecap="round" stroke-linejoin="round" d="M4 20h4L19.5 8.5a2.1 2.1 0 0 0-3-3L5 17v3Z" />
              <path stroke-linecap="round" stroke-linejoin="round" d="m13.5 6.5 3 3" />
            </svg>
          }
          @case ('delete') {
            <svg class="h-5 w-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" aria-hidden="true">
              <path stroke-linecap="round" stroke-linejoin="round" d="M4 7h16M9 7V5a1 1 0 0 1 1-1h4a1 1 0 0 1 1 1v2m-9 0 1 13a1 1 0 0 0 1 1h8a1 1 0 0 0 1-1l1-13" />
            </svg>
          }
          @case ('reset') {
            <svg class="h-5 w-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" aria-hidden="true">
              <path stroke-linecap="round" stroke-linejoin="round" d="M4 4v6h6M20 20v-6h-6" />
              <path stroke-linecap="round" stroke-linejoin="round" d="M20 9A8 8 0 0 0 5.6 5.6L4 7m0 8a8 8 0 0 0 14.4 3.4L20 17" />
            </svg>
          }
          @case ('check') {
            <svg class="h-5 w-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" aria-hidden="true">
              <path stroke-linecap="round" stroke-linejoin="round" d="M5 13l4 4L19 7" />
            </svg>
          }
          @case ('test') {
            <svg class="h-5 w-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" aria-hidden="true">
              <path stroke-linecap="round" stroke-linejoin="round" d="M9 3h6m-3 0v5.5L7.5 18a2 2 0 0 0 1.8 3h5.4a2 2 0 0 0 1.8-3L12 8.5" />
              <path stroke-linecap="round" d="M7.5 14h9" />
            </svg>
          }
          @case ('filter') {
            <svg class="h-5 w-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" aria-hidden="true">
              <path stroke-linecap="round" stroke-linejoin="round" d="M4 5h16l-6 8v6l-4-2v-4L4 5Z" />
            </svg>
          }
          @case ('search') {
            <svg class="h-5 w-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" aria-hidden="true">
              <circle cx="11" cy="11" r="7" />
              <path stroke-linecap="round" d="m20 20-3.5-3.5" />
            </svg>
          }
          @case ('clear') {
            <svg class="h-5 w-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" aria-hidden="true">
              <path stroke-linecap="round" d="M6 6l12 12M18 6 6 18" />
            </svg>
          }
          @case ('plus') {
            <svg class="h-5 w-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" aria-hidden="true">
              <path stroke-linecap="round" d="M12 5v14M5 12h14" />
            </svg>
          }
          @case ('eye') {
            <svg class="h-5 w-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" aria-hidden="true">
              <path stroke-linecap="round" stroke-linejoin="round" d="M2.5 12S6 5 12 5s9.5 7 9.5 7-3.5 7-9.5 7-9.5-7-9.5-7Z" />
              <circle cx="12" cy="12" r="2.5" />
            </svg>
          }
          @case ('download') {
            <svg class="h-5 w-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" aria-hidden="true">
              <path stroke-linecap="round" stroke-linejoin="round" d="M12 4v11m0 0 4-4m-4 4-4-4M4 20h16" />
            </svg>
          }
          @case ('close') {
            <svg class="h-5 w-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" aria-hidden="true">
              <path stroke-linecap="round" d="M6 6l12 12M18 6 6 18" />
            </svg>
          }
        }
      }
    </button>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class IconButtonComponent {
  readonly icon = input.required<IconButtonIcon>();
  /** Libelle accessible : expose en `aria-label` et en infobulle `title`. */
  readonly label = input.required<string>();
  readonly disabled = input(false);
  readonly loading = input(false);
  readonly variant = input<IconButtonVariant>('default');

  readonly ariaLabel = computed(() =>
    this.loading() ? `${this.label()} (chargement…)` : this.label(),
  );
}
