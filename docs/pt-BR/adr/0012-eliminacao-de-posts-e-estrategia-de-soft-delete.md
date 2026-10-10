# ADR 0012: Eliminação de Posts e Estratégia de Soft Delete

* **Estado:** Aceite
* **Data:** 2026-10-10
* **Decisores:** Lucas Freitas

## Contexto
Após o [ADR-0007](0007-fluxo-de-criacao-de-posts-e-autoria-de-traducoes.md), [ADR-0009](0009-atualizacao-de-posts-e-mutacao-de-slug.md), [ADR-0010](0010-gestao-de-rascunhos-e-feed-de-autoria.md) e [ADR-0011](0011-autenticacao-jwt-administrador-unico.md), o sistema permite criar, atualizar, pré-visualizar rascunhos e publicar posts sob proteção de JWT do administrador único.

No entanto, a gestão de conteúdos necessita de eliminar rascunhos incorretos, artigos obsoletos ou publicações retratadas.

Arquiteturalmente, comparamos duas opções:
1. **Hard Delete (`DELETE FROM posts`)**: Destrói registos permanentemente. Erros acidentais não podem ser recuperados sem backups. Relações e integridade de histórico de traduções podem sofrer anomalias.
2. **Soft Delete (`PostStatus.Archived`)**: Preserva registos na base de dados sob o estado `Archived`. Previne perda de dados, mantém o histórico dos grupos de tradução e viabiliza restauração futura.

## Decisão
Adotamos a **Estratégia de Soft Delete** via `DELETE /api/v1/posts/{id}`:

### 1. Endpoint e Autorização
* **Rota**: `DELETE /api/v1/posts/{id}`
* **Segurança**: Protegido por `.RequireAuthorization()` (Bearer JWT).
* **Resposta**: `204 No Content` após arquivamento bem-sucedido.
* **Erro**: Retorna `404 Not Found` se o post não existir ou se já se encontrar no estado `Archived`.

### 2. Transição de Estado no Domínio
* A entidade `Post` disponibiliza o método `Archive()`:
  - Define `Status = PostStatus.Archived`.
  - Atualiza `UpdatedAt = DateTime.UtcNow`.
* Posts arquivados não podem ser atualizados (`UpdatePostHandler`) nem publicados (`PublishPostHandler`), devolvendo `404 Not Found`.

### 3. Segregação de Consultas
* **Consultas Públicas (`GET /api/v1/posts`, `GET /api/v1/posts/{slug}`)**: Filtram estritamente por `Status == PostStatus.Published`.
* **Consultas de Rascunhos (`GET /api/v1/posts/drafts`, `GET /api/v1/posts/drafts/{id}`)**: Filtram estritamente por `Status == PostStatus.Draft`.
* Posts arquivados ficam imediata e completamente invisíveis quer para leitores públicos quer para o feed de rascunhos.

## Consequências

### Positivas
* **Prevenção de Perda de Dados**: Cliques acidentais não destroem texto em Markdown, metadados ou histórico de autoria.
* **Padrão Semântico REST**: Disponibiliza `DELETE /api/v1/posts/{id}` com resposta limpa `204 No Content`.
* **Feeds Limpos**: Posts arquivados desaparecem instantaneamente dos feeds sem quebrar chaves estrangeiras.
* **Operação Segura**: Exige credenciais administrativas válidas.

### Negativas / Compensações
* Registos arquivados continuam a ocupar espaço no PostgreSQL.
  - *Mitigação*: Desprezível num blog pessoal; uma rotina ou comando administrativo de purga física pode ser adicionado futuramente se for necessário.
