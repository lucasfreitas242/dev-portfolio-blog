# ADR 0006: Adotar Bruno como Cliente de API e Ferramenta de Testes de Contrato no Git

* **Estado:** Aceite
* **Data:** 2026-10-09
* **Decisores:** Lucas Freitas

## Contexto
À medida que a API cresce com novos endpoints de consulta e mutação, precisamos de uma forma confiável e colaborativa de documentar, validar manualmente e automatizar requisições HTTP contra nossos endpoints.

Clientes tradicionais de API como Postman ou Insomnia exigem cada vez mais sincronização em nuvem proprietária, armazenam coleções em formatos JSON complexos com diffs ruidosos no Git, ou limitam a colaboração em equipe com planos pagos.

Precisamos de uma ferramenta de documentação e teste de APIs que:
1. Armazene requisições como ficheiros de texto simples e legíveis diretamente em nosso repositório Git.
2. Integre-se de forma limpa em revisões de código (Pull Requests).
3. Suporte asserções automatizadas e execução via linha de comando (`bru run`) em pipelines de CI/CD.

## Decisão
Adotamos o **Bruno** como o cliente HTTP oficial e ferramenta de testes de contrato da API, localizado em `docs/specs/bruno/`:

1. **Armazenamento no Repositório (Repository-First)**:
   - Cada requisição é armazenada como um ficheiro `.bru` em `docs/specs/bruno/`.
   - Configurações de ambiente ficam em `docs/specs/bruno/environments/` (ex.: `local.bru`). Segredos sensíveis são excluídos do controle de versão.

2. **Definição de Pronto (Definition of Done - DoD)**:
   - Todo endpoint novo ou modificado **deve** incluir um ficheiro `.bru` novo ou atualizado.
   - Cada requisição deve conter asserções automatizadas no bloco `tests { ... }` verificando no mínimo:
     - Status code HTTP esperado (200, 201, 400, 404, etc.).
     - Propriedades críticas do payload e invariantes de contrato.

3. **Integração com CI/CD**:
   - Pipelines automatizados de teste podem executar `npx @usebruno/cli run docs/specs/bruno --env local` contra instâncias de teste em containers como validação de contrato e testes de fumaça (smoke tests).

## Consequências

### Positivas
- **Nativo do Git**: Diffs limpos em Pull Requests tornam as alterações de API visíveis junto ao código C# que as implementa.
- **Sem Lock-in de Nuvem**: Funcionamento offline completo, sem dependência de workspaces ou contas em nuvens externas.
- **Onboarding Rápido**: Clonar o repositório já disponibiliza toda a coleção configurada e pronta para execução.
- **Documentação Viva e Executável**: Garante que a documentação da API não fique desatualizada em relação à implementação.

### Negativas / Trade-offs
- Desenvolvedores precisam ter o aplicativo Bruno ou a ferramenta `@usebruno/cli` instalados.
- Contribuidores precisam manter e atualizar os ficheiros `.bru` como parte do fluxo contínuo de entrega de features.
