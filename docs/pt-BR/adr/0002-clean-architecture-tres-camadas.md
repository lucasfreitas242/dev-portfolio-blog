# 002. Adotar Clean Architecture Pragmática em 3 Camadas

* **Estado:** Aceite
* **Data:** 2026-10-08
* **Decisores:** Lucas Freitas

## Contexto
Necessidade de isolar a lógica de negócio dos detalhes de transporte HTTP e base de dados, mantendo o código direto e sem camadas redundantes.

## Decisão
Estruturámos a solução em 3 projetos:
1. **`Blog.Domain`:** Entidades com comportamentos e invariantes, enums e interfaces (`IPostRepository`). Isento de dependências externas.
2. **`Blog.Application`:** Orquestração de casos de uso organizados verticalmente (Commands/Queries/Handlers).
3. **`Blog.Api`:** Minimal APIs, contexto do EF Core (`AppDbContext`), autenticação e configuração.

## Consequências
* Testabilidade simples do domínio e das regras da aplicação.
* Eliminação de interfaces supérfluas e mapeamentos DTO desnecessários.
* A camada `Blog.Api` assume temporariamente a infraestrutura de dados para evitar dispersão de projetos.