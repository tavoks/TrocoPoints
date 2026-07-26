# Contexto: preparação técnica Exadel / Super Troco

## Objetivo
Preparar entrevista técnica para vaga Middle Software Engineer (.NET)
na Exadel (consultoria), alocado no cliente Super Troco.
Fase 2 de 3: técnica com especialista Exadel. Fase 3: cliente.

## Meu perfil
- ~7 anos em backend .NET, nível pleno. São Paulo.
- Forte: C#/.NET Core, RabbitMQ (exchanges, DLQ, Outbox Pattern),
  REST APIs, Clean Architecture, SQL Server/PostgreSQL, Dapper,
  microsserviços, mensageria de alto volume, Kubernetes/Rancher.
- Gaps a cobrir: Oracle Database, Oracle Cloud (OCI), MongoDB.

## Domínio do cliente
Plataforma de fidelidade integrada a milhares de PDVs. Converte troco
em pontos creditados por CPF. Alto volume transacional, idempotência
crítica (não creditar ponto duas vezes), consistência financeira,
auditoria regulatória (SUSEP, LGPD), sorteios semanais.

## Stack alvo da vaga
C#/.NET Core, mensageria, REST APIs, Oracle DB, MongoDB, Docker,
Kubernetes, OCI, Git, Scrum/Kanban. Nice-to-have: Kafka, IBM MQ,
Azure Service Bus, CI/CD, observabilidade, testes automatizados.

## Como quero trabalhar aqui
- Praticar código real, não pseudocódigo.
- Me questionar como um entrevistador faria: pedir justificativa das
  escolhas, apontar quando minha resposta está rasa.
- Priorizar os gaps Oracle Database, MongoDB, CI/CD, Docker, Kubernetes, observabilidade, Terraform, RabbitMQ,
IA (Mcps, harness enginneer, models, rag, skills) sem deixar cair o que já sei.

## Descrição da vaga
Why Join Exadel
We’re an AI-first global tech company with 25+ years of engineering leadership, 2,000+ team members, and 500+ active projects powering Fortune 500 clients, including HBO, Microsoft, Google, and Starbucks.

From AI platforms to digital transformation, we partner with enterprise leaders to build what’s next.
What powers it all? Our people are ambitious, collaborative, and constantly evolving.

About the Client
An innovative loyalty platform designed to solve the shortage of physical change at retail checkouts. Integrated directly into thousands of point-of-sale systems, the platform enables consumers to seamlessly convert small change into digital reward points. Today, this zero-cost tool reduces operational overhead for retailers while allowing users to redeem points for products, services, and cash prize drawings. 

What You’ll Do
Design, develop, maintain, and enhance enterprise applications using C#/.NET and .NET Core.
Develop and support messaging-based integration solutions.
Design, build, and maintain RESTful APIs.
Develop and optimize solutions using Oracle Database and MongoDB.
Deploy, monitor, and maintain applications in Kubernetes-based environments.
Participate in technical analysis, solution design, code reviews, and implementation activities.
Identify opportunities to improve application performance, scalability, reliability, and maintainability.
Collaborate with cross-functional teams in an Agile (Scrum/Kanban) environment.
Contribute to software quality through best development practices and continuous improvement initiatives.
What You Bring
Professional experience developing applications using C#/.NET Core (or newer versions).
Experience working with Oracle Cloud Infrastructure (OCI).
Hands-on experience with Oracle Database.
Experience working with MongoDB.
Knowledge of Docker and Kubernetes.
Experience designing and developing RESTful APIs.
Proficiency using Git for version control.
Experience working within Agile methodologies such as Scrum or Kanban.
Strong analytical and problem-solving skills.
Excellent communication and collaboration abilities.
Proactive mindset with a strong sense of ownership.
Commitment to software quality and continuous improvement.
Bachelor's degree in Computer Science, Computer Engineering, Information Systems, Software Engineering, or a related field (completed or in progress).
Nice to have
Experience with messaging platforms such as RabbitMQ, Apache Kafka, IBM MQ, or Azure Service Bus.
Experience with microservices architecture.
Familiarity with CI/CD pipelines.
Experience with monitoring and observability tools.
Experience writing and maintaining automated tests.
English level
Intermediate/Upper-Intermediate

Work type: On-site

Legal & Hiring Information
Exadel is proud to be an Equal Opportunity Employer committed to inclusion across minority, gender identity, sexual orientation, disability, age, and more
Reasonable accommodations are available to enable individuals with disabilities to perform essential functions
Please note: this job description is not exhaustive. Duties and responsibilities may evolve based on business needs
Compensation details are shared with candidates at the early stage of the recruitment process.
The offer is not binding until a signed contract is in place.
Your Benefits at Exadel
Exadel benefits vary by location and contract type. Your recruiter will fill you in on the details.

International projects
In-office, hybrid, or remote flexibility
Medical healthcare
Recognition program
Ongoing learning & reimbursement 
Well-being program
Team events & local benefits 
Sports compensation 
Referral bonuses 
Top-tier equipment provision
Exadel Culture
We lead with trust, respect, and purpose. We believe in open dialogue, creative freedom, and mentorship that helps you grow, lead, and make a real difference.
Ours is a culture where ideas are challenged, voices are heard, and your impact matters.

##Perguntas tecnicas
abaixo perguntas tecnicas para estudo teorico, selecione as mais importantes para esta vaga para estudarmos depois do nosso estudo prático:

Porque é mais seguro manter a connectionString no appsettings?
Para que serve o Program.cs e como funciona?
Para que serve WebApplication.CreateBuilder(args)?
Para que serve AddControllers()?
Porque IssuerSigningKey no JWT?
O que acontece no builder.Build()?
Para que serve UseHttpsRedirection?
O que faz AddAuthorization e UseAuthorization?
Para que serve UseAuthentication?
Para que serve MapControllers?
O que faz app.Run?
Diferença entre IEnumerable e List?
Como _cache.Set funciona por debaixo dos panos?
Como _cache.Remove funciona?
Porque criar um DTO de response separado da entidade?
O que é AbstractValidator?
Diferença entre ArgumentException e Exception?
Porque o método Atualizar dentro da entidade?
Diferença entre Classe e Struct?
Diferença entre heap e stack?
Explique Garbage Collector?
Quais os pilares de POO?
Sobrecarga vs Sobreposição?
Princípios do SOLID?
Diferença entre Transient, Scoped e Singleton?
O que é SNS?
O que é SQS?
Quando utilizar SNS e quando utilizar SQS?
O que é DLQ?
O que é Elastic Beanstalk?
O que é Lambda?
Como hospedaria uma API na AWS?
ECS Fargate vs EKS?
Qual a diferença entre imagem, container, pod, cluster e Kubernetes e qual seria a hierarquia?
Tipos de JOIN no SQL?
ORM vs Micro ORM vs ADO puro?
Como otimizaria performance em uma API?
Diferença entre programação síncrona e assíncrona?
Diferença entre Task e ValueTask?
Diferença entre await e .Result?
Como .Result pode causar deadlock?
O que é IAsyncEnumerable e yield return?
O que é Task.WhenAny?
O que é WaitAsync?
O que é injeção de dependência e porque usar?
O que é e como funciona uma API Gateway?
Como você faria testes de segurança na API?
o que é load balance e como usar em c#?
o que é rate limit e como usar em c#?
o que é outbox pattern e como utilizar em um sistema c#?
Como lidaria com erro 429?
Como registraria requisições de cada usuário na API?
Como prepararia a API para um aumento grande de usuários?
Diferença entre banco relacional e não relacional?
Como funciona a estrutura de um banco não relacional?
Quando usar relacional vs não relacional?
quais os tipos de Exchanges no rabbitmq e quando usar cada um?
quais os tipos de filas no rabbitmq e quando usar cada um?