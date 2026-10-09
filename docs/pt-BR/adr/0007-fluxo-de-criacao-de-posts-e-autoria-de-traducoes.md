# ADR 0007: Fluxo de Criação de Posts e Autoria de Traduções

* **Estado:** Aceite
* **Data:** 2026-10-09
* **Decisores:** Lucas Freitas

## Contexto
Após a aprovação do [ADR-0004](0004-posts-multilingues-e-estrategia-slug.md) (Estratégia Multilíngue) e do [ADR-0005](0005-tratamento-de-erros-e-problem-details.md) (Tratamento de Erros e ProblemDetails), precisamos de uma especificação formal para o ciclo de vida e fluxo de criação de artigos.

Os principais desafios na autoria de posts incluem:
1. **Ergonomia do slug vs. controle de SEO**: Gerar slugs legíveis, localizados e amigáveis para SEO sem causar quebras por colisão no banco de dados.
2. **Vinculação de traduções**: Associar artigos traduzidos de forma confiável prevenindo anomalias lógicas (como dois posts no mesmo idioma dentro do mesmo grupo de tradução).
3. **Taxonomia e Tags**: Gerenciar a associação de tags sem forçar o autor a pré-cadastrar tags em telas separadas.
4. **Ciclo de vida de publicação**: Equilibrar rascunhos vs. publicação imediata.

## Decisão
Adotamos um **Fluxo Unificado de Criação de Posts e Traduções** via `POST /api/v1/posts`:

1. **Endpoint e Contrato**:
   - Rota: `POST /api/v1/posts`.
   - Resposta: `201 Created` com cabeçalho `Location: /api/v1/posts/{slug}` e payload enxuto `PostCreatedDto` contendo `id`, `slug`, `language`, `translationGroupId`, `status` e `createdAt`.

2. **Geração de Slug e Tratamento de Colisões**:
   - Se `slug` for omitido ou vazio, o sistema gera automaticamente um slug em kebab-case a partir do `title` (removendo acentuação e caracteres especiais).
   - Se o cliente fornecer um `slug` explícito, esse valor é validado e utilizado.
   - Caso o slug já exista no banco de dados, a API lança `ConflictException("Um post com o slug '{slug}' já existe.")`, retornando `409 Conflict`. Rejeitamos sufixos aleatórios com hash para preservar a integridade de SEO.

3. **Agrupamento de Tradução e Validação de Invariantes**:
   - **Post Raiz**: Se `translationGroupId` for omitido, o sistema gera um novo `Guid.NewGuid()`, definindo o post como raiz de um novo grupo de tradução.
   - **Post Traduzido**: Se `translationGroupId` for informado:
     1. O sistema valida se o `translationGroupId` informado existe.
     2. O sistema impõe **Unicidade de Idioma no Grupo**: Não podem existir dois posts com o mesmo `Language` compartilhando o mesmo `TranslationGroupId`. Caso já exista uma versão naquele idioma, a API lança `ConflictException`, retornando `409 Conflict`.

4. **Taxonomia de Tags via Upsert**:
   - O payload aceita uma lista de nomes/slugs de tags: `tags: ["clean-architecture", ".NET"]`.
   - O sistema busca tags existentes por slug ou cria novas entidades `Tag` sob demanda, associando-as ao post na mesma transação.

5. **Ciclo de Vida de Publicação**:
   - Posts são criados com status `PostStatus.Draft` por padrão.
   - Um booleano opcional `publishNow: true` no payload permite criar e publicar atomicamente, preenchendo `PublishedAt = DateTime.UtcNow`.
   - Uma rota dedicada `POST /api/v1/posts/{id}/publish` é mantida para publicação posterior de rascunhos.

6. **Validação de Categoria**:
   - `categoryId` é obrigatório e deve apontar para uma `Category` existente. Caso não exista, a API lança `NotFoundException("Category", categoryId)`, retornando `404 Not Found`.

## Consequências

### Positivas
- **Ergonomia para o Autor**: Autores podem criar posts rapidamente com slugs automáticos e tags sob demanda, ou customizar slugs quando necessário.
- **Integridade dos Dados**: Garante consistência multilíngue estrita—sem traduções órfãs ou colisões de idiomas repetidos dentro do mesmo grupo.
- **Boas Práticas de REST e SEO**: Emite `201 Created` padrão com cabeçalho `Location`; previne colisões de URL de forma determinística via `409 Conflict`.

### Negativas / Trade-offs
- A busca e criação de tags sob demanda e a validação do grupo de tradução requerem consultas adicionais no banco dentro do handler de comando.
  - *Mitigação*: Consultas leves e indexadas executadas dentro de uma única transação no banco de dados.
