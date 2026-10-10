# ADR 0008: Gerenciamento de Categorias e Fluxo de Autoria

* **Estado:** Aceite
* **Data:** 2026-10-10
* **Decisores:** Lucas Freitas

## Contexto
Após a aceitação do [ADR-0004](0004-posts-multilingues-e-estrategia-slug.md) e do [ADR-0007](0007-fluxo-de-criacao-de-posts-e-autoria-de-traducoes.md), a criação de um post requer a associação a uma categoria existente (`categoryId`).

No entanto, sem endpoints dedicados de descoberta e gestão de categorias:
1. **Taxonomia opaca**: Autores e frontends clientes não tinham como listar categorias disponíveis sem inspecionar a base de dados diretamente.
2. **Fricção na autoria**: Criar um post exigia obter previamente os UUIDs internos das categorias.
3. **Risco de fragmentação taxonómica**: Ao contrário das tags — que possuem alta cardinalidade, são fluidas e seguras para upsert inline —, as categorias representam a estrutura central, a navegação primária e a hierarquia de rotas do blog. Permitir a geração espontânea de categorias na criação do post geraria duplicados ou erros de digitação (ex.: `dotnet`, `.NET`, `c-sharp`).

## Decisão
Adotamos a **Abordagem A (Taxonomia Curada com Descoberta e Resolução Dupla por Slug/Id)**:

### 1. Fatias Verticais e Endpoints Dedicados de Categoria
Disponibilizamos endpoints dedicados sob `/api/v1/categories`:
* **`GET /api/v1/categories`**: Lista todas as categorias com os respetivos metadados (`id`, `name`, `slug`, `isFeatured`) e a contagem de artigos publicados (`postCount`).
* **`GET /api/v1/categories/{slug}`**: Obtém informações detalhadas de uma categoria pelo slug. Retorna `404 Not Found` se não for encontrada.
* **`POST /api/v1/categories`**: Cria uma nova categoria a partir de `{ "name": "...", "slug": "...", "isFeatured": false }`.
  - Se `slug` for omitido, é gerado automaticamente a partir de `name` via `SlugHelper.Generate`.
  - Garante unicidade do slug; conflitos retornam `409 Conflict`.
  - Retorna `201 Created` com cabeçalho `Location: /api/v1/categories/{slug}`.

### 2. Resolução Dupla de Categoria na Criação de Posts
O comando `CreatePostCommand` passa a aceitar:
* `categoryId` (`Guid?`): Procura direta pelo UUID da entidade.
* `categorySlug` (`string?`): Procura direta pelo slug kebab-case.

Se nenhum for informado, o handler emite `400 Bad Request` (`ValidationException`). Se a categoria especificada não for encontrada, emite `404 Not Found` (`NotFoundException`).

### 3. Isolamento no Repositório e Schema
* Introduzida a interface `ICategoryRepository` em `Blog.Domain.Interfaces` e implementada em `Blog.Api.Data.Repositories.CategoryRepository`.
* Índice único `IX_Categories_Slug` e limites de comprimento adicionados a `Categories.Slug` e `Categories.Name` via `CategoryConfiguration`.

## Consequências

### Positivas
* **Ergonomia e Flexibilidade**: Autores e automações podem criar artigos usando slugs legíveis (ex.: `"categorySlug": "architecture"`) sem consultar UUIDs na base de dados.
* **Integridade Taxonómica**: Evita poluição por erros de digitação e preserva a intencionalidade na árvore de categorias.
* **Pronto para Frontend**: Clientes web podem consumir `GET /api/v1/categories` para renderizar menus, filtros e seletores com contagens precisas de artigos.
* **Clean Architecture**: Respeita o Princípio da Responsabilidade Única ao desacoplar operações de categorias de `IPostRepository`.

### Negativas / Compensações
* Exige que autores criem categorias inéditas via `POST /api/v1/categories` antes de redigir um post para esse novo tema.
  - *Mitigação*: O endpoint `POST /api/v1/categories` é leve e integra-se com facilidade a modais de criação no fluxo de autoria.
