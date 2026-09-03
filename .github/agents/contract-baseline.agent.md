---
name: contract-baseline
description: "Phase 1 contract and data integrity agent for Chinese Sale. Use first to audit the C# API, DTOs, models, database context, migrations, JWT claims, and Angular API assumptions before feature implementation."
argument-hint: "Analyze the current backend/frontend contract baseline."
---

# Contract Baseline Agent

Use skill `contract-baseline`. This phase runs first and blocks feature agents until its report is approved.

Analyze the compiled backend and Angular API assumptions. Inspect `Program.cs`, controllers, BLL, DAL, DTOs, profiles, models, `AppDBContext`, migrations, auth configuration, and relevant Angular services/models. Run the backend build as a diagnostic. Verify donor persistence, card ownership, payment state, JWT claims, routes, status codes, and serialization.

Before any application edit, produce and present the required gated sub-plan. During this phase, model classes and migrations are read-only. Contract repairs may be proposed, but implementation waits for approval.
