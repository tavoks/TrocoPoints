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

Alternativa via Kubernetes local (Docker Desktop, ver Fase 6):

```bash
docker compose -f docker/docker-compose.yml up -d
docker build -f src/TrocoPoints.Api/Dockerfile -t trocopoints-api:local .
docker build -f src/TrocoPoints.Worker/Dockerfile -t trocopoints-worker:local .
kubectl apply -f k8s/
# Api em http://localhost:30080 (NodePort, 3 réplicas)
```

Credenciais locais: usuário `trocopoints`, senha `TrocoPoints123` em tudo
(Oracle, RabbitMQ, Mongo, Redis). Os `appsettings.Development.json` reais
estão no `.gitignore` (secrets locais) - usar os `.example` como base. Os
manifests `k8s/*-secret.yaml` replicam essa mesma senha em texto plano
(propósito didático local - nunca faria isso apontando pra credenciais
reais).

Repositório: https://github.com/tavoks/TrocoPoints (branch `master`, todo
o progresso já commitado e pushado até a Fase 6 inclusive).

## Arquitetura (Clean Architecture)

```
src/TrocoPoints.Domain/          # Cliente, Transacao, ContaPontos, PontosLedger,
                                  # AuditoriaTransacao, Value Objects Cpf/Dinheiro
src/TrocoPoints.Application/     # Casos de uso + interfaces (IUnitOfWork, repos)
src/TrocoPoints.Infrastructure/  # Dapper+Oracle, RabbitMQ, MongoDB, OpenTelemetry
src/TrocoPoints.Api/             # Web API (Sdk.Web) - controllers + HealthChecks
src/TrocoPoints.Worker/          # Sdk.Web (não Sdk.Worker!) - hospeda os
                                  # BackgroundServices (OutboxPublisher,
                                  # RabbitMqConsumer) + expõe /health via HTTP.
                                  # Em k8s roda como 2 Deployments separados
                                  # (WORKER_ROLE=publisher|consumer) - local
                                  # via docker compose sobe os dois no mesmo
                                  # processo (WORKER_ROLE ausente).
k8s/                              # Manifests (Deployment/Service/ConfigMap/
                                  # Secret) para rodar no Kubernetes do Docker
                                  # Desktop local. Ver Fase 6 abaixo.
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
7. **Fase 6** — Docker + Kubernetes. Dockerfiles multi-stage (build com
   `sdk:10.0`, runtime com `aspnet:10.0`, porta 8080) para Api e Worker.
   Cluster local via **Kubernetes do Docker Desktop** (não `kind` -
   compartilha o daemon Docker, então as imagens `:local` buildadas ficam
   disponíveis direto, sem registry/`kind load`, só `imagePullPolicy: Never`).
   Api com 3 réplicas + `Service NodePort` (`localhost:30080`). Worker
   **dividido em 2 Deployments** (`worker-publisher`, `worker-consumer`,
   1 réplica cada) via env var `WORKER_ROLE` lida no `Program.cs` - motivo:
   o `OutboxRepository.BuscarPendentesAsync` não usa `FOR UPDATE SKIP
   LOCKED`, então 2 réplicas do `OutboxPublisher` concorrentes podem pegar
   a mesma mensagem pendente no mesmo ciclo (5s) e publicar duplicado no
   RabbitMQ - inseguro escalar esse lado horizontalmente sem antes resolver
   isso no SQL. O `RabbitMqConsumer` não tem esse problema (competing
   consumers é um padrão seguro do RabbitMQ). Dependências (Oracle,
   RabbitMQ, MongoDB, Redis, Jaeger) continuam no `docker-compose` do host,
   pods acessam via `host.docker.internal`. Testado ponta a ponta: POST
   transação → outbox → RabbitMQ → crédito de pontos → auditoria Mongo,
   com idempotência confirmada reenviando a mesma `TransacaoExternaId`
   contra as 3 réplicas da Api.

## Fases pendentes (ordem do plano)

8. **Fase 7** — Resiliência: Rate Limiting nativo do ASP.NET Core, estudo
   de load balancing (conectar com as réplicas do k8s da Fase 6). Adiada
   a pedido do usuário - indo direto para CI/CD, retomar depois.
9. **Fase 8 (em andamento)** — CI/CD GitHub Actions (build+test no push,
   deploy em ambiente k8s efêmero + smoke test na tag).

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
- **Kubernetes do Docker Desktop vem desativado por padrão** - precisa
  ativar manualmente em Settings → Kubernetes → Enable Kubernetes (não dá
  pra automatizar via CLI, é toggle de GUI). É gratuito (não é feature paga
  do Docker Desktop, só o Docker Desktop em si tem licenciamento por porte
  de empresa).
- **k8s do Docker Desktop compartilha o daemon Docker local** - imagens
  buildadas com `docker build -t nome:local .` já ficam visíveis pro
  cluster, sem precisar de registry nem `kind load docker-image`. Só usar
  `imagePullPolicy: Never` no manifest pra não tentar puxar do Docker Hub.
- **`host.docker.internal`** é como os pods alcançam serviços rodando no
  `docker-compose` do host (fora do cluster) - `localhost` de dentro do
  pod é o próprio container, não o host.
- **`ConfigMap` vs `Secret`**: mesma mecânica de `data`/`stringData` na
  raiz do manifest (não tem `spec`), diferença é só convenção de uso -
  `Secret` não é criptografado por padrão, é base64 (mas `stringData`
  evita ter que converter manualmente, o k8s converte ao salvar).
- **Build context do Dockerfile é a raiz do repo, não a pasta do
  projeto** - os `.csproj` da Api/Worker referenciam Domain/Application/
  Infrastructure por caminho relativo (`../TrocoPoints.Domain/...`), então
  o build precisa rodar de `docker build -f src/TrocoPoints.Api/Dockerfile .`
  a partir da raiz.
