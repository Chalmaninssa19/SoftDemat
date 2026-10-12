import { Injectable, signal } from '@angular/core';
import { ConfirmOptions, DialogState, NotifyOptions } from './dialog.models';

const DEFAULT_CONFIRM_TEXT = 'Confirmer';
const DEFAULT_CANCEL_TEXT = 'Annuler';

/**
 * Service centralise de dialogues SOFT-DEMAT.
 * Remplace les appels natifs `alert()`, `confirm()` et `prompt()`.
 * Un seul dialogue est affiche a la fois via `DialogHostComponent`.
 */
@Injectable({ providedIn: 'root' })
export class DialogService {
  readonly current = signal<DialogState | null>(null);

  private resolver: ((confirmed: boolean) => void) | null = null;

  confirm(options: ConfirmOptions): Promise<boolean> {
    return this.open({
      kind: 'confirm',
      title: options.title,
      message: options.message,
      confirmText: options.confirmText ?? DEFAULT_CONFIRM_TEXT,
      cancelText: options.cancelText ?? DEFAULT_CANCEL_TEXT,
      danger: options.danger ?? false,
    });
  }

  /**
   * Confirmation standard avant toute suppression.
   * Message adapte au contexte quand le libelle de l'element est connu.
   */
  confirmDelete(itemLabel?: string): Promise<boolean> {
    const target = itemLabel?.trim() ? `« ${itemLabel.trim()} »` : 'cet élément';
    return this.confirm({
      title: 'Confirmer la suppression',
      message: `Êtes-vous sûr de vouloir supprimer ${target} ? Cette action peut être irréversible.`,
      confirmText: 'Supprimer',
      cancelText: 'Annuler',
      danger: true,
    });
  }

  success(options: NotifyOptions | string): Promise<boolean> {
    const resolved = typeof options === 'string' ? { message: options } : options;
    return this.open({
      kind: 'success',
      title: resolved.title ?? 'Opération réussie',
      message: resolved.message,
      confirmText: resolved.buttonText ?? 'Fermer',
      cancelText: null,
      danger: false,
    });
  }

  error(options: NotifyOptions | string): Promise<boolean> {
    const resolved = typeof options === 'string' ? { message: options } : options;
    return this.open({
      kind: 'error',
      title: resolved.title ?? 'Une erreur est survenue',
      message: resolved.message,
      confirmText: resolved.buttonText ?? 'Compris',
      cancelText: null,
      danger: false,
    });
  }

  info(options: NotifyOptions | string): Promise<boolean> {
    const resolved = typeof options === 'string' ? { message: options } : options;
    return this.open({
      kind: 'info',
      title: resolved.title ?? 'Information',
      message: resolved.message,
      confirmText: resolved.buttonText ?? 'Fermer',
      cancelText: null,
      danger: false,
    });
  }

  warning(options: NotifyOptions | string): Promise<boolean> {
    const resolved = typeof options === 'string' ? { message: options } : options;
    return this.open({
      kind: 'warning',
      title: resolved.title ?? 'Avertissement',
      message: resolved.message,
      confirmText: resolved.buttonText ?? 'Compris',
      cancelText: null,
      danger: false,
    });
  }

  /** Ferme le dialogue courant. `true` = confirme, `false` = annule. */
  close(confirmed: boolean): void {
    const pending = this.resolver;
    this.resolver = null;
    this.current.set(null);
    pending?.(confirmed);
  }

  cancel(): void {
    this.close(false);
  }

  private open(state: DialogState): Promise<boolean> {
    // Un seul dialogue a la fois : annule le precedent sans declencher d'action.
    this.cancel();
    return new Promise<boolean>((resolve) => {
      this.resolver = resolve;
      this.current.set(state);
    });
  }
}
