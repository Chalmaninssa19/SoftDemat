import { HttpContextToken } from '@angular/common/http';

/**
 * Quand `true`, `errorInterceptor` laisse le composant appelant afficher
 * le retour (modale d'erreur unifiee). Evite le doublon toast + modale
 * sur les actions destructives (suppressions, envoi).
 */
export const SKIP_ERROR_TOAST = new HttpContextToken<boolean>(() => false);
