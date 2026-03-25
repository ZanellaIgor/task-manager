# CLAUDE.md

## Purpose

This file defines the engineering rules for agents and contributors working in this repository.
Use it as an execution guide, not as generic documentation.

Primary goals:

- strong typing
- predictable behavior
- clear architecture
- DRY
- clean code
- environment-driven configuration
- low regression risk

## Project Summary

This repository is a full stack task manager.

Backend stack:

- ASP.NET Core 8 Web API
- C# 12
- Entity Framework Core 8
- SQLite
- FluentValidation
- Mapster
- Swagger

Frontend stack:

- Vue 3
- TypeScript
- Vite
- Vue Router
- Pinia
- Axios
- TanStack Query
- VeeValidate
- Zod
- Tailwind CSS 4

Repository structure:

- `backend/TaskManager.Api`: app bootstrap, controllers, middleware, API configuration
- `backend/TaskManager.Application`: services, DTOs, validators, application contracts
- `backend/TaskManager.Domain`: entities, enums, domain exceptions, business rules
- `backend/TaskManager.Infrastructure`: persistence, EF Core, repositories, migrations
- `frontend/src/views`: screens
- `frontend/src/components`: UI and layout components
- `frontend/src/services`: API access layer
- `frontend/src/queries`: TanStack Query integration
- `frontend/src/stores`: Pinia stores
- `frontend/src/schemas`: validation schemas
- `frontend/src/types`: shared frontend types

## Agent Operating Rules

### 1. Work from the real codebase

- Do not assume architecture, config, or naming patterns without checking the repository.
- Reuse existing patterns before introducing a new one.
- Prefer consistency with current code over theoretical purity.

### 2. Keep changes scoped

- Make the smallest correct change that fully solves the problem.
- Do not refactor unrelated areas in the same change unless required for correctness.
- Separate cleanup commits from feature or behavior commits when possible.

### 3. Preserve repository hygiene

- Do not commit generated artifacts.
- Do not commit `bin/`, `obj/`, `.run/`, `dist/`, local SQLite runtime artifacts, or real `.env` files.
- Example config files are allowed, such as `.env.example` and `appsettings.example.json`.

## Type Safety Rules

- Never use `any`.
- Avoid `unknown`.
- Only use `unknown` at true external boundaries, then narrow immediately.
- Do not use unsafe casts just to satisfy the compiler.
- Prefer explicit DTOs, typed interfaces, and clear contracts.
- Preserve backend nullability and frontend strict typing.
- If data has a known shape, model it with a dedicated type.
- Prefer validation plus narrowing over permissive typing.

## Backend Rules

### Architecture

- Controllers must stay thin.
- Business rules belong in `Application` and `Domain`, not in controllers.
- Persistence belongs in `Infrastructure`.
- Validation belongs in FluentValidation validators when applicable.
- Exceptions should be meaningful and domain-specific.

### Implementation

- Prefer async operations for I/O.
- Do not hardcode URL, CORS, connection string, or environment-dependent behavior.
- Read runtime configuration from `appsettings` and environment variables.
- Keep API payloads stable and typed.
- Avoid leaking persistence concerns into domain or controller code.

### Backend change checklist

- Does the change belong to the right layer?
- Is validation explicit?
- Is configuration environment-driven?
- Are exceptions and error messages coherent?
- Does the change preserve existing endpoint contracts unless intentionally changed?

## Frontend Rules

### Architecture

- UI components should not own business logic that belongs in services, schemas, queries, or stores.
- HTTP calls must go through `services`.
- Form validation belongs in `schemas`.
- Shared contracts should live in `types` or schemas, not be duplicated ad hoc.

### Implementation

- Keep props, emits, and service responses strongly typed.
- Avoid spreading API shape assumptions across multiple components.
- Prefer data normalization in one place.
- Maintain consistency between frontend types and backend contracts.
- Use `VITE_API_URL` for configurable API access.

### Frontend change checklist

- Are props and return values typed?
- Is form validation centralized?
- Is API access isolated in services?
- Is business logic kept out of presentation-only components?
- Is fallback behavior preserved when relevant?

## Clean Code Rules

- Prefer clear names over abbreviations.
- Prefer small focused functions over large procedural blocks.
- Each module should have one clear responsibility.
- Extract duplication when the same rule appears more than once.
- Comments should explain intent or constraints, not restate obvious code.
- If a block becomes difficult to explain, simplify it.

## Configuration Rules

- No hardcoded environment URLs.
- No hardcoded frontend origin in backend code.
- No hardcoded connection strings in code.
- Backend config must support `appsettings` and environment variables.
- Frontend config must support `.env` via `VITE_API_URL`.
- Documentation and example config must match actual runtime behavior.

## What to Avoid

- `any`
- unnecessary `unknown`
- weak or implicit contracts
- controller-heavy business logic
- duplicated validation logic
- duplicated request/response types
- environment hardcoding
- large mixed-purpose commits
- committing generated files

## Expected Delivery Standard

When making a change:

- implement the fix completely
- keep typing strict
- update docs if behavior or configuration changes
- keep the repository clean
- validate the relevant build or test path when feasible
- call out any remaining risk explicitly

## Preferred Decision Heuristics

- Prefer explicitness over magic.
- Prefer typed boundaries over dynamic objects.
- Prefer existing project conventions over introducing new patterns.
- Prefer maintainability over cleverness.
- Prefer one good source of truth over duplicated state.
