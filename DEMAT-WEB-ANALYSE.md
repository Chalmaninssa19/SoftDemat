# DEMAT Web — Analyse, architecture cible et plan de migration

**Statut :** analyse préalable. Aucun code métier n’a été écrit.
**Sources :** `D:\Projets\DEMAT\DOCUMENTATION.md`, code `D:\Projets\DEMAT\GIZDT`, `D:\Projets\SoftDemat\CLAUDE-ARCHI.md`, `D:\Projets\SoftDemat\CLAUDE-ARCHI-ANGULAR.md`, `(SOFTWELL) CHARTE GRAPHIQUE 2025.pdf`, squelettes `SoftDemat` (.NET 10) et `softdemat.front` (Angular 22).
**Hors périmètre :** traçabilité (`Tracabilite`, `PTracabilite`, exports CSV/PDF associés, `T_SUIVI`, `T_FILTER`, fichier `%AppData%\GIZDT\list`).

Les tickets d’implémentation sont dans `DEMAT-WEB-TICKETS.md`.

---

## 1. Synthèse

SOFT-DEMAT (projet GIZDT 1.0.0, Softwell Madagascar) est une application WinForms (.NET Framework 4.7.2) qui dématérialise les bulletins de paie produits par Sage Paie : elle lit des PDF, les envoie aux salariés, les archive, et conserve l’historique d’envoi.

La version Web reprend ce métier sur le squelette déjà créé :

| Couche | Squelette existant | Écart à combler |
|---|---|---|
| Backend | `SoftDemat.Domain`, `Application`, `Infrastructure`, `Api` — .NET 10, dossiers Clean Architecture vides, EF Core et BCrypt référencés, Serilog sur l’API | Socle transversal (middleware, `ApiResponse`, JWT, deux bases) puis fonctionnalités |
| Frontend | `softdemat.front`, Angular 22, zones `core` / `shared` / `features` vides, routes vides | Workspace Micro Frontends, Tailwind, écrans |
| Données | Bases Desktop **SDT** (écriture) et **Sage Paie** (lecture) | Mapping sans refonte du schéma métier |

Découpage retenu : un shell et **trois** Micro Frontends (identité, paramétrage, dématérialisation), pas un remote par écran. Le backend reste les **quatre projets** du squelette, organisés par dossiers métier.

---

## 2. Fonctionnement du Desktop

### 2.1 Parcours de démarrage

1. `Program.Main` ouvre `SageDb` (fichier `.prh` Sage).
2. La connexion applicative est lue dans `%AppData%\SDT\sdt` (serveur, base `SDT`, identifiants). Sinon l’écran `AppDb` s’ouvre.
3. Les paramètres Sage sont enregistrés dans `T_BDD_SAGE` (serveur, login, mot de passe, nom de base).
4. `Authentication` compare le mot de passe chiffré (AES/Rijndael, classe `Cipher`) à `G_USERS`.
5. `Dashboard` affiche l’utilisateur, le nom de la base Sage et le menu.

Un double-clic sur le logo de l’écran de connexion révèle un accès masqué vers la configuration Sage. Ce geste caché n’est pas reproduit sur le Web : la configuration Sage devient un écran d’administration explicite.

### 2.2 Modules et écrans

| Écran Desktop | Rôle | Web |
|---|---|---|
| `SageDb` | Import `.prh`, test puis insert/update `T_BDD_SAGE` | Conservé, écran admin |
| `AppDb` | Choix instance SQL, auth Windows ou SQL, écriture du fichier `sdt` | Remplacé par la configuration serveur (pas de fichier par poste) |
| `Authentication` | Login | Conservé |
| `Dashboard` | Menu, déconnexion, accès profil | Shell |
| `Profil` | Changement de mot de passe | Conservé |
| `Dematerialisation` | Cœur métier : filtres, liste, envoi | Conservé |
| `Parametre` | Dossier d’archive, CCI, modèles de mail | Conservé |
| `HistoriqueDemat` | Historique filtré | Conservé |
| `GUsers` / `UpdateUser` | Utilisateurs, réservé admin | Conservé |
| `Tracabilite` / `PTracabilite` | Audit des modifications Sage | **Exclu** |

Menu latéral : Dématérialisation, Traçabilité (exclue), Gestion des profils (si `IdRole = 1`), Se déconnecter. Clic sur le nom d’utilisateur : profil.

### 2.3 Rôles

| `IdRole` | Libellé Desktop | Effet |
|---|---|---|
| `0` | Utilisateur | Menu « Gestion des profils » masqué |
| `1` | Administrateur | Voit et utilise la gestion des profils |

L’utilisateur `Id = 1` est exclu de la liste. Il peut toujours se connecter. Cette règle est conservée.

Le champ `PC` sert, sur le Desktop, de lien avec `T_SUIVI.T_User` pour la traçabilité. Sur le Web le champ reste stocké et éditable afin de ne pas perdre la donnée, mais il n’ouvre aucun droit et ne conditionne pas l’envoi.

### 2.4 Dématérialisation

Filtres : établissement, salarié, matricule, nom, dossier des PDF, type de mail. La liste affiche matricule, nom, prénom, e-mail, état.

Convention de fichier : `{matricule}_{AAAAMMJJ}.pdf` (exemple `0042_20260331.pdf`). Si le préfixe fait moins de 4 caractères, il est complété **à gauche par des espaces** avant la recherche dans `T_SAL.MatriculeSalarie`.

Le salarié courant est celui dont `T_HST_ETABLISSEMENT.DateHist` est nul, joint à `T_ETA`. Le dernier envoi du même jour de paie est lu dans `D_DEMAT`.

États :

| État | Affichage Desktop | Case |
|---|---|---|
| `Status.notSend` (0) | « A envoyer », rouge | Cochée |
| `Status.send` (1) | « Déjà envoyé », bleu | Non cochée |

À l’envoi, pour chaque ligne cochée :

1. E-mail vide ou blanc : journal « non envoyé », ligne `D_DEMAT` en statut non envoyé, pas d’archive.
2. Sinon : envoi Outlook (destinataire, CCI, objet, corps HTML, PDF), copie d’archive, ligne `D_DEMAT` en statut envoyé.
3. Journal texte `%AppData%\GIZDT\logJJMMAAAA.txt`.

Placeholders, dans cet ordre : `MM` (nom du mois fr-FR), `AA` (année), `PNOM`, `NOM`, `MAT`. Les retours ligne deviennent `<br/>`.

Archive :

```text
{ArchFolder}\{année}\{nom du mois fr-FR}\{établissement ou "Sans Etablissement"}\{MailCode}_{MM_yyyy}_{matricule}_{prenom}.pdf
```

Un `/` dans l’intitulé d’établissement est remplacé par `-`.

### 2.5 Paramètres

`D_PGENERAL` (ligne `Id = 1`) : `ArchFolder`, `Cc`.
`D_PMAIL` : `MailType`, `MailObject`, `MailContent`, `MailCode`. L’écran ne crée pas de type : il met à jour objet et contenu des types déjà en base.

### 2.6 Historique

Filtres : période sur `DateSend`, établissement, salarié, statut Non envoyé / Envoyé. Colonnes : matricule (`No`), nom, e-mail (affiché depuis les données jointes côté UI), date d’envoi, état.

### 2.7 Utilisateurs

Création : nom, PC, identifiant, rôle, mot de passe et confirmation.
Mot de passe : au moins 12 caractères et au moins 3 familles parmi majuscule, minuscule, chiffre, caractère spécial (`\W` ou `_`).
Modification via `UpdateUser`. Suppression avec confirmation. Réinitialisation Desktop : mot de passe forcé à la valeur fixe `123` (voir écarts).

---

## 3. Règles métier à conserver

| Id | Règle |
|---|---|
| RM-01 | Deux bases : SDT en lecture/écriture, Sage Paie en lecture seule pour le Web |
| RM-02 | Rôles `0` utilisateur et `1` administrateur. La gestion des utilisateurs est admin |
| RM-03 | Le compte `G_USERS.Id = 1` n’apparaît pas dans la liste |
| RM-04 | Login refusé si identifiant ou mot de passe vide. Message unique si les identifiants sont faux |
| RM-05 | Mot de passe : longueur ≥ 12 et ≥ 3 familles (maj, min, chiffre, spécial) |
| RM-06 | Confirmation obligatoire à la création et au changement |
| RM-07 | Changement de mot de passe : l’ancien mot de passe doit correspondre |
| RM-08 | PDF attendu `{matricule}_{yyyyMMdd}.pdf` |
| RM-09 | Matricule fichier de longueur &lt; 4 : padding espaces à gauche, sur 4 |
| RM-10 | Établissement courant : `T_HST_ETABLISSEMENT.DateHist IS NULL` |
| RM-11 | « Déjà envoyé » si une ligne `D_DEMAT` existe pour le matricule et le jour de `DatePaie`, statut envoyé |
| RM-12 | Lignes « À envoyer » présélectionnées |
| RM-13 | E-mail vide : pas d’envoi, pas d’archive, `D_DEMAT.Status = non envoyé` |
| RM-14 | Envoi réussi : mail (To + CCI), archive, puis `D_DEMAT.Status = envoyé` |
| RM-15 | Échec d’envoi : `D_DEMAT.Status = non envoyé`, pas d’archive |
| RM-16 | Placeholders `MM`, `AA`, `PNOM`, `NOM`, `MAT` dans cet ordre, mois en français |
| RM-17 | Arborescence d’archive décrite en section 2.4, écrasement si le fichier existe |
| RM-18 | CCI et dossier d’archive : singleton `D_PGENERAL` |
| RM-19 | Les types de mail existants sont éditables (objet, contenu). Pas de création implicite |
| RM-20 | Historique filtrable par période, établissement, salarié, statut |
| RM-21 | Sélection globale : tout cocher / tout décocher sur la liste visible |
| RM-22 | Traçabilité non implémentée. Les tables restent en base |

---

## 4. Base de données

Aucun script `CREATE` n’est dans le dépôt. Le dossier `D:\Projets\DEMAT\BASE` contient des sauvegardes (`SDT.bak`, `DEMO.bak`) et des fichiers de connexion d’exemple. Le schéma ci-dessous est **reconstruit depuis les requêtes**. Les types, clés, index, vues, procédures, fonctions et triggers doivent être confirmés sur une base réelle (ticket `DEMAT-AN-003`). Aucune procédure stockée n’est appelée par le code.

### 4.1 SDT (base applicative)

| Table | Colonnes vues dans le code | Usage Web |
|---|---|---|
| `G_USERS` | `Id`, `Name`, `Username`, `Password`, `IdRole`, `PC` | Auth et administration |
| `D_DEMAT` | `Id`, `No`, `Name`, `DateSend`, `FilePath`, `FileName`, `DatePaie`, `Status` | Historique d’envoi |
| `D_PMAIL` | `Id`, `MailType`, `MailObject`, `MailContent`, `MailCode` | Modèles |
| `D_PGENERAL` | `Id`, `Cc`, `ArchFolder` | CCI et archive |
| `T_BDD_SAGE` | `ID`, `SERVEUR`, `TLOGIN`, `TMDP`, `TYPE_AUTH`, `NOM_BD` | Connexion Sage |
| `T_SUIVI` | utilisée par la traçabilité | Non exposée |
| `T_FILTER` | `T_Table`, `T_Column` | Non exposée |

`Status` est lu comme booléen et écrit comme `0`/`1`.

### 4.2 Sage Paie (lecture seule)

| Table | Colonnes utilisées par la dématérialisation |
|---|---|
| `T_SAL` | `MatriculeSalarie`, `Nom`, `Prenom`, `Email` / `EMail`, `SA_CompteurNumero` |
| `T_ETA` | `CodeEtab`, `Intitule` |
| `T_HST_ETABLISSEMENT` | `NumSalarie`, `CodeEtab`, `DateHist` |
| `T_CST`, `T_RUB` | Traçabilité uniquement. Non lues par le Web |

`EtatPaie` (`!= 5` « non actif », `!= 1` « en sommeil ») n’est utilisé que par la traçabilité. La dématérialisation ne filtre pas sur cet état.

### 4.3 Intégrité constatée dans le code

- Pas de clé étrangère exprimée dans l’application. Les liens sont applicatifs : `D_DEMAT.No` = matricule Sage, `T_HST_ETABLISSEMENT.NumSalarie` = `T_SAL.SA_CompteurNumero`, `T_HST_ETABLISSEMENT.CodeEtab` = `T_ETA.CodeEtab`.
- `D_PGENERAL.Id = 1` est traité comme une ligne unique.
- Les écritures Sage sont absentes du périmètre dématérialisation. Le Web ne doit pas obtenir de droit d’écriture sur Sage.

### 4.4 Modification de schéma autorisée

Aucune colonne métier existante n’est renommée ni supprimée. Le seul ajout envisagé, justifié par `CLAUDE-ARCHI.md` (JWT + refresh révocable), est une **table nouvelle** de sessions, sans impact sur les `SELECT`/`INSERT` explicites du Desktop :

`G_AUTH_SESSION` (`Id` uniqueidentifier, `UserId`, `RefreshTokenHash`, `ExpiresAt`, `RevokedAt`, `CreatedAt`).

Elle n’est créée qu’après validation du ticket `DEMAT-AR-004`. Pas de migration EF qui altère Sage ou les tables SDT existantes.

---

## 5. Dépendances externes

| Dépendance Desktop | Rôle | Cible Web |
|---|---|---|
| SQL Server, ADO.NET | SDT et Sage | EF Core, SQL paramétré, deux `DbContext` |
| Microsoft Outlook COM | Envoi | SMTP dans Infrastructure uniquement |
| PDFsharp / MigraDoc, CsvHelper | Exports traçabilité | Non repris |
| `ultimaXDeathUI`, `UXDComboBox` | UI WinForms | Tailwind, charte Softwell |
| Fichier `%AppData%\SDT\sdt` | Connexion SDT par poste | Configuration serveur (secrets hors code) |
| Fichier `.prh` | Chaîne de connexion Sage | Écran admin, mêmes champs |
| Dossier local des PDF | Entrée métier | Dossier réseau visible par l’API |
| Journal `%AppData%\GIZDT\log*.txt` | Trace technique | Serilog structuré. Le métier reste dans `D_DEMAT` |

`ReadSignature()` (signature Outlook RTF) n’est pas appelé par l’envoi. Il n’est pas repris.

---

## 6. Défauts Desktop connus

Ces points ne sont pas des règles métier. Le Web implémente l’intention documentée.

| Défaut | Comportement observé | Comportement Web |
|---|---|---|
| `Demat.SendEmailOutlook` avale les exceptions | L’archive et le statut « envoyé » peuvent être écrits même si Outlook a échoué | Archive et statut « envoyé » seulement après succès SMTP |
| `Demat.Filtre` | Chaque critère **remplace** le précédent au lieu de les combiner | Filtres combinés (ET) |
| `Historique.GetHistorique` | `No in {@list}` ne développe pas la liste des matricules | Filtre établissement réellement appliqué, SQL paramétré |
| `Historique.SetQuery` et `BaseSage` | SQL concaténé | SQL paramétré |
| `CreateArchiveFolder` | Teste `File.Exists` sur le même chemin pour chaque niveau | Création réelle de l’arborescence |
| Message de fin d’envoi | Toujours « succès », y compris en échec partiel | Bilan : envoyés / non envoyés |
| Réinitialisation | Mot de passe forcé à `123`, incompatible avec RM-05 et avec BCrypt | Réinitialisation conforme à RM-05, changement obligatoire à la prochaine connexion |
| `PMail.GetMailById` | N’affecte pas le résultat | Non reproduit. Lecture par identifiant correcte |

---

## 7. Écarts Desktop → Web (justifiés)

| Sujet | Desktop | Web | Justification |
|---|---|---|---|
| Traçabilité | Écrans, CSV, PDF, `T_SUIVI`, `T_FILTER` | Absente | Décision produit explicite |
| Transport mail | Outlook du poste | SMTP serveur | Un serveur Web ne pilote pas Outlook. Destinataire, CCI, objet, corps, pièce jointe : identiques |
| Choix du dossier PDF | Explorateur du poste | Parcours d’une racine réseau autorisée, configurée sur le serveur | Le navigateur ne peut pas donner à l’API un chemin local du poste. Le contrat de nommage des PDF ne change pas |
| Journal fichier | `%AppData%\GIZDT` | Journaux serveur structurés | Pas de profil Windows par utilisateur métier sur le serveur. `D_DEMAT` reste le journal métier |
| Mots de passe | AES puis comparaison du chiffré | BCrypt | Imposé par `CLAUDE-ARCHI.md`. Voir décision d’authentification |
| Identifiants exposés | Entiers / chaînes existants | Conservés | La règle « GUID » de l’architecture ne s’applique pas aux clés déjà en base. Les nouvelles sessions utilisent un GUID |
| Liste | Non paginée | Paginée (`page`, `size`, `search`, `sortBy`, `sortDirection`) | Imposé par l’architecture. L’envoi porte sur la sélection explicite, pas sur « la page courante » seulement si l’utilisateur a coché toute la sélection voulue |
| Accès Sage caché | Double-clic logo | Écran admin | Même fonction, visible et autorisée |
| Couleurs d’état | Rouge / bleu hors charte | Navy `#2E427B` et or `#E1C340` | Charte 2025. Les libellés « À envoyer » et « Déjà envoyé » ne changent pas |
| Réinitialisation `123` | Oui | Non | Contredit RM-05 et BCrypt |

Aucune autre fonctionnalité n’est retirée.

### 7.1 Décision d’authentification (à trancher avant le développement du login)

Le Desktop chiffre le mot de passe et stocke le résultat dans `G_USERS.Password`. Dès qu’une ligne est réécrite en BCrypt, **ce compte ne peut plus se connecter au Desktop**.

| Option | Effet |
|---|---|
| A — bascule (recommandée si le Web remplace le Desktop) | Le Web accepte encore le chiffré historique au premier login, puis réécrit un hash BCrypt. Le Desktop cesse de fonctionner pour ce compte |
| B — coexistence | Le Web vérifie BCrypt s’il est présent dans `G_AUTH_SESSION` / un marqueur, sinon le chiffré historique, **sans réécrire** `G_USERS.Password`. Le Desktop continue de fonctionner. Écart assumé à « BCrypt uniquement » pendant la coexistence |

Hypothèse du plan : **option A**, sauf décision contraire au ticket `DEMAT-AR-004`.

`T_BDD_SAGE.TMDP` est stocké en clair. Le Web ne le journalise pas. Un chiffrement au repos changerait la valeur lue par le Desktop : interdit tant que la coexistence sur cette table est possible. Le compte SQL Sage du Web doit être en lecture seule.

---

## 8. Points sensibles et risques

| Risque | Impact | Mitigation |
|---|---|---|
| Schéma réel différent du code | Mapping EF faux | `DEMAT-AN-003` bloquant avant les entités |
| Bascule des mots de passe | Utilisateurs Desktop bloqués | Décision A/B signée |
| Dossier PDF ou archive inaccessible au serveur | Envoi impossible | Droits du compte de service, ticket ops |
| SMTP rejeté, pièces jointes lourdes | Bulletins non partis | Timeout, taille max, bilan partiel, pas de statut envoyé sans succès |
| Volume Sage (`T_SAL` complet à chaque filtre) | Lenteur | Requêtes filtrées, `AsNoTracking`, pagination |
| Secrets dans `BASE\sdt` et `BASE\sgdt` | Fuite d’identifiants | Ne pas les copier dans le dépôt Web ni dans la configuration commitée |
| Polices Evolve Sans et Garet | Charte incomplète si les fichiers ne sont pas fournis | Poppins pour le texte, polices de marque dès réception des licences |
| Défauts historiques `D_DEMAT` | Des lignes « envoyé » peuvent correspondre à un mail Outlook avorté | Recette sur un jeu récent, pas de correction rétroactive des données |
| Deux contextes EF | Couplage accidentel, écriture Sage | `SageDbContext` sans `SaveChanges` métier, compte SQL lecture seule |

---

## 9. Architecture cible

```text
                         DEMAT WEB
                             │
         ┌───────────────────┴────────────────────┐
         │                                        │
      FRONTEND                                 BACKEND
   Angular 22 MFE                          ASP.NET Core .NET 10
   Shell + 3 remotes                       Clean Architecture
   Tailwind                                REST + ApiResponse<T>
         │                                        │
         └──────────────────┬─────────────────────┘
                            │
                      SQL SERVER
                 ┌──────────┴──────────┐
              SDT (R/W)            Sage Paie (R/O)
```

### 9.1 Frontend — Micro Frontends

`CLAUDE-ARCHI-ANGULAR.md` impose `core` / `shared` / `features` dans une application Angular (lazy loading, signals, OnPush, pas d’import entre features). Il ne décrit pas Native Federation. Le besoin produit impose les MFE.

**Décision :** le workspace devient un shell et des remotes. **Chaque** application respecte le document Angular en interne. Une librairie `ui` porte uniquement des composants sans HTTP ni métier (l’équivalent de `shared`). Aucun remote n’importe un autre remote.

| Application | Route hôte | Domaine | Pourquoi ce découpage |
|---|---|---|---|
| `shell` | `/` | Cadre, session, menu | Chargé une fois. N’a pas de métier paie |
| `mfe-identite` | `/auth`, `/profil`, `/utilisateurs` | Comptes et session | Cycle de revue sécurité distinct |
| `mfe-parametrage` | `/parametrage` | Sage, CCI, archive, modèles | Secrets et configuration, peu d’écrans, public admin |
| `mfe-dematerialisation` | `/dematerialisation`, `/historique` | Envoi des bulletins | Cœur métier quotidien |

Trois remotes suffisent. La traçabilité n’a pas de remote.

Communication :

- Singletons partagés par Native Federation : session (jeton en mémoire), notifications.
- Contrat npm interne `@softdemat/contracts` : `ApiResponse<T>`, `PaginatedResult<T>`, codes de permission. Pas de logique.
- Événement de déconnexion émis par le shell. Pas de store global métier.
- Les guards du shell sont de l’UX. L’API refuse sans jeton ou sans rôle.

État : signals dans la feature concernée. Pas de NgRx (absent du document Angular, inutile pour ce périmètre).

### 9.2 Backend — quatre projets existants

Pas de nouveau projet par domaine. Cela casserait le squelette et `CLAUDE-ARCHI.md`.

| Projet | Responsabilité DEMAT |
|---|---|
| `SoftDemat.Domain` | Entités, enums (`UserRole`, `SendStatus`), exceptions, interfaces de repositories. Zéro package |
| `SoftDemat.Application` | DTO records, validateurs, interfaces de services, profils AutoMapper |
| `SoftDemat.Infrastructure` | `SdtDbContext`, `SageDbContext` (lecture), repositories, services, BCrypt, JWT, SMTP, fichiers |
| `SoftDemat.Api` | Contrôleurs minces, middleware d’exceptions, auth, CORS, rate limiter |

`SageDbContext` ne participe pas aux migrations et n’est pas utilisé pour écrire.

Ordre des middlewares, non négociable : `UseExceptionHandler → UseHsts → UseHttpsRedirection → UseCors → UseAuthentication → UseAuthorization → UseRateLimiter → MapControllers`.

### 9.3 API (prévision)

Préfixe `api/`. Listes paginées. Réponses `ApiResponse<T>`. `[Authorize]` par défaut.

| Méthode | Route | Rôle | Rôle métier |
|---|---|---|---|
| POST | `/api/auth/login` | anonyme | Connexion |
| POST | `/api/auth/refresh` | anonyme | Rotation du refresh |
| POST | `/api/auth/logout` | authentifié | Révocation |
| PUT | `/api/auth/password` | authentifié | Profil |
| GET/POST/PUT/DELETE | `/api/users` | admin | Profils. `Id = 1` filtré en lecture liste |
| POST | `/api/users/{id}/password-resets` | admin | Réinitialisation conforme |
| GET/PUT | `/api/sage-connections` | admin | `T_BDD_SAGE` |
| GET/PUT | `/api/general-parameters` | authentifié lecture, admin écriture | CCI, archive |
| GET/PUT | `/api/mail-templates` | idem | `D_PMAIL` |
| GET | `/api/establishments` | authentifié | `T_ETA` |
| GET | `/api/employees` | authentifié | `T_SAL` + établissement courant |
| GET | `/api/payslip-files` | authentifié | Scan du dossier autorisé |
| POST | `/api/dispatches` | authentifié | Envoi de la sélection |
| GET | `/api/dispatches` | authentifié | Historique `D_DEMAT` |

Les verbes dans l’URL sont évités. `password-resets` et `dispatches` nomment la ressource créée.

### 9.4 Erreurs et journaux

| Exception | HTTP |
|---|---|
| `DomainException` | 400 |
| `UnauthorizedException` | 401 |
| `ForbiddenException` | 403 |
| `NotFoundException` | 404 |
| `ConflictException` | 409 |
| Non gérée | 500, détail masqué |

Login : même message que l’identifiant existe ou non. Aucun mot de passe, jeton ou chaîne de connexion dans les logs. Chaque envoi journalise matricule, résultat, corrélation. Pas l’adresse en clair si la politique interne l’interdit : à défaut, l’adresse est une donnée métier déjà stockée dans Sage ; le log d’envoi reprend le matricule et le statut, pas le corps du mail.

---

## 10. Écrans Web

| Route | Écran | Remplace |
|---|---|---|
| `/auth/connexion` | Connexion | `Authentication` |
| `/` | Accueil : utilisateur, base Sage, raccourcis | `Dashboard` |
| `/profil` | Ancien mot de passe, nouveau, confirmation | `Profil` |
| `/utilisateurs` | Liste, création, édition, suppression, réinitialisation | `GUsers`, `UpdateUser` |
| `/parametrage/sage` | Serveur, base, login, test de connexion | `SageDb` |
| `/parametrage/envoi` | Archive, CCI, modèles | `Parametre` |
| `/dematerialisation` | Dossier, filtres, liste, envoi, progression | `Dematerialisation` |
| `/historique` | Période, filtres, tableau paginé | `HistoriqueDemat` |

Pas d’écran de connexion à SDT dans l’UI métier : la chaîne SDT est une configuration de déploiement.

---

## 11. UI / UX — charte Softwell 2025

Le PDF (11 pages) fixe l’identité, pas une bibliothèque de composants. Les composants sont dérivés de ces tokens.

| Token | Valeur | Usage |
|---|---|---|
| Navy | `#2E427B` | Actions principales, navigation active, état « Déjà envoyé » |
| Navy secondaire | `#575D8F` | Liens, focus, en-têtes secondaires |
| Or | `#BC9C22` | Accent, survol fort |
| Or clair | `#E1C340` | Surlignage, état « À envoyer » |
| Or profond | `#AA9E39` | Bordures d’accent |
| Grège | `#AEB091` | Bordures, textes secondaires |
| Blanc / encre | `#FFFFFF` / `#000000` | Fond de page, texte |
| Titres | Evolve Sans | Dès licence reçue |
| Sous-titres | Garet | Dès licence reçue |
| Texte | Poppins | Corps, formulaires, tableaux |

Règles d’application : logo sur fond uni, de préférence blanc, proportions intactes, symbole et logotype non dissociés. Pages sur fond blanc, marges généreuses, pas de dégradés hors or/navy. Boutons navy, texte blanc. Bouton secondaire : contour navy. Danger (suppression) : encre, pas un rouge hors charte saturé ; la confirmation modale porte l’avertissement.

Composants communs : bouton, champ, erreur de champ, tableau paginé, filtres, modal, toast, loader, état vide, badge de statut. Tous en classes Tailwind, dans la librairie `ui`. Pas de CSS au fil de l’eau dans les features.

Accessibilité : contraste navy/blanc, focus visible, libellés de champs, tableaux avec en-têtes, cibles tactiles suffisantes, formulaires utilisables au clavier. Responsive : menu replié sous 1024 px, filtres empilés, tableau scrollable horizontalement.

---

## 12. Matrice Desktop → Web

| Fonctionnalité Desktop | Module Web | Écran Web | API | Tables | Statut |
|---|---|---|---|---|---|
| Connexion base applicative SDT | Paramétrage serveur | Aucun écran métier | Configuration | SDT (connexion) | À développer (ops, pas UI poste) |
| Import fichier `.prh` / connexion Sage | `mfe-parametrage` | `/parametrage/sage` | `GET/PUT /api/sage-connections` | `T_BDD_SAGE` | À développer |
| Test de connexion Sage | `mfe-parametrage` | `/parametrage/sage` | `POST /api/sage-connections/tests` | `T_BDD_SAGE`, Sage | À développer |
| Authentification | `mfe-identite` | `/auth/connexion` | `POST /api/auth/login` | `G_USERS` | À développer |
| Tableau de bord et menu | `shell` | `/` | `GET /api/auth/session` | `G_USERS`, `T_BDD_SAGE` | À développer |
| Masquage menu admin | `shell` | navigation | claims rôle | `G_USERS.IdRole` | À développer |
| Déconnexion | `shell` | navigation | `POST /api/auth/logout` | `G_AUTH_SESSION` | À développer |
| Profil / mot de passe | `mfe-identite` | `/profil` | `PUT /api/auth/password` | `G_USERS` | À développer |
| Liste utilisateurs (hors Id 1) | `mfe-identite` | `/utilisateurs` | `GET /api/users` | `G_USERS` | À développer |
| Création utilisateur | `mfe-identite` | `/utilisateurs` | `POST /api/users` | `G_USERS` | À développer |
| Modification utilisateur | `mfe-identite` | `/utilisateurs` | `PUT /api/users/{id}` | `G_USERS` | À développer |
| Suppression utilisateur | `mfe-identite` | `/utilisateurs` | `DELETE /api/users/{id}` | `G_USERS` | À développer |
| Réinitialisation mot de passe | `mfe-identite` | `/utilisateurs` | `POST /api/users/{id}/password-resets` | `G_USERS` | À développer (règle renforcée) |
| Dossier d’archive | `mfe-parametrage` | `/parametrage/envoi` | `GET/PUT /api/general-parameters` | `D_PGENERAL` | À développer |
| Adresse CCI | `mfe-parametrage` | `/parametrage/envoi` | idem | `D_PGENERAL` | À développer |
| Modèles de mail | `mfe-parametrage` | `/parametrage/envoi` | `GET/PUT /api/mail-templates` | `D_PMAIL` | À développer |
| Liste établissements | `mfe-dematerialisation` | filtres | `GET /api/establishments` | `T_ETA` | À développer |
| Liste salariés | `mfe-dematerialisation` | filtres | `GET /api/employees` | `T_SAL`, `T_HST_ETABLISSEMENT`, `T_ETA` | À développer |
| Lecture dossier PDF et statuts | `mfe-dematerialisation` | `/dematerialisation` | `GET /api/payslip-files` | fichiers + `D_DEMAT` + Sage | À développer |
| Filtres liste | `mfe-dematerialisation` | `/dematerialisation` | query de `payslip-files` | idem | À développer |
| Tout sélectionner | `mfe-dematerialisation` | `/dematerialisation` | aucun | — | À développer |
| Envoi + archive + historique immédiat | `mfe-dematerialisation` | `/dematerialisation` | `POST /api/dispatches` | `D_DEMAT`, `D_PMAIL`, `D_PGENERAL`, fichiers | À développer |
| Historique des envois | `mfe-dematerialisation` | `/historique` | `GET /api/dispatches` | `D_DEMAT`, Sage (libellés) | À développer |
| Traçabilité | — | — | — | `T_SUIVI` | **Exclu** |
| Colonnes suivies | — | — | — | `T_FILTER` | **Exclu** |
| Export CSV / PDF traçabilité | — | — | — | — | **Exclu** |
| Dictionnaire `%AppData%\GIZDT\list` | — | — | — | — | **Exclu** (sert la traçabilité) |

---

## 13. Plan de migration

Complexité relative : XS (&lt; 0,5 j), S (0,5–1 j), M (2–3 j), L (4–5 j), XL (&gt; 5 j).

### Phase 0 — Cadrage bloquant

- **Objectif :** figer schéma, coexistence des mots de passe, SMTP, chemins réseau.
- **Prérequis :** accès à une base SDT et à une base Sage de recette, compte lecture seule Sage.
- **Tâches :** `DEMAT-AN-003` à `DEMAT-AN-006`, `DEMAT-AR-004`, `DEMAT-AR-005`.
- **Livrables :** schéma signé, ADR auth, ADR fichiers/SMTP.
- **Dépendances :** aucune.
- **Risques :** base de recette indisponible.
- **Validation :** les colonnes du code existent avec les types relevés. Décision A ou B écrite.
- **Complexité :** L.

### Phase 1 — Socles

- **Objectif :** API et workspace MFE compilent, charte appliquée aux composants vides, sécurité de base.
- **Prérequis :** phase 0.
- **Tâches :** socle backend, design system, shell, federation, auth technique.
- **Livrables :** login technique, shell navigable, pipeline de build.
- **Validation :** `dotnet build` et `ng build` sans warning. Dépendances de couches respectées. Domain sans package.
- **Complexité :** XL.

### Phase 2 — Identité

- **Objectif :** login, profil, utilisateurs, autorisation serveur.
- **Prérequis :** phase 1, décision mots de passe.
- **Validation :** RM-02 à RM-07. Un non-admin reçoit 403 sur `/api/users`.
- **Complexité :** L.

### Phase 3 — Paramétrage

- **Objectif :** Sage, CCI, archive, modèles.
- **Prérequis :** phase 2 (rôle admin).
- **Validation :** test de connexion Sage en lecture. Enregistrement `D_PGENERAL` et `D_PMAIL` sans création de type fantôme.
- **Complexité :** L.

### Phase 4 — Dématérialisation

- **Objectif :** scan, filtres, envoi SMTP, archive, historique.
- **Prérequis :** phases 2 et 3, dossier réseau et SMTP de recette.
- **Validation :** RM-08 à RM-21 sur un jeu de PDF fictifs. Aucune écriture Sage.
- **Complexité :** XL.

### Phase 5 — Recette et durcissement

- **Objectif :** non-régression métier, sécurité, performance sur volume réaliste.
- **Prérequis :** phase 4.
- **Validation :** matrice section 12 toute verte sauf lignes Exclu. Recette gestionnaire de paie.
- **Complexité :** L.

### Phase 6 — Déploiement

- **Objectif :** production, bascule, retrait du parcours Desktop pour les fonctions reprises.
- **Prérequis :** phase 5, décision de bascule.
- **Validation :** envoi réel d’un lot pilote, historique consultable, rollback documenté.
- **Complexité :** L.

---

## 14. Structure recommandée

Le squelette actuel est conservé. Les ajouts sont des dossiers et un workspace Angular.

```text
SoftDemat/
├── DEMAT-WEB-ANALYSE.md
├── DEMAT-WEB-TICKETS.md
├── docs/
│   ├── adr/                          # décisions (auth, SMTP, schéma)
│   ├── data-model.md                 # schéma confirmé
│   └── deploiement.md
├── src/
│   ├── SoftDemat.Domain/
│   │   ├── Entities/                 # UserAccount, Dispatch, MailTemplate, GeneralParameter, SageConnection
│   │   ├── Enums/                    # UserRole, SendStatus
│   │   ├── Exceptions/
│   │   └── Interfaces/
│   ├── SoftDemat.Application/
│   │   ├── DTOs/
│   │   ├── Validators/
│   │   ├── Mappings/
│   │   └── Services/Interfaces/
│   ├── SoftDemat.Infrastructure/
│   │   ├── Context/                  # SdtDbContext, SageDbContext
│   │   ├── Configurations/
│   │   ├── Repositories/
│   │   └── Services/                 # Auth, Dispatch, Mail, FileArchive
│   ├── SoftDemat.Api/
│   │   ├── Controllers/
│   │   ├── Middleware/
│   │   └── Filters/
│   └── softdemat.front/              # workspace Angular
│       ├── projects/
│       │   ├── shell/
│       │   ├── mfe-identite/
│       │   ├── mfe-parametrage/
│       │   ├── mfe-dematerialisation/
│       │   ├── ui/                   # Tailwind, composants muets
│       │   └── contracts/            # types partagés, sans métier
│       └── src/                      # actuel, migré vers projects/shell
└── tests/
    ├── SoftDemat.Tests/              # unitaires domaine + application
    └── SoftDemat.IntegrationTests/   # API + SQL de test
```

Dans chaque remote :

```text
src/app/
├── core/          # guards, interceptors locaux, services techniques
├── shared/        # réexport UI si besoin, aucun HTTP
└── features/      # lazy, une feature = un dossier, pas d’import croisé
```

---

## 15. Stratégie de tests

| Niveau | Outil | Cible |
|---|---|---|
| Unitaire backend | xUnit, Moq, FluentAssertions | Services. SUT nommé `sut`. Positif et négatif. Pas de SQL |
| Unitaire front | Vitest (déjà dans le squelette Angular 22) | Services data-access et stores. HTTP mocké |
| Intégration API | WebApplicationFactory + base SDT de test | Contrats, rôles, pagination, erreurs |
| Compatibilité données | Jeu restauré depuis `SDT.bak` anonymisé | Lecture des lignes historiques `D_DEMAT` |
| Règles métier | Tests dédiés RM-08 à RM-17 | Padding matricule, chemin d’archive, ordre des placeholders, e-mail vide |
| Non-régression | Recette sur la matrice section 12 | Aucune ligne « À développer » restante hors Exclu |
| Sécurité | Tests 401/403, message de login identique, absence de secret dans les logs | — |

La comparaison Desktop / Web se fait sur des PDF fictifs et une base de recette, pas en prod. Cas minimum : matricule sur 4 caractères, matricule court paddé, salarié sans e-mail, salarié d’un autre établissement, déjà envoyé le même jour, échec SMTP, CCI vide, établissement avec `/` dans l’intitulé, mois français dans l’objet.

---

## 16. Hypothèses

1. Le Web remplace le Desktop pour l’envoi et l’administration des comptes (option A), sous réserve de `DEMAT-AR-004`.
2. Le schéma réel est celui lu par le code, jusqu’à preuve du relevé SQL.
3. Angular 22 satisfait « Angular 20+ » du document front. .NET 10 satisfait ASP.NET Core du document back. On ne downgrade pas le squelette.
4. Native Federation est le mécanisme MFE, parce que le squelette Angular 22 utilise le builder moderne, pas webpack.
5. Le compte SQL Sage de production sera créé en lecture seule. Ce n’est pas le compte `sa` des fichiers d’exemple.
6. Les polices de marque seront fournies par Softwell. Poppins est utilisée immédiatement pour le corps.
7. Il n’y a pas d’autre fonctionnalité hors dépôt `GIZDT` (pas d’écran manquant dans l’installeur au-delà de ce source).

## 17. Informations manquantes

- Types SQL, clés, index, vues, procédures, fonctions, triggers de SDT et de la base Sage cible.
- Contenu réel de `D_PMAIL` (quels types de mail existent).
- Serveur SMTP, expéditeur, taille max des PDF, dossier réseau des bulletins et des archives.
- Volume (salariés, bulletins par mois).
- Décision formelle de coexistence Desktop.
- Fichiers de logo et licences Evolve Sans / Garet.
- Politique exacte sur la présence de l’e-mail dans les journaux techniques.

Ces manques bloquent la phase 0, pas la lecture de ce plan.
