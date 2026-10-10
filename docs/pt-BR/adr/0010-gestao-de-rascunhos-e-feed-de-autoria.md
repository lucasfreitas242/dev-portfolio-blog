# ADR 0010: Gestão de Rascunhos e Feed de Autoria

* **Estado:** Aceite
* **Data:** 2026-10-10
* **Decisores:** Lucas Freitas

## Contexto
No [ADR-0007](0007-fluxo-de-criacao-de-posts-e-autoria-de-traducoes.md), novos artigos são criados por defeito com o estado `PostStatus.Draft`, a menos que explicitamente marcados com `publishNow: true`.
Contudo, o feed público (`GET /api/v1/posts`) e a consulta individual (`GET /api/v1/posts/{slug}`) filtram rigorosamente por `Status == PostStatus.Published`.

Isto gerava um ponto cego no ciclo de vida de autoria:
1. Autores não tinham endpoint na API para listar os seus rascunhos em aberto.
2. Era impossível pré-visualizar a renderização de Markdown antes da publicação sem aceder diretamente à base de dados.
3. Não havia filtragem de rascunhos por categoria ou idioma.

## Decisão
Implementamos a fatia vertical dedicada de **Gestão de Rascunhos e Feed de Autoria**:

### 1. Endpoints Dedicados
* **`GET /api/v1/posts/drafts`**:
  - Lista rascunhos paginados (`page`, `pageSize`), ordenados por `UpdatedAt` descendente (mais recentemente editados no topo).
  - Suporta filtros opcionais de query: `category` (slug) e `language`.
  - Retorna `PagedResult<DraftSummaryDto>`.
* **`GET /api/v1/posts/drafts/{id}`**:
  - Obtém os detalhes completos do rascunho com o Markdown integral (`contentMd`), autor, categoria e tags.
  - Retorna `200 OK` (`DraftDetailDto`) se encontrado e no estado `Draft`; caso contrário retorna `404 Not Found`.

### 2. Desambiguação de Rotas
Nas Minimal APIs do ASP.NET Core, segmentos literais (`/drafts` e `/drafts/{id}`) têm prioridade na árvore de rotas sobre rotas com parâmetros abrangentes (`/{slug}`). Isto assegura que chamadas a `/api/v1/posts/drafts` nunca colidem com a pesquisa de artigos por slug.

### 3. Isolamento em Relação ao Feed Público
Os rascunhos ficam completamente segregados das rotas públicas de leitura (`/api/v1/posts`), mantendo a integridade do feed público.

## Consequências

### Positivas
* **Ciclo de Vida Completo**: Fecha o ciclo criar rascunho -> editar -> pré-visualizar -> publicar.
* **Pré-visualização para Frontend**: Clientes e editores web podem listar rascunhos e exibir pré-visualizações fiéis de Markdown antes do lançamento público.
* **Preparação para Segurança**: Estabelece limites claros para aplicação futura de políticas de autorização de administrador (JWT ou API Key) sem afetar o cache ou os leitores públicos.

### Negativas / Compensações
* Introduz métodos adicionais de consulta em `IPostRepository`.
  - *Mitigação*: Consultas utilizam índices já existentes no PostgreSQL (`Status`, `Category`, `Language`, `UpdatedAt`).
