# 001. Adotar .NET 8 LTS como Runtime da Aplicação

* **Estado:** Aceite
* **Data:** 2026-10-08
* **Decisores:** Lucas Freitas

## Contexto
O projeto necessita de uma plataforma fiável para correr Minimal APIs, Entity Framework Core e contentores Linux de forma previsível e duradoura. Versões com suporte a curto prazo (STS) ou previews acarretam quebras e esforço de atualização recorrente.

## Decisão
Padronizámos a solução no **.NET 8 LTS (Long Term Support)** em todos os projetos (`Blog.Domain`, `Blog.Application`, `Blog.Api`).

## Consequências
* Suporte oficial e atualizações de segurança alargadas.
* Imagens Docker Alpine estáveis e reduzidas.
* Ecossistema maduro para drivers de base de dados (`Npgsql`) e autenticação.