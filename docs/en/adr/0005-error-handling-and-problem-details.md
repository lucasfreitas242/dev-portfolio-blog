# ADR 0005: Error Handling and ProblemDetails Strategy

* **Status:** Accepted
* **Date:** 2026-10-09
* **Deciders:** Lucas Freitas

## Context
APIs require predictable, standardized error responses for client applications (e.g., Next.js, Astro, mobile clients). 
Without a centralized strategy, error representations become inconsistent (e.g., bare strings, empty bodies, ad-hoc JSON objects), and status code handling leaks into route endpoints.

Furthermore, RFC 7807 (updated by RFC 9457) establishes an industry-standard media type (`application/problem+json`) for HTTP error reporting.

## Decision
We adopt **Typed Exceptions with ASP.NET Core 8 `IExceptionHandler` and RFC 7807 `ProblemDetails`**:

1. **Domain & Application Exceptions**:
   - Business and domain rules throw specific typed exceptions:
     - `NotFoundException`: Resource does not exist (e.g., post slug, category).
     - `ConflictException`: Conflict with existing state (e.g., duplicate slug).
     - `ValidationException`: Input fails semantic or domain constraints.
     - `DomainRuleViolationException`: Invariant rules violated within domain entities.

2. **Global Exception Handler (`IExceptionHandler`)**:
   - Implement `GlobalExceptionHandler : IExceptionHandler` in `Blog.Api`.
   - Intercepts uncaught exceptions and maps them directly to `ProblemDetails`:
     - `NotFoundException` -> `404 Not Found`
     - `ValidationException` -> `400 Bad Request` (with `errors` dictionary)
     - `ConflictException` -> `409 Conflict`
     - `DomainRuleViolationException` -> `422 Unprocessable Content`
     - Unhandled exceptions -> `500 Internal Server Error` (logged with stack trace; sanitized response in production)

3. **Minimal API Simplicity**:
   - Route handlers only write the happy path (`Results.Ok(result)`). 
   - No conditional status-checking boilerplate (`if null then 404`) in route definitions.

## Consequences

### Positive
- **Simplicity**: Keeps Minimal API routes clean, declarative, and focused solely on the happy path.
- **Standards-Compliant**: All error responses return standard `application/problem+json` with `type`, `title`, `status`, `detail`, `instance`, and `traceId`.
- **Security**: Internal stack traces and sensitive database errors are shielded from public API consumers.
- **Observability**: Centralized logging of failures with correlation trace IDs.

### Negative / Trade-offs
- Uses exceptions for expected flow branches (e.g., resource not found). In a blog engine, the performance impact is negligible compared to database I/O.
