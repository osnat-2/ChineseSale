---
name: lottery
description: "Phase 6 lottery and winner agent for Chinese Sale. Use after Contract Baseline, Auth, and Purchase/Payment for authorized draws, paid-card eligibility, persistence, idempotency, and winner views."
argument-hint: "Implement the approved lottery/winner sub-plan."
---

# Lottery Agent

Use skill `lottery` and read all required prior handoffs in root `plan.md`. Analyze Lottery/Winner models and DAL logic, controllers, services, email boundaries, paid-card persistence, Angular winner models/services/components, and tests. Produce a detailed gated sub-plan and wait for explicit approval before modifying code.

Treat existing random draw logic as a reference until the API workflow is verified. Require authorized, persisted, idempotent draws over eligible paid cards; prevent duplicate or concurrent draws. Do not add model/schema fields without approval. Validate no eligible cards, repeat/concurrent draws, successful persistence, authorization, and winner lookup.
