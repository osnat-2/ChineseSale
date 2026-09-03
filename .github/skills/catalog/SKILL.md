---
name: catalog
description: "Workflow for implementing the public Chinese Sale catalogue in Angular. Use for present/category services, routes, search, filters, details, images, and loading/empty/error states against verified read APIs."
---

# Catalog Skill

## Scope

Own public read-only presents and categories. Do not implement purchase, payment, admin writes, or lottery behavior.

## Required Analysis

- Read root `plan.md`, Contract Baseline handoff, and frontend component/style/API instructions.
- Inspect present/category models, services, routes, components, templates, styles, and tests.
- Verify backend read endpoint paths, query parameters, authorization, response shapes, active filtering, and errors.
- Identify responsive, accessibility, image, filtering, and URL-state requirements.

## Required Sub-Plan

Before edits, list exact Angular/backend contract surfaces, component/data flow, states, boundaries, unverified assumptions, and service/component validation criteria. Stop for approval.

## Acceptance Checks

- Requests match source-verified endpoints and DTOs.
- Loading, empty, error, detail, image, and filter states are covered.
- Catalogue remains read-only.
- Focused service and component tests pass.
