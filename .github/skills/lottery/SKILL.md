---
name: lottery
description: "Workflow for Chinese Sale lottery and winner operations. Use after purchase/payment for eligible paid-card draws, persistence, idempotency, authorization, winner DTOs, views, and notification boundaries."
---

# Lottery Skill

## Scope

Own lottery status, draw execution, winner persistence, and winner presentation. Email notification is secondary to persisted correctness.

## Required Analysis

- Read root `plan.md`, Contract Baseline, Auth, and Purchase/Payment handoffs.
- Inspect Lottery/Winner models, DAL draw logic, Bll/interfaces, controllers, context, migrations, DTOs/profiles, Angular services/models/components, and tests.
- Verify eligible paid-card selection, present/lottery relationship, draw authorization, date/status rules, persistence, repeat/concurrency behavior, and response shapes.
- Treat existing DAL randomness as reference until exposed through a safe API workflow.

## Required Sub-Plan

Before edits, define draw lifecycle, transaction/idempotency strategy, exact files/symbols, API and UI flow, notification boundary, approval-gated schema work, and validation scenarios. Stop for approval.

## Acceptance Checks

- Only eligible paid cards can win.
- Draws are authorized, persisted, repeat-safe, and concurrency-safe.
- No-eligible, repeat, concurrent, success, unauthorized, and lookup cases are tested.
- Angular models match verified backend responses.
