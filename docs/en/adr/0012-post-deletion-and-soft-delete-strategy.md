# ADR 0012: Post Deletion and Soft Delete Strategy

* **Status:** Accepted
* **Date:** 2026-10-10
* **Deciders:** Lucas Freitas

## Context
Following [ADR-0007](0007-post-creation-and-translation-authoring-flow.md), [ADR-0009](0009-post-update-and-slug-mutation-strategy.md), [ADR-0010](0010-draft-posts-management-and-authoring-feed.md), and [ADR-0011](0011-single-admin-jwt-authentication.md), the system supports creating, updating, previewing drafts, and publishing posts under single-admin JWT protection.

However, content management requires discarding obsolete posts, mistaken drafts, or retracted publications. 

Architecturally, we must decide between:
1. **Hard Delete (`DELETE FROM posts`)**: Destroys records permanently. Accidental clicks cannot be recovered without database backups. Foreign keys or references can lead to cascade anomalies.
2. **Soft Delete (`PostStatus.Archived`)**: Preserves records in the database with an `Archived` lifecycle state. Prevents data loss, maintains translation group historical integrity, and allows future restoration if desired.

## Decision
We adopt the **Soft Delete Lifecycle Strategy** via `DELETE /api/v1/posts/{id}`:

### 1. Endpoint and Authorization
* **Route**: `DELETE /api/v1/posts/{id}`
* **Security**: Protected with `.RequireAuthorization()` (Bearer JWT).
* **Response**: `204 No Content` on successful archiving.
* **Error**: Returns `404 Not Found` if the post does not exist or has already been marked as `Archived` (idempotent resource safety).

### 2. Domain State Transition
* The `Post` entity provides an `Archive()` method:
  - Sets `Status = PostStatus.Archived`.
  - Updates `UpdatedAt = DateTime.UtcNow`.
* Archived posts cannot be updated (`UpdatePostHandler`) or published (`PublishPostHandler`), returning `404 Not Found`.

### 3. Query Segregation
* **Public Queries (`GET /api/v1/posts`, `GET /api/v1/posts/{slug}`)**: Strictly filter `Status == PostStatus.Published`.
* **Draft Queries (`GET /api/v1/posts/drafts`, `GET /api/v1/posts/drafts/{id}`)**: Strictly filter `Status == PostStatus.Draft`.
* Soft-deleted (`Archived`) posts are immediately and completely hidden from public readers and draft feeds.

## Consequences

### Positive
* **Zero Data Loss**: Mistaken deletions do not wipe markdown content, metadata, or authorship history from the database.
* **REST Semantic Standard**: Exposes canonical `DELETE /api/v1/posts/{id}` returning `204 No Content` to HTTP clients and frontends.
* **Clean Feeds**: Archived posts immediately vanish from both public readers and the authoring draft feed.
* **Protected Operations**: Requires valid Admin JWT credentials, preventing unauthorized deletion.

### Negative / Trade-offs
* Archived records consume storage in PostgreSQL over time.
  - *Mitigation*: Negligible for a personal blog; a maintenance script or administrative purge command can be added in the future if permanent hard pruning is needed.
