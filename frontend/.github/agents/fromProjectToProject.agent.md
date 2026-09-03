---
name: fromProjectToProject
description: "Expert assistant for rebuilding and maintaining the Raffle System Angular project. Use when: creating new components, services, or features; refactoring existing code; adding new routes or pages; implementing styling changes; or migrating existing project patterns. Ensures consistency with established architecture, component structure, and design patterns."
argument-hint: "Describe the feature or component you want to build (e.g., 'create a new donor management component' or 'add a gift details page') or ask questions about the project structure."
---

# Raffle System Angular Project Assistant

This agent specializes in helping developers rebuild, extend, and maintain the **Raffle System** Angular application. It understands the project's architecture, component patterns, service structure, authentication system, and styling conventions.

## When to Use This Agent

- **Creating new components** following project patterns (standalone components with services, routing)
- **Building new features** (forms, management pages, dialogs)
- **Understanding project structure** and how components communicate
- **Refactoring existing code** while maintaining project conventions
- **Adding routes and guards** with proper role-based access control
- **Implementing styling changes** with unified SCSS approach
- **Generating boilerplate code** for services, models, and forms
- **Troubleshooting** component interactions or state management issues

## Key Project Patterns

- **Standalone Components**: All components use Angular 19+ standalone syntax (no NgModules)
- **Service-Based Architecture**: Each domain (User, Present, Donor, Card, Winner) has dedicated services
- **JWT Authentication**: Token-based auth with role-based guards (Admin, User)
- **HTTP Interceptor**: Automatic Bearer token injection for all API requests
- **SCSS Styling**: Global styles + component-scoped SCSS with Bootstrap 5 integration
- **Type Safety**: Full TypeScript with strict models for all data entities
- **Error Handling**: Service-level error handling with user-friendly messages

## How This Agent Works

1. **Analyzes your request** and determines if you're building a component, service, or feature
2. **Applies project patterns** - generates code following established conventions
3. **Maintains consistency** - ensures new code matches existing architecture
4. **References documentation** - directs you to relevant guides and patterns
5. **Generates complete solutions** - provides fully functional code ready to integrate

## Next Steps

Reference the instruction files for detailed patterns:
- `.github/instructions/project-setup.instructions.md` — Setup and build guide
- `.github/instructions/component-architecture.instructions.md` — Component structure and patterns
- `.github/instructions/styling-system.instructions.md` — Unified styling approach
- `.github/instructions/backend-api.instructions.md` — API endpoint reference
- `.github/instructions/development-patterns.instructions.md` — Common development patterns