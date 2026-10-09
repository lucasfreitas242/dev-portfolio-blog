# ADR 0007: Post Creation and Translation Authoring Flow

* **Status:** Accepted
* **Date:** 2026-10-09
* **Deciders:** Lucas Freitas

## Context
Following the acceptance of [ADR-0004](0004-multilingual-posts-and-slug-strategy.md) (Multilingual Strategy) and [ADR-0005](0005-error-handling-and-problem-details.md) (Error Handling & ProblemDetails), we need a formalized specification for the post creation lifecycle and authoring flow.

Key challenges in post authoring include:
1. **Slug ergonomics vs SEO control**: Generating human-readable, SEO-friendly localized slugs without causing database collision crashes.
2. **Translation linking**: Associating translated posts reliably while preventing logical anomalies (such as two posts in the same language within the same translation group).
3. **Taxonomy & Tags**: Managing tag association without forcing authors to pre-register tags in a disconnected workflow.
4. **Publishing lifecycle**: Balancing drafts vs immediate publishing.

## Decision
We adopt a unified **Post Creation and Translation Authoring Flow** via `POST /api/v1/posts`:

1. **Endpoint & Contract**:
   - Route: `POST /api/v1/posts`.
   - Response: `201 Created` with `Location: /api/v1/posts/{slug}` and a lightweight `PostCreatedDto` payload containing `id`, `slug`, `language`, `translationGroupId`, `status`, and `createdAt`.

2. **Slug Generation & Collision Handling**:
   - If `slug` is omitted or empty, the system automatically generates a kebab-case slug from the `title` (stripping diacritics and special characters).
   - If the client supplies an explicit `slug`, that value is validated and used.
   - If the resulting slug already exists in the database, the API throws `ConflictException("A post with slug '{slug}' already exists.")`, returning `409 Conflict`. We reject automatic hash suffixes to protect SEO integrity.

3. **Translation Grouping & Invariant Validation**:
   - **Root Post**: When `translationGroupId` is omitted, the system generates a new `Guid.NewGuid()`, designating the post as the root of a new translation group.
   - **Translated Post**: When `translationGroupId` is provided:
     1. The system verifies that the target `translationGroupId` exists.
     2. The system enforces **Language Uniqueness within Group**: No two posts sharing the same `TranslationGroupId` can have the same `Language`. If a post in that language already exists for the group, the API throws `ConflictException`, returning `409 Conflict`.

4. **Tag Taxonomy via Upsert**:
   - The payload accepts a list of tag names/slugs: `tags: ["clean-architecture", ".NET"]`.
   - The system matches existing tags by slug or creates new `Tag` entities on the fly, associating them with the post within the same transaction.

5. **Publication Lifecycle**:
   - Posts are created in `PostStatus.Draft` by default.
   - An optional boolean `publishNow: true` in the payload allows creating and publishing atomically, setting `PublishedAt = DateTime.UtcNow`.
   - A dedicated `POST /api/v1/posts/{id}/publish` endpoint is provided for transitioning drafts later.

6. **Category Validation**:
   - `categoryId` is mandatory and must refer to an existing `Category`. If not found, the API throws `NotFoundException("Category", categoryId)`, returning `404 Not Found`.

## Consequences

### Positive
- **Author Ergonomics**: Authors can quickly write posts with auto-slugs and inline tag creation, or explicitly customize slugs when desired.
- **Data Integrity**: Enforces strict multilingual consistency—no orphan translations or duplicate-language collisions within a translation group.
- **REST & SEO Best Practices**: Emits standard `201 Created` with canonical `Location` header; prevents ugly slug collisions with deterministic `409 Conflict` feedback.

### Negative / Trade-offs
- Inline tag upserting and translation group validation require multiple database lookups within the command handler.
  - *Mitigation*: Lookups are lightweight indexed queries executed within a single database transaction.
