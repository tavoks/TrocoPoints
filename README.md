# TrocoPoints

Projeto de portfólio construído para estudo e preparação técnica para uma
vaga de Middle Software Engineer (.NET) — inspirado no domínio de uma
plataforma de fidelidade que converte troco de compras em pontos
creditados por CPF.

O objetivo não é só ter um repositório funcionando, mas praticar de forma
guiada tópicos frequentemente cobrados em entrevistas técnicas .NET:
Oracle Database, MongoDB, Docker, Kubernetes, mensageria (RabbitMQ),
observabilidade, testes automatizados e CI/CD.

## Domínio

1. Um PDV envia uma transação de troco (`CPF`, `valor`, `pdvId`, `id externo da transação`).
2. A API valida e persiste a transação no Oracle de forma **idempotente**
   (constraint única no id externo da transação), gravando também um
   registro na tabela outbox, na mesma transação de banco.
3. Um publisher assíncrono (Fase futura) lê a outbox e publica o evento
   `TransacaoRecebida` no RabbitMQ.
4. Um Worker consome o evento, credita pontos (idempotente) e grava
   auditoria da transação.
5. Falhas de consumo vão para uma dead-letter queue após N tentativas.

## Arquitetura

Clean Architecture em 5 projetos:

```
src/
  TrocoPoints.Domain/          # Entidades e Value Objects — sem dependências externas
  TrocoPoints.Application/     # Casos de uso e interfaces (contratos)
  TrocoPoints.Infrastructure/  # Dapper + Oracle, RabbitMQ, MongoDB, OpenTelemetry
  TrocoPoints.Api/             # Web API (ASP.NET Core) + HealthChecks
  TrocoPoints.Worker/          # OutboxPublisher + RabbitMqConsumer (hosted services)
tests/
  TrocoPoints.UnitTests/
  TrocoPoints.IntegrationTests/  # Testcontainers.Oracle
docker/
  docker-compose.yml           # Oracle, RabbitMQ, MongoDB, Redis, Jaeger locais
  init-db/                     # Scripts de criação de schema
k8s/
  namespace.yaml
  api-*.yaml                   # Deployment (3 réplicas) + Service (NodePort) + ConfigMap/Secret
  worker-publisher-deployment.yaml   # WORKER_ROLE=publisher — só o OutboxPublisher
  worker-consumer-deployment.yaml    # WORKER_ROLE=consumer — só o RabbitMqConsumer
```

## Stack

C#/.NET 10, ASP.NET Core, Dapper, Oracle Database (via Docker), Value
Objects para invariantes de domínio (`Cpf`, `Dinheiro`), Unit of Work +
Repository Pattern, Outbox Pattern para consistência entre banco e
mensageria.

## Rodando localmente

Pré-requisitos: .NET 10 SDK, Docker Desktop.

```bash
# 1. Sobe o Oracle XE local
docker compose -f docker/docker-compose.yml up -d

# 2. Copia o appsettings de exemplo e ajusta se necessário
cp src/TrocoPoints.Api/appsettings.Development.json.example src/TrocoPoints.Api/appsettings.Development.json

# 3. Roda a API
dotnet run --project src/TrocoPoints.Api
```

A API sobe com Swagger em `/swagger`.

### Endpoints disponíveis

- `POST /api/transacoes` — recebe uma transação de troco.
- `GET /api/pontos/{cpf}` — consulta o saldo de pontos de um cliente.
- `GET /api/auditoria/{transacaoExternaId}` — consulta a auditoria (MongoDB).
- `GET /health` / `GET /health/ready` — liveness/readiness (Api e Worker).

## Rodando no Kubernetes local

Pré-requisitos: Docker Desktop com Kubernetes habilitado (Settings →
Kubernetes → Enable Kubernetes). As dependências (Oracle, RabbitMQ,
MongoDB, Redis, Jaeger) continuam no `docker-compose` do host — os pods
acessam via `host.docker.internal`, não são recriadas dentro do cluster.

```bash
docker compose -f docker/docker-compose.yml up -d

# build local — o k8s do Docker Desktop usa o mesmo daemon, sem precisar de registry
docker build -f src/TrocoPoints.Api/Dockerfile -t trocopoints-api:local .
docker build -f src/TrocoPoints.Worker/Dockerfile -t trocopoints-worker:local .

kubectl apply -f k8s/
```

A Api fica exposta em `http://localhost:30080` (Service `NodePort`), com
3 réplicas balanceadas. O Worker roda como **dois Deployments
independentes** (`worker-publisher` e `worker-consumer`), cada um
hospedando só um dos dois `BackgroundService` via env var `WORKER_ROLE` —
ver seção de Gotchas abaixo sobre o porquê.

## Status

Fases concluídas: domínio/API core (Oracle), mensageria (RabbitMQ +
Outbox Pattern + PontosLedger), auditoria (MongoDB), observabilidade
(Serilog + HealthChecks + OpenTelemetry/Jaeger), cache distribuído
(Redis, cache-aside), testes automatizados (xUnit + Testcontainers.Oracle)
e deploy em Kubernetes local (Docker Desktop) com múltiplas réplicas da
Api e Workers separados por responsabilidade. Próximas fases: rate
limiting/resiliência e CI/CD (GitHub Actions).

Detalhes de retomada de contexto e gotchas técnicos: ver
[`PROGRESS.md`](PROGRESS.md).
