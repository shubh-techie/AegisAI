# AegisAI

**Adaptive Intelligence for Secure Distributed Systems**

Observe. Assess. Authorize. Respond.

**Under development — solution foundation only.** A .NET 8 solution and minimal
API expose `GET /health` and authenticated `GET /identity`; Model A RBAC is available through an experimental decision endpoint.

AegisAI is intended to explore adaptive authorization, contextual risk assessment,
behavioral anomaly detection, and policy-controlled response in distributed systems.

## Status

- **IMPLEMENTED**: .NET 8 Clean Architecture solution, nullable reference types,
  health endpoint, JWT authentication, Model A RBAC, unit tests, integration tests,
  architecture tests, and foundation documentation.
- **PLANNED**: business functionality and its tests, authorization,
  risk assessment, telemetry, and deployment support.
- **EXPERIMENTAL**: Model A RBAC baseline is implemented; no research experiments or benchmarks have run.

No research findings, benchmark results, datasets, or publications are claimed.

## History

This repository originated as **Securing-Microservices in 2022**. **AegisAI
development begins in 2026**; no intervening activity is claimed. The old
GloboTicket application is removed from the current working tree and will not be
reused. Its historical commits remain recoverable at
`securing-microservices-legacy` (`b9518fa`).

See [project history](docs/aegisai/PROJECT_HISTORY.md) and the
[development log](docs/aegisai/DEVELOPMENT_LOG.md).

## Repository structure

```text
AGENTS.md                    Agent instructions and engineering rules
docs/aegisai/
  PROJECT_HISTORY.md         Historical provenance
  DEVELOPMENT_LOG.md         Chronological AegisAI development record
  architecture/              Architecture documentation
  adr/                       Architecture decision records
  specs/                     Specifications
  research/                  Research methodology and documentation
  threat-model/              Threat modeling
src/                         Domain, Application, Infrastructure, Api
tests/                       Domain, Application, Integration, Architecture tests
research/
  datasets/                  Dataset provenance and future data
  experiments/               Reproducible experiment definitions
  notebooks/                 Exploratory analysis
  results/                   Actual experiment outputs
benchmarks/                  Future benchmark harnesses and measurements
deployment/
  docker/                    Future container configuration
  kubernetes/                Future Kubernetes manifests
papers/                      Future research manuscripts
```

Empty directories contain `.gitkeep` placeholders; their presence does not imply
implemented functionality. Existing planning documents remain under `docs/aegisai/`.

## Development

Read [AGENTS.md](AGENTS.md) before every task. New implementation must target
.NET 8, follow Clean Architecture and SOLID, include meaningful tests, and contain
no secrets. Clearly distinguish IMPLEMENTED, PLANNED, and EXPERIMENTAL work.
Never fabricate research evidence or rewrite Git history.

With a .NET 8 SDK installed, run from the repository root:

```sh
dotnet restore
dotnet build
dotnet test
dotnet run --project src/AegisAI.Api -- --urls http://localhost:5080
```

`GET http://localhost:5080/health` returns `{"status":"healthy"}`. This checks
process liveness only. Domain and Application tests cover RBAC concepts, decision rules, and identity
invariants; integration and architecture tests cover the implemented foundation.

See [SPEC-002](docs/aegisai/specs/SPEC-002-SOLUTION-FOUNDATION.md) and
[ADR-001](docs/aegisai/adr/ADR-001-CLEAN-ARCHITECTURE.md).

## License

No root LICENSE file is currently present; no license is asserted here.

## Authentication baseline

IMPLEMENTED: standard JWT bearer authentication and a provider-neutral Application
identity boundary. `/health` remains public; `/identity` requires a validated token.
No tokens are trusted until authority and audience are configured. See
[SPEC-004](docs/aegisai/specs/SPEC-004-AUTHENTICATION.md) for example configuration,
validation rules, tests, and remaining provider integration work.

## Model A — RBAC baseline

`POST /authorization/evaluate` accepts `{"resource":"reports","action":"read"}`
for the authenticated caller and returns ALLOW or DENY with a reason and policy
version. Roles and permissions come from server configuration; the default policy
has no grants. This experimental endpoint evaluates decisions without executing
protected actions. See [SPEC-005](docs/aegisai/specs/SPEC-005-RBAC.md) for configuration,
assumptions, algorithm, test scenarios, and limitations. ABAC and risk remain PLANNED.
