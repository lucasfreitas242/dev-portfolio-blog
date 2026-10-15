# ADR 0013: CORS Policy for Frontend Consumption

* **Status:** Accepted
* **Date:** 2026-10-15
* **Decision-Makers:** Lucas Freitas

## Context
The Blog API operates as a decoupled headless backend hosted under a dedicated domain (`api.lucasefreitas.dev`), consumed by the Next.js frontend in both local development (`localhost:3000`) and production (`lucasefreitas.dev`).

While public Server Component routes perform server-to-server HTTP requests (immune to browser CORS restrictions), client-side browser actions within the Admin CMS trigger preflight `OPTIONS` requests, requiring explicit Cross-Origin Resource Sharing (CORS) headers on the ASP.NET Core API.

## Decision
We configure a dedicated CORS policy (`FrontendPolicy`) in the ASP.NET Core pipeline:

1. **Authorized Origins:**
   - `http://localhost:3000` and `http://127.0.0.1:3000` (local development).
   - `https://lucasefreitas.dev` and `https://www.lucasefreitas.dev` (production domain).

2. **Permissions:**
   - `AllowAnyHeader()`: Enables transmission of `Authorization`, `Content-Type`, and `Accept` headers.
   - `AllowAnyMethod()`: Unrestricted support for all required HTTP verbs (`GET`, `POST`, `PUT`, `DELETE`, `OPTIONS`).

3. **Middleware Pipeline Placement:**
   - Registered via `app.UseCors("FrontendPolicy")` immediately following `UseHttpsRedirection()` and before `UseAuthentication()` and `UseAuthorization()`, ensuring preflight requests are responded to prior to auth challenges.

## Consequences

### Positive
* **Full Browser Compatibility:** Enables direct, secure client-side mutations from the Admin CMS.
* **Origin-Based Security:** Automatically blocks cross-origin browser requests originating from untrusted domains.
* **Architecture Flexibility:** Seamlessly supports both direct browser consumption and server-side BFF proxying.

### Negative / Trade-offs
* Additional frontend domains (such as ephemeral preview deployments) must be explicitly registered within the allowed origins list.
