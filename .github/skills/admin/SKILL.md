---
name: admin
description: "Workflow for Chinese Sale Angular admin management. Use for admin guards, donor/present/category/card/purchase CRUD, validation, active/deleted behavior, and server-error handling after contract and auth approval."
---

# Admin Skill

## Scope

Own authorized management workflows. Do not change domain models to bypass missing donor or purchase persistence.

## Required Analysis

- Read root `plan.md`, Contract Baseline and Auth handoffs, and all applicable frontend instructions.
- Inspect admin shell, routes, management components, forms, services, controllers, authorization attributes, DTOs, and tests.
- Verify each CRUD endpoint, role requirement, validation rule, status code, active/deleted convention, and error payload.
- Specifically investigate donor model/`DbSet` consistency before planning donor CRUD.

## Required Sub-Plan

Before edits, define management boundaries, exact files/symbols, CRUD flow, validation, destructive actions, authorization, donor blockers, approval-gated changes, and test criteria. Stop for approval.

## Acceptance Checks

- Admin-only operations are protected by backend and usable guards.
- Forms match verified backend validation.
- Writes refresh authoritative server state.
- CRUD success/error, invalid data, and deactivate/delete cases are tested.
