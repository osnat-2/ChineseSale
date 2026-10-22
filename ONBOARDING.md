# Chinese Sale Onboarding Guide

Welcome to the project! This guide is designed to help you get started working on the **Chinese Sale MVP** in a calm, organized, and independent way.

## 1. Project Overview

Chinese Sale is a website for selling tickets in a Chinese auction. Users can register and sign in, browse gifts and categories, purchase tickets, and participate in a drawing when the relevant features are available. The system also includes administrator and donor roles, management workflows, development-mode payment, and winner-related features.

The architecture has two parts:

- **Backend**: An ASP.NET Core Web API on .NET 8, with controllers, BLL and DAL layers, DTOs, AutoMapper, JWT, and Entity Framework Core.
- **Frontend**: An Angular 20 application with routes, components, services, models, and a UI that also uses Angular Material and Bootstrap.
- **Communication**: The frontend calls the API using `HttpClient`. The API URL configured in the HTTP service is `https://localhost:7142/api/`.

## 2. Set Up Your Development Environment

Install the following:

- Git.
- .NET 8 SDK.
- A version of Node.js compatible with Angular 20, including `npm`.
- A development environment such as VS Code or Visual Studio.
- SQL Server or the database environment configured for the team, if you need access to data-backed operations.

After cloning the repository, open two separate terminals: one for the backend and one for the frontend. This lets you run both parts at the same time.

## 3. Git: Getting Started and Creating a Personal Branch

Work on a personal branch, not directly on `Dev`.

```powershell
git clone <REPOSITORY_URL>
Set-Location ChineseSale
git switch -c Gilli
git push -u origin Gilli
```

If the `Gilli` branch already exists on the remote:

```powershell
git fetch origin
git switch Gilli
git pull --ff-only origin Gilli
```

Before starting work, check the repository status:

```powershell
git status --short --branch
git branch -vv
```

### Recommended Workflow

1. At the start of each day, bring updates from `Dev` into your personal branch:

   ```powershell
   git fetch origin
   git pull --ff-only origin Dev
   ```

   If you have uncommitted local changes or the command cannot fast-forward, stop and coordinate before continuing. Do not run `reset`, `clean`, `checkout`, or delete existing work without explicit approval.

2. Work, test, and create small, descriptive commits on your personal branch.
3. Before sharing your work, run the relevant tests and review the diff.
4. Push your branch:

   ```powershell
   git push origin Gilli
   ```

5. When a task is complete, open a pull request from `Gilli` to `Dev`. Include a brief description of the changes, tests performed, and known limitations.

For Git operations, use the `git` agent and the `git-workflow` skill. They help check branches, upstream settings, changes, commits, and pull requests while preserving existing work. Do not automatically commit, push, or open a pull request without first confirming the scope of the action.

## 4. Working with Agents and Skills

The `.github` directory contains the project's working instructions:

- `.github/agents/` contains dedicated agents: `contract-baseline`, `auth`, `catalog`, `purchase-payment`, `admin`, `lottery`, `chinese-sale-phase`, and `git`.
- `.github/skills/` contains the knowledge and procedures required for each area, including `contract-baseline`, `auth`, `catalog`, `purchase-payment`, `admin`, `lottery`, and `git-workflow`.
- `.github/instructions/` contains shared safety and workflow rules for C#, Angular, and documentation files.

An agent specializes in a work area; a skill provides the procedure and rules the agent should follow. Before starting a task, choose the appropriate agent and skill, read `plan.md`, and confirm that the work is at the right stage.

### Gated Sub-Plan: Approval Gate

Each significant phase follows a staged approval process:

1. Read `plan.md`, the agent and skill instructions, and the code owned by the module.
2. Analyze contracts, dependencies, risks, and the files and symbols that may change.
3. Present a detailed **Gated Sub-Plan** with the change boundaries, exact files, validation criteria, and dependencies.
4. Stop and wait for explicit approval.
5. Only after approval, edit code, run tests, and update `plan.md` with decisions, results, and handoffs.

In particular, do not modify `backend/ApiProject/Project/Models/` or `Migrations/`, or run migration or database-update commands, without explicit approval for that specific change. Backend authorization is the source of truth; Angular guards improve navigation but are not security controls.

## 5. Dividing Work in Parallel

To reduce merge conflicts, each person should primarily work within a clearly defined module boundary. Coordinate with your teammate and update the sub-plan before changing a shared file.

### Backend Contracts and Infrastructure

Responsible for reviewing existing models, DTOs, profiles, controllers, BLL, and DAL, and documenting API contracts. This infrastructure phase (`contract-baseline`) must be completed before Angular adjustments. Do not add entities or change the schema without separate approval.

### Auth

Responsible for `UserController`, `UserService`, JWT, claims, the interceptor, logout, token expiration, the auth guard, and the admin guard. This includes registration, sign-in, and distinguishing `User`, `Donor`, and `Admin` according to the approved contract.

### Catalog

Responsible for the public catalog: gifts, categories, search, filtering, lists, and gift details. On the Angular side, this includes services, models, routes, and components related to home, presents, and categories, including loading, empty, and error states.

### Purchase and Payment

Responsible for tickets, ownership, availability, checkout, the personal area, and development-mode payment. Ownership and payment status are determined by the server; do not trust `userId`, `CreatedBy`, or `IsPaid` values sent by the client.

### Admin and Donor

Responsible for management screens for gifts, categories, donors, tickets, and purchases, including Admin authorization, validation, active/deleted behavior, and server-error handling. A donor is represented in the system as a `User` with the appropriate role, not as a new entity without approval.

### Lottery and Winner

Responsible for the drawing, eligibility of paid tickets, winner persistence, idempotency, and authorization. Winner screens and notifications must use data returned by the API, not mock data in the frontend.

### Shared Files

`plan.md`, primary routes, `app.config.ts`, the HTTP service, shared models, and style files may cause conflicts. Change them only after coordinating, in a focused commit, and notify your teammate right away.

## 6. First Steps for Running the Project Locally

### Backend

```powershell
Set-Location backend\ApiProject\Project
dotnet restore
dotnet build
dotnet run --launch-profile http
```

After the backend starts, open:

- Swagger: `http://localhost:5023/swagger`
- API over HTTPS: `https://localhost:7142`
- API over HTTP: `http://localhost:5023`

You can also run the project from Visual Studio or VS Code using the `http` or `https` profile. If you see a local HTTPS certificate warning, follow the .NET instructions for your local development environment.

### Frontend

In a second terminal:

```powershell
Set-Location frontend
npm install
ng s -o
```

Open `http://localhost:4200/`. During development, Angular rebuilds after changes. Useful checks:

```powershell
npm run build
npm test
```

If the frontend cannot connect, first check that the API is running, that the URL in the HTTP service matches the selected profile, and that the HTTPS certificate or CORS is not blocking the request.

## 7. `plan.md` Is the Source of Truth

`plan.md` is the project's central working document—a shared starting point for understanding the context before each task. It defines the phase order, approved decisions, constraints, dependencies, status, handoffs, and test results.

Before starting a task:

1. Read the Mission, Status, and Execution Contract.
2. Check the task's phase and whether anything is blocking it.
3. Confirm that the planned files are within your area of responsibility.
4. After the work, add a brief handoff log listing changed files, tests, risks, and dependencies for the next phase.

## 8. Recommended First-Day Checklist

1. Clone the repository and create the `Gilli` branch.
2. Read `plan.md` and this guide.
3. Run `dotnet build` and `npm run build` to check the baseline.
4. Start the API and confirm that Swagger opens.
5. Start Angular and confirm that the app loads at `localhost:4200`.
6. Choose one module, read its relevant agent and skill instructions, and prepare a Gated Sub-Plan before changing code.
7. Coordinate with your teammate on shared files and task boundaries.

This workflow helps us maintain a steady pace, a clear history, fewer conflicts, and a way to integrate the MVP's parts incrementally.

## API Environment Configuration

The API reads secrets and machine-specific settings from environment variables. Do not
put them in `appsettings*.json`, commit them, or paste live values into documentation.
For a local Development run, set:

```powershell
$env:JWTSettings__SecretKey = "<new random secret of at least 32 bytes>"
$env:ConnectionStrings__DefaultConnection = "<local SQL Server connection string>"
dotnet run --launch-profile http
```

The Development profile allows only `localhost`/`127.0.0.1` hosts and the
`http://localhost:4200` frontend origin. For deployment, provide all of the following
through the deployment environment or secret manager:

- `JWTSettings__SecretKey` (unique secret, at least 32 bytes)
- `JWTSettings__Issuer`
- `JWTSettings__Audience`
- `JWTSettings__ExpirationInMinutes`
- `ConnectionStrings__DefaultConnection`
- `AllowedHosts` (semicolon-separated exact API host names; no wildcard)
- `Cors__AllowedOrigins__0`, `Cors__AllowedOrigins__1`, ... (exact HTTP(S) origins,
  with no path or wildcard)

Startup fails when required JWT/database settings, explicit hosts, or explicit CORS
origins are missing or invalid. Rotate any JWT signing key exposed in prior commits by
setting a new random key in the trusted deployment secret store; this invalidates
existing tokens. The configured local connection string previously used Windows
Integrated Authentication, so verify and rotate/restrict the database service identity
and its permissions separately if that identity or database endpoint was exposed.

The simulated payment route is Development-only. It is not a payment provider and
must not be used to settle real purchases.
