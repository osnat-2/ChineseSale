---
name: auth
description: "Workflow for implementing verified authentication in the Chinese Sale Angular and C# application. Use for JWT state, HTTP interceptors, guards, roles, logout, expiry, and auth tests after contract baseline approval."
---

# Auth Skill

## Scope

Own authentication state and protected navigation. Backend authorization remains authoritative.

## Required Analysis

- Read root `plan.md` and the Contract Baseline handoff.
- Verify actual login/register request and response shapes from backend source.
- Trace token generation, claims, role assignment, expiry, and `NameIdentifier`.
- Inspect Angular user service, app providers, routes, components, existing models, and tests.
- Identify storage, SSR/browser, logout, refresh, and unauthorized-response risks.

## Required Sub-Plan

Before edits, specify exact files/symbols, token lifecycle, interceptor exclusions, guard behavior, role assumptions, scope boundaries, approval-gated backend work, and focused validation. Stop for approval.

## Acceptance Checks

- Verified token is stored and attached only where appropriate.
- Login, logout, expiry, and unauthorized behavior are deterministic.
- Customer/admin guards match verified claims.
- Tests cover token attachment and guard decisions.
