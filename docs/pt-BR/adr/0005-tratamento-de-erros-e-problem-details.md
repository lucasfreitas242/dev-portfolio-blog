# ADR 0005: Tratamento de Erros e Estratégia de ProblemDetails

* **Estado:** Aceite
* **Data:** 2026-10-09
* **Decisores:** Lucas Freitas

## Contexto
APIs precisam de respostas de erro previsíveis e padronizadas para aplicações clientes (ex.: Next.js, Astro, clientes mobile).
Sem uma estratégia centralizada, representações de erro tornam-se inconsistentes (strings soltas, corpos vazios, objetos JSON ad-hoc), e a lógica de status codes vaza para dentro das rotas.

Além disso, a RFC 7807 (atualizada pela RFC 9457) estabelece um formato padrão de mídia (`application/problem+json`) para reporte de erros HTTP.

## Decisão
Adotamos **Exceções Tipadas com `IExceptionHandler` do ASP.NET Core 8 e `ProblemDetails` (RFC 7807)**:

1. **Exceções de Domínio e Aplicação**:
   - Regras de negócio e domínio lançam exceções tipadas específicas:
     - `NotFoundException`: Recurso não existe (ex.: slug de post, categoria).
     - `ConflictException`: Conflito com estado existente (ex.: slug duplicado).
     - `ValidationException`: Falha em validações semânticas ou de entrada.
     - `DomainRuleViolationException`: Invariantes violadas dentro de entidades do domínio.

2. **Tratador Global de Exceções (`IExceptionHandler`)**:
   - Implementamos `GlobalExceptionHandler : IExceptionHandler` em `Blog.Api`.
   - Intercepta exceções não tratadas e as mapeia diretamente para `ProblemDetails`:
     - `NotFoundException` -> `404 Not Found`
     - `ValidationException` -> `400 Bad Request` (com dicionário `errors`)
     - `ConflictException` -> `409 Conflict`
     - `DomainRuleViolationException` -> `422 Unprocessable Content`
     - Exceções não tratadas -> `500 Internal Server Error` (registradas com stack trace no log; resposta sanitizada em produção)

3. **Simplicidade em Minimal APIs**:
   - Endpoints focam apenas no caminho feliz (`Results.Ok(result)`).
   - Sem código repetitivo de verificação de status (`if null return 404`) nos endpoints.

## Consequências

### Positivas
- **Simplicidade**: Mantém as rotas de Minimal APIs limpas, declarativas e focadas no fluxo de sucesso.
- **Padrão da Indústria**: Respostas de erro retornam `application/problem+json` com `type`, `title`, `status`, `detail`, `instance` e `traceId`.
- **Segurança**: Stack traces e detalhes internos do banco de dados são ocultados de clientes externos.
- **Observabilidade**: Logs centralizados de falhas com IDs de rastreamento (TraceId).

### Negativas / Trade-offs
- Utiliza exceções para desvios esperados de fluxo (ex.: recurso não encontrado). Em uma aplicação de blog, o impacto de performance é desprezível diante do I/O de banco de dados.
