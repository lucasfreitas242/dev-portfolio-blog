# ADR 0010: Draft Posts Management and Authoring Feed

* **Status:** Accepted
* **Date:** 2026-10-10
* **Deciders:** Lucas Freitas

## Context
In [ADR-0007](0007-post-creation-and-translation-authoring-flow.md), newly authored posts default to `PostStatus.Draft` unless explicitly published with `publishNow: true`.
However, the public feed (`GET /api/v1/posts`) and post lookup (`GET /api/v1/posts/{slug}`) deliberately filter for `Status == PostStatus.Published`.

This created a functional blindspot in the authoring lifecycle:
1. Authors had no API mechanism to inspect or retrieve their saved drafts.
2. Content previews (reviewing Markdown formatting before public release) were impossible without direct database access.
3. Managing draft pipelines (filtering drafts by topic category or language) was unsupported.

## Decision
We implement a dedicated **Draft Posts Management and Authoring Feed** slice:

### 1. Dedicated Endpoints
* **`GET /api/v1/posts/drafts`**:
  - Lists draft posts with pagination (`page`, `pageSize`), sorted by `UpdatedAt` descending (most recently edited first).
  - Supports optional query filters: `category` (slug) and `language`.
  - Returns `PagedResult<DraftSummaryDto>`.
* **`GET /api/v1/posts/drafts/{id}`**:
  - Retrieves complete draft details including full Markdown content (`contentMd`), author metadata, category, and tags.
  - Returns `200 OK` (`DraftDetailDto`) if found and in `Draft` status; otherwise returns `404 Not Found`.

### 2. Route Routing Disambiguation
In ASP.NET Core Minimal APIs, literal route segments (`/drafts` and `/drafts/{id}`) take priority over catch-all parameter routes (`/{slug}`). This guarantees that calls to `/api/v1/posts/drafts` are dispatched directly to the draft handlers without conflicting with a post whose slug might resemble `"drafts"`.

### 3. Separation from Public Feeds
Draft feeds are isolated from public readers. Public consumers continue querying `/api/v1/posts`, which strictly preserves `Status == PostStatus.Published`.

## Consequences

### Positive
* **Full Content Lifecycle**: Completes the draft -> edit -> preview -> publish workflow.
* **Author Feedback**: Authoring frontends and CLI tools can render draft lists and real-time Markdown preview panes before triggering publication.
* **Security Preparation**: Dedicated draft endpoints provide a clean demarcation boundary for applying future authentication/authorization policies (e.g. admin JWT or API key) without impacting public caching or unauthenticated public readers.

### Negative / Trade-offs
* Introduces additional query methods in `IPostRepository`.
  - *Mitigation*: Implementation leverages existing PostgreSQL indexed fields (`Status`, `Category`, `Language`, `UpdatedAt`).
