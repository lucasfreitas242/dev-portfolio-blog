# ADR 0013: Política de CORS para Consumo do Frontend

* **Estado:** Aceite
* **Data:** 2026-10-15
* **Decisores:** Lucas Freitas

## Contexto
A API do Blog opera como um backend headless desacoplado, hospedada em um domínio dedicado (`api.lucasefreitas.dev`), consumida pelo frontend em Next.js tanto em ambiente de desenvolvimento local (`localhost:3000`) quanto em produção (`lucasefreitas.dev`).

Enquanto páginas públicas com Server Components executam chamadas server-to-server (imunes a regras de CORS de navegador), o painel administrativo (CMS) e chamadas autenticadas no browser executam requisições client-side com preflight `OPTIONS`, exigindo cabeçalhos CORS explícitos na API.

## Decisão
Configuramos uma política explícita de CORS (`FrontendPolicy`) no pipeline da aplicação ASP.NET Core:

1. **Origens Autorizadas:**
   - `http://localhost:3000` e `http://127.0.0.1:3000` (desenvolvimento local).
   - `https://lucasefreitas.dev` e `https://www.lucasefreitas.dev` (produção).

2. **Permissões:**
   - `AllowAnyHeader()`: Suporte irrestrito a `Authorization`, `Content-Type` e `Accept`.
   - `AllowAnyMethod()`: Suporte completo aos verbos REST (`GET`, `POST`, `PUT`, `DELETE`, `OPTIONS`).

3. **Ordem dos Middlewares:**
   - Injetado via `app.UseCors("FrontendPolicy")` logo após `UseHttpsRedirection()` e antes de `UseAuthentication()` e `UseAuthorization()`, garantindo que requisições preflight (`OPTIONS`) sejam respondidas antes dos filtros de segurança.

## Consequências

### Positivas
* **Compatibilidade Completa:** Permite chamadas diretas e seguras do frontend administrativo no navegador.
* **Segurança por Origem:** Bloqueia requisições cross-origin automáticas de origens não autorizadas.
* **Resiliência:** Compatível tanto com consumo direto quanto com arquiteturas de proxy BFF.

### Negativas / Compensações
* Novos domínios ou ambientes de preview (ex: Vercel Preview Deployments) necessitam de inclusão prévia na lista de origens.
