# ADR 0006: Adopt Bruno as Version-Controlled API Client and Contract Testing Tool

* **Status:** Accepted
* **Date:** 2026-10-09
* **Deciders:** Lucas Freitas

## Context
As the API expands with new query and mutation endpoints, we need a reliable, collaborative way to document, manually verify, and automate HTTP requests against our endpoints.

Traditional API clients like Postman or Insomnia increasingly require proprietary cloud synchronization, store collections in complex JSON blobs with noisy Git diffs, or gate team collaboration behind paywalls.

We need an API testing and documentation tool that:
1. Stores requests as clean, human-readable plain-text files directly in our Git repository.
2. Integrates seamlessly into code reviews (Pull Requests).
3. Supports automated assertions and CLI execution (`bru run`) in CI/CD pipelines.

## Decision
We adopt **Bruno** as the official HTTP client and API contract testing tool, located at `docs/specs/bruno/`:

1. **Repository-First Storage**:
   - Every request is stored as a `.bru` file under `docs/specs/bruno/`.
   - Environment configurations reside in `docs/specs/bruno/environments/` (e.g., `local.bru`). Sensitive secrets are excluded from version control.

2. **Definition of Done (DoD)**:
   - Any new or modified endpoint **must** include an updated or newly created `.bru` file.
   - Each request must include automated assertions in its `tests { ... }` block verifying at minimum:
     - Expected HTTP status code (200, 201, 400, 404, etc.).
     - Critical payload properties and contract invariants.

3. **CI/CD Integration Pathway**:
   - Automated testing pipelines can execute `npx @usebruno/cli run docs/specs/bruno --env local` against running test instances as automated smoke and contract validation.

## Consequences

### Positive
- **Git-Native**: Clean diffs in pull requests make API changes visible alongside the C# code implementing them.
- **Zero Cloud Lock-in**: Full offline capability with no reliance on external cloud workspaces or accounts.
- **Fast Developer Onboarding**: Cloning the repository immediately provides a fully configured test collection without importing third-party workspaces.
- **Living Executable Documentation**: Guarantees that documentation does not drift from implementation.

### Negative / Trade-offs
- Developers must have the Bruno app or `@usebruno/cli` installed locally.
- Contributors must remember to maintain and update `.bru` files as part of their feature workflow.
