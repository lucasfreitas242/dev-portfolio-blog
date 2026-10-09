# ADR 0004: Posts Multilíngues e Estratégia de Slugs (i18n)

* **Estado:** Aceite
* **Data:** 2026-10-09
* **Decisores:** Lucas Freitas

## Contexto
A plataforma de blog deve suportar artigos publicados em múltiplos idiomas (inicialmente `pt-BR` e `en-US`).
É necessário definir como os artigos em idiomas diferentes são identificados, como são consultados via API, e como fornecemos ao frontend os metadados necessários para SEO internacional (especificamente tags `<link rel="alternate" hreflang="..." />` e seletor de idiomas na página).

Duas abordagens arquiteturais foram avaliadas:
1. **Prefixação Hierárquica na Rota**: `/api/v1/posts/{language}/{slug}` usando um slug idêntico para todos os idiomas.
2. **Slugs Localizados com Agrupamento de Tradução**: `/api/v1/posts/{slug}` usando slugs localizados no idioma nativo (ex.: `building-...` vs. `construindo-...`), vinculados através de um `TranslationGroupId` compartilhado.

## Decisão
Decidimos adotar a **Abordagem 2: Slugs Localizados com `TranslationGroupId` e Rota Direta `GET /api/v1/posts/{slug}`**:

1. **Entidade de Domínio `Post`**:
   - Inclui a propriedade `string Language` (formato IETF tag, ex.: `pt-BR`, `en-US`).
   - Inclui a propriedade `Guid TranslationGroupId`. Posts que são traduções mútuas compartilham o mesmo identificador de grupo.
   - O `Slug` permanece globalmente único no banco de dados (`HasIndex(p => p.Slug).IsUnique()`), pois títulos traduzidos naturalmente geram slugs distintos por idioma.

2. **Contrato de API**:
   - A rota de consulta pública permanece limpa e RESTful: `GET /api/v1/posts/{slug}`.
   - O payload de resposta `PostDetailDto` inclui:
     - `string Language`: Idioma do post retornado.
     - `IReadOnlyList<PostTranslationDto> AvailableTranslations`: Lista de traduções irmãs contendo `{ Language, Slug }` vinculadas ao mesmo `TranslationGroupId`.
   - A listagem `GET /api/v1/posts` suporta filtro opcional por idioma via `?language=...`.

3. **Integração com Frontend & SEO**:
   - O frontend (Next.js, Astro ou Angular) gerencia prefixos de rota de UI (ex.: `/pt-BR/blog/...` ou `/en-US/blog/...`).
   - Utilizando `AvailableTranslations`, o cliente injeta automaticamente as tags `<link rel="alternate" hreflang="..." href="..." />` e preenche o seletor de idiomas sem precisar inferir slugs no lado do cliente.

## Consequências

### Positivas
- **Otimização para Mecanismos de Busca (SEO)**: URLs limpas e localizadas com palavras-chave nativas melhoram expressivamente a indexação regional.
- **Desacoplamento**: A API REST não mantém estado de sessão ou prefixos de idioma nas URLs; as consultas usam a chave canônica pública do recurso (`slug`).
- **Resolução em Requisição Única**: O frontend obtém o conteúdo localizado e links para traduções em uma única chamada HTTP.

### Negativas / Trade-offs
- A criação e edição de posts requer associar o `TranslationGroupId` adequado ao vincular versões traduzidas.
  - *Mitigação*: No método fábrica `Post.Create`, omitir `translationGroupId` gera automaticamente um novo `Guid.NewGuid()`, definindo o post como raiz da tradução.
- Por conta da unicidade global de slugs, títulos com termos idênticos entre idiomas exigem diferenciação de slug.
