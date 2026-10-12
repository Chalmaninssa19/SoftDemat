export type PayslipSearchCriterion = 'name' | 'matricule';

export interface PayslipSearchFilter {
  name: string;
  matricule: string;
}

/**
 * Traduit le critere visible (« Par nom » / « Par matricule ») et la saisie
 * en parametres de requete. Recherche partielle (contains) : le backend
 * compare en insensible a la casse. Espaces superflus supprimes, champ vide
 * = aucun filtre texte (les autres criteres actifs s'appliquent).
 */
export function buildPayslipSearchFilter(
  criterion: PayslipSearchCriterion,
  rawValue: string | null | undefined,
): PayslipSearchFilter {
  const term = (rawValue ?? '').trim();
  if (!term) {
    return { name: '', matricule: '' };
  }
  return criterion === 'name'
    ? { name: term, matricule: '' }
    : { name: '', matricule: term };
}
