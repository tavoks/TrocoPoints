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
  TrocoPoints.Infrastructure/  # Dapper + Oracle, implementações concretas
  TrocoPoints.Api/             # Web API (ASP.NET Core)
  TrocoPoints.Worker/          # Consumidor de eventos (RabbitMQ) — em construção
tests/
  TrocoPoints.UnitTests/
  TrocoPoints.IntegrationTests/
docker/
  docker-compose.yml           # Oracle XE local
  init-db/                     # Scripts de criação de schema
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

## Status

Em desenvolvimento incremental. Fase atual: domínio, casos de uso e API
core funcionando ponta a ponta contra Oracle real. Próximas fases:
mensageria (RabbitMQ + Outbox publisher + Worker), cache distribuído
(Redis), observabilidade, testes automatizados, Kubernetes e CI/CD.
