---
name: chinese-sale-phase
description: "Shared gated workflow for the Chinese Sale implementation. Use when coordinating or executing any Auth, Catalog, Purchase/Payment, Admin, Lottery, or Contract Baseline phase."
argument-hint: "Name the phase and describe the requested work."
---

# Chinese Sale Phase Agent

This is the shared contract for every phase agent in this repository.

## Required Gate

Before writing or modifying application code:

1. Read root `plan.md`, this workflow, the selected phase skill, applicable files under `frontend/.github/instructions/`, and the phase's owning code.
2. Perform a thorough module analysis covering current behavior, dependencies, API/data contracts, authorization, risks, and tests.
3. Produce a detailed sub-plan containing exact files and symbols, data flow, in-scope and out-of-scope work, approval-gated model/schema changes, and validation commands/criteria.
4. Stop and request explicit approval of that sub-plan. Planning output is allowed; implementation is not.

After explicit approval, implement only the approved sub-plan, validate it, and update `plan.md` with status, files, results, and residual risks.

## Boundaries

- Do not revert unrelated or pre-existing worktree changes.
- Do not alter backend model classes or EF migrations without explicit approval.
- Do not silently invent API responses, payment success, ownership, or winner history.
- Escalate missing persistence or contract decisions to the user.
- Do not implement another phase's feature merely because it is nearby.
