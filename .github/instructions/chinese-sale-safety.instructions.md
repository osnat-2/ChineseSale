---
name: chinese-sale-safety
description: "Shared safety and workflow rules for the Chinese Sale C# and Angular project. Use when implementing or reviewing backend contracts, authentication, catalogue, purchases, payments, admin, lottery, or winner features."
applyTo: "**/*.{cs,csproj,ts,html,scss,json,md}"
---

# Chinese Sale Safety Rules

- Read the repository-root `plan.md` before phase work.
- Treat existing uncommitted changes as user-owned. Do not revert them.
- Backend models and EF migrations must not be changed without explicit user approval.
- Before implementation, analyze the owning module and write a detailed sub-plan with exact files, boundaries, risks, and validation criteria. Stop for approval.
- Prefer existing DTO, controller, service, and Angular patterns. Verify endpoint paths and response shapes from source; do not trust stale documentation.
- Keep changes scoped to the active phase. Record dependencies and handoffs in `plan.md`.
- Backend authorization is authoritative. Angular guards improve navigation but are not security.
- Never treat a client-provided payment flag as proof of payment.
- Do not fabricate purchase history or winner data in the frontend.
- Run the narrowest relevant build/test after implementation and report pre-existing failures separately.
