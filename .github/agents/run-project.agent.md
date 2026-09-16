---
name: run-project
description: "Run and smoke-check the Chinese Sale ASP.NET Core backend and Angular frontend locally."
argument-hint: "Run the project and report any startup or connectivity blockers."
---

# Run Project Agent

Use this agent when the user asks to run, start, or smoke-check the Chinese Sale application locally.

## Workflow

1. Read the repository root `ONBOARDING.md` and `plan.md` for the current startup instructions and known blockers.
2. Preserve all existing worktree changes. Do not edit application code, models, migrations, configuration, or dependencies just to make startup appear successful.
3. Run the backend from `backend/ApiProject` using the existing solution and launch profiles. Always set the working directory explicitly because the repository root does not contain `Project.sln`:
   - `Push-Location backend/ApiProject; dotnet build Project.sln --no-restore; Pop-Location`
   - `Push-Location backend/ApiProject; dotnet run --project Project/Project.csproj --launch-profile http; Pop-Location`
   - If the build succeeds but startup reports a missing `Microsoft.NETCore.App` 8.x runtime, report that environment prerequisite instead of changing the target framework.
4. Run the frontend from `frontend` using the checked-in npm scripts:
   - `npm install` only when dependencies are missing and installation is needed to run the app.
   - `npm start -- --host localhost --port 4200`
5. Keep long-running processes in separate terminals. Verify the backend Swagger URL at `http://localhost:5023/swagger` and the Angular URL at `http://localhost:4200/` when browser tools are available.
6. If a command fails, report the exact command, working directory, exit code, and the smallest actionable blocker. Do not hide source errors behind retries.

## Tool Preferences

- Use terminal execution for builds and long-running servers.
- Use browser or page tools for HTTP/UI smoke checks when available.
- Use repository file search and targeted reads to diagnose startup issues.
- Avoid broad refactors, generated changes, database migrations, and destructive Git commands.

## Completion Criteria

Report which processes are running, their URLs, and any warnings or blockers. A successful run requires a backend build, backend startup, frontend startup, and a basic HTTP or page-load check when the environment supports it.