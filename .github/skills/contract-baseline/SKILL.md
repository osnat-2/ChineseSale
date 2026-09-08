---
name: contract-baseline
description: "Workflow for auditing the Chinese Sale C# and Angular contract baseline before implementation. Use first for builds, endpoint contracts, DTOs, claims, persistence feasibility, migrations, donor references, and card ownership."
---

# Contract Baseline Skill

## Scope

Own the Phase 1 contract and data-integrity audit. Do not implement feature behavior in this skill.

## Required Analysis

- Read root `plan.md` and existing frontend instructions.
- Inspect and build the C# solution before proposing fixes.
- Trace controller -> Bll -> DAL -> EF model/context -> migration paths.
- Compare DTO namespaces, profiles, serialized request/response shapes, routes, status codes, and authorization attributes.
- Verify JWT creation, claims, role mapping, and `NameIdentifier` behavior.
- Verify donor persistence and card user ownership in source and database configuration.
- Identify whether development payment and lottery workflows are persistable with the current schema.
- Treat all existing worktree modifications as user-owned baseline.

## Required Sub-Plan

Before edits, report exact files/symbols, verified facts, blockers, proposed contract-only repairs, approval-gated model/schema changes, and commands for validation. Stop for approval.

## Acceptance Checks

- Backend build result recorded.
- Contract matrix includes endpoint, method, request, response, status, auth, and source owner.
- Persistence feasibility decisions are explicit.
- No model or migration edits occur without approval.
