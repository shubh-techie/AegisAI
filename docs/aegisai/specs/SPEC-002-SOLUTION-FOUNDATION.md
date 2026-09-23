# SPEC-002 — Solution Foundation

Date: 2026-09-23  
Task: AEGISAI-002  
Status: IMPLEMENTED; pending review

## Scope

Create AegisAI.sln with four production projects and four test projects targeting
net8.0, with nullable reference types and implicit usings enabled. global.json
selects .NET SDK 8.0.204 or a later installed .NET 8 feature band.

## Dependency contract

| Project | Direct project references |
| --- | --- |
| AegisAI.Domain | None |
| AegisAI.Application | Domain |
| AegisAI.Infrastructure | Application, Domain |
| AegisAI.Api | Application, Infrastructure |
| AegisAI.Domain.Tests | Domain |
| AegisAI.Application.Tests | Application |
| AegisAI.IntegrationTests | Api |
| AegisAI.ArchitectureTests | None; inspects production project declarations |

Domain, Application, and Infrastructure contain no business code. The API is the
composition root and contains only minimal startup and the health route.

## HTTP contract

GET /health returns HTTP 200, application/json, and:

```json
{"status":"healthy"}
```

This is process liveness only; no database, external dependency, readiness, or
security assessment is performed. No business endpoints are implemented.

## Validation

From the repository root, run dotnet restore, dotnet build, and dotnet test.
Integration tests host the actual API through WebApplicationFactory and check the
health response and an unmapped route. Four architecture cases enforce the exact
direct production project references and prohibit external dependencies in the
inner layers. These checks inspect project declarations; they do not constitute
comprehensive future source-level architecture analysis.

Domain and Application test projects are intentionally empty until business
behavior exists; no placeholder passing tests are used.

## Deferred work

PLANNED: domain behavior, application use cases, infrastructure adapters,
authentication, authorization, telemetry, deployment, and corresponding tests.
EXPERIMENTAL: none. No research results or benchmarks are claimed.
