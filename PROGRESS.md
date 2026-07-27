# Status do projeto TrocoPoints — para retomar em uma nova conversa

Este arquivo existe pra permitir continuar o trabalho mesmo numa conversa
nova (janela de contexto zerada). Leia isto + o `CLAUDE.md` (contexto da
vaga/perfil) + o `README.md` (visão geral do projeto) antes de continuar.

## Modo de trabalho combinado (importante)

O usuário está estudando pra entrevista técnica e **quer escrever o código
de negócio ele mesmo** — Claude deve guiar, questionar decisões como um
entrevistador faria, revisar bugs, e só implementar diretamente
tooling/config/infra (docker-compose, pacotes NuGet, DI wiring) ou quando
o usuário pedir explicitamente pra agilizar. Ver histórico de correções
recorrentes abaixo — vários bugs reais já foram encontrados e corrigidos
ao vivo, isso faz parte do processo de aprendizado, não escondam isso do
usuário nem tentem "already fixed" antecipando erros dele.

## Ambiente local (como retomar)

```bash
docker compose -f docker/docker-compose.yml up -d
# Oracle, RabbitMQ, MongoDB, Jaeger, Redis - todos com healthcheck

dotnet run --project src/TrocoPoints.Worker   # ASPNETCORE_URLS=http://localhost:5081
dotnet run --project src/TrocoPoints.Api      # ASPNETCORE_URLS=http://localhost:5080
```
Credenciais locais: usuário `trocopoints`, senha `TrocoPoints123` em tudo
(Oracle, RabbitMQ, Mongo, Redis). Os `appsettings.Development.json` reais
estão no `.gitignore` (secrets locais) - usar os `.example` como base.

Repositório: https://github.com/tavoks/TrocoPoints (branch `master`, todo
o progresso já commitado e pushado até a Fase 5 inclusive).

## Arquitetura (Clean Architecture)

```
src/TrocoPoints.Domain/          # Cliente, Transacao, ContaPontos, PontosLedger,
                                  # AuditoriaTransacao, Value Objects Cpf/Dinheiro
src/TrocoPoints.Application/     # Casos de uso + interfaces (IUnitOfWork, repos)
src/TrocoPoints.Infrastructure/  # Dapper+Oracle, RabbitMQ, MongoDB, OpenTelemetry
src/TrocoPoints.Api/             # Web API (Sdk.Web) - controllers + HealthChecks
src/TrocoPoints.Worker/          # Sdk.Web (não Sdk.Worker!) - hospeda 2
                                  # BackgroundServices (OutboxPublisher,
                                  # RabbitMqConsumer) + expõe /health via HTTP
```

**Fluxo de negócio**: `POST /api/transacoes` → grava `Transacao` + linha na
`OutboxMessages` (Outbox Pattern, mesma transação Oracle) → `OutboxPublisher`
(polling a cada 5s) publica no RabbitMQ → `RabbitMqConsumer` credita pontos
em `ContaPontos` + grava `PontosLedger` (idempotência via constraint única
em `TransacaoExternaId`) + audita no MongoDB + invalida cache Redis.

## Fases concluídas (commitadas e testadas de ponta a ponta)

1. **Fase 1** — Domain/Application/Infrastructure/Api core, Oracle real via
   Docker, idempotência de transação validada.
2. **Fase 2** — RabbitMQ (topologia topic+retry+DLQ), Outbox Publisher,
   PontosLedger, RabbitMqConsumer com retry via header `x-death`.
3. **Fase 2.1** — Auditoria MongoDB (`GET /api/auditoria/{id}`).
4. **Fase 3** — Serilog (JSON estruturado), HealthChecks (`/health`,
   `/health/ready`) via pacotes da comunidade, OpenTelemetry + Jaeger com
   propagação manual de trace através do RabbitMQ (Outbox desacopla o
   contexto HTTP original - só conectamos Publisher↔Consumer).
5. **Fase 4** — Cache distribuído Redis (cache-aside em `ConsultarSaldo`,
   chave `saldo:cliente:{id}`, TTL 5min, invalidado pelo Worker ao creditar).
6. **Fase 5** — 31 testes de unidade (xUnit+Moq) + 1 teste de integração
   real com `Testcontainers.Oracle` (idempotência ponta a ponta).

## Fases pendentes (ordem do plano)

7. **Fase 6** — Docker + Kubernetes (kind), múltiplas réplicas da Api,
   Dockerfiles multi-stage, probes usando os HealthChecks da Fase 3.
8. **Fase 7** — Resiliência: Rate Limiting nativo do ASP.NET Core, estudo
   de load balancing (conectar com as réplicas do k8s da Fase 6).
9. **Fase 8** — CI/CD GitHub Actions (build+test no push, deploy em kind
   efêmero + smoke test na tag).

Plano completo e mais detalhado (Context original da decisão de escopo):
`C:\Users\AREK-GAMEPLAY\.claude\plans\sequential-finding-lake.md` (fora do
repo, é um arquivo de sessão do Claude Code - pode não existir numa
máquina/sessão nova; este PROGRESS.md é a fonte de verdade portátil).

## Gotchas técnicos importantes já descobertos (não redescobrir)

- **Oracle CDB/PDB**: scripts de init precisam de
  `ALTER SESSION SET CONTAINER = XEPDB1; ALTER SESSION SET CURRENT_SCHEMA = trocopoints;`
  no topo, senão objetos são criados no schema `SYS` do container raiz.
  **Testcontainers.Oracle não precisa disso** - já conecta direto no schema certo.
- **Oracle**: `FETCH FIRST n ROWS ONLY` vem **depois** do `ORDER BY`, não antes.
- **Oracle**: não tem tipo nativo de GUID - sempre `VARCHAR2(36)` +
  `.ToString()`/`Guid.Parse()` manual.
- **Dapper + `dynamic`**: `.Select()` do LINQ em cima de `dynamic` faz o
  tipo de retorno da cadeia inteira virar `dynamic` - usar `foreach` explícito.
- **RabbitMQ.Client v7**: API totalmente assíncrona (`IChannel`,
  `CreateChannelAsync`, `BasicPublishAsync` etc.) - maioria dos exemplos
  online mostra a v6 síncrona, não serve.
- **`AspNetCore.HealthChecks.MongoDb`/`.Rabbitmq` (pacotes recentes)**:
  pedem uma factory (`Func<IServiceProvider, ...>`), não a connection
  string direto.
- **`IDistributedCache` + Redis**: grava como **hash** internamente
  (campos `data`/`absexp`/`sldexp`), não como string simples - `GET` puro
  no Redis dá `WRONGTYPE`.
- **`TrocoPoints.Worker` usa `Microsoft.NET.Sdk.Web`**, não `Sdk.Worker` -
  precisou disso pra expor `/health` via HTTP.
- **OpenTelemetry**: Outbox Pattern desacopla o trace HTTP original do que
  acontece depois no RabbitMQ (a request já terminou quando o Publisher
  processa) - só conectamos Publisher→Consumer, não a requisição original.
