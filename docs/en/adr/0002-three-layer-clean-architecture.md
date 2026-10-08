# 002. Adopt Pragmatic 3-Layer Clean Architecture

* **Status:** Accepted
* **Date:** 2026-10-08
* **Deciders:** Lucas Freitas

## Context
Need for clean separation between business logic, persistence, and HTTP transport without the boilerplate of over-engineered layered architectures.

## Decision
We organized the solution into 3 focused projects:
1. **`Blog.Domain`:** Pure business entities, invariants, enums, and repository abstractions.
2. **`Blog.Application`:** Vertical use case slices (Queries, Commands, Handlers).
3. **`Blog.Api`:** Minimal API endpoints, EF Core database configuration, and host wiring.

## Consequences
* Highly testable core business rules.
* Reduced friction without redundant intermediate projects.
* Persistence setup is kept in `Blog.Api` to maintain a streamlined layout.