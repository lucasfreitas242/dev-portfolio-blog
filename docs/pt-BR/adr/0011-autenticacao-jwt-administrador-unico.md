# ADR 0011: Estratégia de Autenticação JWT para Administrador Único

* **Estado:** Aceite
* **Data:** 2026-10-10
* **Decisores:** Lucas Freitas

## Contexto
Após o [ADR-0007](0007-fluxo-de-criacao-de-posts-e-autoria-de-traducoes.md), [ADR-0008](0008-gerenciamento-de-categorias-e-fluxo-de-autoria.md), [ADR-0009](0009-atualizacao-de-posts-e-mutacao-de-slug.md) e [ADR-0010](0010-gestao-de-rascunhos-e-feed-de-autoria.md), a API disponibiliza o ciclo de vida integral de posts, categorias, rascunhos e atualizações.

Contudo, os endpoints de escrita e de rascunhos não exigiam autenticação. Por se tratar de um portfólio e blog técnico pessoal com um único autor administrativo, apenas o proprietário deve poder criar, alterar, publicar ou visualizar rascunhos, mantendo o acesso de leitura a artigos e categorias totalmente público e irrestrito.

Sistemas multi-tenant comerciais (OAuth2 com provedores externos, base de dados para rotação de refresh tokens) adicionam complexidade acidental desnecessária a um projeto pessoal. Por outro lado, prescindir de autenticação exporia o conteúdo a mutações não autorizadas.

## Decisão
Adotamos uma **Arquitetura de Autenticação JWT para Administrador Único** via ASP.NET Core `JwtBearer`:

### 1. Endpoint de Autenticação e Verificação
* **Endpoint**: `POST /api/v1/auth/login`
* **Requisição**: `LoginCommand(string Email, string Password)`
* **Verificação**: Consulta o registro do administrador na tabela `Users` do PostgreSQL através de um `IPasswordHasher` seguro baseado em PBKDF2.
* **Resposta**: `200 OK` com `LoginResultDto(Token, ExpiresAt, Email, Name)`.
* **Erros**: Retorna `401 Unauthorized` (`ProblemDetails`) perante credenciais inválidas de forma neutra.

### 2. Especificação e Duração do Token
* **Algoritmo**: HMAC-SHA256 (`HmacSha256`) assinado com chave secreta simétrica configurada em `JwtSettings:SecretKey`.
* **Claims**: Contém `sub` (UUID do usuário), `email`, `name`, `role: Admin` e `jti` (identificador único do token).
* **Validade**: Configurável (padrão de 7 dias / 168 horas) para proporcionar sessões de autoria contínuas no painel administrativo sem interrupções frequentes.

### 3. Segmentação de Autorização nas Rotas
* **Endpoints Protegidos (`.RequireAuthorization()`)**:
  - `POST /api/v1/posts` (Criar Post)
  - `PUT /api/v1/posts/{id}` (Atualizar Post)
  - `POST /api/v1/posts/{id}/publish` (Publicar Rascunho)
  - `GET /api/v1/posts/drafts` (Listar Rascunhos)
  - `GET /api/v1/posts/drafts/{id}` (Inspecionar Rascunho)
  - `POST /api/v1/categories` (Criar Categoria)
* **Endpoints Públicos (Acesso Anônimo)**:
  - `GET /api/v1/posts` (Listar Posts Publicados)
  - `GET /api/v1/posts/{slug}` (Obter Post por Slug)
  - `GET /api/v1/categories` (Listar Categorias)
  - `GET /api/v1/categories/{slug}` (Obter Categoria por Slug)
  - `POST /api/v1/auth/login` (Login do Administrador)

### 4. Integração Interativa no OpenAPI / Swagger
O Swagger UI está configurado com o esquema de segurança `Bearer`, permitindo a injeção do token nos testes locais.

## Consequências

### Positivas
* **Sem Complexidade Acidental**: Sem dependência de serviços externos de identidade ou tabelas adicionais de sessão.
* **Pronto para Frontend**: O frontend administrativo conecta-se diretamente a `POST /api/v1/auth/login` e anexa o token em `Authorization: Bearer <token>`.
* **Segregação Estrita**: Leituras públicas continuam anônimas e compatíveis com cache; operações de escrita exigem autenticação válida.
* **Segurança Robusta**: Senhas armazenadas com salt e PBKDF2 (100.000 iterações); tokens com expiração definida.

### Negativas / Compensações
* Revogação imediata de token antes da expiração requer rotação da chave secreta.
  - *Mitigação*: Suficiente para um blog pessoal de administrador único. Alterar `JwtSettings:SecretKey` invalida instantaneamente todos os tokens emitidos.
