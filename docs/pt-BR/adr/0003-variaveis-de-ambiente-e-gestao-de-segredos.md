# ADR 0003: Gestão de Variáveis de Ambiente e Segredos

* **Estado:** Aceite
* **Data:** 2026-10-09
* **Decisores:** Lucas Freitas

## Contexto
A API do blog necessita de acesso a credenciais sensíveis e valores de configuração, tais como strings de conexão do PostgreSQL, chaves de assinatura JWT e credenciais administrativas iniciais de seed. Armazenar dados sensíveis em texto simples dentro de ficheiros controlados por versão introduz vulnerabilidades críticas de segurança e acarreta o risco de exposição acidental em repositórios públicos.

Precisamos de uma estratégia padronizada que proteja os segredos no controle de versão, mantendo-se simples e direta no desenvolvimento local, em ambientes conteinerizados (Docker) e em pipelines automatizados de CI/CD.

## Decisão
Decidimos adotar um modelo de configuração em camadas utilizando o **.NET Secret Manager (User Secrets)** para desenvolvimento local e **Variáveis de Ambiente** padrão para ambientes conteinerizados e produção:

1. **Desenvolvimento Local**:
   - Valores sensíveis são armazenados fora da árvore do projeto utilizando `dotnet user-secrets` com escopo em `Blog.Api`.
   - Os ficheiros do repositório (`appsettings.json`, `appsettings.Development.json`) contêm apenas padrões não sensíveis, chaves estruturais e valores de placeholder.

2. **Ambientes Conteinerizados e Produção**:
   - As configurações são injetadas através de variáveis de ambiente hierárquicas suportadas nativamente pelo provedor de configuração do .NET (ex.: `ConnectionStrings__DefaultConnection`, `Jwt__SecretKey`).
   - Um ficheiro `.env.example` é mantido na raiz do repositório para documentar as chaves obrigatórias, enquanto ficheiros `.env` reais permanecem estritamente ignorados pelo `.gitignore`.

3. **Fallback e Ingestão**:
   - O pipeline de hospedagem do ASP.NET Core carrega as configurações através de `builder.Configuration`, preservando a ordem padrão de precedência: ficheiros base de configuração, ficheiros JSON específicos do ambiente, user secrets (em modo de Desenvolvimento) e variáveis de ambiente.

## Consequências

### Positivas
- **Segurança**: Previne vazamentos acidentais de tokens sensíveis, senhas e strings de conexão no Git.
- **Integração Nativa**: Utiliza o provedor nativo `IConfiguration` do ASP.NET Core sem dependência de bibliotecas de terceiros.
- **Paridade de Ambientes**: Alinha-se aos princípios do Twelve-Factor App para configurações em containers.

### Negativas / Trade-offs
- Novos desenvolvedores precisam inicializar os segredos locais manualmente antes de executar a aplicação.
  - *Mitigação*: Fornecer documentação clara e comandos de configuração no `README.md` (`dotnet user-secrets set ...`).
