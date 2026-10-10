# ADR 0011: Single-Admin JWT Authentication Strategy

* **Status:** Accepted
* **Date:** 2026-10-10
* **Deciders:** Lucas Freitas

## Context
Following [ADR-0007](0007-post-creation-and-translation-authoring-flow.md), [ADR-0008](0008-category-management-and-authoring-flow.md), [ADR-0009](0009-post-update-and-slug-mutation-strategy.md), and [ADR-0010](0010-draft-posts-management-and-authoring-feed.md), the blog engine provides full lifecycle operations for posts, categories, drafts, and updates.

However, write and draft endpoints were unauthenticated. Because this is a personal developer portfolio and blog with a dedicated administrative author, only the owner should be authorized to create, update, publish, or view draft content, while the general public retains unrestricted read-only access to published articles and categories.

Commercial multi-tenant solutions (OAuth2 external providers, refresh token rotation databases) introduce accidental complexity for a single-admin architecture. Conversely, omitting authentication entirely exposes content to unauthorized mutation.

## Decision
We implement a **Single-Admin JWT Authentication Architecture** using ASP.NET Core `JwtBearer`:

### 1. Authentication Endpoint & Credential Verification
* **Endpoint**: `POST /api/v1/auth/login`
* **Request**: `LoginCommand(string Email, string Password)`
* **Verification**: Checks the registered administrator record in the PostgreSQL `Users` table via a constant-time PBKDF2 `IPasswordHasher`.
* **Response**: `200 OK` with `LoginResultDto(Token, ExpiresAt, Email, Name)`.
* **Errors**: Returns `401 Unauthorized` (`ProblemDetails`) on invalid email or password without revealing which component failed.

### 2. Token Specification & Lifetime
* **Algorithm**: HMAC-SHA256 (`HmacSha256`) signed with a 256-bit secret key configured in `JwtSettings:SecretKey`.
* **Claims**: Contains `sub` (User UUID), `email`, `name`, `role: Admin`, and `jti` (unique token ID).
* **Lifetime**: Configurable (default 7 days / 168 hours) to provide smooth authoring sessions across frontend dashboards without session interruptions.

### 3. Route Authorization Segmentation
* **Protected Endpoints (`.RequireAuthorization()`)**:
  - `POST /api/v1/posts` (Create Post)
  - `PUT /api/v1/posts/{id}` (Update Post)
  - `POST /api/v1/posts/{id}/publish` (Publish Draft)
  - `GET /api/v1/posts/drafts` (List Drafts)
  - `GET /api/v1/posts/drafts/{id}` (Inspect Draft)
  - `POST /api/v1/categories` (Create Category)
* **Public Endpoints (Anonymous Access)**:
  - `GET /api/v1/posts` (List Published Posts)
  - `GET /api/v1/posts/{slug}` (Get Post by Slug)
  - `GET /api/v1/categories` (List Categories)
  - `GET /api/v1/categories/{slug}` (Get Category Details)
  - `POST /api/v1/auth/login` (Admin Login)

### 4. Interactive OpenAPI / Swagger Integration
Swagger UI is configured with the `Bearer` HTTP Security Scheme, enabling interactive token injection directly within development environments.

## Consequences

### Positive
* **Zero Accidental Complexity**: No external OAuth providers or complex token exchange tables needed.
* **Frontend-Ready**: The frontend admin dashboard connects to `POST /api/v1/auth/login`, stores the token, and attaches it as `Authorization: Bearer <token>`.
* **Strict Demarcation**: Public read access remains cache-friendly and anonymous, while all mutating operations require valid credentials.
* **Security Hygiene**: Passwords are saved with salted PBKDF2 hashing (100,000 iterations); tokens expire automatically.

### Negative / Trade-offs
* Token invalidation before expiry requires secret rotation or expiration elapsed time.
  - *Mitigation*: Acceptable for a single-admin personal blog. In case of credential compromise, rotating `JwtSettings:SecretKey` immediately invalidates all issued tokens.
