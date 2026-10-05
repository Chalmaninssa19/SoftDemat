# SoftDemat — Règles d'architecture et de codage (OBLIGATOIRES)

> **Contrat de développement.** Toute IA ou développeur qui modifie ce projet DOIT respecter ce fichier.
> Basé sur le standard interne « Règles de Codage C# .NET » v3.0 (Clean Architecture, SOLID, Clean Code).
> En cas de doute : ne pas improviser — appliquer la règle la plus stricte ci-dessous.

---

## 1. Architecture — Clean Architecture stricte (4 couches)

**Règle d'or : les dépendances pointent TOUJOURS vers l'intérieur.**

| Projet | Contient | Peut référencer | NE PEUT JAMAIS référencer |
|---|---|---|---|
| `SoftDemat.Domain` | Entités, Interfaces (repos + UoW), Exceptions métier, Enums, Value Objects | **RIEN** (0 ProjectReference, 0 PackageReference) | Tout |
| `SoftDemat.Application` | DTOs (records), Interfaces de services, Validators FluentValidation, Profils AutoMapper | Domain uniquement | Infrastructure, Api, EF Core, BCrypt, JWT, SMTP, HttpClient |
| `SoftDemat.Infrastructure` | DbContext, implémentations Repositories/Services, Configurations EF, Migrations, Interceptors | Application + Domain | Api |
| `SoftDemat.Api` | Controllers minces, Middleware, Filters, Program.cs | Application + Infrastructure (DI) | — |

### Interdictions absolues (P0 — un manquement = travail refusé)

- ❌ Ajouter un `PackageReference` à `SoftDemat.Domain` — le Domain est du C# pur (System.* uniquement)
- ❌ Référencer Infrastructure ou Api depuis Application
- ❌ Injecter `DbContext` directement dans un service métier → passer par `IRepository` / repository spécialisé
- ❌ Utiliser EF Core, BCrypt, JWT, SMTP en dehors d'Infrastructure
- ❌ Mettre de la logique métier, de la validation manuelle ou du mapping dans un controller
- ❌ Exposer une entité du Domain dans une réponse HTTP → toujours un DTO
- ❌ `Console.WriteLine` → toujours `ILogger<T>`
- ❌ `catch` vide ou catch qui avale l'exception
- ❌ Endpoint GET retournant une liste sans pagination
- ❌ Secrets / connection strings en dur dans le code
- ❌ `NotImplementedException` laissé dans le code livré
- ❌ Dupliquer plus de 10 lignes de code existant

---

## 2. Où placer chaque fichier (carte de placement)

| Je crée… | Emplacement | Exemple |
|---|---|---|
| Une entité | `Domain/Entities/` | `Order.cs` (hérite de `BaseEntity`) |
| Une interface de repository | `Domain/Interfaces/` | `IOrderRepository.cs` |
| Une exception métier | `Domain/Exceptions/` | `NotFoundException.cs` |
| Un enum métier | `Domain/Enums/` | `OrderStatus.cs` (singulier) |
| Un Value Object | `Domain/ValueObjects/` | `Money.cs` (record) |
| Un DTO | `Application/DTOs/` | `CreateOrderRequest.cs` (record) |
| Une interface de service | `Application/Services/Interfaces/` | `IOrderService.cs` |
| Un validator | `Application/Validators/` | `CreateOrderRequestValidator.cs` |
| Un profil AutoMapper | `Application/Mappings/` | `OrderProfile.cs` |
| Le DbContext | `Infrastructure/Context/` | `ApplicationDbContext.cs` |
| Une implémentation de repository | `Infrastructure/Repositories/` | `OrderRepository.cs` |
| Une implémentation de service | `Infrastructure/Services/` | `OrderService.cs` |
| Une config EF (`IEntityTypeConfiguration<T>`) | `Infrastructure/Configurations/` | `OrderConfiguration.cs` |
| Un controller | `Api/Controllers/` | `OrdersController.cs` |
| Un middleware | `Api/Middleware/` | `ExceptionMiddleware.cs` |

**Rappel piège : l'INTERFACE du repository vit dans Domain, son IMPLÉMENTATION dans Infrastructure.**

---

## 3. Nommage (résumé exécutoire)

| Élément | Convention | Exemple |
|---|---|---|
| Classe / méthode / propriété | PascalCase | `OrderService`, `CalculateTotal()` |
| Interface | `I` + PascalCase | `IOrderRepository` |
| Méthode async | PascalCase + `Async` | `GetAllAsync()` |
| Variable locale / paramètre | camelCase | `orderTotal`, `cancellationToken` |
| Champ privé | `_camelCase` | `_repository` |
| Champ privé static | `s_camelCase` | `s_instance` |
| Constante / static readonly | PascalCase | `MaxRetryCount` |
| Enum | PascalCase singulier, membres PascalCase | `OrderStatus.InProgress` |
| DTO | suffixe `Dto` / `Request` / `Response` | `CreateOrderRequest` |
| Test | `Method_Scenario_Expected` | `Login_WrongPassword_ThrowsUnauthorized` |
| Fichier | même nom que la classe, 1 classe publique par fichier | `OrderService.cs` |

Interdits : notation hongroise, noms à une lettre (hors lambdas), `Get_All_Users`, `handler1/handler2`, mots génériques seuls (`Manager`, `Helper`, `Utils`), booléens négatifs (`isNotActive`).

**Limites de taille** : méthode ≤ 30 lignes · classe ≤ 300 lignes · ≤ 4 paramètres · imbrication ≤ 3 niveaux · ligne ≤ 120 caractères · héritage ≤ 2 niveaux.

---

## 4. Règles de code obligatoires

### Controllers (Api)
- Minces : **≤ 5 lignes par action** — recevoir, déléguer au service via l'interface, retourner.
- Attributs obligatoires : `[ApiController]`, `[Route("api/[controller]")]`, `[Authorize]` par défaut, `[Produces("application/json")]`, `[ProducesResponseType(...)]`.
- `[AllowAnonymous]` uniquement sur login / register / forgot-password.
- Jamais de try/catch dans un controller — le Global Exception Middleware s'en charge.
- Toute réponse enveloppée dans `ApiResponse<T>`.

### Exceptions → codes HTTP (via middleware)
| Exception | HTTP |
|---|---|
| `DomainException` | 400 |
| `UnauthorizedException` | 401 |
| `ForbiddenException` | 403 |
| `NotFoundException` | 404 |
| `ConflictException` | 409 |
| Exception non gérée | 500 (loguée, détails masqués) |

Toujours `throw;` (jamais `throw ex;`). Exceptions typées uniquement (jamais `throw new Exception("...")`).

### API REST
- URLs : pluriel, kebab-case, **jamais de verbe** (`GET /api/orders/123`, pas `/api/getOrder`).
- Codes : GET 200 · POST 201 (+ Location) · PUT/PATCH 200 · DELETE 204.
- **Pagination obligatoire** sur toute liste : `page`, `size`, `search`, `sortBy`, `sortDirection`.

### Async
- Tout I/O est async, suffixe `Async`, `CancellationToken` en dernier paramètre (`ct = default`) et **propagé de bout en bout**.
- Jamais `.Result` / `.Wait()` / `async void` (hors event handlers) / `Task.Run` pour du travail ASP.NET.

### EF Core
- `AsNoTracking()` sur toute lecture seule · `Include` explicites (pas de lazy loading) · `Select()` pour projeter · `Skip/Take` pour paginer · pas de N+1 (jamais de requête dans une boucle).
- Config entités via `IEntityTypeConfiguration<T>`, un fichier par entité.
- Migrations nommées de façon descriptive, jamais modifiées après application en prod.

### Sécurité
- Mots de passe : **BCrypt uniquement** (jamais MD5/SHA256/clair).
- JWT court (15-30 min) + refresh token long stocké en DB, révocable, avec rotation.
- Jamais de secret/mot de passe/token dans les logs ni dans le code source.
- IDs exposés = GUID (jamais séquentiels). SQL paramétré uniquement. CORS restreint.
- Message d'erreur de login identique que l'email existe ou non.

### Logging
- `ILogger<T>` structuré avec placeholders nommés : `_logger.LogInformation("Order {Reference} created", reference);` — pas de concaténation.

### DTOs et validation
- DTOs = `record` immuables. **Chaque DTO d'entrée a un validator FluentValidation.**
- Jamais retourner `null` pour une collection → collection vide.
- `<Nullable>enable</Nullable>` partout.

### Tests
- xUnit + Moq + FluentAssertions. Structure AAA (Arrange / Act / Assert commentés). SUT nommé `sut`.
- Nommage `Method_Scenario_Expected`. Couvrir cas positifs + négatifs. Aucune dépendance externe en unitaire.

---

## 5. Workflow imposé pour toute nouvelle feature

Implémenter **dans cet ordre**, sans sauter d'étape :

1. **Domain** — entité (+ enum/exception si besoin), puis interface de repository.
2. **Application** — DTOs (records) + validator FluentValidation + interface de service (+ mapping).
3. **Infrastructure** — configuration EF, implémentation du repository, implémentation du service.
4. **Api** — controller mince + enregistrement DI dans `Program.cs`.
5. **Tests** — tests unitaires du service (positifs + négatifs).

Ordre des middlewares dans `Program.cs` (ne pas réordonner) :
`UseExceptionHandler → UseHsts → UseHttpsRedirection → UseCors → UseAuthentication → UseAuthorization → UseRateLimiter → MapControllers`.

---

## 6. Checklist avant de terminer toute tâche

- [ ] La solution compile sans warning
- [ ] Chaque fichier est dans la bonne couche (voir carte section 2)
- [ ] `SoftDemat.Domain.csproj` contient toujours 0 PackageReference
- [ ] Constructeurs : uniquement des interfaces injectées (pas de `new` de service concret)
- [ ] Chaque DTO d'entrée a son validator
- [ ] `CancellationToken` propagé partout
- [ ] Controllers ≤ 5 lignes/action, `[Authorize]` présent, `ApiResponse<T>` utilisé
- [ ] Listes paginées, requêtes EF en lecture avec `AsNoTracking`
- [ ] Aucune entité exposée en réponse HTTP
- [ ] Exceptions typées, pas de catch vide
- [ ] Logging structuré sur les événements métier importants
- [ ] Tests unitaires écrits et verts (`dotnet test`)

