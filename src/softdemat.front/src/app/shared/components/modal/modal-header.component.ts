import { ChangeDetectionStrategy, Component } from '@angular/core';

/**
 * En-tete commun de toutes les modales SOFT-DEMAT.
 * Reprend l'identite visuelle de la carte de connexion (Login) :
 * logo, titre SOFT-DEMAT et sous-titre « Dématérialisation des bulletins ».
 */
@Component({
  selector: 'app-modal-header',
  template: `
    <div class="flex items-center gap-3">
      <img
        src="softDemat-removebg.png"
        alt="SOFT-DEMAT"
        class="h-11 w-11 rounded-xl object-contain"
      />
      <span>
        <span class="block text-sm font-semibold">SOFT-DEMAT</span>
        <span class="block text-xs opacity-70">Dématérialisation des bulletins</span>
      </span>
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ModalHeaderComponent {}
