# ADR 0009: Atualização de Posts e Mutação de Slugs

* **Estado:** Aceite
* **Date:** 2026-10-10
* **Decisores:** Lucas Freitas

## Contexto
Após o [ADR-0007](0007-fluxo-de-criacao-de-posts-e-autoria-de-traducoes.md) (Criação de Posts) e o [ADR-0008](0008-gerenciamento-de-categorias-e-fluxo-de-autoria.md) (Gestão de Categorias), o conteúdo dos artigos evolui continuamente. Autores precisam de corrigir gralhas, refinar explicações técnicas, atualizar exemplos de código, reatribuir categorias e ajustar tags.

Contudo, mutações de posts levantam desafios arquiteturais:
1. **Estabilidade de URLs vs Mutação de Slug**: Regenerar slugs automaticamente ao editar títulos quebra links externos e prejudica SEO.
2. **Ergonomia da Categoria**: Edições parciais não devem exigir reenvio obrigatório da categoria se ela não mudou, mas devem permitir alteração via `categoryId` ou `categorySlug`.
3. **Recálculo de Tempo de Leitura**: Alterações de conteúdo afetam o tempo de leitura e requerem atualização atómica de `ReadingTimeMinutes` e `UpdatedAt`.
4. **Sincronização de Tags**: A coleção de tags deve sincronizar de forma limpa via upsert.

## Decisão
Implementamos o endpoint de atualização via `PUT /api/v1/posts/{id}`:

### 1. Contrato e Rota
* **Rota**: `PUT /api/v1/posts/{id}`
* **Payload**: `UpdatePostCommand`:
  - `title` (`string`, obrigatório)
  - `summary` (`string`, obrigatório)
  - `contentMd` (`string`, obrigatório)
  - `categoryId` (`Guid?`, opcional)
  - `categorySlug` (`string?`, opcional)
  - `slug` (`string?`, opcional)
  - `coverImage` (`string?`, opcional)
  - `tags` (`IReadOnlyList<string>?`, opcional)
* **Resposta**: `200 OK` com `PostUpdatedDto` contendo todos os dados atualizados, novo tempo de leitura e carimbos de data/hora.

### 2. Estratégia de Preservação de Slugs
* Se `slug` for omitido ou vazio, o post preserva o `Slug` existente, salvaguardando a estabilidade dos links de SEO.
* Se um novo `slug` for fornecido, é sanitizado por `SlugHelper.Generate`. Se diferente do atual, valida-se a unicidade excluindo o próprio post (`ExistsBySlugAsync(slug, excludePostId: post.Id)`). Conflitos resultam em `409 Conflict`.

### 3. Resolução de Categoria com Fallback
* Se `categoryId` ou `categorySlug` forem passados, a categoria é verificada e atualizada.
* Se omitidos, mantém-se a categoria atual `post.CategoryId`.

### 4. Recálculo Atómico de Tempo de Leitura
* O domínio recalcula `ReadingTimeMinutes` a partir do `ContentMd` (fórmula de 200 palavras/minuto) e atualiza `UpdatedAt = DateTime.UtcNow`.

## Consequências

### Positivas
* **Estabilidade de SEO**: Slugs não quebram acidentalmente por meras edições de texto.
* **Ergonomia**: Autores editam conteúdo sem necessidade de reenviar dados imutáveis de categoria ou slug.
* **Consistência**: Alinhamento com a arquitetura de ProblemDetails e fluxo de tags já estabelecidos.

### Negativas / Compensações
* A lista de tags substitui a associação anterior quando fornecida.
  - *Mitigação*: Passar `null` preserva as tags existentes; passar array vazio `[]` remove as tags.
