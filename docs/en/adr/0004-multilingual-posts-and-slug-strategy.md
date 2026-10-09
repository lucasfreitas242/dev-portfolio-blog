# ADR 0004: Multilingual Posts and Slug Strategy (i18n)

* **Status:** Accepted
* **Date:** 2026-10-09
* **Deciders:** Lucas Freitas

## Context
The blog platform must support articles published in multiple languages (initially `pt-BR` and `en-US`).
We need to define how articles across different languages are identified, how they are queried through the API, and how we provide the frontend with the necessary metadata for international SEO (specifically `<link rel="alternate" hreflang="..." />` tags and an in-page language switcher).

Two architectural approaches were evaluated:
1. **Hierarchical Route Prefixing**: `/api/v1/posts/{language}/{slug}` using an identical slug across all locales.
2. **Localized Slugs with Translation Grouping**: `/api/v1/posts/{slug}` using localized slugs in their native language (e.g., `building-...` vs. `construindo-...`), linked via a shared `TranslationGroupId`.

## Decision
We decided to adopt **Approach 2: Localized Slugs with `TranslationGroupId` and Direct Query Route `GET /api/v1/posts/{slug}`**:

1. **`Post` Domain Entity**:
   - Includes a `string Language` property (IETF tag format, e.g., `pt-BR`, `en-US`).
   - Includes a `Guid TranslationGroupId` property. Posts that are mutual translations share the same group identifier.
   - The `Slug` remains globally unique at the database level (`HasIndex(p => p.Slug).IsUnique()`), as localized titles naturally produce distinct slugs per language.

2. **API Contract**:
   - The public query route remains clean and RESTful: `GET /api/v1/posts/{slug}`.
   - The `PostDetailDto` response payload includes:
     - `string Language`: Locale of the retrieved post.
     - `IReadOnlyList<PostTranslationDto> AvailableTranslations`: A list of sibling translations containing `{ Language, Slug }` linked to the same `TranslationGroupId`.
   - The list query route `GET /api/v1/posts` supports optional language filtering via `?language=...`.

3. **Frontend & SEO Integration**:
   - The frontend layer (Next.js, Astro, or Angular) manages UI route prefixes (e.g., `/pt-BR/blog/...` or `/en-US/blog/...`).
   - Utilizing `AvailableTranslations`, the client automatically injects `<link rel="alternate" hreflang="..." href="..." />` tags and populates language dropdowns without relying on client-side slug inference.

## Consequences

### Positive
- **Search Engine Optimization (SEO)**: Clean, localized URLs incorporating native keywords significantly improve regional indexation (e.g., `/construindo-clean-architecture` ranks higher in Portuguese search queries than `/pt-br/building-clean-architecture`).
- **Decoupling**: The REST API does not maintain session state or URL language prefixes; lookups rely directly on the resource's canonical public key (`slug`).
- **Single-Roundtrip Resolution**: The frontend retrieves the localized article content and alternate language links in a single HTTP request.

### Negative / Trade-offs
- Creating or editing posts requires associating the proper `TranslationGroupId` when linking translated versions.
  - *Mitigation*: In the `Post.Create` factory method, omitting `translationGroupId` automatically assigns a new `Guid.NewGuid()`, defaulting the post as the translation root.
- Because slugs are globally unique, titles with identical words across languages require deliberate slug differentiation.
