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
| 2. Auth | `auth` | Blocked by Phase 1 | Pending |
| 3. Catalog | `catalog` | Blocked by Phase 1 | Pending |
| 4. Purchase/Payment | `purchase-payment` | Blocked by Phases 1-2 | Pending |
| 5. Admin | `admin` | Blocked by Phases 1-2 | Pending |
| 6. Lottery | `lottery` | Blocked by Phases 1-2-4 | Pending |
| 7. Verification | Main workflow | Not started | Pending |

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

Implement verified JWT state, interceptor, logout/expiry handling, auth guard, admin guard, and focused tests. Align claims and roles with Phase 1 findings.

## Phase 3: Catalog

Implement public present/category routes and typed services, search/filtering, details, and loading/empty/error states using verified read endpoints.

## Phase 4: Purchase/Payment

Implement the supported card lifecycle, ownership/availability checks, checkout, personal area, and a server-controlled development payment flow. Pause for approval if persistence cannot enforce ownership or payment state.

## Phase 5: Admin

Complete authorized admin routes and management workflows for presents, categories, donors, cards/purchases, and supported user operations. Donor work remains blocked if it requires an unapproved model/schema change.

## Phase 6: Lottery

Expose and implement an authorized, persisted, idempotent draw workflow over eligible paid cards. Align winner DTOs and frontend views with verified contracts. Notification is secondary to persisted correctness.

## Phase 7: Verification

Run backend and Angular builds/tests, API and database checks, and the complete manual journey. Document local setup, development payment semantics, configuration, and deferred approval-gated changes.

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
