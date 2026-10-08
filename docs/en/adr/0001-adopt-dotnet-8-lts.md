# 001. Adopt .NET 8 LTS as Application Runtime

* **Status:** Accepted
* **Date:** 2026-10-08
* **Deciders:** Lucas Freitas

## Context
The project requires a dependable foundation for Minimal APIs, Entity Framework Core, and Linux containerization. STS releases or preview SDKs present frequent breaking changes and unnecessary maintenance overhead.

## Decision
We standardized the solution on **.NET 8 LTS (Long Term Support)** across all solution projects (`Blog.Domain`, `Blog.Application`, `Blog.Api`).

## Consequences
* Extended official support and long-term security fixes.
* Stable, lightweight Alpine Docker images.
* Robust third-party library ecosystem (`Npgsql`, JWT libraries).