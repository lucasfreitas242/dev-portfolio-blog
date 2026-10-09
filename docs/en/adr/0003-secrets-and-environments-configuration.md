# ADR 0003: Environment Variables and Secrets Management

* **Status:** Accepted
* **Date:** 2026-10-09
* **Deciders:** Lucas Freitas

## Context
The blog API requires access to sensitive credentials and configuration values, such as PostgreSQL connection strings, JWT signing keys, and initial seed administrative credentials. Storing sensitive data in plain text within source-controlled files introduces critical security vulnerabilities and risks accidental exposure in public repositories. 

We need a standardized strategy that protects secrets in source control while remaining straightforward across local development, containerized setups (Docker), and automated CI/CD pipelines.

## Decision
We decided to adopt a layered configuration model using **.NET Secret Manager (User Secrets)** for local development and standard **Environment Variables** for containerized environments and production:

1. **Local Development**:
   - Sensitive values are stored outside the project tree using `dotnet user-secrets` scoped to `Blog.Api`.
   - The repository files (`appsettings.json`, `appsettings.Development.json`) contain only non-sensitive defaults, structural keys, and placeholder values.

2. **Containerized Environments & Production**:
   - Configurations are injected via standard hierarchical environment variables supported natively by the .NET configuration provider (e.g., `ConnectionStrings__DefaultConnection`, `Jwt__SecretKey`).
   - A `.env.example` file is maintained in the repository root to document required variable keys, while actual `.env` files remain strictly ignored by `.gitignore`.

3. **Fallback & Ingestion**:
   - The ASP.NET Core hosting pipeline loads settings through `builder.Configuration`, preserving standard configuration precedence: base configuration files, environment-specific JSON files, user secrets (in Development mode), and environment variables.

## Consequences

### Positive
- **Security**: Prevents inadvertent leaks of sensitive tokens, passwords, and connection strings into Git repositories.
- **Native Integration**: Leverages ASP.NET Core's built-in `IConfiguration` provider without requiring third-party library dependencies.
- **Environment Parity**: Aligns with Twelve-Factor App configuration principles for container deployments.

### Negative / Trade-offs
- Developers onboarding to the project must manually initialize local secrets before running the application.
  - *Mitigation*: Provide clear onboarding documentation and setup commands in `README.md` (`dotnet user-secrets set ...`).