---
name: system-audit
description: "Perform a read-only, plan.md-led system-wide audit of the Chinese Sale ASP.NET Core and Angular application. Use for architecture reviews, security-gap assessment, performance bottleneck analysis, API/frontend consistency checks, and organized Blocker/High/Medium/Low reports."
---

# Chinese Sale System Audit

## Purpose and Boundaries

Assess the whole Chinese Sale system against the intended architecture and decisions recorded in the repository-root `plan.md`. Identify actionable architectural issues, security gaps, static performance risks, cross-layer inconsistencies, and system improvements. This is an audit only: produce evidence and recommendations, never implement fixes.

The audit may read/search source, configuration, documentation, and tests. It must not modify files or run commands, builds, tests, migrations, database updates, servers, or other potentially state-changing operations. Do not delegate audit areas. Never include secret values in the report; redact them and identify only the location and type.

## Workflow

1. **Establish intent from the plan.** Read the full root `plan.md` first, including non-negotiable rules, phase status and approvals, handoffs, residual risks, final review checklist, and any later approved phases. Treat it as the primary reference for expected architecture and scope.
2. **Read applicable guidance.** Read the shared Chinese Sale safety instructions and the relevant agent/skill handoffs where they define the audited contract or approved behavior.
3. **Map the system.** Identify the backend project/layers, Angular application surfaces, API boundary, configuration, and relevant test coverage before drawing conclusions.
4. **Inspect the backend.** Follow relevant request flows through ASP.NET Core startup/configuration, routing/controllers, authorization, business services, data access, EF entities/context/migrations, DTO mapping, validation, and error handling.
5. **Inspect Angular.** Follow corresponding flows through routes/guards, components/templates, services/models, HTTP interceptors, forms, state/loading/error behavior, environment/runtime configuration, and relevant tests.
6. **Trace integration and cross-cutting concerns.** Compare actual paths, methods, DTOs, wrappers, status/error shapes, identity/roles, ownership, payment or winner state, and serialization across both layers. Check whether backend enforcement is authoritative and whether the UI handles actual server outcomes.
7. **Assess static risks.** Look for architectural coupling, duplicated or conflicting sources of truth, unsafe configuration/secrets, missing authorization or validation, data-integrity hazards, unbounded or repeated work, inefficient query patterns, avoidable client requests/render work, weak observability/error handling, and gaps between planned requirements and current source. Do not claim measured runtime impact without measurements.
8. **Reconcile evidence.** Verify plan claims against source. Record whether a concern is (a) source-confirmed, (b) a plan/source discrepancy, or (c) a risk requiring runtime/load/security validation. Do not infer implementation from a status entry, comments, or stale docs.
9. **Report; do not fix.** Include only actionable, evidence-backed findings. Give exact file/line references and a concrete recommendation. State coverage and validation limits, including that no executable checks were run due to the read-only audit boundary.

## Severity Rubric

Use the project's established severity levels:

- **Blocker:** Release-critical failure, severe security exposure, or core data-integrity/availability issue that makes an essential flow unsafe or unusable.
- **High:** Serious vulnerability or correctness/reliability defect with significant impact, or a major architecture/API mismatch affecting important flows.
- **Medium:** Material but bounded risk, recurring performance concern, or cross-layer/operational inconsistency with a practical workaround.
- **Low:** Minor-impact defect or worthwhile maintainability, efficiency, or operational improvement.

Rank by severity, then confidence. Severity reflects demonstrated impact; uncertainty must be explicit and must not be used to inflate severity. Performance findings from static inspection are hypotheses unless supported by measurements.

## Coverage Checklist

Cover each area or state specifically why it was not applicable or could not be verified:

- `plan.md`: scope, phase/approval status, documented decisions, deferred work, residual risks, and final-review criteria.
- ASP.NET Core: startup and dependency configuration, endpoint design, authorization, validation, service/data-access separation, EF persistence, transaction/concurrency behavior, DTO mapping, error responses, and observability.
- Angular: routing and guards, service/API boundaries, models, interceptor behavior, component responsibility, user-visible state/error handling, environment configuration, and accessibility/reliability concerns where relevant.
- API and data boundary: endpoint/request/response/status consistency, claim/role interpretation, server-owned state, and error handling end to end.
- Security and data integrity: identity, authorization, input validation, secret/configuration handling, ownership, payment, admin operations, and lottery/winner behavior as applicable.
- Performance and scale: static query/request/render hotspots and bottleneck hypotheses; avoid unsupported performance claims.
- Validation evidence: existing relevant tests and documented builds; report missing coverage or recorded failures without executing validation.

## Report Shape

Start with `## System Audit`, summarize scope and limitations, then provide:

1. Overall assessment.
2. Coverage summary across plan, ASP.NET Core, Angular, integration, security, performance, and validation.
3. Findings grouped in this order: `### Blocker`, `### High`, `### Medium`, `### Low`. Omit empty groups.
4. Each finding: ID, category, concise issue, evidence with workspace-relative clickable file path and 1-based line reference, impact, actionable recommendation, confidence, and whether the evidence is source-confirmed, a plan/source discrepancy, or an unverified risk.
5. A short closing list of system-wide recommendations or verification work not represented as a confirmed defect.

If there are no actionable findings, say so clearly and list the principal areas reviewed and any verification limitations. Never present a recommendation as a confirmed vulnerability or defect without supporting evidence.
