---
name: purchase-payment
description: "Workflow for Chinese Sale card purchases, checkout, personal area, and development payment. Use for ownership, availability, paid-state transitions, idempotency, and API-backed purchase history."
---

# Purchase and Payment Skill

## Scope

Own the customer card/purchase/payment journey. Real-money provider integration is out of scope for the MVP.

## Required Analysis

- Read root `plan.md`, Contract Baseline and Auth handoffs, and relevant frontend instructions.
- Trace Card controller, BLL, DAL, entity, context, migration, DTO/profile, and Angular card/payment/personal-area code.
- Verify authenticated user ownership, quantity semantics, availability, duplicate requests, and paid-state persistence.
- Identify whether server-controlled development payment can be implemented without model/schema changes.
- Never accept client-side `IsPaid` as payment proof.

## Required Sub-Plan

Before edits, define the lifecycle, endpoints, authorization, state transitions, retry/idempotency behavior, exact files/symbols, approval-gated persistence changes, and focused tests. Stop for approval.

## Acceptance Checks

- Ownership and eligibility are server-enforced.
- Payment confirmation is server-controlled and idempotent.
- History is loaded from the API, not client-only state.
- Invalid, duplicate, unavailable, unauthorized, success, and refresh cases are tested.
