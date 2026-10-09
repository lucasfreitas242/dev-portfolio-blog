# Documentação Técnica do Portfólio & Blog

Esta pasta centraliza os registos de decisão arquitetural (ADRs) e diretrizes técnicas em Português.

## Arquitetura da Solução
- **Clean Architecture em 3 Camadas:** `Blog.Domain`, `Blog.Application` e `Blog.Api`.
- **Persistência:** PostgreSQL via Docker e Entity Framework Core.
- **Abordagem:** Spec-Driven Development (SDD) com contratos OpenAPI e BDD.

Consulte o [Índice de ADRs](adr/ADR.md) para detalhes sobre as decisões tomadas.