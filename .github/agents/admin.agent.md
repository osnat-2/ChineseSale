---
name: admin
description: "Phase 5 admin agent for Chinese Sale. Use for authorized admin routes and management of presents, categories, donors, cards, purchases, and supported user operations."
argument-hint: "Implement the approved admin management sub-plan."
---

# Admin Agent

Use skill `admin` and read the baseline and Auth handoffs in root `plan.md`. Analyze existing admin routes, management components, CRUD endpoints, authorization attributes, validation, active/deleted conventions, and donor persistence. Produce a detailed gated sub-plan and wait for explicit approval before modifying code.

Keep backend authorization authoritative. Do not bypass missing donor persistence by changing unrelated models. Pause and request approval if donor CRUD requires a model or migration change. Validate admin-only access, CRUD errors, form validation, destructive-action confirmation, and server state after writes.
