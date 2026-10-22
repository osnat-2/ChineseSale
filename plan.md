# Chinese Sale Implementation Plan

## Mission

Build a complete Chinese sale web application with the existing ASP.NET Core API and Angular frontend. This file is the single source of truth for phase order, decisions, approvals, implementation status, validation, and handoffs.

## Non-Negotiable Rules

- Backend model classes and Entity Framework migrations are read-only unless the user explicitly approves a proposed change.
- Existing uncommitted backend changes belong to the current worktree baseline and must not be reverted or overwritten.
- Every phase agent must analyze its module, produce a detailed sub-plan, and stop for explicit approval before creating or modifying application code.
- DTO, controller, service, and Angular changes are preferred when existing persistence supports the requirement.
- Client-side route guards do not replace backend authorization.
- Client-side state cannot prove payment or create authoritative purchase history.
- Agents must stay inside their phase and record cross-phase dependencies here.

## Status

| Phase | Agent | Status | Approval |
|---|---|---|---|
| 1. Contract Baseline | `contract-baseline` | Completed: backend contract-only fix validated; build succeeded with warnings | Approved |
| 2. Auth | `auth` | Implemented: JWT state, interceptor, guards, active endpoint authorization; donor/admin role persistence deferred | Approved |
| 3. Catalog | `catalog` | Implemented: public read-only catalogue, typed services, filters, details, and focused tests | Approved |
| 4. Purchase/Payment | `purchase-payment` | Implemented: server-owned card lifecycle, owner history, development payment, guarded personal area, and focused UI tests | Approved |
| 5. Admin | `admin` | Expanded: server-backed present/donor CRUD, confirmed soft deletes, and lottery execution/results in management views | Approved |
| 6. Lottery | `lottery` | Implemented: server-side draw validation, no-paid-card friendly failure, and atomic winner+lottery completion persistence | Approved |
| 7. Verification | Main workflow | Completed with documented residual risks | Approved |

## Execution Contract

Each phase follows this exact workflow:

1. Read this plan, the phase agent, the phase skill, applicable frontend instructions, and the owning code paths.
2. Analyze the module thoroughly: current behavior, dependencies, contracts, risks, and tests.
3. Write a detailed sub-plan with exact files/symbols, data flow, boundaries, approval-gated work, and validation criteria.
4. Stop and request explicit user approval. No application code may be generated or modified before approval.
5. After approval, make focused edits and run the phase's validation commands.
6. Update this plan with decisions, changed files, validation output, and residual risks.

## Phase 1: Contract Baseline

### Conditional Approval Received

On 2026-09-04, the user approved the following implementation direction, subject to this exact sub-plan being reviewed before execution:

- Donors are represented by the existing `User` model and identified by role values `User`, `Donor`, and `Admin`; no separate donor entity will be created.
- Card ownership must be assigned from the authenticated user and use the existing `BaseModel` design; the client must not submit the authoritative owner.
- Server-controlled development payment is approved.
- Raffle and winner implementation is approved.
- DTO contracts must be corrected against the existing backend models first; Angular contracts are adapted only after the C# contract is verified.
- Migrations must not be run at this stage because the connection string must be updated first.

### Current Source Reconciliation

The current worktree must be reconciled with those decisions before implementation:

- `User.RoleId` and `User.Role` are still commented in `Models/User.cs` in the current source snapshot. The agent must verify whether this is an uncommitted user change, a stale file view, or a required contract discrepancy. It must not uncomment or otherwise edit the model without a separate explicit approval for that exact model edit.
- `Card` has no `UserId`; `BaseModel.CreatedBy` and `CreatedByUser` are the existing persisted user-linked fields. The proposed no-model-change ownership path is to set `Card.CreatedBy` server-side from `ClaimTypes.NameIdentifier` and treat it as the purchaser/owner for card queries and payment authorization. This semantic reuse must be called out in the implementation report because `CreatedBy` is also an audit field.
- `Present.DonorId` already points to `User` through `Present.Donor`; donor filtering and management will use users whose role is `Donor`, subject to verified role persistence.

### Phase 1 Verification Result

Completed on 2026-09-08 under the contract-only backend correction pass.

- Verified build command: `cd "c:\Users\משתמש\Desktop\Student\מסלול\Projects\ChineseSale\backend\ApiProject"; dotnet build Project.sln`
- Result: backend build succeeded with 112 warnings and 0 errors in 2.3s.
- Contract corrections applied without changing backend model files or migrations, including DTO alignment for `Present`, stale namespace cleanup, DAL property corrections, and removal of legacy `Category` / `User` navigation assumptions.
- Residual risk: the project still emits 112 nullable/legacy warnings, and the winner flow remains intentionally deferred until the approved owner contract is implemented. These warnings are not blockers for the current contract-only phase, but they should be addressed in a later cleanup pass.

### Exact Proposed Changes Before Execution

#### Step A: C# DTO and contract correction, first

Only after the user approves this sub-plan, inspect and correct DTOs against the unchanged model classes and update consuming code. Proposed files and symbols:

- `backend/ApiProject/Project/Dto/UserDto.cs`: define request properties matching the existing `User` validation surface (`Name`, `Phone`, `Email`, `Password`) without exposing entity navigation/audit fields. Do not add `RoleId` unless the current model is confirmed to contain it and the user separately approves exposing it in the DTO.
- `backend/ApiProject/Project/Dto/CardDto.cs`: define only client-allowed input (`PresentId`). Do not accept `UserId`, `CreatedBy`, `IsPaid`, audit fields, or navigation properties from Angular.
- `backend/ApiProject/Project/Dto/PresentDto.cs`: define properties matching the unchanged `Present` model (`Name`, `Description`, `DonorId`, `CategoryId`, `ImageUrl`, `Quantity`, `Price`) and exclude base audit fields unless an endpoint explicitly requires them.
- `backend/ApiProject/Project/Dto/CategoryDto.cs`, `DonorDto.cs`, `LotteryDto.cs`, and `WinnerDto.cs`: compare each DTO with its corresponding unchanged model and define only supported request/response fields. Donor DTOs represent `User` with a role, not a new `Donor` entity.
- `backend/ApiProject/Project/Profiles/*.cs`: align AutoMapper profiles with the corrected DTO namespace and properties; prevent client DTO mapping from overwriting `Id`, `CreatedBy`, `CreatedAt`, `IsActive`, `IsPaid`, or other server-controlled values.
- `backend/ApiProject/Project/Controllers/*.cs`, `Bll/*.cs`, and `Dal/*.cs`: replace stale DTO namespace imports and update property access to the corrected DTO contracts. Preserve endpoint names unless a compatibility alias is explicitly approved.
- `backend/ApiProject/Project/Result.cs`: preserve the existing wrapper unless the verified login/feature contract requires a narrowly scoped response DTO. Login currently returns the JWT in `Result.Message`; document and consume that verified shape before proposing a response change.

#### Step A security and role behavior

- `UserService`/JWT creation: use verified role data and emit `ClaimTypes.Role` with `User`, `Donor`, or `Admin`; do not accept role elevation from registration input.
- User registration: force the default role to `User` through server logic using the available role design; Donor/Admin assignment must be an authorized operation.
- Donor queries and writes: use `User` records filtered by the verified Donor role and existing `Present.DonorId` relationship.
- Card creation: require authentication, derive `CreatedBy` from `ClaimTypes.NameIdentifier`, and ignore any client owner value.
- Payment: add/enable controller, Bll, and DAL operations using the existing `Card.IsPaid` field, validating the authenticated `CreatedBy` owner. Do not add a payment entity or run a migration in this step.
- Lottery/winners: expose the approved workflow only after verifying that existing `Lottery`, `Winner`, and `Card` fields can persist the required relationships. Any required model/schema repair remains a separate approval request; approval of the feature does not authorize unlisted model edits.

#### Step B: Angular adaptation only after Step A

After the C# DTOs, endpoints, wrappers, and status codes compile and are documented, update:

- `frontend/src/app/models/*.ts`: make TypeScript request/response models match the verified C# DTOs; remove client-owned `userId`, `CreatedBy`, `IsPaid`, and navigation fields from create/payment requests.
- `frontend/src/app/services/userService/user-service.ts`: consume the verified login/register wrapper and role claim contract.
- `frontend/src/app/services/httpService/http-service.ts`, `frontend/src/app/app.config.ts`, and new interceptor/guard files: attach JWTs and protect verified routes.
- `frontend/src/app/services/presentService`, `categoryService`, `cardService`, `donorService`, `lotteryService`, and `winnerService`: implement methods using source-verified paths, wrappers, query parameters, and status behavior.
- `frontend/src/app/components/**`: adapt forms and views only to the verified C# contracts; preserve loading, empty, error, unauthorized, duplicate, and unavailable states.
- Angular tests: update generated model/service assumptions and add contract-focused tests after the API shapes are fixed.

### Explicitly Forbidden in This Execution

- Do not edit `backend/ApiProject/Project/Models/*.cs`.
- Do not edit or generate files under `backend/ApiProject/Project/Migrations/`.
- Do not run `dotnet ef database update`, `dotnet ef migrations add`, or any migration command.
- Do not run database operations that depend on the current connection string.
- Do not create `Donor.cs`, donor `DbSet`, or donor migration.
- Do not add `Card.UserId`, payment tables, winner relationship fields, or other schema fields.
- Do not activate commented model properties without separate explicit approval naming the exact model change.

### Phase 1 Execution Boundary

The approved next execution is a contract-only backend pass in this order:

1. Capture the current worktree and run `dotnet build` as a diagnostic; do not revert user changes.
2. Correct DTO definitions and namespace consumers against the unchanged models.
3. Correct profiles and service/controller/DAL property access needed for those DTOs.
4. Verify role and claim behavior without changing models or migrations.
5. Implement only payment/raffle controller/service exposure that is representable by the unchanged schema; stop and report any unrepresentable relationship.
6. Run backend build/tests without database or migration commands.
7. Produce the verified C# contract matrix.
8. Only then prepare a separate Angular sub-plan and wait for approval before Angular edits.

**Acceptance criteria:** C# DTOs match unchanged models, stale namespace/property references are resolved, role and ownership rules are explicit, payment/raffle feasibility is demonstrated without schema operations, backend build/test results are recorded, and Angular changes remain unexecuted until the C# contract is accepted.

## Phase 2: Auth

### Implementation Result

Completed on 2026-09-08 after explicit approval of the Auth sub-plan.

- Added browser-safe Angular JWT state in `frontend/src/app/services/authService/auth-service.ts`, including token validation, expiry handling, role extraction, and logout.
- Added functional JWT interceptor in `frontend/src/app/interceptors/auth-interceptor.ts`; login/register requests are excluded, authenticated API requests receive the Bearer header, and 401 responses clear state and navigate to login.
- Added functional `AuthGuard` and `AdminGuard` and activated the existing protected Angular routes.
- Corrected login handling to consume the verified backend `Result<string>` contract (`success` plus JWT in `message`).
- Registered the interceptor in `app.config.ts` and added focused AuthService tests.
- Added explicit `[AllowAnonymous]` to login/register and `[Authorize]` to the active card controller endpoint surface.

### Validation

- Backend: `dotnet build Project.sln --no-restore` from `backend/ApiProject`; succeeded with 111 existing warnings and 0 errors.
- Angular build: blocked because frontend dependencies are not installed; `@angular/build:application` is unavailable.
- Static diagnostics: no errors reported in the new AuthService, interceptor, guards, routes, UserService, or Login source files; app config dependency diagnostics are caused by missing `node_modules`.

### Residual Risks and Explicit Deferrals

- The unchanged `User` model has role properties commented out, and no active donor/admin creation endpoint exists. Therefore server-side assignment of persisted `Donor` and `Admin` roles cannot be completed without an explicit model/schema approval. Public registration remains non-elevating and JWT issuance currently uses the fixed `User` role claim.
- Backend authentication middleware already validates issuer, audience, signature, and lifetime with zero clock skew; invalid or expired bearer tokens are rejected by the framework with 401, while role mismatches return 403.
- No model files or EF migrations were changed.

## Model-Aligned Backend Update: RoleId and IsDeleted

Completed on 2026-09-08 after explicit approval.

- Added `User.Role` persistence configuration and a `Role` DbSet; public registration resolves the server-side `User` role, while admin donor/admin endpoints resolve `Donor` and `Admin` roles through dedicated service methods.
- JWT role claims now use the persisted role name, and inactive users cannot be returned by the login lookup.
- Removed client-facing `RoleId` from `UserDto`; AutoMapper ignores role, deletion, timestamps, IDs, and audit fields.
- Added EF query filters for all `BaseModel` entities so normal DAL queries exclude `IsDeleted` records.
- Present and category deletion now sets `IsDeleted`, `IsActive`, `DeletedAt`, and `UpdatedAt` instead of only changing `IsActive`.
- Added server-managed field protections to category, present, card, lottery, and winner profiles.

Validation:

- `dotnet build Project.sln --no-restore` from `backend/ApiProject`: succeeded with 0 errors and 114 warnings.
- No database migration or database update command was run.

Migration notice:

- The model contract now requires the database schema to contain `User.RoleId`, `BaseModel.IsDeleted`, `BaseModel.DeletedAt`, and the configured `User.RoleId -> Role` relationship. The existing migration set must be reviewed and a separate migration/database-update pass is required if those columns, table, or relationship are not already present.

## Phase 3: Catalog

Backend read contract enhancement completed on 2026-09-08 after explicit approval. `GET /api/present/getAllPresents` now accepts optional `search`, `categoryId`, `sortBy` (`name` or `price`), and `sortDirection` (`asc` or `desc`) query parameters. The DAL applies server-side name/description search, category filtering, deterministic sorting, and strict `IsActive == true && IsDeleted == false` filtering. `GET /api/present/{id}` applies the same active/non-deleted constraint. Existing `onlyActive` query compatibility and `Result<Present>` response shape were preserved. No model or migration files were changed.
## Phase 6: Lottery

### Implementation Result

Completed on 2026-09-16 after explicit approval of the Phase 6 sub-plan and the additional validation constraints.

- Implemented the winner draw flow in the server DAL, service, and controllers using the existing `Card`, `Present`, and `Lottery` model contract without creating a migration or model change.
- Added a friendly validation error when a present has zero eligible paid cards, returning a non-500 result message: "No paid cards are available for this present, so a draw cannot be completed."
- Ensured the winner record creation and `Lottery.IsMadeOut` completion update are persisted together in the same database transaction. For relational databases, the update is wrapped in `BeginTransactionAsync`, with a test-safe fallback for the in-memory provider.
- Exposed `POST /api/lottery/draw/{presentId}` and `POST /api/winner/draw/{presentId}` endpoints using the verified `Result<Winner>` contract.
- Added a focused backend regression test project for draw validation and transaction behavior.

### Validation

- `cd "d:\Student\מסלול\Projects\ChineseSale\backend\ApiProject"; dotnet test Project.Tests/Project.Tests.csproj --nologo --verbosity minimal`
- Result: project compiled and the focused tests were executed successfully after aligning the test project to the installed .NET 8 runtime. The initial attempt failed only because the test project targeted .NET 9 while the machine had no .NET 9 runtime installed.

### Residual Risks

- The draw logic currently selects one winner by random choice from the eligible paid cards and should remain a server-authoritative operation; no separate winner notification system has been added yet.
- No migration or model schema changes were made, so the current design remains within the approved persisted contract.
Validation: `dotnet build Project.sln --no-restore` from `backend/ApiProject` succeeded with 0 errors and 114 existing warnings. Angular catalogue implementation remains pending.

### Angular Implementation Result

Completed on 2026-09-16 after explicit approval of the Angular Phase 6 sub-plan.

- Added typed Angular lottery and winner models aligned with the verified `Result<Winner>` draw response.
- Added `LotteryService.drawWinner` and `WinnerService.drawWinner` for the verified draw endpoints.
- Implemented the admin winner draw screen with active-present loading, server-authoritative draw execution, disabled in-flight/duplicate controls, and clear no-paid-card/already-drawn error messages.
- Successful draws immediately update shared local winner state and mark the present as completed for the current session.
- Updated winner management to display the latest persisted draw result available in the current session and explicitly report that the current backend has no historical winner-list endpoint.
- Added focused service and management component tests.

Validation:

- Focused Angular tests: `npx ng test --watch=false --browsers=ChromeHeadless --include='src/app/services/lotteryService/lottery-service.spec.ts' --include='src/app/services/winnerService/winner-service.spec.ts' --include='src/app/components/management/winner-management/winner-management.spec.ts'`: 5 passed.
- Angular build: `npm run build` from `frontend`: passed; existing unused-import and bundle-budget warnings remain.
- Static diagnostics: no errors in changed lottery, winner, service, or component TypeScript files.

Residual risk: winner management cannot display historical winners across page reloads because the verified backend exposes draw execution only and has no winner-list endpoint. No backend models, migrations, or API contracts were changed in this UI phase.

Next: implement public present/category routes and typed Angular services, search/filtering UI, details, and loading/empty/error states against this verified read contract.

### Phase 3 Implementation Result

Completed on 2026-09-08 after explicit approval of the Phase 3 catalogue scope.

- Added typed `Result<T>`, present, and category models aligned to the verified `data` array wrapper.
- Added read-only `PresentService` methods for active catalogue queries and present detail lookup, including the verified search, category, sort, and direction parameters.
- Added read-only `CategoryService.getAllCategories()` against anonymous `GET /api/category`.
- Added public `/catalog` and `/catalog/:id` routes while preserving the guarded admin `/present` route.
- Added catalogue list and detail components with loading, empty, error, invalid-id, responsive, accessible labels, and image fallback states.
- Added the fallback asset to Angular's build assets and added focused service/component specs.
- Corrected the stale generated `DonatorService` test import so the Angular test target can compile; no donor application behavior changed.

### Phase 3 Validation

- Angular diagnostics: no errors in the Phase 3 models, services, routes, or components.
- `npm run build` from `frontend`: passed; existing unused-import and initial bundle-budget warnings remain.
- Focused catalogue tests: `npx ng test --watch=false --browsers=ChromeHeadless --include='src/app/services/presentService/present-service.spec.ts' --include='src/app/services/categoryService/category-service.spec.ts' --include='src/app/components/catalog/catalog.spec.ts' --include='src/app/components/catalog-detail/catalog-detail.spec.ts'`: 8 passed.
- Full Angular test target executes but currently reports 11 unrelated/pre-existing failures in generated service specs without `provideHttpClient()` and the stale app title assertion. The Phase 3 specs pass independently.

### Phase 3 Residual Risks

- The frontend API base URL remains hardcoded to `https://localhost:7142/api/`; environment-specific configuration is deferred to deployment/runtime configuration work.
- The category endpoint's HTTP failure is intentionally non-blocking for catalogue rendering, so a category-load failure falls back to the all-category selector without an inline category error.
- Existing Angular suite failures outside catalogue remain and should be repaired during the broader verification phase.

### Catalogue Hover Controls — 2026-10-21

- Moved authenticated quantity and Add to Basket controls into a card-image overlay and replaced the expanding `max-height` reveal with opacity/transform transitions. This keeps card/grid geometry fixed on hover and keyboard focus.
- Touch devices continue to show controls inline. Reduced-motion preferences disable the reveal motion. Add-to-basket behavior, quantity validation, and API contracts are unchanged.
- Static diagnostics report no errors. In the live catalog, browser verification confirmed the card bounding box has the same position and height before and after hover, with controls visible as an absolute overlay.
- The configured catalog component spec could not be run because VS Code test discovery did not find a matching test item.

### Catalogue Add Button Placement — 2026-10-21

- Moved Add to Basket from the image overlay into a persistent row beside the present name immediately below the image. Kept the quantity selector and validation visible in a compact row below the title/action row, so users can select quantity without hover.
- Removed the image dim/zoom and hover overlay styles. No catalog API, add behavior, or backend contract changed.
- Static diagnostics report no errors. Live browser measurements confirmed the button is physically left of the title in the current RTL view and both remain below the image. The focused catalog spec could not run because VS Code test discovery did not resolve the spec.

### Catalogue Add-to-Basket Quantity Dialog — 2026-10-21

- The card-level Add to Basket button now opens an accessible quantity dialog instead of immediately calling the add API. The dialog contains the numeric selector, existing quantity validation, the actual Add to Basket submit action, and close/cancel controls.
- Added Escape and backdrop dismissal, prevented dismissal while a request is pending, focused the quantity input when opened, and restored focus to the initiating card button when closed. API errors keep the dialog open; successful adds close it. Existing authenticated add-card API and request shape are unchanged.
- Added English and Hebrew dialog copy and focused component tests for opening without posting, submitting quantity, validation, pending state, error retention, cancel, Escape, and focus behavior.
- Static diagnostics report no errors. Live browser check confirmed the dialog title, prompt, default numeric quantity, input focus, distinct Add/Cancel actions, Escape dismissal, and focus restoration; closing it does not add a card. The focused npm test command could not compile because of TypeScript errors in unrelated card/personal-area specs and winner-management, so catalog tests did not execute. The frontend build is likewise blocked by a TypeScript error in winner-management.
- Follow-up: on fine-pointer devices, the card-level Add to Basket button is visually hidden at rest and revealed on card hover or keyboard focus without changing card geometry; it remains visible on coarse-pointer/touch devices and respects reduced-motion settings. Live browser verification on desktop confirmed opacity 0 at rest, visible on hover/focus, and unchanged card dimensions. Dialog behavior and API flow are unchanged.

## Phase 4: Purchase/Payment

### Implementation Result

Completed on 2026-09-16 after explicit approval of the Phase 4 sub-plan.

- Card creation now derives ownership from `ClaimTypes.NameIdentifier`; client requests cannot provide `userId`, `CreatedBy`, or `IsPaid`.
- Card creation validates active present availability, completed raffles, and `Present.Quantity` reservations.
- Added owner-scoped card history, unpaid-card removal, and server-controlled development payment endpoints.
- Payment retries are idempotent because payment sets the owner’s cards to the persisted paid state and repeats remain successful.
- Added API-backed Angular card, checkout, and personal-area views with loading, disabled, success, empty, and failed-request states.
- Added guarded `/personal-area` routing.
- Restricted the Angular bearer-token interceptor to `/api/card` requests, including payment calls; unrelated API requests are forwarded without an Authorization header.
- Updated focused Angular specs with HTTP/router providers.

Changed files:

- `backend/ApiProject/Project/Controllers/CardController.cs`
- `backend/ApiProject/Project/Bll/CardService.cs`
- `backend/ApiProject/Project/Bll/Interfaces/ICardService.cs`
- `backend/ApiProject/Project/Dal/CardDal.cs`
- `backend/ApiProject/Project/Dal/Interfaces/ICardDAL.cs`
- `frontend/src/app/models/card.ts`
- `frontend/src/app/models/ModelsDto/cardDto.ts`
- `frontend/src/app/services/cardService/card-service.ts`
- `frontend/src/app/interceptors/auth-interceptor.ts`
- `frontend/src/app/app.routes.ts`
- `frontend/src/app/components/card/*`
- `frontend/src/app/components/payment/*`
- `frontend/src/app/components/personal-area/*`

Validation:

- Backend: `dotnet build backend/ApiProject/Project.sln --no-restore` passed with 0 errors and 121 warnings.
- Angular: `npm run build` from `frontend` passed; existing unused-import and bundle-budget warnings remain.
- Focused Angular tests: 4 passed for CardService, Card, Payment, and PersonalArea.

Residual risks:

- `Present.Quantity` reservation uses a count-before-insert check without a schema-supported concurrency token or transaction boundary; simultaneous requests may require a later persistence approval for strict inventory guarantees.
- The development payment endpoint marks all current owner cards paid; no payment entity or transaction reference exists by design.
- Database migrations and database update commands were not run.

### Personal Area Card Summary — 2026-10-21

- Replaced the per-card ID reference in the personal area with summaries grouped by present and paid/unpaid state. Each row displays the present name, card count, and payment state; missing present navigation uses a localized fallback.
- The personal area requests all owner cards when no `paid` filter is supplied. Updated `CardController.GetMyCards` to default the nullable `paid` filter to null while preserving explicit `paid=true` and `paid=false` filtering.
- Added focused frontend coverage for grouped counts, multiple presents, mixed payment states, and hidden identifiers, plus backend DAL and controller coverage for the unfiltered default and explicit payment filters.
- No DTO, model, migration, schema, payment lifecycle, or ownership changes were made.
- Static diagnostics reported no errors in the touched frontend and backend files. Validation is otherwise pending: VS Code test discovery did not resolve the PersonalArea spec, and the configured npm test command could not start because sandboxed commands cannot access the local proxy. The backend test and frontend build have not yet run.

### Card Page Summary — 2026-10-21

- The earlier personal-area display change did not affect `/card`, which uses its own Card component and template. Updated that page to group unpaid cards by present and show only the present name and number of unpaid cards.
- Preserved add-by-present-number and checkout behavior. Replaced each per-card remove button with “Remove one”; it still calls the existing owner-scoped delete endpoint and decrements/removes the group after server success. Card IDs remain internal and are not rendered.
- Added English/Hebrew count, missing-present-name, and remove-one labels, plus focused Card component tests for grouping, hidden identifiers, and removal.
- No backend or persistence changes were made. Static diagnostics report no errors in touched files. VS Code test discovery did not resolve the Card spec; the configured frontend test/build commands remain unverified due to sandbox command-execution restrictions.

### Personal Area Paid and Unpaid Sections — 2026-10-21

- Updated the personal area to make separate API requests for `paid=true` and `paid=false` rather than depend on the server's unfiltered default. Each status is grouped by present name and card count, with a localized empty-state message when one status has no cards.
- Added English/Hebrew section headings and empty-state translations, plus component coverage for displaying both statuses and status-specific empty states. If either API request fails, the existing load error is shown instead of partial history.
- No backend, payment, ownership, model, or persistence changes were made.
- Static diagnostics report no errors in changed files. Reloaded the authenticated local page: it displays the paid summaries and explicitly reports that there are currently no unpaid cards for this account. Automated test execution remains unavailable because VS Code test discovery does not resolve these specs and sandboxed command execution is blocked.

## Phase 5: Admin

### Implementation Result

Completed on 2026-09-16 after explicit approval of the Phase 5 sub-plan.

- Added a working admin dashboard shell in [frontend/src/app/components/admin/admin.ts](frontend/src/app/components/admin/admin.ts) and [frontend/src/app/components/admin/admin.html](frontend/src/app/components/admin/admin.html) with navigation cards for present, donor, card, winner, and purchase management.
- Added admin management placeholder screens with empty states for donor, present, card, purchase, and winner views. These views intentionally do not fabricate backend data and instead reflect the server-backed contract and loading/empty states.
- Corrected stale Angular card contract issues by removing the client-supplied `userId` from [frontend/src/app/models/ModelsDto/cardDto.ts](frontend/src/app/models/ModelsDto/cardDto.ts) and importing the `presentModel` type in [frontend/src/app/models/card.ts](frontend/src/app/models/card.ts).
- Kept the donor path aligned with the approved donor-as-User model and did not create a separate donor table or schema change.

### Validation

- Angular build: `cd frontend && npm run build` succeeded with warnings only; no build errors.
- Focused admin test: `cd frontend && npm test -- --watch=false --browsers=ChromeHeadless --include='src/app/components/admin/admin.spec.ts'` passed with 2/2 success.

### Residual Risks and Explicit Deferrals

- Purchase history and historical winner listing remain deferred because those read APIs are not available; the lottery view displays the persisted result from the current draw.
- Donor deletion is a soft delete that preserves existing present references. No donor table, model, or migration was added.
- No database migration, database update, or live end-to-end database journey was run.

### Management UI Expansion — 2026-10-06

- Connected `/admin/presents` to the present read/create/update/soft-delete APIs. Added validation-matched forms, donor/category selection, price editing, list refresh after writes, and per-item confirmation.
- Added Admin-only donor list/update/soft-delete endpoints over existing `User` records with the `Donor` role. Donor creation reuses the existing Admin-only `/api/auth/addDonor` endpoint and its required initial-password contract. Responses use the password-free `UserResponseDto`. Deletion is rejected while a non-deleted present references the donor.
- Added the present price to `PresentDto`, honored `onlyActive=false` for admin listing, initialized new presents as active, and persisted edited image URLs and prices.
- Connected `/admin/winners` to the existing Admin-only persisted `/api/lottery/draw/{presentId}` endpoint, with a permanently visible Create Lottery action, present selection, and result/error states.
- Added focused Angular management/service tests and backend donor/present/DTO tests. Angular management/service tests passed 17/17; backend tests passed 31/31 when excluding the unrelated `UserPasswordStorageTests` assertion that expects a 60-character password column while the current worktree uses 255. The full backend suite otherwise passed 31/32.
- Angular production build passed with the existing unused-import and bundle-budget warnings. No model or migration files were modified.

### Category, Role, and User Management — 2026-10-07

Implemented after the user's request and approval of the bounded Phase 5 sub-plan.

- Added Admin dashboard links and guarded `/admin/categories`, `/admin/roles`, and `/admin/users` routes. The category, role, and user screens keep their primary Add action visible with empty lists and show per-item update/delete controls with confirmation where applicable.
- Connected category management to the existing category CRUD API. Added an active-state field to the existing category DTO flow so newly created categories default to active and administrators can edit category status; requests to include inactive categories are Admin-only.
- Connected role management to the existing Admin-only role CRUD API. Built-in `User`, `Donor`, and `Admin` roles remain protected in both the UI and role API.
- Added Admin-only `/api/admin/users` list/create/update/soft-delete endpoints over the existing `User` model. Passwords are hashed server-side and omitted from management response DTOs; assignment requires an active role. Self-deactivation/deletion, removal of the last active Admin, and deletion of donors still referenced by presents are rejected.
- Added English/Hebrew management copy and focused frontend component/service tests plus backend category/user-management tests. No model, migration, or database changes were made.

Validation:

- Angular production build: passed. Existing unused-import, initial-bundle-budget, and stale Baseline Browser Mapping warnings remain.
- Focused Angular management/service tests: 16 passed, 0 failed.
- Backend test project excluding the pre-existing `UserPasswordStorageTests` column-length assertion: 44 passed, 0 failed. The standard test output is isolated under `C:\Temp\ChineseSaleValidation\bin` because the running API process locks the default output files.
- Backend test restore reports existing NuGet vulnerability warnings for AutoMapper and Newtonsoft.Json.
- No migration generation, database update, or live authenticated API journey was run.

Residual risks:

- The last-active-Admin rule is enforced in application logic and is not concurrency-safe if multiple administrator updates race; strict protection requires a database-supported constraint or transaction strategy.
- The known password-column test mismatch remains outside this approved scope.

### Management Popup UX — 2026-10-08

Implemented after explicit approval to move management create/update/delete interactions into pop-ups.

- Present, donor, category, role, and user create/edit forms now open in modal dialogs; delete actions use modal confirmation dialogs instead of inline confirmations.
- User activation/deactivation is now edited in the same update modal rather than submitted immediately from the list.
- Lottery execution opens a confirmation dialog before calling the existing draw API; failed draws remain visible within the dialog and successful draws close it.
- Shared modal styling is applied consistently with labelled dialog semantics, Escape/cancel controls, responsive layout, and protection from dismissing while a request is saving. No backend or persistence contract changed.

Validation:

- Focused Angular management specs passed: 26/26.
- Angular production build passed. Existing router unused-import, browser-mapping, and initial bundle-budget warnings remain.

### Management Navigation Consolidation — 2026-10-21

Implemented after explicit approval of a UI-only scope.

- Added a Lottery management dashboard entry that opens the existing Admin-protected draw screen and renamed that screen consistently; draw authorization and persistence behavior are unchanged.
- Combined card activity and purchase history into one management page, with separate, truthful empty states. The current API does not expose admin-wide card or purchase-history listing endpoints, so this change does not fabricate or expose user-scoped records.
- Redirected the legacy `/admin/cards` route to `/admin/purchases`; the `/admin/winners` route remains unchanged.
- Updated English/Hebrew copy and focused dashboard, purchases, and lottery management tests. No backend, model, migration, or API changes were made.

Validation:

- Angular production build passed with existing initial-bundle and catalog-style budget warnings.
- Focused Angular tests compiled, but Karma could not launch ChromeHeadless in this environment. VS Code test discovery also reported no matching test items.
- `git diff --check` passed for the changed implementation and plan files.

### Annual Lottery Management — 2026-10-21

Implemented after approval of the annual-event workflow: one annual event processes every active present atomically and does not partially draw if any present lacks enough paid cards.

- Added Admin-only lottery list/create/update/process endpoints. The event date uses the existing `Lottery.Time` (`DateOnly`) field; the selected calendar year identifies the annual event. Date edits are rejected once the event has winners or is complete.
- Annual event processing selects the configured number of distinct active, non-deleted, paid cards for every active present, persists all winners against the same `LotteryId`, and marks `IsMadeOut` only when all processing succeeds. Relational processing uses a serializable transaction; repeat processing is rejected.
- Scoped the legacy per-present draw guard to the selected lottery so a present can be drawn again for a later annual event. Legacy per-present draws now require an explicit existing lottery ID instead of silently creating one.
- Enforced one non-deleted lottery per calendar year in the application create/update flow. The existing schema has no unique year constraint; no model or migration was changed. Database-level uniqueness remains a separate schema approval if required.
- Added ISO `yyyy-MM-dd` DateOnly JSON handling because the API uses Newtonsoft.Json 12 and Angular sends/reads the `Lottery.Time` date as a string.
- Replaced the single-present admin draw screen with annual lottery add/update/list/process controls, all-active-present summary, confirmation, errors, and persisted results. `/winner` now redirects to the guarded annual management route.
- Added backend coverage for DateOnly JSON, duplicate-year prevention, edits after winners, complete multi-present processing, insufficient-card all-or-nothing behavior, empty present lists, repeat processing, and reusing a present in a later year. Added focused Angular service and management coverage.

Validation:

- Angular production build passed with the existing initial bundle and catalogue-style budget warnings.
- Focused Angular tests compiled, but Karma could not launch ChromeHeadless in this environment.
- Backend tests could not run: MSBuild was denied access to `C:\Users\משתמש\.gitconfig` by the sandbox. No workaround or retry was attempted.
- No model, migration, database update, or live authenticated API test was performed.

## Phase 6: Lottery

Expose and implement an authorized, persisted, idempotent draw workflow over eligible paid cards. Align winner DTOs and frontend views with verified contracts. Notification is secondary to persisted correctness.

## Phase 7: Verification

Run backend and Angular builds/tests, API and database checks, and the complete manual journey. Document local setup, development payment semantics, configuration, and deferred approval-gated changes.

### Verification Result

Completed on 2026-09-16 after explicit approval of the Phase 7 verification sub-plan, including negative authorization and UI API-error loading-state checks.

#### Changes made during verification

- Removed the invalid leading `/n` bytes from `backend/ApiProject/Project/Project.csproj.user`, which prevented MSBuild from loading the existing project settings.
- Restricted `POST /api/lottery/draw/{presentId}` and `POST /api/winner/draw/{presentId}` to `Admin` roles. ASP.NET Core authorization will return 403 for authenticated users whose role is not `Admin`; unauthenticated requests remain 401.
- Added `provideHttpClient()` to `frontend/src/app/components/winner/winner.spec.ts` so the standalone component test can construct its existing `PresentService` dependency.

#### Validation evidence

- Backend build: `dotnet build backend/ApiProject/Project.sln --no-restore` passed with 0 errors and 4 existing package-vulnerability warnings.
- Backend tests: `dotnet test backend/ApiProject/Project.Tests/Project.Tests.csproj --nologo --verbosity minimal` passed, 2/2 tests.
- Angular build: `npm run build` passed; existing unused-import, stale browser-mapping, and bundle-budget warnings remain.
- Focused Angular verification: catalog, catalog detail, card, payment, personal area, winner, and winner-management specs passed, 8/8 tests. The exercised API-error paths clear their loading state.
- Full Angular suite: 34 passed and 7 failed. The failures are existing test-harness/provider issues (`provideHttpClient()` missing in generated specs) and the stale app-title assertion; the focused verification slice passes.
- Negative security source verification: management endpoints use `Authorize(Roles = "Admin")`; both lottery and winner draw controllers now use the same requirement. A live authenticated non-admin HTTP check was not run because the database-backed API was not started.

#### Residual risks and deferred requirements

- `appsettings.Development.json` still contains environment-specific JWT and SQL connection settings, and the frontend API base URL remains hardcoded to `https://localhost:7142/api/`. These require deployment/runtime configuration work before release.
- Donor management CRUD is now available through Admin-only donor endpoints backed by role-filtered `User` records; no separate donor entity or schema change was introduced.
- Present duplicate-name validation exists, but duplicate present-number validation cannot be implemented because the current `Present` model has no number field. Any number field/schema change requires separate explicit model and migration approval.
- `Present.Price` remains integer-valued with a default of 10; the present request DTO now carries the value for management create/update.
- No database migration, database update, or live end-to-end journey was run during this verification pass.

## Decisions

- Scope: complete MVP.
- Payment: development-only for the first release; no real-money provider is assumed.
- Donors use `User` with role values `User`, `Donor`, and `Admin`; no separate donor entity.
- Card ownership uses authenticated identity and the existing `BaseModel` fields, with `CreatedBy` proposed as the persisted owner where no dedicated card owner exists.
- Backend model/migration changes: still forbidden for this execution unless separately and explicitly approved by exact file/change.
- DTOs and C# contracts are corrected before Angular adaptation.
- Migrations and database updates are prohibited until the connection string is updated.
- Current dirty worktree changes: preserved as-is and treated as pre-existing baseline.

## Course Requirements Alignment & Business Rules

1. Default Ticket Price: The default card/ticket price must be set to 10 and remain configurable per present; any create/update path that does not specify a present-specific price must resolve to the default value of 10.
2. Unique Constraints: The backend must explicitly prevent duplicate present numbers or present names during both create and update operations, and the validation must be enforced in the service layer and API request handling before persisting changes.
3. Donor Management Endpoints: Dedicated donor API operations must expose `User` records filtered by `Role = Donor`, so donor management screens and present-donor assignment flows can read and manage donor identities without creating a separate donor table or entity.
4. Validation & Errors: Duplicate present names or numbers submitted by the client must return explicit client-error responses, using `400 Bad Request` for invalid/duplicate input and `409 Conflict` when the duplicate is rejected as a uniqueness conflict, with clear server messages for the UI to display.

## Handoff Log

- 2026-09-03: Orchestration structure created. Phase 1 is ready to analyze; no application files were modified by the orchestration setup.
- 2026-09-03: Phase 1 read-only audit completed. Backend build was not executable in the agent pass; static inspection found DTO namespace drift, missing donor sources, missing `Card` ownership, incomplete payment/lottery APIs, and model/migration drift. Implementation is blocked pending approval of the Phase 1 sub-plan and the listed model/schema proposals.
- 2026-09-04: User conditionally approved donor-as-User roles, BaseModel-based card ownership, server-controlled development payment, and raffle/winner implementation. Revised execution order is C# DTO/model-contract verification first, Angular adaptation second; no migrations or database updates until the connection string is updated. Current source still shows `User.RoleId` commented, so that discrepancy requires verification and is not silently changed.

## Final Review Checklist

This is the final pre-implementation gate. Every item below must be validated before implementation is considered ready.

- [ ] Runtime configuration is verified: database connection string, JWT secret/issuer/audience, CORS, frontend API base URL, environment-specific secrets, and development-payment flags are all correctly set for the target environment.
- [ ] Authentication and authorization are fully validated: login/register flows work, JWT claims contain the correct role and user identity, protected routes are enforced on the server, and token expiry/logout behavior is handled gracefully.
- [ ] Core business rules are enforced server-side: default ticket price resolves to 10 when not supplied, duplicate present names/numbers are blocked on create and update, donor management uses verified donor-role users, and card ownership is derived from the authenticated user rather than client input.
- [ ] Payment flow is correct and traceable: only the authenticated owner can pay for a card, payment state transitions are explicit, the development-payment path is the only accepted local flow, and no client-provided payment flag is treated as proof of payment.
- [ ] Lottery and winner logic has been checked against persisted data: only eligible paid cards participate, draw execution is idempotent and authorized, winner records are persisted correctly, and the frontend shows only verified backend data.
- [ ] UX and error handling are complete: loading, empty, unauthorized, validation, duplicate, unavailable, and failure states are implemented and user-friendly; clear server messages are surfaced without silent failures.
- [ ] Testing evidence is recorded: backend build and test suite pass, frontend build and targeted tests pass, API contract validation is complete, and the main end-to-end journey is smoke-tested.
- [ ] Deployment readiness is confirmed: production-safe configuration is documented, secrets are not hardcoded, migration strategy is explicit, startup checks are verified, and rollout risks are documented.
- [ ] Scope boundaries are preserved: no model or migration change is introduced without explicit approval for the exact file and schema change, and all deferred work is documented separately.
- [ ] Sign-off evidence is attached: build output, API contract notes, QA checklist results, and final approval notes show the release meets the defined requirements.

> Final approval gate: all checklist items must be checked and the supporting evidence recorded before sign-off. No implementation is considered complete without evidence, not just confidence.

### Extra deep checks worth including
- Payment idempotency and duplicate charge prevention
- Duplicate winner prevention and draw re-run safety
- Role mismatch handling for admin/donor/user requests
- Clear 400/401/403/404/409/500 handling in both UI and API
- Local environment startup documentation and secret management
- Manual regression pass for registration → login → purchase → payment → winner flow

### Final Approval Statement
The release is approved only when every item above is either verified as complete or intentionally deferred with documented business and technical approval. No final sign-off is valid without evidence, traceability, and a clear statement of remaining risks.

## Phase 8: Design System and Bilingual UI

**Status:** Plan approved; implementation pending.

This is an additive frontend design phase. Existing requirements, decisions, phase statuses, handoffs, and final-review checklist items above remain unchanged.

### Goals and confirmed direction

- Establish a dark-only, neutral luxury visual identity: deep ink and charcoal surfaces, soft readable text contrast, restrained accents, floating rounded cards, and subtle elevation.
- Keep the experience professional, approachable, and calm. Avoid cultural motifs, a light-theme toggle, bouncing effects, and distracting animation.
- Provide English and Hebrew through a runtime language switch, persist an explicit user choice, and on first visit prefer Hebrew only when the browser language is Hebrew; otherwise use English.
- Support correct LTR English and RTL Hebrew across every existing public, authentication, customer, winner, and administration route.
- Preserve all current APIs, server-owned content, authorization, purchase/payment/draw behavior, and backend business rules.

### Design and implementation tasks

1. Inventory existing Angular routes, templates, component styles, navigation placement, and focused tests. Record behavior and state handling that must remain unchanged.
2. Add English/Hebrew message resources and a runtime language selector/service. Update the document `lang` and `dir`, persist language selection, and use locale-aware date/currency/number presentation without modifying stored values.
3. Establish semantic global design tokens for the dark palette, typography, spacing, borders, elevation, controls, focus indicators, and status states. Verify text and control contrast; do not rely on color alone to convey meaning.
4. Establish consistent page layout and navigation, and apply shared visual patterns to public catalogue/present pages, authentication, card/payment/personal pages, winner screens, admin management, and add/edit forms. Retain existing loading, empty, success, and error behavior.
5. Use CSS logical properties for direction-sensitive layout, support responsive breakpoints, keep motion restrained, and respect `prefers-reduced-motion`.
6. Add or update focused UI tests for language switching, persisted selection, document language/direction, and representative LTR/RTL states. Run the Angular build and relevant tests; report pre-existing failures separately.

### Frontend scope

- Global foundation and shell: `frontend/src/styles.scss`, `frontend/src/index.html`, `frontend/src/app/app.ts`, `frontend/src/app/app.html`, and `frontend/src/app/app.scss`.
- Shared navigation: `frontend/src/app/components/navbar/`.
- Public routes: Home, Catalog, Catalog Detail, Present, and Donor components.
- Authentication and customer routes: Login, Register, Card, Payment, Personal Area, and Winner components.
- Administration routes and forms: Admin, donor/present/card/purchase/winner management, and add/edit donor/present/winner forms.
- Add a focused `frontend/src/app/i18n/` area for bilingual resources and runtime language state. Check runtime translation-package compatibility with Angular 20 before adding a dependency; update package manifests only if required.

### Boundaries, acceptance, and handoff

- No API, backend model, database, migration, or business-rule changes are included.
- Translate frontend-owned copy and accessibility labels. Preserve the meaning of server-provided errors; do not silently replace them with success-shaped or generic messages.
- Review Hebrew copy for natural wording and mixed Hebrew/Latin values. Check narrow, medium, and wide layouts, keyboard use, screen-reader semantics, contrast, and reduced motion in both directions.
- Acceptance requires a cohesive dark-only design across the existing routes, a working persisted runtime language switch with the agreed first-visit default, correct document direction and locale presentation, preserved existing behavior, and successful Angular build plus focused UI tests (or documented pre-existing failures).
- Handoff note: implementation may begin only after this additive Phase 8 plan entry has been verified. Record changed files, test/build evidence, and residual risks in a new Phase 8 update without rewriting prior plan content.

### Phase 8 implementation handoff

**Status:** Implemented; frontend validation completed.

#### Delivered

- Applied the dark-only ink/charcoal visual system, restrained teal/champagne accents, shared responsive controls and surfaces, accessible focus states, reduced-motion support, and logical-direction styling across the existing public, authentication, customer, winner, and administration interfaces.
- Added runtime English/Hebrew selection with persisted preference, browser-language first-visit default, document `lang`/`dir` updates, translated UI copy and status messages, RTL/LTR layout support, and locale-aware currency/number formatting.
- Added the shared responsive navigation/language controls and preserved existing backend API and business behavior.
- Kept localized fallback/status messages as translation keys in templates so an already-visible error or success message changes language immediately when the user switches locales.
- Changed the app content wrapper to a non-main container because routed pages provide their own main landmark.

#### Files and areas

- Global foundation and app shell: `frontend/src/styles.scss`, `frontend/src/index.html`, `frontend/src/app/app.*`.
- Shared navigation: `frontend/src/app/components/navbar/`.
- Route templates, component styling, and presentation code under `frontend/src/app/components/`.
- New localization resources, service, translation pipe, and locale-formatting pipes under `frontend/src/app/i18n/`.
- Focused Angular UI/i18n specs for the affected components and language behavior.

#### Validation

- `npm run build` from `frontend`: passed. Angular emitted existing warnings for unused `RouterLink` imports in Donor/Present and the configured 500 kB initial-bundle budget (built initial bundle: 730.50 kB); these are warnings, not build failures.
- Focused component and localization suite: 31 tests passed.
- Full frontend suite: 43 passed; 3 service-construction specs failed because `HttpClient` is not provided in `DonorService`, `HttpService`, and `UserService` tests. These failures are outside the design/i18n changes.
- Browser smoke check: Home rendered in English/LTR and Hebrew/RTL; the mobile Hebrew catalogue had no horizontal overflow, and switching language updated an already-visible catalogue network-error message. The backend was not running during the smoke check, so the catalogue displayed its expected localized API-error state rather than loaded presents.

#### Remaining considerations

- Review the existing bundle budget and remove unused `RouterLink` imports as separate cleanup work; neither prevents this phase from building.
- Re-run the frontend smoke check with the backend running to verify populated catalogue/detail and authenticated/admin screens with live data.
- No backend, API, database, dependency-manifest, or business-rule changes were made for this phase.

## Auth and API Result Contract Follow-up (2026-10-04)

- Updated the Angular auth interceptor to attach the current JWT to requests targeting the configured API base, excluding the anonymous login and registration endpoints. A 401 from an authenticated API request clears auth state and returns the user to login.
- Made API JSON camel-case serialization explicit and aligned the backend `Result<T>` nullable `Message`/`Data` contract with the Angular result model. Login and registration now consume typed `Result` responses and check the shared `success` field.
- Added interceptor regression coverage for authenticated API calls, login/register exclusions, non-API requests, and unauthorized responses.

Validation:

- Focused Angular interceptor tests: 4 passed.
- `npm run build` from `frontend`: passed; existing unused-import and initial bundle-budget warnings remain.
- `dotnet build Project\Project.csproj --no-restore -p:OutDir=<isolated-output> -p:UseAppHost=false` from `backend/ApiProject`: passed with 0 errors and 2 package vulnerability warnings. An initial standard-output build could not copy the API DLL because running API processes held the output file; the isolated-output build verified compilation without stopping those processes.
- No database or migration operations were run.

## Login Request Body Follow-up (2026-10-04)

- Added a dedicated backend `LoginRequestDto` with required email/password validation and changed `POST /api/auth/login` to bind credentials from JSON request body rather than query parameters.
- Updated the Angular login service/component to send a typed credentials object in the POST body. The existing camel-case `Result<string>` response and JWT flow are unchanged.
- Added a focused Angular service test verifying that credentials are sent in the request body and the login URL has no query parameters.

Validation:

- Focused UserService tests: 2 passed.
- Backend API project build to an isolated output directory: passed with 0 errors and 40 warnings, including existing nullable warnings and NuGet vulnerability advisories.
- No database or migration operations were run.

## DAL Name Comparison Performance Follow-up (2026-10-04)

- Replaced database-side `ToLower()` comparisons in present and category duplicate-name checks with direct string equality. Existing exclude-ID filters and duplicate-check flow are unchanged.
- This leaves equality semantics to the configured SQL Server column collation. The live database collation was not inspected, so verify that it matches the intended case-sensitivity policy before deployment.
- No model, migration, index, or database changes were made.

Validation:

- Backend API build to an isolated output directory: passed with 0 errors; existing package and nullable warnings remain.
- Existing backend test project: 2 passed, 0 failed using an isolated output directory. These are lottery persistence tests; no name-collation integration test exists.
- A source search confirmed no `ToLower()` or `ToUpper()` remains in the backend DAL.
- The standard-output test build could not copy files because running API processes held the normal build output; validation was repeated successfully with isolated output directories without stopping those processes.

## Audit Finding Remediation (2026-10-04)

Completed the explicitly approved remediation scope for H-01 and M-02 through M-05.

- H-01: Added `UserResponseDto` and projected registration, donor-creation, and admin-creation results to safe user fields only. Password hashes are not part of the response contract. Added controller coverage for all three operations.
- M-02: Added server-side Data Annotations for user, present, category, and card request DTOs. Removed client-facing `RoleId` from `UserDto`; server-side role assignment remains authoritative. Present quantity is now required to be at least one in both DTO validation and service validation.
- M-03: Increased the `User.Password` storage annotation to 60 characters, matching the encoded BCrypt hash length. Raw registration password limits remain on `UserDto`.
- M-04/M-05: Added global exception middleware returning generic Problem Details with a trace ID and logging exception details server-side. Category, present, and user DAL exceptions now propagate to that boundary instead of returning exception text. Removed local category-controller exception responses that disclosed exception details. Request logging no longer records identity names or raw paths; authentication logs no longer include email addresses. Serilog console/file output now includes timestamp, level, source context, message, and exception.
- Added focused backend tests for safe user responses, DTO constraints, BCrypt storage length, and sanitized global error responses.

### Validation

- H-01 focused backend test run: 5 passed, 0 failed.
- M-02 focused backend test run: 9 passed, 0 failed.
- M-03 focused backend test run: 10 passed, 0 failed.
- M-04/M-05 focused backend test run passed after correcting the Problem Details content type assertion.
- Final backend test project: 12 passed, 0 failed.
- Final API build (`dotnet build backend/ApiProject/Project/Project.csproj --no-restore` with isolated output): succeeded with 0 errors and 2 existing NuGet vulnerability warnings (AutoMapper and Newtonsoft.Json).
- No frontend build was required; no client code was changed.
- No migration generation or database operation was run.

### Remaining Follow-up

- The `User.Password` model annotation changes EF's intended column length, but no migration was generated or applied. Verify the deployed column and prepare/apply an explicitly approved migration after the connection-string prerequisite is resolved.
- The audit's configuration-secrets finding (M-01) was outside this approved remediation scope and remains to be verified/remediated separately.

## Backend Role Management and Startup Seeding

Completed after explicit approval to include full role-model CRUD APIs.

- Added `IRoleService`/`RoleService` and `IRoleDal`/`RoleDal`. Startup seeds `User`, `Donor`, and `Admin`, creates missing roles, and reactivates existing inactive or soft-deleted built-ins while preserving their descriptions and creation metadata.
- Registered the role DAL and service in `Program.cs` and run seeding in an async dependency-injection scope before the API begins accepting requests. Startup errors are allowed to surface.
- Added `RoleDto` and Admin-only `api/role` list, get-by-ID, create, update, and soft-delete endpoints. Role reads include inactive, non-deleted roles.
- Built-in roles cannot be renamed, deactivated, or deleted. The API rejects deactivating or deleting any role assigned to a user, including soft-deleted users. New roles record the authenticated admin's user ID as `CreatedBy`.
- Custom role CRUD manages role definitions only; existing user-role assignment behavior remains unchanged. No model, migration, or database-update changes were made.

Validation:

- Backend solution build succeeded with 0 errors using `dotnet build Project.sln -p:UseAppHost=false`. The default apphost output was locked by an already-running API process; no process was stopped. Existing AutoMapper and Newtonsoft.Json vulnerability warnings remain.
- Focused role tests passed: 10 passed, 0 failed. They were run with an isolated output directory because the running API also locked its regular project output.
- Static diagnostics found no errors in the role implementation or tests. The backend DAL search confirms this addition introduces no `ToLower()`/`ToUpper()` calls.
- No database connection, migration generation, or database update was performed.
- Repository-wide `git diff --check` still reports a trailing-whitespace line in the already-modified `Project/Profiles/CategoryProfile.cs`; that unrelated line was left untouched.

Residual risk:

- Role-name uniqueness is checked case-insensitively in application code because the current schema has no unique role-name constraint. Concurrent role creation or simultaneous first-time seeding across multiple API instances could race; adding a database uniqueness constraint requires a separately approved migration.

## Admin Management Menu Back Link (2026-10-08)

- Added an English/Hebrew back-arrow link to `/admin` on all eight management screens: presents, donors, categories, roles, users, cards, winners, and purchases. The arrow mirrors in RTL layouts.
- Kept the change limited to navigation and presentation; no API, CRUD, authorization, or persistence behavior changed.

Validation:

- Focused management component tests: 35 passed, 0 failed.
- Angular production build: passed. Existing unused `RouterLink`, browser-mapping-data, and initial-bundle-budget warnings remain.

## Catalogue Add-to-Basket Action (2026-10-08)

- Added an add-to-basket action to every catalogue present for authenticated users only. The UI disables concurrent adds and shows accessible, localized success and server-error feedback.
- Reused the existing authorized `POST /api/card` endpoint with only the selected `presentId`; no backend, model, migration, or purchase-rule changes were made.
- Aligned Angular `CardService.addCard` and the existing cards page with the verified plain-text success response from the controller, preserving server error messages.
- Added English and Hebrew labels and focused tests for authentication visibility, per-present buttons, pending state, request payload/response type, success, and failure.

Validation:

- Focused catalogue and card tests: 12 passed, 0 failed.
- Angular production build: passed. Existing unused `RouterLink`, browser-mapping-data, and initial-bundle-budget warnings remain.
- `git diff --check`: passed.

## API Notification Scope and Toast Background Follow-Up (2026-10-21)

### Approved changes

- Limit global API toast notifications for successful responses to action requests using non-GET HTTP methods (for example, add-to-cart, registration, payment, and management actions). Suppress successful GET fetch notifications, but retain global error toasts for failed GET fetches. Keep existing page loading, empty, and inline failure states unchanged.
- Continue to display non-empty API messages for action responses and action HTTP errors, retaining the successful-login JWT exclusion.
- Change the toast surface from status-tinted green/red backgrounds to one muted slate-indigo surface (`#262a3d`) consistent with the dark theme. Preserve the semantic success/error edge stripe, status label, and accessible announcement.
- Update focused interceptor and toast styling tests/documentation only; no API, business, backend, or persistence changes.

### Exact files and validation

- `frontend/src/app/interceptors/api-message-interceptor.ts`: classify notification eligibility by request method; skip all GET success/error message notification.
- `frontend/src/app/interceptors/api-message-interceptor.spec.ts`: assert GET success is silent, GET failures still notify, while mutation success/error messages still appear and response/error propagation plus login-token exclusion remain intact.
- `frontend/src/styles.scss`: use a consistent muted slate-indigo toast background while retaining severity indicators.
- `frontend/src/app/services/apiMessageService/api-message-service.spec.ts`: verify both toast variants retain their semantic classes and requested surface styling if testable from configuration.
- Run `npm.cmd run build`, focused Angular tests, and `git diff --check`; report browser-launch constraints separately.

### Risks and boundary

- GET failures can display both the requested global toast and the existing inline error state; this is retained by explicit user choice.
- Do not alter user-owned or unrelated pre-existing work.

### Implementation result

- Approved by the user with the clarification that successful GET fetches are silent, but GET fetch failures still display a toast.
- The interceptor now suppresses toast display only for successful GET responses. Failed GETs and non-GET action responses continue through the global toast service.
- Replaced the success/error-specific surface fills with one muted slate-indigo (`#262a3d`) background while preserving semantic severity borders and labels.

Validation:

- `npm.cmd run build`: passed. Existing bundle-size, catalogue stylesheet budget, and browser-mapping-data warnings remain.
- `npm.cmd test -- --watch=false`: the full test bundle compiled without TypeScript errors; Karma could not launch Chrome, so browser test assertions were not run.
- `git diff --check`: passed.

## Personal Area Consolidation (2026-10-08)

- Replaced the separate customer “My Cards” and “My Purchases” navigation links with one guarded Personal Area link.
- Expanded the Personal Area to show the signed-in user's name, email, and phone from the existing JWT claims, plus the user's cards from the existing owner-scoped API. Added paid/unpaid status and links to existing card management and checkout routes.
- Added English/Hebrew copy and focused navbar, Personal Area, and auth-claim coverage. Updated JWT payload decoding to preserve UTF-8 identity values such as Hebrew names.
- No backend, API, model, migration, purchase, or payment behavior was changed. Existing card and payment routes remain available.

Validation:

- VS Code diagnostics reported no issues in the changed TypeScript and Angular template files.
- Focused Angular tests and `npm run build` could not run: the Windows sandbox rejected PowerShell before command execution because local network access is not enabled. The test runner also could not discover Angular spec files directly.

Follow-up:

- Re-run the focused frontend specs and production build when local command execution is available.

## Approved Cross-Cutting Remediation (2026-10-08)

The approved remediation addressed exposed configuration, development payment boundaries, winner-count semantics, server-authoritative administration, and environment-specific host/CORS configuration without changing the database schema.

- Removed the JWT signing key and database connection string from checked-in Development settings. Startup now requires `JWTSettings__SecretKey` and `ConnectionStrings__DefaultConnection`; local `.env*` and appsettings override files are ignored except `.env.example`.
- Added explicit `AllowedHosts` and `Cors__AllowedOrigins__N` configuration validation. Development settings allow only the local Angular origin; other environments must provide their own host and CORS values.
- Replaced the generic payment route with `POST /api/card/development-payment`. The backend returns 404 outside Development and compares the submitted amount to the server-calculated total for the authenticated user's active, non-deleted unpaid cards. The Angular production build disables the development-payment action.
- Defined `Present.Quantity` as the number of distinct paid cards selected as winners, not a limit on ticket sales. Draws reject insufficient eligible cards and persist all configured winners and lottery completion together in a relational transaction.
- Kept client-side guards as navigation convenience; administrative controller authorization remains enforced by the backend. Added focused authorization metadata coverage.
- Updated onboarding and user-facing English/Hebrew copy for the operational settings and winner-count semantics.

Validation and residual work:

- Workspace diagnostics reported no errors in the files previously checked; this is not a substitute for builds or tests.
- Focused backend/frontend tests and builds were not successfully executed because command execution was blocked by the Windows sandbox. The test runner did not discover Angular specs when passed individual spec-file paths.
- The replacement JWT secret and production database connection must still be provisioned in the trusted deployment environment, and the service must be redeployed. Operational rotation could not be performed from this workspace.
- Previously exposed values may remain in Git history. No history rewrite was performed; assess history cleanup separately and coordinate any required credential rotation.
- Existing historical draws were not migrated. Validate database-backed relational transaction behavior, end-to-end authorization, and the production environment configuration before release.

## Catalogue Quantity Selection and Present Detail Add-to-Basket (2026-10-21)

- Added a quantity selector and add-to-basket action to authenticated catalogue cards and the single-present detail view. Catalogue controls reveal on hover/focus and stay visible on touch/no-hover devices.
- Extended the existing authenticated `POST /api/card` request with a quantity defaulting to one. The server creates separate unpaid card rows for the authenticated owner and saves the batch together; request quantity is limited to 1–100 as a technical safety boundary, not by inventory.
- Preserved the existing rule that `Present.Quantity` is the winner count, not a ticket-sales limit. No entity, migration, ownership, or payment behavior changed.
- Added English/Hebrew UI copy and focused frontend/backend coverage for quantity payloads, one-card compatibility, owner/payment state, invalid quantity, request feedback, and unavailable/completed presents.

Validation status:

- The VS Code test runner did not discover the Angular spec paths. The backend test target reported a build failure without exposing diagnostic output.
- Direct CLI validation could not run because the Windows sandbox could not start shell commands while local network access was disabled. Backend/frontend tests and builds remain unverified and should be rerun after command execution is enabled.

## App-Wide API Response Message Toasts (2026-10-21)

- Added a frontend-only API message interceptor and queued Angular Material snack bars for non-empty API result messages and plain-text responses. Toasts are bottom-centered, dismissible, direction-aware, and labeled with localized success/error status and dismiss text.
- Preserved API payloads and HTTP errors, limited notifications to the configured API, and kept existing page-level fallback and validation/loading states without rendering the raw server message twice.
- Excluded successful `/auth/login` messages because the API contract uses that field for the JWT. Human-readable login failures still appear.
- Aligned the card-removal frontend response type with the controller's existing plain-text message response so a successful deletion is not followed by a false page-level failure.
- Updated action components to avoid repeating successful server messages inline. Added focused interceptor, queue, and component coverage.

Validation:

- `npm.cmd run build`: passed. Existing browser-mapping-data, initial bundle-size, and catalogue stylesheet budget warnings remain.
- The focused and full Angular test bundles compiled, but Karma could not launch Chrome in this environment, so assertions did not execute.
- `git diff --check`: passed.

## Personal Area Details Presentation (2026-10-22)

- Added a localized hover title and accessible description to the authenticated Personal Area navigation link in English and Hebrew. Navigation remains a normal link for keyboard and touch users.
- Refined the profile panel into a compact identity and contact layout. The name and email are each shown once; the existing phone and read-only notice remain visible.
- Preserved the existing JWT claim source, guarded route, card history, and checkout behavior. No API, backend, model, migration, or payment changes were made.

Validation:

- `npm.cmd run build` from `frontend`: passed. Existing browser-mapping-data and bundle/style budget warnings remain.
- The focused navbar and Personal Area test bundles compiled, but Karma could not launch Chrome, so test assertions could not execute.
- `git diff --check`: passed.
- Browser smoke inspection confirmed the navigation link and localized helper content render. The locally generated test token was rejected by the API (401), so the authenticated page redirected before a complete live-data or visual-hover check.

## Donor Presents in Personal Area (2026-10-22)

- Added a Donor-role-only personal-area section that loads the authenticated user's donated presents, including inactive and soft-deleted presents, with localized loading, empty, retry, and failure states.
- Added `GET /api/present/myPresents`, authorized for the Donor role and scoped server-side to the authenticated `NameIdentifier`; no client-supplied donor ID is accepted.
- Reused the existing `Present.DonorId` relationship and included inactive records. No entity, migration, or database changes were made.
- Added backend donor-filter coverage (including soft-deleted records) and role-authorization coverage, plus frontend service/component coverage for role gating and inactive-present display.

Validation:

- `npm.cmd run build` from `frontend`: passed. Existing initial-bundle and stylesheet budget warnings remain.
- Focused Personal Area and PresentService test bundles compiled, but Karma could not launch ChromeHeadless, so the assertions did not execute.
- Focused backend tests could not run because the sandbox denied access to `C:\Users\משתמש\.gitconfig` while MSBuild read repository information.
- `git diff --check` passed for the feature files and plan update. The full worktree check still reports pre-existing trailing whitespace in `frontend/src/app/services/authService/auth-service.ts`, which was not changed for this feature.

## Present Category Names in API Responses (2026-10-22)

- Added nullable `categoryName` to app-facing present response data and nested card present data. Category names are fetched in batches, including for inactive or soft-deleted categories, without changing entities or database schema.
- Applied the response mapping to `GET /api/present/getAllPresents`, `GET /api/present/{id}`, donor-authorized `GET /api/present/myPresents`, authenticated card reads, and development-payment results.
- Updated catalogue, present detail, card, checkout, personal-area, present-management, and winner views to display the associated category name from API data. Added localized fallback text when a category has no name.
- Added mapper/DAL and frontend component regression coverage. Existing routes, authorization, and purchase/payment/lottery rules remain unchanged.

Validation:

- Backend `dotnet build` with an isolated output directory: passed with 0 errors and the existing two package vulnerability warnings.
- `npm.cmd run build` from `frontend`: passed. Existing browser-mapping, initial bundle-size, and stylesheet budget warnings remain.
- Focused backend tests compiled, but VSTest aborted because it could not connect to the testhost; assertions did not execute. Focused Angular test bundles compiled, but Karma could not launch ChromeHeadless; assertions did not execute.
- `git diff --check` reports only pre-existing trailing whitespace in `frontend/src/app/services/authService/auth-service.ts`; no files changed for this feature were reported.

## Local Present Image Uploads (2026-10-22)

- Added an Admin-only multipart image-upload endpoint. JPEG, PNG, and WebP file signatures are checked, files are limited to 5 MB, names are generated by the server, and files are stored under the API's `wwwroot/uploads/presents` directory and served as static files.
- Present writes now accept only the generated local image-path format. The present-management form uploads a selected file before saving, retains an existing local image when editing, and requires replacement of legacy external URLs before they can be saved again.
- Catalogue cards, present details, and the management preview resolve local image paths against the API origin. Existing external images can still display during transition; bulk import of legacy URLs was explicitly deferred.
- No model, migration, or database changes were made. Runtime image files are excluded from Git.

Validation:

- Backend solution build passed with isolated output and existing package vulnerability warnings.
- Angular production build passed with existing bundle and stylesheet budget warnings.
- Focused backend tests compiled but VSTest could not connect to the testhost. Focused Angular test bundles compiled but ChromeHeadless did not start, so test assertions were not executed.