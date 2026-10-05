# DEMAT Web — Tickets

Ordre de réalisation : dépendances croissantes, de haut en bas dans chaque epic, et epics dans l’ordre ci-dessous. Le détail métier est dans `DEMAT-WEB-ANALYSE.md`. Ne pas implémenter un ticket dont un prérequis est encore ouvert.

**Échelle :** XS &lt; 0,5 j · S 0,5–1 j · M 2–3 j · L 4–5 j · XL &gt; 5 j.
**Statuts :** `À faire` · `Bloqué` · `Livré — à valider`.

## Organisation

```text
EPIC
 ├── Analyse            DEMAT-AN
 ├── Architecture       DEMAT-AR
 ├── Backend            DEMAT-BE
 ├── Frontend           DEMAT-FE
 ├── UI/UX              DEMAT-UX
 ├── Sécurité           DEMAT-SE
 ├── Tests              DEMAT-QA
 ├── Déploiement        DEMAT-OPS
 └── Documentation      DEMAT-DOC
```

| Ordre | Epic | Tickets | Bloque |
|---|---|---|---|
| 1 | Analyse | AN-001 → AN-006 | le mapping et la recette |
| 2 | Architecture | AR-001 → AR-009 | les socles |
| 3 | UI/UX | UX-001 → UX-004 | les écrans |
| 4 | Backend | BE-001 → BE-022 | les écrans branchés |
| 5 | Frontend | FE-001 → FE-016 | la recette |
| 6 | Sécurité | SE-001 → SE-004 | la prod |
| 7 | Tests | QA-001 → QA-007 | la bascule |
| 8 | Déploiement | OPS-001 → OPS-006 | la prod |
| 9 | Documentation | DOC-001 → DOC-004 | la passation |

La traçabilité n’a aucun ticket d’implémentation.

---

# Epic Analyse

## DEMAT-AN-001 — Valider l’inventaire fonctionnel Desktop

**Epic :** Analyse
**Module :** Produit
**Type :** Analysis
**Priorité :** Haute
**Statut :** Livré — à valider
**Estimation :** S

### Description

L’inventaire des écrans, menus, rôles et flux est rédigé dans `DEMAT-WEB-ANALYSE.md` à partir de `DOCUMENTATION.md` et du code `GIZDT`. Il doit être confirmé par un référent paie.

### Objectif

S’assurer qu’aucune fonction hors traçabilité n’a été oubliée.

### Pré-requis

Aucun.

### Tâches

- Relire la section 2 et la matrice section 12 avec le référent métier.
- Noter les écarts éventuels dans un compte-rendu court ajouté en fin de `DEMAT-WEB-ANALYSE.md`.

### Dépendances

Aucune.

### Fichiers concernés

- `DEMAT-WEB-ANALYSE.md`
- `D:\Projets\DEMAT\DOCUMENTATION.md`

### Critères d'acceptation

- Chaque ligne de la matrice est marquée Confirmée ou corrigée.
- La traçabilité reste explicitement exclue.

### Tests à réaliser

- Parcours commenté des écrans Desktop : connexion, dématérialisation, paramètres, historique, utilisateurs.

---

## DEMAT-AN-002 — Valider les règles métier

**Epic :** Analyse
**Module :** Produit
**Type :** Analysis
**Priorité :** Haute
**Statut :** Livré — à valider
**Estimation :** S

### Description

Les règles RM-01 à RM-22 et les défauts Desktop (section 6) sont extraits du code. Le métier doit confirmer les corrections proposées, en particulier l’échec SMTP qui ne doit plus être archivé comme « envoyé ».

### Objectif

Figer le comportement Web là où le Desktop est ambigu ou défectueux.

### Pré-requis

DEMAT-AN-001.

### Tâches

- Passer en revue RM-01 à RM-22.
- Trancher les lignes de la section 6 (filtres ET, filtre historique par établissement, message de bilan).

### Dépendances

DEMAT-AN-001.

### Fichiers concernés

- `DEMAT-WEB-ANALYSE.md`

### Critères d'acceptation

- Chaque règle est acceptée ou amendée par écrit.
- Aucune règle amendée ne réintroduit la traçabilité.

### Tests à réaliser

- Relecture croisée avec trois cas : e-mail vide, déjà envoyé, établissement filtré.

---

## DEMAT-AN-003 — Relever le schéma SQL réel

**Epic :** Analyse
**Module :** Données
**Type :** Analysis
**Priorité :** Haute
**Statut :** À faire
**Estimation :** M

### Description

Le dépôt ne contient pas de scripts `CREATE`. Les colonnes listées dans l’analyse viennent des requêtes C#. Il faut relever types, clés, index, contraintes, vues, procédures, fonctions et triggers sur SDT, puis les tables Sage lues par la dématérialisation.

### Objectif

Produire `docs/data-model.md` fidèle à la base, sans la modifier.

### Pré-requis

Accès à une base SDT de recette ou restauration contrôlée de `SDT.bak` sur un serveur de recette. Accès lecture au catalogue Sage.

### Tâches

- Relever `G_USERS`, `D_DEMAT`, `D_PMAIL`, `D_PGENERAL`, `T_BDD_SAGE`, `T_SUIVI`, `T_FILTER`.
- Relever `T_SAL`, `T_ETA`, `T_HST_ETABLISSEMENT` pour les colonnes utilisées.
- Lister vues, procédures, fonctions, triggers. Le code applicatif n’en appelle aucune : le confirmer ou l’infirmer.
- Ne pas copier de mot de passe ni de chaîne de connexion dans le document.

### Dépendances

Aucune.

### Fichiers concernés

- `docs/data-model.md` (à créer)
- `D:\Projets\DEMAT\BASE\SDT.bak` (source, ne pas committer)

### Critères d'acceptation

- Chaque colonne citée par le code existe, avec son type.
- Les objets absents sont indiqués « aucun ».
- Aucun secret n’apparaît dans le document.

### Tests à réaliser

- Comparer le relevé aux requêtes de `User.cs`, `Historique.cs`, `PMail.cs`, `Salarie.cs`, `Etablissement.cs`, `BaseSage.cs`.

---

## DEMAT-AN-004 — Cartographier les droits SQL Sage

**Epic :** Analyse
**Module :** Données
**Type :** Analysis
**Priorité :** Haute
**Statut :** À faire
**Estimation :** S

### Description

Le Web ne doit lire que les tables nécessaires à la dématérialisation. Le compte utilisé par le Desktop dans `T_BDD_SAGE` n’est pas forcément adapté.

### Objectif

Définir un compte SQL Sage en lecture seule pour l’API.

### Pré-requis

DEMAT-AN-003.

### Tâches

- Lister les droits minimaux : `SELECT` sur `T_SAL`, `T_ETA`, `T_HST_ETABLISSEMENT`.
- Vérifier que ce compte ne peut pas écrire.
- Documenter le nom logique du compte (pas le mot de passe) dans `docs/data-model.md`.

### Dépendances

DEMAT-AN-003.

### Fichiers concernés

- `docs/data-model.md`

### Critères d'acceptation

- Un essai d’`INSERT` avec ce compte échoue.
- Un `SELECT` des trois tables réussit.

### Tests à réaliser

- Connexion de test depuis un script SQL, hors dépôt.

---

## DEMAT-AN-005 — Inventorier paramètres, fichiers et volumes

**Epic :** Analyse
**Module :** Exploitation
**Type :** Analysis
**Priorité :** Haute
**Statut :** À faire
**Estimation :** S

### Description

L’envoi Web dépend d’informations absentes du code : SMTP, expéditeur, racine des PDF Sage, racine d’archive, taille des PDF, nombre de salariés.

### Objectif

Remplir les paramètres cibles de recette et de production, hors secrets commités.

### Pré-requis

DEMAT-AN-001.

### Tâches

- Recueillir serveur SMTP, port, expéditeur, mode d’authentification.
- Recueillir les chemins réseau des PDF et des archives.
- Estimer le volume mensuel et la taille moyenne d’un bulletin.
- Lister les lignes réelles de `D_PMAIL` (types, pas les contenus s’ils sont confidentiels : au minimum les `MailType` et `MailCode`).

### Dépendances

DEMAT-AN-001.

### Fichiers concernés

- `docs/adr/smtp-et-fichiers.md` (à créer, sans secret)

### Critères d'acceptation

- Chaque paramètre a une valeur de recette ou la mention « non disponible » avec un responsable.
- Aucun mot de passe SMTP dans le fichier.

### Tests à réaliser

- Le compte de service de l’API peut lister la racine PDF et créer un fichier d’essai dans l’archive de recette, puis le supprimer.

---

## DEMAT-AN-006 — Atelier des écarts de bascule

**Epic :** Analyse
**Module :** Produit
**Type :** Analysis
**Priorité :** Haute
**Statut :** À faire
**Estimation :** S

### Description

Trois écarts engagent le métier : fin d’Outlook, dossier réseau à la place du dossier local, et fin possible des mots de passe Desktop (section 7.1).

### Objectif

Obtenir une décision écrite option A (bascule) ou option B (coexistence).

### Pré-requis

DEMAT-AN-002, DEMAT-AN-005.

### Tâches

- Présenter la section 7 de l’analyse.
- Faire signer l’option de mots de passe.
- Confirmer que le bilan partiel d’envoi remplace le message de succès systématique.

### Dépendances

DEMAT-AN-002, DEMAT-AN-005.

### Fichiers concernés

- `DEMAT-WEB-ANALYSE.md`
- `docs/adr/authentification.md`

### Critères d'acceptation

- L’option A ou B est écrite dans l’ADR.
- Le référent accepte SMTP et le dossier réseau, ou décrit une contrainte nouvelle.

### Tests à réaliser

- Aucun test logiciel. Relecture du compte-rendu par le référent.

---

# Epic Architecture

## DEMAT-AR-001 — Figer l’architecture sur les squelettes existants

**Epic :** Architecture
**Module :** Architecture
**Type :** Technical
**Priorité :** Haute
**Statut :** Livré — à valider
**Estimation :** S

### Description

L’architecture est décrite en sections 9 à 14 de `DEMAT-WEB-ANALYSE.md`. Elle respecte les quatre projets .NET déjà créés et transforme le front Angular 22 en workspace MFE sans ajouter de couche hors documents internes.

### Objectif

Faire valider le découpage shell + 3 remotes et l’absence de projet .NET supplémentaire.

### Pré-requis

DEMAT-AN-001.

### Tâches

- Revue d’architecture interne.
- Confirmer Native Federation comme mécanisme MFE.

### Dépendances

DEMAT-AN-001.

### Fichiers concernés

- `DEMAT-WEB-ANALYSE.md`
- `src/SoftDemat.Api`, `src/SoftDemat.Application`, `src/SoftDemat.Domain`, `src/SoftDemat.Infrastructure`
- `src/softdemat.front`

### Critères d'acceptation

- Le compte-rendu ne demande pas de 5e projet .NET.
- Le découpage des trois remotes est accepté.

### Tests à réaliser

- Contrôle des `ProjectReference` : Domain ne référence rien, Application ne référence que Domain, Infrastructure ne référence pas Api.

---

## DEMAT-AR-002 — ADR structure Angular MFE

**Epic :** Architecture
**Module :** Frontend
**Type :** Technical
**Priorité :** Haute
**Statut :** À faire
**Estimation :** S

### Description

`CLAUDE-ARCHI-ANGULAR.md` décrit `core` / `shared` / `features` dans une application. Le produit demande des Micro Frontends. L’ADR explique que chaque remote applique ce document, et que `projects/ui` est la seule librairie visuelle partagée.

### Objectif

Éviter un remote par écran et les imports croisés.

### Pré-requis

DEMAT-AR-001.

### Tâches

- Rédiger `docs/adr/mfe.md` : shell, `mfe-identite`, `mfe-parametrage`, `mfe-dematerialisation`, `ui`, `contracts`.
- Décrire les singletons partagés (session, notifications) et l’interdiction d’importer un remote depuis un autre.

### Dépendances

DEMAT-AR-001.

### Fichiers concernés

- `docs/adr/mfe.md`

### Critères d'acceptation

- Les routes de la section 10 de l’analyse sont reprises.
- Aucune feature d’un remote n’est prévue pour importer une feature d’un autre.

### Tests à réaliser

- Relecture contre les sections 1 et 6 de `CLAUDE-ARCHI-ANGULAR.md`.

---

## DEMAT-AR-003 — ADR deux contextes de données

**Epic :** Architecture
**Module :** Backend
**Type :** Technical
**Priorité :** Haute
**Statut :** À faire
**Estimation :** S

### Description

SDT est en lecture/écriture. Sage est en lecture seule. Un seul `DbContext` masquerait cette frontière.

### Objectif

Décider `SdtDbContext` et `SageDbContext`, sans migration sur les tables existantes.

### Pré-requis

DEMAT-AN-003, DEMAT-AR-001.

### Tâches

- Rédiger `docs/adr/donnees.md`.
- Interdire `SaveChanges` métier sur le contexte Sage.
- Réserver l’unique migration éventuelle à `G_AUTH_SESSION`.

### Dépendances

DEMAT-AN-003, DEMAT-AR-001.

### Fichiers concernés

- `docs/adr/donnees.md`

### Critères d'acceptation

- L’ADR cite les tables de la section 4 de l’analyse.
- Il interdit de remodeler les clés existantes en GUID.

### Tests à réaliser

- Relecture contre la règle EF de `CLAUDE-ARCHI.md`.

---

## DEMAT-AR-004 — ADR authentification et mots de passe

**Epic :** Architecture
**Module :** Sécurité
**Type :** Technical
**Priorité :** Haute
**Statut :** À faire
**Estimation :** S

### Description

L’architecture interne impose BCrypt, JWT court (15–30 min) et refresh révocable avec rotation. Le Desktop stocke un chiffré AES dans `G_USERS.Password`.

### Objectif

Enregistrer l’option A ou B issue de DEMAT-AN-006, le schéma de `G_AUTH_SESSION`, et la durée des jetons.

### Pré-requis

DEMAT-AN-006.

### Tâches

- Rédiger `docs/adr/authentification.md`.
- Décrire la vérification historique puis la réécriture BCrypt si option A.
- Décrire la non-réécriture de `G_USERS.Password` si option B.
- Fixer la réinitialisation : mot de passe conforme à RM-05, changement obligatoire, jamais la valeur fixe du Desktop.

### Dépendances

DEMAT-AN-006.

### Fichiers concernés

- `docs/adr/authentification.md`

### Critères d'acceptation

- L’ADR ne contient ni clé AES, ni vecteur, ni mot de passe d’exemple.
- La durée d’access token est entre 15 et 30 minutes.
- Le refresh est stocké hashé, révocable, rotatif.

### Tests à réaliser

- Contrôle que l’ADR contredit ni RM-05 ni le message de login unique.

---

## DEMAT-AR-005 — ADR fichiers et SMTP

**Epic :** Architecture
**Module :** Dématérialisation
**Type :** Technical
**Priorité :** Haute
**Statut :** À faire
**Estimation :** S

### Description

L’envoi quitte Outlook. Les PDF sont lus sur une racine serveur autorisée. L’archive reprend le chemin métier de la section 2.4 de l’analyse.

### Objectif

Figer le contrat d’envoi : succès SMTP avant archive et avant statut envoyé.

### Pré-requis

DEMAT-AN-005, DEMAT-AN-006.

### Tâches

- Rédiger `docs/adr/smtp-et-fichiers.md` (compléter le relevé AN-005).
- Décrire le refus de tout chemin hors racine configurée.
- Décrire les placeholders et l’arborescence d’archive comme règles de code, pas comme paramètres libres.

### Dépendances

DEMAT-AN-005, DEMAT-AN-006.

### Fichiers concernés

- `docs/adr/smtp-et-fichiers.md`

### Critères d'acceptation

- L’ADR reprend RM-13 à RM-17.
- SMTP est cantonné à Infrastructure.

### Tests à réaliser

- Relecture par un développeur backend et le référent paie.

---

## DEMAT-AR-006 — Stratégie d’erreurs et de journaux

**Epic :** Architecture
**Module :** Transverse
**Type :** Technical
**Priorité :** Moyenne
**Statut :** À faire
**Estimation :** XS

### Description

Le mapping exception → HTTP est celui de `CLAUDE-ARCHI.md`. Le journal fichier `%AppData%` disparaît au profit de Serilog, déjà référencé par l’API.

### Objectif

Une convention unique de logs d’envoi et d’erreurs.

### Pré-requis

DEMAT-AR-001.

### Tâches

- Documenter dans `docs/adr/observabilite.md` les champs autorisés : corrélation, matricule, statut, type d’erreur.
- Interdire mot de passe, jeton, chaîne de connexion, corps de mail.

### Dépendances

DEMAT-AR-001.

### Fichiers concernés

- `docs/adr/observabilite.md`

### Critères d'acceptation

- Le tableau HTTP de l’analyse section 9.4 est repris tel quel.

### Tests à réaliser

- Relecture sécurité.

---

## DEMAT-AR-007 — Stratégie de permissions

**Epic :** Architecture
**Module :** Sécurité
**Type :** Technical
**Priorité :** Haute
**Statut :** À faire
**Estimation :** XS

### Description

Deux rôles seulement. Le masquage de menu ne suffit pas.

### Objectif

Tableau rôle → routes API, appliqué par `[Authorize(Roles)]` côté serveur.

### Pré-requis

DEMAT-AR-004.

### Tâches

- Compléter `docs/adr/authentification.md` avec la matrice : utilisateur = dématérialisation, historique, profil, lecture des paramètres ; administrateur = utilisateurs, connexion Sage, écriture des paramètres.
- Décider si l’écriture des modèles de mail est admin seulement. Hypothèse de l’analyse : oui pour l’écriture, lecture pour tout authentifié. La faire confirmer.

### Dépendances

DEMAT-AR-004.

### Fichiers concernés

- `docs/adr/authentification.md`

### Critères d'acceptation

- Chaque route de la section 9.3 a un rôle.
- `Id = 1` reste absent des listes.

### Tests à réaliser

- Relecture de la matrice par le référent.

---

## DEMAT-AR-008 — Contrat de communication entre MFE

**Epic :** Architecture
**Module :** Frontend
**Type :** Technical
**Priorité :** Moyenne
**Statut :** À faire
**Estimation :** XS

### Description

Les remotes ne partagent pas de store métier. Ils partagent des types et la session du shell.

### Objectif

Décrire le contenu de `projects/contracts` et les singletons federation.

### Pré-requis

DEMAT-AR-002.

### Tâches

- Lister les types : `ApiResponse`, `PaginatedResult`, rôles, routes exposées au menu.
- Décrire l’événement de logout.

### Dépendances

DEMAT-AR-002.

### Fichiers concernés

- `docs/adr/mfe.md`

### Critères d'acceptation

- Aucun service HTTP métier n’est placé dans `contracts` ni dans `ui`.

### Tests à réaliser

- Relecture contre l’interdiction d’import entre features.

---

## DEMAT-AR-009 — Modèle de réponse API et pagination

**Epic :** Architecture
**Module :** Backend
**Type :** Technical
**Priorité :** Haute
**Statut :** À faire
**Estimation :** XS

### Description

Toute réponse est `ApiResponse<T>`. Toute liste accepte `page`, `size`, `search`, `sortBy`, `sortDirection`.

### Objectif

Un contrat unique avant le premier contrôleur métier.

### Pré-requis

DEMAT-AR-001.

### Tâches

- Spécifier les champs `success`, `message`, `data`, `timestamp` et `items`, `totalCount`, `page`, `pageSize`, `totalPages`.
- Fixer les valeurs par défaut de page et de taille, et une taille maximale.

### Dépendances

DEMAT-AR-001.

### Fichiers concernés

- `docs/adr/api.md`

### Critères d'acceptation

- Le contrat est aligné sur `CLAUDE-ARCHI-ANGULAR.md` section 5.

### Tests à réaliser

- Exemple JSON anonymisé pour une liste vide (tableau vide, pas null).

---

# Epic UI/UX

## DEMAT-UX-001 — Tokens de la charte Softwell 2025

**Epic :** UI/UX
**Module :** Design system
**Type :** Technical
**Priorité :** Haute
**Statut :** À faire
**Estimation :** S

### Description

La charte fixe les couleurs `#2E427B`, `#575D8F`, `#BC9C22`, `#E1C340`, `#AA9E39`, `#AEB091`, `#FFFFFF`, `#000000`, et les polices Evolve Sans, Garet, Poppins. Elle ne spécifie pas les composants.

### Objectif

Un thème Tailwind unique, fond blanc, logo non déformé.

### Pré-requis

Fichiers de logo. Polices de marque si les licences sont disponibles. Sinon Poppins seule, documenté.

### Tâches

- Déclarer les couleurs dans la configuration Tailwind sous les noms de la section 11 de l’analyse.
- Charger Poppins. Brancher Evolve Sans et Garet seulement avec des fichiers licenciés.
- Prévoir le logo dans l’en-tête du shell, fond blanc.

### Dépendances

DEMAT-FE-002.

### Fichiers concernés

- `src/softdemat.front/projects/ui`
- Charte `D:\Projets\SoftDemat\(SOFTWELL) CHARTE GRAPHIQUE 2025.pdf`

### Critères d'acceptation

- Aucune couleur hors tokens, hors états sémantiques documentés (succès/erreur dérivés, contrastés).
- Le logo n’est ni étiré ni séparé de son logotype.

### Tests à réaliser

- Capture desktop et mobile de l’en-tête sur fond blanc.

---

## DEMAT-UX-002 — Composants transverses

**Epic :** UI/UX
**Module :** Design system
**Type :** Feature
**Priorité :** Haute
**Statut :** À faire
**Estimation :** L

### Description

Bouton, champ, message d’erreur, tableau, pagination, filtres, modal, toast, loader, état vide, badge « À envoyer » / « Déjà envoyé ».

### Objectif

Des composants muets, OnPush, sans HTTP, réutilisés par les trois remotes.

### Pré-requis

DEMAT-UX-001.

### Tâches

- Implémenter les composants dans `projects/ui`.
- Badge « À envoyer » : fond `#E1C340`, texte `#2E427B`. Badge « Déjà envoyé » : fond `#2E427B`, texte blanc.
- États : défaut, survol, focus, désactivé, erreur, chargement.
- Documenter l’usage en commentaire de dossier court ou story locale, sans nouvelle charte parallèle.

### Dépendances

DEMAT-UX-001.

### Fichiers concernés

- `src/softdemat.front/projects/ui/src`

### Critères d'acceptation

- Aucun composant n’injecte `HttpClient`.
- `ChangeDetectionStrategy.OnPush` partout.
- Focus clavier visible.

### Tests à réaliser

- Test de composant du champ en erreur et du tableau vide.
- Navigation clavier d’une modal.

---

## DEMAT-UX-003 — Gabarit des écrans métier

**Epic :** UI/UX
**Module :** Design system
**Type :** Feature
**Priorité :** Moyenne
**Statut :** À faire
**Estimation :** M

### Description

Gabarit commun : en-tête, menu, zone de filtres, contenu, feedback. Les écrans de la section 10 s’y rangent.

### Objectif

Une navigation identique pour l’identité, le paramétrage et la dématérialisation.

### Pré-requis

DEMAT-UX-002, DEMAT-FE-004.

### Tâches

- Définir la grille des pages liste et des pages formulaire.
- Menu : Accueil, Dématérialisation, Historique, Paramétrage (admin), Utilisateurs (admin), Profil, Déconnexion.
- Aucune entrée Traçabilité.

### Dépendances

DEMAT-UX-002, DEMAT-FE-004.

### Fichiers concernés

- `src/softdemat.front/projects/shell`

### Critères d'acceptation

- Sous 1024 px le menu se replie et les filtres passent sur une colonne.
- L’utilisateur de rôle 0 ne voit pas Utilisateurs ni l’écriture de paramétrage.

### Tests à réaliser

- Vérification visuelle desktop et largeur 390 px sur le shell avec menu fictif.

---

## DEMAT-UX-004 — Accessibilité de base

**Epic :** UI/UX
**Module :** Design system
**Type :** Technical
**Priorité :** Moyenne
**Statut :** À faire
**Estimation :** S

### Description

L’application est utilisée au quotidien par le service paie. Les formulaires et tableaux doivent être utilisables au clavier et annoncer les erreurs.

### Objectif

Un niveau praticable sans audit complet WCAG, avec les défauts bloquants corrigés.

### Pré-requis

DEMAT-UX-002.

### Tâches

- Associer chaque champ à son libellé et à son message d’erreur.
- Donner un nom accessible aux boutons d’icône.
- Annoncer le résultat d’envoi (toast ou zone live).

### Dépendances

DEMAT-UX-002.

### Fichiers concernés

- `src/softdemat.front/projects/ui`

### Critères d'acceptation

- Contraste navy/blanc conforme à un contraste fort (texte blanc sur `#2E427B`).
- Un envoi peut être suivi sans souris jusqu’à la confirmation.

### Tests à réaliser

- Parcours clavier : login, liste, ouverture de modal d’envoi, fermeture.

---

# Epic Backend

## DEMAT-BE-001 — Socle Clean Architecture sur la solution existante

**Epic :** Backend
**Module :** Architecture
**Type :** Technical
**Priorité :** Haute
**Statut :** À faire
**Estimation :** M

### Description

La solution `SoftDemat.slnx` et les quatre projets .NET 10 existent. `Program.cs` ne contient pas encore le pipeline imposé. Le Domain n’a aucun package : le conserver ainsi.

### Objectif

Exceptions typées, `ApiResponse<T>`, middleware, CORS, authentification branchée, rate limiter, health check sans secret.

### Pré-requis

DEMAT-AR-006, DEMAT-AR-009.

### Tâches

- Ajouter les exceptions Domain : `DomainException`, `UnauthorizedException`, `ForbiddenException`, `NotFoundException`, `ConflictException`.
- Ajouter le middleware qui mappe ces exceptions. Pas de `try/catch` dans les contrôleurs.
- Ordonner les middlewares comme dans `CLAUDE-ARCHI.md` section 5.
- Retirer le contrôleur météo de démonstration.
- Enregistrer FluentValidation et AutoMapper.

### Dépendances

DEMAT-AR-006, DEMAT-AR-009.

### Fichiers concernés

- `src/SoftDemat.Domain/Exceptions`
- `src/SoftDemat.Api/Program.cs`
- `src/SoftDemat.Api/Middleware`
- `src/SoftDemat.Application`

### Critères d'acceptation

- `dotnet build` sans warning.
- `SoftDemat.Domain.csproj` reste sans `PackageReference`.
- Une exception non gérée renvoie 500 sans stack au client.
- L’ordre des middlewares est celui du document d’architecture.

### Tests à réaliser

- Test unitaire du mapping d’exception si la logique est extraite. Sinon test d’intégration minimal du middleware.

---

## DEMAT-BE-002 — Configuration des deux connexions SQL

**Epic :** Backend
**Module :** Données
**Type :** Technical
**Priorité :** Haute
**Statut :** À faire
**Estimation :** S

### Description

SDT et Sage sont deux bases. Les chaînes viennent de la configuration, jamais du code. Ne pas réutiliser les fichiers `BASE\sdt` et `BASE\sgdt`.

### Objectif

Deux contextes ouvrables en recette, Sage en lecture.

### Pré-requis

DEMAT-AN-003, DEMAT-AN-004, DEMAT-AR-003, DEMAT-BE-001.

### Tâches

- Ajouter les clés de configuration pour SDT et Sage.
- Créer `SdtDbContext` et `SageDbContext`.
- Désactiver les migrations automatiques au démarrage sur les tables existantes.
- Vérifier la connexion au démarrage via un health check qui ne révèle pas la chaîne.

### Dépendances

DEMAT-AN-003, DEMAT-AN-004, DEMAT-AR-003, DEMAT-BE-001.

### Fichiers concernés

- `src/SoftDemat.Infrastructure/Context`
- `src/SoftDemat.Api/appsettings.json`
- `src/SoftDemat.Api/appsettings.Development.json` (sans secret réel commité)

### Critères d'acceptation

- Aucun mot de passe dans le dépôt.
- Le health check distingue « SDT joignable » et « Sage joignable ».

### Tests à réaliser

- Démarrage contre la recette. Échec propre si une chaîne est absente.

---

## DEMAT-BE-003 — Mapper les tables existantes

**Epic :** Backend
**Module :** Données
**Type :** Technical
**Priorité :** Haute
**Statut :** À faire
**Estimation :** M

### Description

Entités et `IEntityTypeConfiguration` calées sur `docs/data-model.md`. Noms de colonnes réels. Pas de renommage en base.

### Objectif

Lire `G_USERS`, `D_DEMAT`, `D_PMAIL`, `D_PGENERAL`, `T_BDD_SAGE` et les trois tables Sage sans altérer le schéma.

### Pré-requis

DEMAT-BE-002.

### Tâches

- Créer les entités Domain et les configurations Infrastructure.
- `AsNoTracking` prévu pour les lectures (dans les repositories).
- Ne pas mapper `T_SUIVI` ni `T_FILTER` tant qu’aucun cas d’usage ne les lit.

### Dépendances

DEMAT-BE-002.

### Fichiers concernés

- `src/SoftDemat.Domain/Entities`
- `src/SoftDemat.Infrastructure/Configurations`

### Critères d'acceptation

- Une lecture de chaque table métier SDT retourne des lignes de recette.
- Aucune migration n’est générée contre Sage.
- Le Domain reste sans package NuGet.

### Tests à réaliser

- Test d’intégration de lecture sur base de recette, ou test ignoré explicite si la base n’est pas dans la CI, avec un test de mapping sur SQLite seulement si les types le permettent. Sinon test documenté manuel.

---

## DEMAT-BE-004 — Sessions et jetons

**Epic :** Backend
**Module :** Identité
**Type :** Technical
**Priorité :** Haute
**Statut :** À faire
**Estimation :** M

### Description

Table nouvelle `G_AUTH_SESSION` uniquement, selon l’ADR. JWT 15–30 min, refresh long hashé, rotation, révocation.

### Objectif

Login, refresh, logout, sans exposer l’entité.

### Pré-requis

DEMAT-AR-004, DEMAT-BE-003.

### Tâches

- Créer l’entité, la configuration, la migration **uniquement** pour cette table, sur SDT.
- Implémenter `IAuthService` dans Infrastructure : émission, rotation, révocation.
- Contrôleur mince : login anonyme, refresh anonyme, logout authentifié.
- Message d’échec identique que le compte existe ou non.
- Ne pas journaliser le mot de passe ni le jeton.

### Dépendances

DEMAT-AR-004, DEMAT-BE-003.

### Fichiers concernés

- `src/SoftDemat.Domain`
- `src/SoftDemat.Application/DTOs`, `Validators`, `Services/Interfaces`
- `src/SoftDemat.Infrastructure/Services`
- `src/SoftDemat.Api/Controllers/AuthController.cs`

### Critères d'acceptation

- Login correct retourne un access token et pose le refresh.
- Mauvais mot de passe : 401, même message pour un identifiant inconnu.
- Refresh déjà tourné : refus.
- Logout révoque la session.
- Chaque DTO d’entrée a un validateur.
- Actions de contrôleur ≤ 5 lignes, réponse `ApiResponse<T>`.

### Tests à réaliser

- `Login_ValidCredentials_ReturnsTokens`
- `Login_UnknownUser_ReturnsSameErrorAsWrongPassword`
- `Refresh_ReusedToken_ThrowsUnauthorized`
- `Logout_RevokesSession`

---

## DEMAT-BE-005 — Compatibilité des mots de passe historiques

**Epic :** Backend
**Module :** Identité
**Type :** Feature
**Priorité :** Haute
**Statut :** À faire
**Estimation :** M

### Description

Selon l’option A ou B de l’ADR. Le code de chiffrement historique reste isolé dans Infrastructure, secrets hors source (coffre ou configuration secrète), le temps de la bascule.

### Objectif

Un utilisateur Desktop peut se connecter au Web selon l’option retenue, sans affaiblir les nouveaux mots de passe.

### Pré-requis

DEMAT-BE-004, DEMAT-AR-004.

### Tâches

- Implémenter la vérification du secret historique uniquement dans Infrastructure.
- Option A : après succès, remplacer par un hash BCrypt.
- Option B : ne pas modifier `G_USERS.Password`.
- Nouveaux mots de passe : BCrypt seulement.
- Supprimer toute recopie de la clé dans le dépôt, les tickets et les logs.

### Dépendances

DEMAT-BE-004, DEMAT-AR-004.

### Fichiers concernés

- `src/SoftDemat.Infrastructure/Services`

### Critères d'acceptation

- Un mot de passe historique de recette ouvre une session dans les conditions de l’ADR.
- Un mot de passe nouveau est illisible en base (hash BCrypt).
- La clé historique n’est pas dans git.

### Tests à réaliser

- `Login_LegacySecret_Succeeds`
- `Login_AfterUpgrade_DesktopFormatNoLongerRequired` (option A)
- `ChangePassword_StoresBcrypt`

---

## DEMAT-BE-006 — Changement de mot de passe profil

**Epic :** Backend
**Module :** Identité
**Type :** Feature
**Priorité :** Haute
**Statut :** À faire
**Estimation :** S

### Description

Reprend `Profil` : ancien mot de passe, nouveau, confirmation. Règle RM-05.

### Objectif

`PUT /api/auth/password`.

### Pré-requis

DEMAT-BE-005.

### Tâches

- Validateur : longueur ≥ 12, au moins 3 familles, confirmation égale, ancien requis.
- Vérifier l’ancien secret (BCrypt ou historique selon l’état du compte).
- Enregistrer en BCrypt. Révoquer les autres sessions.

### Dépendances

DEMAT-BE-005.

### Fichiers concernés

- `src/SoftDemat.Application/Validators`
- `src/SoftDemat.Api/Controllers/AuthController.cs`

### Critères d'acceptation

- Ancien mot de passe faux : 400 ou 401 selon l’ADR, sans détailler la politique au-delà du message métier.
- Nouveau non conforme : 400 avec messages de validation.
- Succès : les refresh précédents sont révoqués.

### Tests à réaliser

- `ChangePassword_WrongCurrent_Throws`
- `ChangePassword_TooShort_ThrowsDomain`
- `ChangePassword_ThreeFamiliesMissing_ThrowsDomain`
- `ChangePassword_Valid_UpdatesHash`

---

## DEMAT-BE-007 — Administration des utilisateurs

**Epic :** Backend
**Module :** Identité
**Type :** Feature
**Priorité :** Haute
**Statut :** À faire
**Estimation :** M

### Description

CRUD de `G_USERS` réservé à l’administrateur. Liste sans `Id = 1`. Champs nom, identifiant, rôle, PC. Le PC est persisté et n’accorde aucun droit.

### Objectif

`/api/users` paginé.

### Pré-requis

DEMAT-BE-004, DEMAT-AR-007.

### Tâches

- Liste paginée, recherche sur nom et identifiant.
- Création avec mot de passe conforme et confirmation.
- Mise à jour. Mot de passe optionnel : s’il est vide, ne pas l’écraser.
- Suppression avec interdiction de supprimer son propre compte et le compte `Id = 1`.
- Conflit si l’identifiant existe déjà.

### Dépendances

DEMAT-BE-004, DEMAT-AR-007.

### Fichiers concernés

- `src/SoftDemat.Application`
- `src/SoftDemat.Infrastructure/Repositories`, `Services`
- `src/SoftDemat.Api/Controllers/UsersController.cs`

### Critères d'acceptation

- Un utilisateur rôle 0 reçoit 403.
- La liste ne contient pas `Id = 1` et ne renvoie jamais le hash.
- Identifiant dupliqué : 409.
- Collection vide si aucun résultat, pas `null`.

### Tests à réaliser

- `GetUsers_ExcludesSystemAccount`
- `CreateUser_DuplicateUsername_ThrowsConflict`
- `DeleteUser_SystemAccount_ThrowsDomain`
- `UpdateUser_EmptyPassword_KeepsHash`
- `GetUsers_AsUser_ThrowsForbidden`

---

## DEMAT-BE-008 — Réinitialisation conforme

**Epic :** Backend
**Module :** Identité
**Type :** Feature
**Priorité :** Haute
**Statut :** À faire
**Estimation :** S

### Description

Le Desktop force le mot de passe `123`. Le Web refuse cette valeur. L’administrateur déclenche une réinitialisation conforme à RM-05. Le compte doit changer ce mot de passe à la connexion suivante. Le flag de changement obligatoire vit dans `G_AUTH_SESSION` ou une colonne de cette table additive, pas dans une altération non prévue de `G_USERS`, sauf si l’ADR le permet sans casser le Desktop.

### Objectif

`POST /api/users/{id}/password-resets`.

### Pré-requis

DEMAT-BE-007, DEMAT-AR-004.

### Tâches

- Générer ou recevoir un mot de passe temporaire conforme. Ne pas le journaliser.
- Marquer le changement obligatoire.
- Au login, si le flag est posé, la réponse l’indique pour que le front ouvre le profil et bloque le reste.

### Dépendances

DEMAT-BE-007, DEMAT-AR-004.

### Fichiers concernés

- `src/SoftDemat.Infrastructure/Services`
- `src/SoftDemat.Api/Controllers/UsersController.cs`

### Critères d'acceptation

- La valeur `123` est rejetée.
- Après réinitialisation, l’accès métier est refusé tant que le mot de passe n’est pas changé.
- Le temporaire n’apparaît pas dans les logs.

### Tests à réaliser

- `ResetPassword_Value123_Throws`
- `Login_MustChangePassword_BlocksBusinessEndpoints`

---

## DEMAT-BE-009 — Paramètres généraux et modèles de mail

**Epic :** Backend
**Module :** Paramétrage
**Type :** Feature
**Priorité :** Haute
**Statut :** À faire
**Estimation :** M

### Description

`D_PGENERAL` (ligne id 1) : dossier d’archive et CCI. `D_PMAIL` : mise à jour de l’objet et du contenu. Pas d’insertion de type.

### Objectif

`/api/general-parameters` et `/api/mail-templates`.

### Pré-requis

DEMAT-BE-003, DEMAT-AR-007.

### Tâches

- Lecture pour tout authentifié.
- Écriture admin.
- Validateur : CCI vide autorisée (le Desktop l’autorise), sinon format e-mail. Chemin d’archive non vide à l’enregistrement.
- Refuser un id de modèle inconnu.

### Dépendances

DEMAT-BE-003, DEMAT-AR-007.

### Fichiers concernés

- `src/SoftDemat.Api/Controllers`
- `src/SoftDemat.Application/Validators`

### Critères d'acceptation

- La ligne `D_PGENERAL` id 1 est mise à jour, pas dupliquée.
- Aucun `INSERT` dans `D_PMAIL`.
- Un non-admin ne peut pas enregistrer.

### Tests à réaliser

- `UpdateGeneralParameters_AsUser_ThrowsForbidden`
- `UpdateMailTemplate_UnknownId_ThrowsNotFound`
- `UpdateMailTemplate_DoesNotInsert`

---

## DEMAT-BE-010 — Connexion Sage

**Epic :** Backend
**Module :** Paramétrage
**Type :** Feature
**Priorité :** Haute
**Statut :** À faire
**Estimation :** M

### Description

Reprend `SageDb` : enregistrer serveur, base, identifiant, mot de passe, type d’auth, puis tester. Le mot de passe Sage n’est jamais renvoyé par l’API ni journalisé. Tant que le Desktop lit `TMDP` en clair, ne pas chiffrer cette colonne (ADR).

### Objectif

`GET/PUT /api/sage-connections` et `POST /api/sage-connections/tests`.

### Pré-requis

DEMAT-BE-002, DEMAT-BE-009.

### Tâches

- Lire la ligne existante. Masquer le mot de passe dans le DTO (`hasPassword: true`).
- Mettre à jour `T_BDD_SAGE` comme le Desktop (update s’il existe, insert sinon).
- Tester avec le compte configuré, en n’ouvrant qu’une connexion, sans écrire dans Sage.
- `PUT` admin seulement.

### Dépendances

DEMAT-BE-002, DEMAT-BE-009.

### Fichiers concernés

- `src/SoftDemat.Infrastructure/Services`
- `src/SoftDemat.Api/Controllers/SageConnectionsController.cs`

### Critères d'acceptation

- Le GET ne contient pas `TMDP`.
- Un test réussi n’insère rien dans Sage.
- Un serveur injoignable renvoie une erreur métier 400, pas une stack.

### Tests à réaliser

- `GetSageConnection_DoesNotReturnPassword`
- `TestSageConnection_WhenUnreachable_ThrowsDomain`

---

## DEMAT-BE-011 — Référentiel salariés et établissements

**Epic :** Backend
**Module :** Dématérialisation
**Type :** Feature
**Priorité :** Haute
**Statut :** À faire
**Estimation :** M

### Description

Lecture paginée de `T_ETA` et des salariés courants (`DateHist IS NULL`), avec e-mail et intitulé d’établissement. Pas de filtre `EtatPaie` (réservé à la traçabilité).

### Objectif

`GET /api/establishments` et `GET /api/employees`.

### Pré-requis

DEMAT-BE-003.

### Tâches

- Repositories Sage en lecture, `AsNoTracking`, projection DTO.
- Recherche par matricule, nom, code établissement.
- Trim des matricules et intitulés comme le Desktop à l’affichage. Conserver la valeur brute nécessaire au padding RM-09 dans le flux d’envoi.

### Dépendances

DEMAT-BE-003.

### Fichiers concernés

- `src/SoftDemat.Infrastructure/Repositories`
- `src/SoftDemat.Api/Controllers`

### Critères d'acceptation

- Aucune écriture Sage.
- Pagination obligatoire.
- Un salarié dont l’historique d’établissement est clos n’apparaît pas comme établissement courant.

### Tests à réaliser

- `GetEmployees_CurrentEstablishmentOnly`
- `GetEmployees_DoesNotFilterPayState`

---

## DEMAT-BE-012 — Lecture du dossier de bulletins

**Epic :** Backend
**Module :** Dématérialisation
**Type :** Feature
**Priorité :** Haute
**Statut :** À faire
**Estimation :** L

### Description

Équivalent de `Demat.CreateDematByPdf` et du filtre. Le chemin demandé doit rester sous la racine autorisée.

### Objectif

`GET /api/payslip-files` : matricule, nom, prénom, e-mail, établissement, nom de fichier, date de paie, statut, sélection par défaut.

### Pré-requis

DEMAT-BE-011, DEMAT-AR-005.

### Tâches

- N’accepter que les `.pdf` dont le nom matche `{matricule}_{yyyyMMdd}.pdf`.
- Appliquer le padding espaces à 4 caractères (RM-09).
- Joindre le salarié et le dernier `D_DEMAT` du même jour de paie.
- Statut « Déjà envoyé » uniquement si une ligne lue a le statut envoyé.
- Filtres combinés en ET : établissement, salarié, matricule, nom.
- Paginier le résultat. Indiquer `selectedByDefault` pour les « À envoyer ».
- Refuser un chemin qui sort de la racine (normalisation du chemin).

### Dépendances

DEMAT-BE-011, DEMAT-AR-005.

### Fichiers concernés

- `src/SoftDemat.Infrastructure/Services`
- `src/SoftDemat.Application/Validators`

### Critères d'acceptation

- `0042_20260331.pdf` et un matricule court paddé retrouvent le bon salarié.
- Un PDF hors motif est ignoré.
- `..\ ` hors racine est rejeté.
- Les filtres se cumulent.

### Tests à réaliser

- `ParseFile_ShortMatricule_PadsWithSpaces`
- `ParseFile_InvalidName_IsIgnored`
- `ListFiles_PathEscape_ThrowsDomain`
- `ListFiles_FiltersAreCombined`
- `ListFiles_AlreadySentSamePayDay_MarksSent`

---

## DEMAT-BE-013 — Composition des messages

**Epic :** Backend
**Module :** Dématérialisation
**Type :** Feature
**Priorité :** Haute
**Statut :** À faire
**Estimation :** S

### Description

`PMail.ModifyContent` : remplacer `MM`, `AA`, `PNOM`, `NOM`, `MAT` dans cet ordre, mois français, puis `\n` par `<br/>`.

### Objectif

Un composant testable, sans dépendance SMTP.

### Pré-requis

DEMAT-BE-001.

### Tâches

- Extraire la substitution dans un service Infrastructure ou une fonction Domain pure si elle n’a aucun I/O. Préférer Domain si aucun package n’est nécessaire.
- Conserver l’ordre, y compris le fait que `PNOM` est traité avant `NOM`.

### Dépendances

DEMAT-BE-001.

### Fichiers concernés

- `src/SoftDemat.Domain` ou `src/SoftDemat.Infrastructure/Services`

### Critères d'acceptation

- « Bulletin MM AA » en mars 2026 devient « Bulletin mars 2026 ».
- `PNOM` n’est pas abîmé par le remplacement de `NOM`.

### Tests à réaliser

- `ApplyPlaceholders_March2026_WritesFrenchMonth`
- `ApplyPlaceholders_FirstNameSurvivesLastNameReplacement`
- `ApplyPlaceholders_NewLineBecomesBreak`

---

## DEMAT-BE-014 — Archivage des PDF

**Epic :** Backend
**Module :** Dématérialisation
**Type :** Feature
**Priorité :** Haute
**Statut :** À faire
**Estimation :** S

### Description

Arborescence RM-17. Écrasement si le fichier existe. Établissement vide → `Sans Etablissement`. `/` → `-`.

### Objectif

Copie fichier après succès d’envoi, chemin calculé de façon déterministe.

### Pré-requis

DEMAT-AR-005, DEMAT-BE-009.

### Tâches

- Créer les répertoires manquants.
- Nom : `{MailCode}_{MM_yyyy}_{matricule}_{prenom}.pdf`.
- Refuser d’écrire hors du dossier `ArchFolder`.

### Dépendances

DEMAT-AR-005, DEMAT-BE-009.

### Fichiers concernés

- `src/SoftDemat.Infrastructure/Services`

### Critères d'acceptation

- Le chemin produit correspond aux exemples de l’analyse.
- Un échec de copie remonte une exception typée, il n’est pas avalé.

### Tests à réaliser

- `Archive_BuildsFrenchPath`
- `Archive_ReplacesSlashInEstablishment`
- `Archive_EmptyEstablishment_UsesSansEtablissement`
- `Archive_OutsideRoot_Throws`

---

## DEMAT-BE-015 — Envoi SMTP et écriture D_DEMAT

**Epic :** Backend
**Module :** Dématérialisation
**Type :** Feature
**Priorité :** Haute
**Statut :** À faire
**Estimation :** L

### Description

Pour chaque élément sélectionné : e-mail vide → non envoyé sans archive ; sinon SMTP (To, CCI, objet, HTML, PDF) puis archive puis statut envoyé. Si SMTP échoue : non envoyé, pas d’archive. Insert paramétré dans `D_DEMAT`.

### Objectif

`POST /api/dispatches` avec bilan envoyés / non envoyés.

### Pré-requis

DEMAT-BE-012, DEMAT-BE-013, DEMAT-BE-014.

### Tâches

- DTO de sélection : identifiants de fichiers déjà listés, type de mail.
- Client SMTP dans Infrastructure uniquement.
- Écrire le journal structuré (matricule, résultat) sans corps de message.
- Ne pas déclarer le lot réussi s’il contient des échecs : retourner le détail dans `ApiResponse`.
- Propager `CancellationToken`.
- Pas de requête SQL dans une boucle au-delà du nécessaire : préparer les lectures, écrire les résultats sans concaténer le SQL.

### Dépendances

DEMAT-BE-012, DEMAT-BE-013, DEMAT-BE-014.

### Fichiers concernés

- `src/SoftDemat.Infrastructure/Services`
- `src/SoftDemat.Api/Controllers/DispatchesController.cs`

### Critères d'acceptation

- E-mail vide : ligne non envoyée, fichier source intact, pas de copie d’archive.
- SMTP en échec : ligne non envoyée, pas d’archive.
- SMTP en succès puis échec d’archive : ligne non envoyée, erreur visible dans le bilan. Ne pas marquer envoyé.
- CCI vide : envoi sans CCI.
- Un non authentifié reçoit 401.

### Tests à réaliser

- `Dispatch_EmptyEmail_RecordsNotSent`
- `Dispatch_SmtpFailure_DoesNotArchive`
- `Dispatch_Success_ArchivesThenMarksSent`
- `Dispatch_ReturnsPartialSummary`

---

## DEMAT-BE-016 — Historique paginé

**Epic :** Backend
**Module :** Dématérialisation
**Type :** Feature
**Priorité :** Haute
**Statut :** À faire
**Estimation :** M

### Description

Reprend `HistoriqueDemat` : période sur `DateSend`, établissement, salarié, statut. Le filtre établissement doit réellement restreindre les matricules (correction du défaut Desktop).

### Objectif

`GET /api/dispatches`.

### Pré-requis

DEMAT-BE-011, DEMAT-BE-015.

### Tâches

- Filtres et tri paramétrés.
- Projeter matricule, nom, e-mail, date d’envoi, date de paie, statut, nom de fichier.
- Ne pas exposer un chemin serveur exploitable hors téléchargement contrôlé. Si un téléchargement d’archive est retenu, le servir par un endpoint authentifié qui vérifie la racine. Sinon reporter le téléchargement : le Desktop ne télécharge pas l’historique, il affiche la grille. **Ne pas ajouter le téléchargement** tant qu’il n’est pas demandé.

### Dépendances

DEMAT-BE-011, DEMAT-BE-015.

### Fichiers concernés

- `src/SoftDemat.Api/Controllers/DispatchesController.cs`
- `src/SoftDemat.Infrastructure/Repositories`

### Critères d'acceptation

- La période et le statut filtrent `D_DEMAT`.
- L’établissement filtre par les matricules de cet établissement.
- Résultat paginé, liste vide si rien.

### Tests à réaliser

- `GetDispatches_Establishment_FiltersMatricules`
- `GetDispatches_Status_Filters`
- `GetDispatches_Empty_ReturnsEmptyItems`

---

## DEMAT-BE-017 — Tests unitaires identité

**Epic :** Backend
**Module :** Tests
**Type :** Test
**Priorité :** Haute
**Statut :** À faire
**Estimation :** M

### Description

Couvrir les services d’auth et d’utilisateurs. Projet `tests/SoftDemat.Tests` déjà en xUnit, Moq, FluentAssertions.

### Objectif

Les tests cités dans BE-004 à BE-008 sont verts.

### Pré-requis

DEMAT-BE-008.

### Tâches

- Écrire les tests AAA, SUT nommé `sut`.
- Aucun accès SQL réel.

### Dépendances

DEMAT-BE-008.

### Fichiers concernés

- `tests/SoftDemat.Tests`

### Critères d'acceptation

- `dotnet test` vert sur ce projet.
- Cas positifs et négatifs présents pour login, mot de passe, rôles.

### Tests à réaliser

- Exécution de la suite.

---

## DEMAT-BE-018 — Tests unitaires dématérialisation

**Epic :** Backend
**Module :** Tests
**Type :** Test
**Priorité :** Haute
**Statut :** À faire
**Estimation :** M

### Description

Couvrir placeholders, archive, scan, envoi. SMTP et système de fichiers mockés.

### Objectif

Les tests cités dans BE-012 à BE-016 sont verts.

### Pré-requis

DEMAT-BE-016.

### Tâches

- Implémenter les scénarios RM-08 à RM-17.
- Vérifier qu’aucune écriture Sage n’est appelée.

### Dépendances

DEMAT-BE-016.

### Fichiers concernés

- `tests/SoftDemat.Tests`

### Critères d'acceptation

- `dotnet test` vert.
- Le cas e-mail vide et le cas échec SMTP sont tous deux présents.

### Tests à réaliser

- Exécution de la suite.

---

## DEMAT-BE-019 — Tests d’intégration API

**Epic :** Backend
**Module :** Tests
**Type :** Test
**Priorité :** Haute
**Statut :** À faire
**Estimation :** L

### Description

`WebApplicationFactory` contre une base SDT de test dédiée. Ne pas pointer la production.

### Objectif

Contrats HTTP, codes, pagination, 401/403.

### Pré-requis

DEMAT-BE-016, DEMAT-OPS-001.

### Tâches

- Créer `tests/SoftDemat.IntegrationTests`.
- Couvrir login, users interdit au rôle 0, liste dispatch, validation mot de passe.
- Nettoyer les données créées.

### Dépendances

DEMAT-BE-016, DEMAT-OPS-001.

### Fichiers concernés

- `tests/SoftDemat.IntegrationTests`

### Critères d'acceptation

- La suite ne dépend pas de la machine d’un développeur nommé.
- Aucune chaîne secrète commitée. Les secrets de CI sont injectés.

### Tests à réaliser

- Pipeline ou script local documenté.

---

## DEMAT-BE-020 — Durcissement des requêtes

**Epic :** Backend
**Module :** Sécurité
**Type :** Technical
**Priorité :** Haute
**Statut :** À faire
**Estimation :** S

### Description

Le Desktop concatène du SQL dans `Historique.SetQuery` et `BaseSage`. Le Web n’a pas le droit de reproduire ce mode.

### Objectif

Revue ciblée : tout accès SQL passe par EF ou par des paramètres.

### Pré-requis

DEMAT-BE-015, DEMAT-BE-010.

### Tâches

- Relire repositories et services.
- Corriger toute concaténation de valeur utilisateur.

### Dépendances

DEMAT-BE-015, DEMAT-BE-010.

### Fichiers concernés

- `src/SoftDemat.Infrastructure`

### Critères d'acceptation

- Aucune valeur métier interpolée dans une chaîne SQL.
- Revue croisée effectuée.

### Tests à réaliser

- Recherche manuelle `FromSqlRaw`, `ExecuteSqlRaw`, interpolation.

---

## DEMAT-BE-021 — Journalisation métier de l’envoi

**Epic :** Backend
**Module :** Dématérialisation
**Type :** Technical
**Priorité :** Moyenne
**Statut :** À faire
**Estimation :** S

### Description

Remplace `logJJMMAAAA.txt`. Serilog est déjà référencé.

### Objectif

Un événement structuré par bulletin traité.

### Pré-requis

DEMAT-AR-006, DEMAT-BE-015.

### Tâches

- Brancher Serilog (fichier ou sink validé par l’exploitation) sans secret dans la configuration commitée.
- Logguer début de lot, résultat par matricule, fin de lot.

### Dépendances

DEMAT-AR-006, DEMAT-BE-015.

### Fichiers concernés

- `src/SoftDemat.Api/Program.cs`
- `src/SoftDemat.Infrastructure/Services`

### Critères d'acceptation

- Un envoi de recette produit un événement par matricule.
- Le corps du mail et le mot de passe en sont absents.

### Tests à réaliser

- Test avec un sink en mémoire ou relecture d’un fichier de test.

---

## DEMAT-BE-022 — Pagination et tris de toutes les listes

**Epic :** Backend
**Module :** API
**Type :** Technical
**Priorité :** Haute
**Statut :** À faire
**Estimation :** S

### Description

Contrôle final : aucun GET de collection sans pagination.

### Objectif

Conformité à `CLAUDE-ARCHI.md`.

### Pré-requis

DEMAT-BE-016.

### Tâches

- Passer en revue les contrôleurs.
- Aligner les noms `page`, `size`, `search`, `sortBy`, `sortDirection`.

### Dépendances

DEMAT-BE-016.

### Fichiers concernés

- `src/SoftDemat.Api/Controllers`

### Critères d'acceptation

- Users, établissements, salariés, fichiers, historique sont paginés.
- Taille maximale refusée au-delà du plafond de l’ADR API.

### Tests à réaliser

- Appel sans paramètre : page par défaut, pas la table entière.

---

# Epic Frontend

## DEMAT-FE-001 — Transformer le squelette en workspace MFE

**Epic :** Frontend
**Module :** Architecture
**Type :** Technical
**Priorité :** Haute
**Statut :** À faire
**Estimation :** L

### Description

`softdemat.front` est une application Angular 22 unique, zones `core` / `shared` / `features` vides. Il faut un shell et trois remotes, sans descendre de version.

### Objectif

`ng build` du shell et de chaque remote. Le shell charge les remotes.

### Pré-requis

DEMAT-AR-002, DEMAT-AR-008.

### Tâches

- Introduire Native Federation compatible Angular 22.
- Créer `projects/shell`, `mfe-identite`, `mfe-parametrage`, `mfe-dematerialisation`.
- Déplacer le squelette actuel vers le shell.
- Dans chaque app : `core`, `shared`, `features`, routes lazy, `title` sur chaque route.
- Interdire les imports entre remotes par la structure des projets.

### Dépendances

DEMAT-AR-002, DEMAT-AR-008.

### Fichiers concernés

- `src/softdemat.front`

### Critères d'acceptation

- Chaque remote se construit seul.
- Le shell affiche une route de chaque remote en développement.
- Aucun `NgModule` nouveau. Standalone, `inject()`, OnPush.
- `npx ng build` sans erreur ni warning.

### Tests à réaliser

- Démarrage local des quatre applications et navigation vers une route témoin par remote.

---

## DEMAT-FE-002 — Librairies ui et contracts

**Epic :** Frontend
**Module :** Architecture
**Type :** Technical
**Priorité :** Haute
**Statut :** À faire
**Estimation :** M

### Description

`ui` : composants Tailwind sans métier. `contracts` : types `ApiResponse` et `PaginatedResult`, rôles, constantes de routes. Tailwind n’est pas encore dans `package.json`.

### Objectif

Un seul design system consommé par le shell et les remotes.

### Pré-requis

DEMAT-FE-001.

### Tâches

- Ajouter Tailwind au workspace.
- Créer les projets librairie.
- Exposer les types miroirs du contrat BE-009 / AR-009.
- Vérifier que `ui` n’importe ni `core` ni `HttpClient`.

### Dépendances

DEMAT-FE-001.

### Fichiers concernés

- `src/softdemat.front/projects/ui`
- `src/softdemat.front/projects/contracts`

### Critères d'acceptation

- Les trois remotes importent un composant bouton de `ui`.
- Aucune URL d’API dans `ui` ou `contracts`.

### Tests à réaliser

- Build des librairies.

---

## DEMAT-FE-003 — Session, intercepteurs, guards

**Epic :** Frontend
**Module :** Identité
**Type :** Feature
**Priorité :** Haute
**Statut :** À faire
**Estimation :** M

### Description

Jeton d’accès en mémoire. Refresh via cookie ou le mécanisme figé dans l’ADR, à travers un seul `StorageService` s’il faut persister. Intercepteur : `Authorization` puis gestion 401 (refresh une fois, sinon login), 403, 404, 5xx. Guards fonctionnels. Le guard n’est pas la sécurité.

### Objectif

Aucune route métier accessible sans session. Logout propre.

### Pré-requis

DEMAT-FE-001, DEMAT-BE-004.

### Tâches

- `authInterceptor`, `errorInterceptor` dans le shell, partagés aux remotes selon l’ADR MFE.
- Guard authentifié et guard admin.
- Redirection vers `/auth/connexion`.
- `environment.apiUrl` uniquement. Pas d’URL en dur.

### Dépendances

DEMAT-FE-001, DEMAT-BE-004.

### Fichiers concernés

- `src/softdemat.front/projects/shell/src/app/core`

### Critères d'acceptation

- 401 après échec de refresh : retour login.
- 403 : notification « accès interdit », pas de détail technique.
- 5xx : message générique.
- Aucun `localStorage` dispersé.
- Aucun `console.log`.

### Tests à réaliser

- Test de l’intercepteur : ajout du header, refresh unique, échec vers login.

---

## DEMAT-FE-004 — Shell, menu, accueil

**Epic :** Frontend
**Module :** Shell
**Type :** Feature
**Priorité :** Haute
**Statut :** À faire
**Estimation :** M

### Description

Remplace `Dashboard` : utilisateur connecté, nom de la base Sage, menu, déconnexion. Pas d’entrée Traçabilité.

### Objectif

Navigation vers les remotes selon le rôle.

### Pré-requis

DEMAT-FE-003, DEMAT-UX-003.

### Tâches

- Layout en-tête + menu + routeur.
- Accueil `/` avec les informations de session.
- Masquer Utilisateurs et Paramétrage en écriture pour le rôle 0. Le serveur reste décideur.

### Dépendances

DEMAT-FE-003, DEMAT-UX-003.

### Fichiers concernés

- `src/softdemat.front/projects/shell/src/app/core/layout`

### Critères d'acceptation

- Rôle 0 : pas de lien Utilisateurs.
- Déconnexion révoque la session et revient au login.
- Titre de page renseigné.

### Tests à réaliser

- Test du menu selon le rôle. Vérification navigateur desktop et 390 px.

---

## DEMAT-FE-005 — Écran de connexion

**Epic :** Frontend
**Module :** Identité
**Type :** Feature
**Priorité :** Haute
**Statut :** À faire
**Estimation :** S

### Description

Identifiant, mot de passe, action Se connecter. Message d’erreur unique. Entrée clavier soumet le formulaire.

### Objectif

`/auth/connexion` branche `POST /api/auth/login`.

### Pré-requis

DEMAT-FE-003, DEMAT-UX-002, DEMAT-BE-005.

### Tâches

- Feature `auth` dans `mfe-identite`, lazy.
- Reactive form typé, non nullable.
- Si le serveur demande un changement de mot de passe, router vers le profil et bloquer le menu métier.

### Dépendances

DEMAT-FE-003, DEMAT-UX-002, DEMAT-BE-005.

### Fichiers concernés

- `src/softdemat.front/projects/mfe-identite/src/app/features/auth`

### Critères d'acceptation

- Champs vides : validation avant appel.
- Identifiants faux : message unique, pas d’indication « utilisateur inexistant ».
- Succès : accueil.
- OnPush, `inject()`, pas de `subscribe` orphelin.

### Tests à réaliser

- Spec du service : mapping de la réponse et de l’erreur 401.
- Parcours navigateur : login puis accueil.

---

## DEMAT-FE-006 — Écran profil

**Epic :** Frontend
**Module :** Identité
**Type :** Feature
**Priorité :** Haute
**Statut :** À faire
**Estimation :** S

### Description

Ancien mot de passe, nouveau, confirmation. Règles RM-05 affichées via le composant d’erreur partagé. Le serveur reste la source de vérité.

### Objectif

`/profil`.

### Pré-requis

DEMAT-FE-005, DEMAT-BE-006.

### Tâches

- Formulaire typé.
- Dupliquer RM-05 côté client.
- Succès : notification et fin de session si le backend révoque les jetons, puis retour login.

### Dépendances

DEMAT-FE-005, DEMAT-BE-006.

### Fichiers concernés

- `src/softdemat.front/projects/mfe-identite/src/app/features/profil`

### Critères d'acceptation

- Confirmation différente : erreur sans appel, ou appel si le validateur serveur doit répondre. Au minimum le client bloque.
- Mot de passe de moins de 12 caractères : erreur.
- Succès notifié.

### Tests à réaliser

- Spec de validation. Parcours navigateur.

---

## DEMAT-FE-007 — Écran utilisateurs

**Epic :** Frontend
**Module :** Identité
**Type :** Feature
**Priorité :** Haute
**Statut :** À faire
**Estimation :** L

### Description

Liste paginée, recherche, création, édition, suppression confirmée, réinitialisation. Rôle admin. Le compte système n’est pas affiché. Le champ PC est éditable et présenté comme information, pas comme un droit.

### Objectif

`/utilisateurs`.

### Pré-requis

DEMAT-FE-004, DEMAT-BE-008, DEMAT-UX-002.

### Tâches

- Feature `users` : page liste, composants de ligne ou utilisation du tableau partagé.
- Modales de création, édition, suppression, réinitialisation.
- Ne jamais afficher le hash.
- Guard admin.

### Dépendances

DEMAT-FE-004, DEMAT-BE-008, DEMAT-UX-002.

### Fichiers concernés

- `src/softdemat.front/projects/mfe-identite/src/app/features/users`

### Critères d'acceptation

- Liste paginée branchée sur `page`, `size`, `search`, `sortBy`, `sortDirection`.
- Suppression : modal de confirmation.
- Rôle 0 : route bloquée et API 403 affichée proprement si l’URL est forcée.
- Création : mot de passe et confirmation, RM-05.

### Tests à réaliser

- Spec du service users. Parcours admin : créer, modifier, réinitialiser, supprimer. Contrôle qu’un compte de test `Id = 1` n’est pas listé si la recette en a un.

---

## DEMAT-FE-008 — Écran connexion Sage

**Epic :** Frontend
**Module :** Paramétrage
**Type :** Feature
**Priorité :** Haute
**Statut :** À faire
**Estimation :** M

### Description

Serveur, nom de base, identifiant, mot de passe, test, enregistrement. Le mot de passe n’est pas prérempli : indiquer seulement qu’un secret est déjà enregistré.

### Objectif

`/parametrage/sage`, admin.

### Pré-requis

DEMAT-FE-004, DEMAT-BE-010.

### Tâches

- Formulaire typé.
- Action Tester distincte d’Enregistrer.
- Messages de succès et d’échec sans stack.

### Dépendances

DEMAT-FE-004, DEMAT-BE-010.

### Fichiers concernés

- `src/softdemat.front/projects/mfe-parametrage/src/app/features/sage`

### Critères d'acceptation

- Le mot de passe saisi n’est pas réaffiché en clair après rechargement.
- Test injoignable : message métier.
- Rôle 0 : accès refusé.

### Tests à réaliser

- Spec du service. Parcours navigateur avec un serveur de recette ou un échec simulé.

---

## DEMAT-FE-009 — Écran paramètres d’envoi

**Epic :** Frontend
**Module :** Paramétrage
**Type :** Feature
**Priorité :** Haute
**Statut :** À faire
**Estimation :** M

### Description

Dossier d’archive, CCI, choix du type de mail, objet, contenu. Aide visible sur les jetons `MM`, `AA`, `PNOM`, `NOM`, `MAT`. Pas de création de type.

### Objectif

`/parametrage/envoi`.

### Pré-requis

DEMAT-FE-004, DEMAT-BE-009.

### Tâches

- Charger les types existants.
- Enregistrer archive, CCI et le modèle modifié.
- Lecture pour l’utilisateur, écriture réservée à l’admin (boutons masqués + 403 géré).

### Dépendances

DEMAT-FE-004, DEMAT-BE-009.

### Fichiers concernés

- `src/softdemat.front/projects/mfe-parametrage/src/app/features/envoi`

### Critères d'acceptation

- Changer de type recharge objet et contenu.
- Enregistrement notifié.
- Aucun bouton « nouveau type ».

### Tests à réaliser

- Spec du service. Parcours : modifier un objet de recette et vérifier le rechargement. Restaurer la valeur ensuite.

---

## DEMAT-FE-010 — Écran dématérialisation

**Epic :** Frontend
**Module :** Dématérialisation
**Type :** Feature
**Priorité :** Haute
**Statut :** À faire
**Estimation :** XL

### Description

Dossier (sous la racine autorisée), filtres établissement, salarié, matricule, nom, type de mail, effectif, tableau, tout sélectionner, envoi. Les lignes « À envoyer » arrivent cochées. « Déjà envoyé » ne l’est pas.

### Objectif

`/dematerialisation` équivalent fonctionnel de l’écran Desktop.

### Pré-requis

DEMAT-FE-009, DEMAT-BE-012, DEMAT-UX-002.

### Tâches

- Sélecteur de dossier limité aux chemins renvoyés ou validés par l’API. Pas de chemin libre hors contrôle.
- Filtres combinés.
- Tableau paginé avec sélection conservée d’une page à l’autre pour les lignes cochées explicitement. Documenter dans l’écran que l’envoi porte sur la sélection, pas sur les lignes non chargées.
- Effectif = nombre de lignes correspondant aux filtres (total, pas seulement la page).
- États vide, chargement, erreur.

### Dépendances

DEMAT-FE-009, DEMAT-BE-012, DEMAT-UX-002.

### Fichiers concernés

- `src/softdemat.front/projects/mfe-dematerialisation/src/app/features/dematerialisation`

### Critères d'acceptation

- Les libellés « À envoyer » et « Déjà envoyé » sont ceux du métier.
- Tout sélectionner ne coche que les lignes « À envoyer » visibles dans le résultat filtré, comme le Desktop coche les lignes affichées. Le préciser dans l’UI.
- Aucun `HttpClient` dans les composants.
- Feature lazy, OnPush, control flow `@if` / `@for` avec `track`.

### Tests à réaliser

- Spec du store ou du service de sélection.
- Parcours navigateur : filtre établissement, effectif, coche, état vide si dossier sans PDF conforme.

---

## DEMAT-FE-011 — Lancement d’envoi et bilan

**Epic :** Frontend
**Module :** Dématérialisation
**Type :** Feature
**Priorité :** Haute
**Statut :** À faire
**Estimation :** M

### Description

Confirmer avant envoi. Afficher une progression ou un état occupé. Afficher le bilan envoyés / non envoyés. Ne pas afficher un succès global s’il y a des échecs.

### Objectif

Brancher `POST /api/dispatches`.

### Pré-requis

DEMAT-FE-010, DEMAT-BE-015.

### Tâches

- Modal de confirmation avec le nombre sélectionné et le type de mail.
- Désactiver le second clic pendant l’appel (`exhaustMap`).
- Toast ou panneau de bilan. Lien vers l’historique.

### Dépendances

DEMAT-FE-010, DEMAT-BE-015.

### Fichiers concernés

- `src/softdemat.front/projects/mfe-dematerialisation/src/app/features/dematerialisation`

### Critères d'acceptation

- Zéro ligne cochée : pas d’appel.
- E-mail vide côté jeu de test : la ligne apparaît dans le bilan « non envoyé ».
- Double soumission impossible.

### Tests à réaliser

- Spec : second clic ignoré. Parcours sur jeux PDF de recette.

---

## DEMAT-FE-012 — Écran historique

**Epic :** Frontend
**Module :** Dématérialisation
**Type :** Feature
**Priorité :** Haute
**Statut :** À faire
**Estimation :** M

### Description

Période, établissement, salarié, statut, tableau paginé. Remplace `HistoriqueDemat`. Pas d’export traçabilité.

### Objectif

`/historique`.

### Pré-requis

DEMAT-FE-004, DEMAT-BE-016, DEMAT-UX-002.

### Tâches

- Filtres et tableau partagé.
- Dates en affichage fr-FR via un pipe `shared` ou `ui`.
- État vide explicite.

### Dépendances

DEMAT-FE-004, DEMAT-BE-016, DEMAT-UX-002.

### Fichiers concernés

- `src/softdemat.front/projects/mfe-dematerialisation/src/app/features/historique`

### Critères d'acceptation

- Les quatre filtres sont envoyés à l’API.
- Le statut affiché reprend « Envoyé » / « Non envoyé ».
- Pagination fonctionnelle.

### Tests à réaliser

- Spec du service (params HTTP). Parcours navigateur sur des lignes de recette.

---

## DEMAT-FE-013 — Notifications et erreurs

**Epic :** Frontend
**Module :** Transverse
**Type :** Feature
**Priorité :** Moyenne
**Statut :** À faire
**Estimation :** S

### Description

Un service de notification du shell, consommé par les remotes via le singleton federation. Pas de `alert`.

### Objectif

Succès, erreur métier, erreur technique générique.

### Pré-requis

DEMAT-FE-003, DEMAT-UX-002.

### Tâches

- Implémenter le service et le composant toast.
- Brancher l’intercepteur d’erreurs.

### Dépendances

DEMAT-FE-003, DEMAT-UX-002.

### Fichiers concernés

- `src/softdemat.front/projects/shell/src/app/core`
- `src/softdemat.front/projects/ui`

### Critères d'acceptation

- Une 400 métier affiche `message` de `ApiResponse` s’il est sûr.
- Une 500 n’affiche pas la stack.

### Tests à réaliser

- Spec de l’intercepteur sur 400, 403, 500.

---

## DEMAT-FE-014 — Responsive des écrans métier

**Epic :** Frontend
**Module :** UI
**Type :** Technical
**Priorité :** Moyenne
**Statut :** À faire
**Estimation :** M

### Description

Les écrans login, utilisateurs, paramètres, dématérialisation et historique doivent rester utilisables en largeur réduite.

### Objectif

Vérification desktop et mobile de chaque écran branché.

### Pré-requis

DEMAT-FE-007, DEMAT-FE-009, DEMAT-FE-011, DEMAT-FE-012.

### Tâches

- Ajuster les gabarits avec les utilitaires Tailwind existants.
- Tableau : défilement horizontal plutôt que colonnes illisibles.

### Dépendances

DEMAT-FE-007, DEMAT-FE-009, DEMAT-FE-011, DEMAT-FE-012.

### Fichiers concernés

- Projets shell et remotes.

### Critères d'acceptation

- À 1440 px et 390 px : login, liste utilisateurs, dématérialisation et historique n’ont pas de chevauchement.
- Les actions principales restent atteignables.

### Tests à réaliser

- Passage navigateur des quatre écrans aux deux largeurs.

---

## DEMAT-FE-015 — Tests unitaires front

**Epic :** Frontend
**Module :** Tests
**Type :** Test
**Priorité :** Haute
**Statut :** À faire
**Estimation :** M

### Description

Vitest est déjà le lanceur du squelette Angular 22. Pas d’appel réseau réel.

### Objectif

Services data-access et sélection d’envoi couverts.

### Pré-requis

DEMAT-FE-012.

### Tâches

- Specs AAA à côté des fichiers.
- `provideHttpClientTesting` ou l’équivalent Angular 22.

### Dépendances

DEMAT-FE-012.

### Fichiers concernés

- `src/softdemat.front/projects/**/*.spec.ts`

### Critères d'acceptation

- `npx ng test` vert.
- Login, users, payslip-files, dispatches ont au moins un cas nominal et un cas d’erreur.

### Tests à réaliser

- Exécution de la suite.

---

## DEMAT-FE-016 — Cohérence inter-pages

**Epic :** Frontend
**Module :** Dématérialisation
**Type :** Test
**Priorité :** Moyenne
**Statut :** À faire
**Estimation :** S

### Description

Après un envoi, l’historique et la liste dématérialisation doivent montrer le même statut, sans cache périmé.

### Objectif

Pas de divergence entre les deux écrans.

### Pré-requis

DEMAT-FE-011, DEMAT-FE-012.

### Tâches

- Invalider le cache ou recharger à l’activation de route après un envoi.
- Vérifier aussi le retour depuis les paramètres (modèle de mail rechargé).

### Dépendances

DEMAT-FE-011, DEMAT-FE-012.

### Fichiers concernés

- `mfe-dematerialisation`, `mfe-parametrage`

### Critères d'acceptation

- Un bulletin passé à « Envoyé » apparaît ainsi dans l’historique sans rechargement complet du navigateur.
- Un objet de mail enregistré est celui proposé à l’envoi suivant.

### Tests à réaliser

- Parcours navigateur enchaîné : paramétrage → dématérialisation → envoi → historique.

---

# Epic Sécurité

## DEMAT-SE-001 — Secrets hors dépôt

**Epic :** Sécurité
**Module :** Configuration
**Type :** Technical
**Priorité :** Haute
**Statut :** À faire
**Estimation :** S

### Description

Les fichiers d’exemple Desktop contiennent des identifiants. Ils ne doivent pas être recopiés. Les secrets Web vivent dans un coffre ou des variables d’environnement.

### Objectif

Dépôt et images sans secret.

### Pré-requis

DEMAT-BE-002.

### Tâches

- Vérifier `appsettings*.json`, environments Angular, ADR.
- Ajouter un modèle `appsettings.Example.json` avec des placeholders.
- Confirmer que `.gitignore` couvre les fichiers de secrets locaux.

### Dépendances

DEMAT-BE-002.

### Fichiers concernés

- `src/SoftDemat.Api`
- `src/softdemat.front/projects/**/environment*.ts`
- `.gitignore`

### Critères d'acceptation

- Recherche dans le dépôt Web : aucun mot de passe réel, aucune clé AES.
- L’exemple documente les clés sans valeurs de production.

### Tests à réaliser

- Recherche manuelle sur `Password`, `TMDP`, `ConnectionString` dans les sources commises.

---

## DEMAT-SE-002 — Autorisation serveur

**Epic :** Sécurité
**Module :** API
**Type :** Technical
**Priorité :** Haute
**Statut :** À faire
**Estimation :** S

### Description

Relecture de chaque endpoint contre la matrice AR-007. `[AllowAnonymous]` limité au login, au refresh et, s’il existe, au health public.

### Objectif

Aucun oubli d’`[Authorize]`.

### Pré-requis

DEMAT-BE-016, DEMAT-AR-007.

### Tâches

- Contrôler les contrôleurs.
- Ajouter les tests 403 manquants.

### Dépendances

DEMAT-BE-016, DEMAT-AR-007.

### Fichiers concernés

- `src/SoftDemat.Api/Controllers`
- `tests/SoftDemat.IntegrationTests`

### Critères d'acceptation

- Utilisateur rôle 0 : 403 sur users, écriture paramètres, écriture Sage.
- Anonyme : 401 sur dématérialisation.

### Tests à réaliser

- Suite d’intégration des rôles.

---

## DEMAT-SE-003 — CORS, en-têtes, limitation de débit

**Epic :** Sécurité
**Module :** API
**Type :** Technical
**Priorité :** Haute
**Statut :** À faire
**Estimation :** S

### Description

CORS restreint aux origines du shell et des remotes. Rate limiter déjà imposé dans l’ordre des middlewares. En-têtes de base (nosniff, frame). HTTPS.

### Objectif

Surface HTTP durcie sans changer le métier.

### Pré-requis

DEMAT-BE-001, DEMAT-OPS-001.

### Tâches

- Configurer les origines par environnement.
- Plafonner le login.
- Vérifier l’ordre des middlewares après ajout.

### Dépendances

DEMAT-BE-001, DEMAT-OPS-001.

### Fichiers concernés

- `src/SoftDemat.Api/Program.cs`

### Critères d'acceptation

- Une origine inconnue ne passe pas.
- L’ordre `UseExceptionHandler → UseHsts → UseHttpsRedirection → UseCors → UseAuthentication → UseAuthorization → UseRateLimiter → MapControllers` est respecté.

### Tests à réaliser

- Requête login en rafale : limitation effective. Requête CORS depuis une origine non listée : refus.

---

## DEMAT-SE-004 — Revue du secret Sage au repos

**Epic :** Sécurité
**Module :** Paramétrage
**Type :** Technical
**Priorité :** Moyenne
**Statut :** À faire
**Estimation :** S

### Description

`T_BDD_SAGE.TMDP` est en clair pour que le Desktop puisse le relire. Le Web ne doit pas l’aggraver (pas de log, pas de GET).

### Objectif

Documenter le risque résiduel et les conditions d’un chiffrement futur, hors de cette version si la coexistence Desktop demeure.

### Pré-requis

DEMAT-AR-004, DEMAT-BE-010.

### Tâches

- Compléter l’ADR données.
- Vérifier l’absence du secret dans les réponses et les logs.
- Recommander le compte Sage en lecture seule (AN-004) comme mesure immédiate.

### Dépendances

DEMAT-AR-004, DEMAT-BE-010.

### Fichiers concernés

- `docs/adr/donnees.md`
- `docs/adr/authentification.md`

### Critères d'acceptation

- Le risque est écrit, avec la raison de ne pas chiffrer maintenant si l’option de coexistence l’interdit.
- Le GET API ne renvoie pas le secret.

### Tests à réaliser

- Appel GET et lecture des logs pendant un test de connexion.

---

# Epic Tests

## DEMAT-QA-001 — Jeux de recette

**Epic :** Tests
**Module :** Recette
**Type :** Test
**Priorité :** Haute
**Statut :** À faire
**Estimation :** M

### Description

PDF fictifs et base SDT anonymisée. Pas de bulletin réel dans le dépôt.

### Objectif

Un dossier de fixtures couvrant RM-08 à RM-17.

### Pré-requis

DEMAT-AN-002, DEMAT-AN-005.

### Tâches

- Créer des PDF nommés : matricule 4 caractères, matricule court, nom invalide, date invalide.
- Préparer salariés de test : avec e-mail, sans e-mail, autre établissement, établissement dont l’intitulé contient `/`.
- Documenter le jeu dans `docs/recette.md` sans donnée personnelle réelle.

### Dépendances

DEMAT-AN-002, DEMAT-AN-005.

### Fichiers concernés

- `docs/recette.md`
- Jeux hors git si volumineux, chemin décrit dans le document.

### Critères d'acceptation

- Chaque règle RM-08 à RM-17 a au moins un cas.
- Aucun PDF de paie réel n’est commité.

### Tests à réaliser

- Inventaire croisé avec la matrice de l’analyse.

---

## DEMAT-QA-002 — Recette d’identité

**Epic :** Tests
**Module :** Identité
**Type :** Test
**Priorité :** Haute
**Statut :** À faire
**Estimation :** M

### Description

Parcours login, profil, utilisateurs, rôles.

### Objectif

RM-02 à RM-07 validées sur l’UI et l’API.

### Pré-requis

DEMAT-FE-007, DEMAT-QA-001.

### Tâches

- Exécuter les cas et noter le résultat dans le compte-rendu de recette.
- Vérifier l’absence du compte `Id = 1`.
- Vérifier qu’un rôle 0 ne gère pas les utilisateurs même en forçant l’URL.

### Dépendances

DEMAT-FE-007, DEMAT-QA-001.

### Fichiers concernés

- `docs/recette.md`

### Critères d'acceptation

- Tous les cas sont passés ou ont un défaut ouvert.
- Le message de login est identique pour un mauvais mot de passe et un identifiant inconnu.

### Tests à réaliser

- Parcours manuel décrit dans le ticket, plus les tests auto BE-017 et FE-015.

---

## DEMAT-QA-003 — Recette de dématérialisation

**Epic :** Tests
**Module :** Dématérialisation
**Type :** Test
**Priorité :** Haute
**Statut :** À faire
**Estimation :** L

### Description

Scan, filtres ET, présélection, envoi, archive, historique, échec SMTP, e-mail vide.

### Objectif

Le comportement métier du Desktop, corrections de la section 6 incluses, est démontré.

### Pré-requis

DEMAT-FE-016, DEMAT-QA-001, DEMAT-BE-018.

### Tâches

- Exécuter le jeu de fixtures.
- Vérifier l’arborescence d’archive et le nom de fichier.
- Vérifier les lignes `D_DEMAT`.
- Vérifier l’absence d’écriture dans Sage (comptage avant/après ou trace SQL).

### Dépendances

DEMAT-FE-016, DEMAT-QA-001, DEMAT-BE-018.

### Fichiers concernés

- `docs/recette.md`

### Critères d'acceptation

- RM-08 à RM-21 cochées.
- Un échec SMTP ne crée pas de fichier d’archive et n’écrit pas le statut envoyé.
- L’historique filtre bien un établissement.

### Tests à réaliser

- Campagne manuelle plus tests auto d’archive et de placeholders.

---

## DEMAT-QA-004 — Non-régression de la matrice

**Epic :** Tests
**Module :** Produit
**Type :** Test
**Priorité :** Haute
**Statut :** À faire
**Estimation :** M

### Description

Repasser la matrice section 12. Chaque ligne est « Conforme » ou « Exclu ».

### Objectif

Aucune fonction Desktop hors traçabilité sans équivalent vérifié.

### Pré-requis

DEMAT-QA-002, DEMAT-QA-003.

### Tâches

- Mettre à jour la colonne statut dans un exemplaire de recette (copie contrôlée, ou tableau en fin de `docs/recette.md`).
- Ouvrir un défaut pour chaque écart.

### Dépendances

DEMAT-QA-002, DEMAT-QA-003.

### Fichiers concernés

- `docs/recette.md`
- `DEMAT-WEB-ANALYSE.md` (ne pas changer la matrice source : le résultat de recette vit dans `docs/recette.md`)

### Critères d'acceptation

- Zéro ligne « À développer » restante.
- Les quatre lignes traçabilité sont « Exclu » et n’ont pas d’écran.

### Tests à réaliser

- Revue du tableau par le référent.

---

## DEMAT-QA-005 — Compatibilité base existante

**Epic :** Tests
**Module :** Données
**Type :** Test
**Priorité :** Haute
**Statut :** À faire
**Estimation :** M

### Description

Lire un historique `D_DEMAT` déjà produit par le Desktop et des paramètres `D_PMAIL` / `D_PGENERAL` existants.

### Objectif

Le Web n’exige pas une base vierge.

### Pré-requis

DEMAT-BE-016, DEMAT-AN-003.

### Tâches

- Restaurer une sauvegarde anonymisée.
- Afficher l’historique.
- Enregistrer un paramètre puis vérifier que les colonnes non concernées sont intactes.

### Dépendances

DEMAT-BE-016, DEMAT-AN-003.

### Fichiers concernés

- `docs/recette.md`

### Critères d'acceptation

- Les lignes historiques s’affichent avec le bon statut.
- Aucune colonne existante n’a été renommée.
- `T_SUIVI` et `T_FILTER` sont intactes.

### Tests à réaliser

- Comparaison de schéma avant/après passage de l’API (hors création de `G_AUTH_SESSION`).

---

## DEMAT-QA-006 — Tests de charge légers

**Epic :** Tests
**Module :** Dématérialisation
**Type :** Test
**Priorité :** Moyenne
**Statut :** À faire
**Estimation :** S

### Description

Un lot représentatif du volume recueilli en AN-005. Pas une campagne de performance complète.

### Objectif

L’envoi d’un mois type se termine, avec progression perceptible et sans saturation mémoire évidente.

### Pré-requis

DEMAT-AN-005, DEMAT-FE-011.

### Tâches

- Préparer N PDF fictifs, N issu du volume réel.
- Lancer un envoi vers un SMTP de test.
- Noter la durée et les échecs.

### Dépendances

DEMAT-AN-005, DEMAT-FE-011.

### Fichiers concernés

- `docs/recette.md`

### Critères d'acceptation

- Le lot se termine.
- Le bilan a le bon nombre de lignes.
- Aucun statut envoyé sans acceptation SMTP.

### Tests à réaliser

- Un tir. Un second tir après correction si le premier échoue.

---

## DEMAT-QA-007 — Recette métier signée

**Epic :** Tests
**Module :** Produit
**Type :** Test
**Priorité :** Haute
**Statut :** À faire
**Estimation :** M

### Description

Un gestionnaire de paie exécute le scénario de `DOCUMENTATION.md` : filtre établissement, envoi, contrôle des non envoyés, sans passer par la traçabilité.

### Objectif

Acceptation métier avant déploiement.

### Pré-requis

DEMAT-QA-004.

### Tâches

- Séance accompagnée.
- Relever les écarts ressentis (libellés, ordre des colonnes, bilan).
- Ne corriger que les écarts qui contredisent une règle confirmée. Les préférences nouvelles deviennent des demandes séparées, hors cette livraison.

### Dépendances

DEMAT-QA-004.

### Fichiers concernés

- `docs/recette.md`

### Critères d'acceptation

- Compte-rendu signé, ou liste de défauts bloquants vide.
- La traçabilité n’a pas été réclamée comme bloquante : si elle l’est, escalade, pas d’implémentation silencieuse.

### Tests à réaliser

- Scénario Marie décrit dans la documentation Desktop, adapté au Web.

---

# Epic Déploiement

## DEMAT-OPS-001 — Environnements

**Epic :** Déploiement
**Module :** Exploitation
**Type :** Technical
**Priorité :** Haute
**Statut :** À faire
**Estimation :** M

### Description

Recette et production : API, shell, trois remotes, SDT, Sage lecture seule, SMTP, dossiers.

### Objectif

Une fiche d’environnement sans secret, et des secrets injectés hors git.

### Pré-requis

DEMAT-AN-005, DEMAT-SE-001.

### Tâches

- Décrire hôtes, origines CORS, racines de fichiers, bases.
- Créer le compte SQL Sage lecture seule en recette.
- Prévoir la création de `G_AUTH_SESSION` comme seule évolution de schéma.

### Dépendances

DEMAT-AN-005, DEMAT-SE-001.

### Fichiers concernés

- `docs/deploiement.md`

### Critères d'acceptation

- La recette permet un login et un scan de dossier.
- La production n’est pas utilisée pour les tests.

### Tests à réaliser

- Health check des deux bases en recette.

---

## DEMAT-OPS-002 — Build et publication API

**Epic :** Déploiement
**Module :** Backend
**Type :** Technical
**Priorité :** Haute
**Statut :** À faire
**Estimation :** M

### Description

Publication .NET 10, hébergement IIS ou service Windows selon l’existant Softwell. Le document d’exploitation choisit la cible. HTTPS.

### Objectif

Un artefact versionné et un mode de redémarrage.

### Pré-requis

DEMAT-BE-001, DEMAT-OPS-001.

### Tâches

- Pipeline ou script de `dotnet publish`.
- Compte de service avec droits sur l’archive et la racine PDF, sans droit admin inutile.
- Variable d’environnement pour les chaînes et le SMTP.

### Dépendances

DEMAT-BE-001, DEMAT-OPS-001.

### Fichiers concernés

- `docs/deploiement.md`
- Scripts de build s’ils sont ajoutés sous `build/` ou pipeline existant.

### Critères d'acceptation

- L’API de recette démarre après publication.
- Les secrets ne sont pas dans l’artefact.

### Tests à réaliser

- Health check après déploiement recette.

---

## DEMAT-OPS-003 — Build et publication des MFE

**Epic :** Déploiement
**Module :** Frontend
**Type :** Technical
**Priorité :** Haute
**Statut :** À faire
**Estimation :** M

### Description

Le shell et les remotes ont des URL d’environnement. Le cache des `remoteEntry` doit être géré pour qu’une mise à jour de remote soit prise en compte.

### Objectif

Déploiement cohérent des quatre applications.

### Pré-requis

DEMAT-FE-001, DEMAT-OPS-001.

### Tâches

- Configurer les URL de remotes par environnement.
- Publier les statiques (IIS ou équivalent).
- Documenter l’ordre : contracts/ui compilés avec les apps, pas déployés seuls s’ils sont empaquetés.

### Dépendances

DEMAT-FE-001, DEMAT-OPS-001.

### Fichiers concernés

- `src/softdemat.front`
- `docs/deploiement.md`

### Critères d'acceptation

- Le shell de recette charge les trois remotes.
- Une origine incorrecte est refusée par CORS (croisement SE-003).

### Tests à réaliser

- Ouverture du shell recette, navigation vers chaque remote.

---

## DEMAT-OPS-004 — SMTP et dossiers de production

**Epic :** Déploiement
**Module :** Exploitation
**Type :** Technical
**Priorité :** Haute
**Statut :** À faire
**Estimation :** S

### Description

Le compte de service envoie via le SMTP validé et écrit dans l’archive.

### Objectif

Un message de test et un fichier d’archive de test, puis nettoyage.

### Pré-requis

DEMAT-OPS-002, DEMAT-AR-005.

### Tâches

- Tester l’authentification SMTP.
- Tester la création d’un dossier `année\mois\établissement`.
- Restreindre la racine PDF.

### Dépendances

DEMAT-OPS-002, DEMAT-AR-005.

### Fichiers concernés

- `docs/deploiement.md`

### Critères d'acceptation

- Mail de test reçu, CCI comprise si configurée.
- Fichier d’essai créé puis supprimé.
- Un chemin hors racine est refusé par l’API déployée.

### Tests à réaliser

- Essai réel sur la recette.

---

## DEMAT-OPS-005 — Supervision

**Epic :** Déploiement
**Module :** Exploitation
**Type :** Technical
**Priorité :** Moyenne
**Statut :** À faire
**Estimation :** S

### Description

Logs Serilog consultables, alerte simple si l’API ou le SMTP tombe. Pas de donnée de paie dans un outil tiers non validé.

### Objectif

Pouvoir expliquer un envoi « non envoyé » après coup.

### Pré-requis

DEMAT-BE-021, DEMAT-OPS-002.

### Tâches

- Définir la rétention des logs.
- Documenter où lire un lot (corrélation).
- Health check surveillé.

### Dépendances

DEMAT-BE-021, DEMAT-OPS-002.

### Fichiers concernés

- `docs/deploiement.md`
- `docs/adr/observabilite.md`

### Critères d'acceptation

- Un échec SMTP de recette est retrouvé par matricule en moins de quelques minutes.
- Les logs ne contiennent pas de mot de passe.

### Tests à réaliser

- Provoquer un échec SMTP et retrouver la ligne de log.

---

## DEMAT-OPS-006 — Bascule et retour arrière

**Epic :** Déploiement
**Module :** Exploitation
**Type :** Technical
**Priorité :** Haute
**Statut :** À faire
**Estimation :** M

### Description

Lot pilote, puis ouverture aux gestionnaires. Si option A, les comptes migrés ne reviennent pas au Desktop. Le retour arrière est donc : arrêter le Web, restaurer les mots de passe seulement si une sauvegarde de `G_USERS` a été prise avant bascule.

### Objectif

Une procédure de pilote et de rollback écrite.

### Pré-requis

DEMAT-QA-007, DEMAT-OPS-004, DEMAT-AR-004.

### Tâches

- Sauvegarder SDT avant le pilote.
- Limiter le pilote à un établissement.
- Décrire l’arrêt du Web et la restauration de sauvegarde.
- Ne pas prévoir de double envoi Desktop + Web sur les mêmes PDF pendant le pilote.

### Dépendances

DEMAT-QA-007, DEMAT-OPS-004, DEMAT-AR-004.

### Fichiers concernés

- `docs/deploiement.md`

### Critères d'acceptation

- La sauvegarde est réalisée et restaurable en recette au moins une fois.
- Le pilote a un responsable métier et un créneau.
- La procédure interdit l’envoi du même bulletin par les deux canaux.

### Tests à réaliser

- Restauration d’essai en recette. Pilote sur un petit lot.

---

# Epic Documentation

## DEMAT-DOC-001 — Modèle de données confirmé

**Epic :** Documentation
**Module :** Données
**Type :** Documentation
**Priorité :** Haute
**Statut :** À faire
**Estimation :** S

### Description

Mettre au propre le relevé AN-003 pour les développeurs : tables, colonnes, liens applicatifs, objets non utilisés par le Web.

### Objectif

`docs/data-model.md` sert de référence pendant le mapping.

### Pré-requis

DEMAT-AN-003, DEMAT-AN-004.

### Tâches

- Rédiger le document depuis le relevé.
- Marquer `T_SUIVI` et `T_FILTER` « présentes, non utilisées ».
- Décrire `G_AUTH_SESSION` comme ajout.

### Dépendances

DEMAT-AN-003, DEMAT-AN-004.

### Fichiers concernés

- `docs/data-model.md`

### Critères d'acceptation

- Un développeur peut mapper une entité sans rouvrir le code Desktop.
- Aucun secret.

### Tests à réaliser

- Relecture par la personne qui a fait le relevé SQL et par un développeur backend.

---

## DEMAT-DOC-002 — ADR consolidés

**Epic :** Documentation
**Module :** Architecture
**Type :** Documentation
**Priorité :** Haute
**Statut :** À faire
**Estimation :** S

### Description

Les ADR rédigés dans l’epic Architecture sont relus ensemble pour détecter les contradictions.

### Objectif

Un index `docs/adr/README.md` avec le statut de chaque décision.

### Pré-requis

DEMAT-AR-002, DEMAT-AR-003, DEMAT-AR-004, DEMAT-AR-005, DEMAT-AR-006.

### Tâches

- Indexer les ADR.
- Vérifier la cohérence option mots de passe / schéma / SMTP.

### Dépendances

DEMAT-AR-002 à DEMAT-AR-006.

### Fichiers concernés

- `docs/adr`

### Critères d'acceptation

- Chaque ADR a un statut Accepté ou Proposé.
- Aucune contradiction ouverte avec `CLAUDE-ARCHI.md` ou `CLAUDE-ARCHI-ANGULAR.md` sans paragraphe de justification.

### Tests à réaliser

- Relecture architecture.

---

## DEMAT-DOC-003 — Guide de déploiement

**Epic :** Documentation
**Module :** Exploitation
**Type :** Documentation
**Priorité :** Haute
**Statut :** À faire
**Estimation :** S

### Description

Regroupe OPS-001 à OPS-006 en procédure exécutable par une personne qui n’a pas suivi le projet.

### Objectif

`docs/deploiement.md` complet.

### Pré-requis

DEMAT-OPS-006.

### Tâches

- Ordonner prérequis, variables, build, ordre de déploiement, vérification, rollback.
- Ne pas y coller de secret.

### Dépendances

DEMAT-OPS-006.

### Fichiers concernés

- `docs/deploiement.md`

### Critères d'acceptation

- Une personne de l’exploitation peut déployer la recette en suivant le guide seul.
- Le health check et le test SMTP y figurent.

### Tests à réaliser

- Répétition à blanc sur la recette par quelqu’un d’autre que l’auteur.

---

## DEMAT-DOC-004 — Guide des écarts pour les utilisateurs

**Epic :** Documentation
**Module :** Produit
**Type :** Documentation
**Priorité :** Moyenne
**Statut :** À faire
**Estimation :** S

### Description

Note courte pour les gestionnaires : où sont passés les menus, fin de la traçabilité, envoi par le serveur, dossier réseau, nouveau mot de passe, bilan d’envoi.

### Objectif

Éviter les tickets de support « l’écran traçabilité a disparu » et « Outlook ne s’ouvre plus ».

### Pré-requis

DEMAT-QA-007.

### Tâches

- Rédiger `docs/ecarts-utilisateurs.md` en langage métier.
- Inclure les libellés d’écran réels une fois les écrans stabilisés.

### Dépendances

DEMAT-QA-007.

### Fichiers concernés

- `docs/ecarts-utilisateurs.md`

### Critères d'acceptation

- La traçabilité est annoncée comme volontairement absente.
- Le scénario d’envoi mensuel tient en une page.

### Tests à réaliser

- Relecture par le gestionnaire qui a fait la recette.

---

# Couverture

Fonctions Desktop et ticket qui les réalisent.

| Fonction | Tickets | Contrôle |
|---|---|---|
| Connexion SDT côté serveur | OPS-001, BE-002 | pas d’écran poste |
| Connexion Sage | BE-010, FE-008 | QA-004 |
| Login | BE-004, BE-005, FE-005 | QA-002 |
| Shell / menu / rôles | FE-004, AR-007, SE-002 | QA-002 |
| Profil | BE-006, FE-006 | QA-002 |
| Utilisateurs | BE-007, BE-008, FE-007 | QA-002 |
| Paramètres d’envoi | BE-009, FE-009 | QA-003 |
| Scan, filtres, sélection | BE-011, BE-012, FE-010 | QA-003 |
| Envoi, archive, statuts | BE-013, BE-014, BE-015, FE-011 | QA-003 |
| Historique | BE-016, FE-012, FE-016 | QA-003 |
| Traçabilité, colonnes suivies, exports, dictionnaire CSV | aucun | Exclu |

Tickets d’analyse AN-001 et AN-002 : contenu déjà dans `DEMAT-WEB-ANALYSE.md`, en attente de signature métier. AN-003 reste bloquant pour BE-003.
