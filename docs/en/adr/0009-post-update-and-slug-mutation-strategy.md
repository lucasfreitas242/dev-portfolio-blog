# ADR 0009: Post Update and Slug Mutation Strategy

* **Status:** Accepted
* **Date:** 2026-10-10
* **Deciders:** Lucas Freitas

## Context
Following [ADR-0007](0007-post-creation-and-translation-authoring-flow.md) (Post Creation) and [ADR-0008](0008-category-management-and-authoring-flow.md) (Category Management), post content naturally evolves. Authors need to fix typos, refine technical explanations, update code snippets, reassign categories, and adjust tags.

However, mutating post content presents specific architectural questions:
1. **Slug Stability vs Mutation**: Indiscriminately regenerating slugs on title edits breaks existing inbound URLs, inbound links, and SEO rankings.
2. **Category Ergonomics**: Edits should not require resupplying category metadata if the category remains unchanged, but should permit updates using either `categoryId` (UUID) or `categorySlug`.
3. **Reading Time Recalculation**: Content changes alter reading length and must update `ReadingTimeMinutes` and `UpdatedAt` timestamps atomically.
4. **Tag Taxonomy Updates**: Tags associated with the post must synchronize with new inputs via upserting without orphan data.

## Decision
We implement the **Post Update Endpoint** via `PUT /api/v1/posts/{id}`:

### 1. Endpoint and Payload Contract
* **Route**: `PUT /api/v1/posts/{id}`
* **Payload**: `UpdatePostCommand`:
  - `title` (`string`, required)
  - `summary` (`string`, required)
  - `contentMd` (`string`, required)
  - `categoryId` (`Guid?`, optional)
  - `categorySlug` (`string?`, optional)
  - `slug` (`string?`, optional)
  - `coverImage` (`string?`, optional)
  - `tags` (`IReadOnlyList<string>?`, optional)
* **Response**: `200 OK` with `PostUpdatedDto` containing full updated post metadata, reading time, and timestamps.

### 2. Slug Preservation Strategy
* By default, if `slug` is null or empty, the post preserves its existing `Slug`, ensuring URLs remain stable across content edits.
* If a new `slug` is explicitly provided, it is sanitized via `SlugHelper.Generate`. If changed from the current value, the system validates slug uniqueness excluding the current post (`ExistsBySlugAsync(slug, excludePostId: post.Id)`). Collisions return `409 Conflict`.

### 3. Category Fallback Resolution
* If `categoryId` or `categorySlug` is provided, the category is verified and updated.
* If neither is provided, the existing `post.CategoryId` is preserved.

### 4. Reading Time & Timestamp Atomicity
* The domain model recalculates `ReadingTimeMinutes` based on updated `ContentMd` (200 words/minute formula) and updates `UpdatedAt = DateTime.UtcNow`.

## Consequences

### Positive
* **SEO & URL Stability**: Slugs do not break accidentally during minor content revisions.
* **Author Ergonomics**: Authors can edit content without having to re-fetch or re-supply categories and existing slugs.
* **Consistency**: Shared tag upserting and ProblemDetails error handling maintain uniform API ergonomics.

### Negative / Trade-offs
* Tag updating replaces the post's current tag associations with the provided list.
  - *Mitigation*: Supplying `null` leaves existing tags untouched; supplying an empty array `[]` explicitly clears tags.
