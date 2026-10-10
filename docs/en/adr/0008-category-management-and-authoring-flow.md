# ADR 0008: Category Management and Authoring Flow

* **Status:** Accepted
* **Date:** 2026-10-10
* **Deciders:** Lucas Freitas

## Context
Following the acceptance of [ADR-0004](0004-multilingual-posts-and-slug-strategy.md) and [ADR-0007](0007-post-creation-and-translation-authoring-flow.md), creating a post requires associating it with an existing category (`categoryId`).

However, without dedicated category discovery and management endpoints:
1. **Opaque taxonomy**: Authors and client frontends could not discover available categories without directly inspecting the database.
2. **Authoring friction**: Creating a post demanded obtaining internal category UUIDs ahead of time.
3. **Taxonomy fragmentation risk**: Unlike tags—which are high-cardinality, fluid, and safe to upsert inline—categories represent the core structural taxonomy, primary navigation, and route hierarchy of the blog. Allowing uncurated, on-the-fly category generation during post creation risks duplicate or misspelled categories (e.g., `dotnet`, `.NET`, `c-sharp`).

## Decision
We adopt **Approach A (Curated Taxonomy with Discovery and Dual Identifier Ergonomics)**:

### 1. Dedicated Category Vertical Slices & Endpoints
We expose dedicated endpoints under `/api/v1/categories`:
* **`GET /api/v1/categories`**: Lists all categories including their metadata (`id`, `name`, `slug`, `isFeatured`) and the count of published posts (`postCount`).
* **`GET /api/v1/categories/{slug}`**: Retrieves detailed information for a single category by its slug. Returns `404 Not Found` if not found.
* **`POST /api/v1/categories`**: Creates a category given `{ "name": "...", "slug": "...", "isFeatured": false }`.
  - If `slug` is omitted, it is generated automatically from `name` via `SlugHelper.Generate`.
  - Enforces slug uniqueness; collisions return `409 Conflict`.
  - Returns `201 Created` with a `Location: /api/v1/categories/{slug}` header.

### 2. Dual Category Resolution in Post Creation
`CreatePostCommand` accepts either:
* `categoryId` (`Guid?`): Direct entity lookup by UUID.
* `categorySlug` (`string?`): Direct entity lookup by localized kebab-case slug.

If neither is supplied, the handler returns `400 Bad Request` (`ValidationException`). If the specified category cannot be found, it returns `404 Not Found` (`NotFoundException`).

### 3. Repository and Schema Isolation
* A dedicated `ICategoryRepository` in `Blog.Domain.Interfaces` is introduced and implemented in `Blog.Api.Data.Repositories.CategoryRepository`.
* A unique index `IX_Categories_Slug` and column length constraints are applied to `Categories.Slug` and `Categories.Name` via `CategoryConfiguration`.

## Consequences

### Positive
* **Ergonomics & Flexibility**: Authors and scripts can create posts using readable category slugs (e.g. `"categorySlug": "architecture"`) without looking up database UUIDs.
* **Taxonomy Integrity**: Prevents typo-driven category pollution while keeping category curation intentional.
* **Frontend-Ready**: Client apps can consume `GET /api/v1/categories` to build header menus, category filters, and dropdown selectors with accurate post counts.
* **Clean Architecture**: Preserves Single Responsibility Principle by decoupling category operations from `IPostRepository`.

### Negative / Trade-offs
* Requires authors to create non-existing categories via `POST /api/v1/categories` before writing a post for that new category.
  - *Mitigation*: The `POST /api/v1/categories` endpoint is lightweight and easily integrated into post authoring workflows or administrative UI modals.
