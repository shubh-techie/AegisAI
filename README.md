# AegisAI

**Adaptive Intelligence for Secure Distributed Systems**

Observe. Assess. Authorize. Respond.

**Under development — V0.1 experimental authorization platform.** A .NET 8 solution and minimal
API expose `GET /health` and authenticated `GET /identity`; Models A/B/C have experimental authorization decision endpoints.

AegisAI is intended to explore adaptive authorization, contextual risk assessment,
behavioral anomaly detection, and policy-controlled response in distributed systems.

## Status

- **IMPLEMENTED**: .NET 8 Clean Architecture solution, nullable reference types,
  health endpoint, JWT authentication, Models A/B/C authorization, unit tests, integration tests,
  architecture tests, structured audit, local Docker workflow, and documentation.
- **PLANNED**: behavioral AI/ML (Model D), live context, protected-operation
  enforcement, durable audit, production identity-provider integration and deployment.
- **EXPERIMENTAL**: Models A/B/C baselines are implemented; an initial synthetic engine-only smoke run has executed.

No research findings, production datasets, or publications are claimed. A synthetic
engine-only smoke run is available for framework validation; it supports no broad
performance claims.

## Quick Start — local Docker development

Prerequisites: Git, a POSIX shell, Docker with Compose, and a running Linux
container daemon. Initial builds download images/packages. Clone this repository
using its hosting URL, then run from the clone root:

```sh
git clone <repository-url> AegisAI
cd AegisAI
./dev up                  # Build, generate temporary local credentials, start API
./dev health              # GET /health; also available at http://127.0.0.1:5080/health
./dev demo                # Exercise A/B/C and show decisions + audit correlation IDs
./dev logs                # JSON decision audit; Ctrl-C stops following
./dev test                # Full Release test suite
./dev experiment my-smoke  # New output directory: research/results/my-smoke/
./dev down
```

The demo uses **synthetic** identities and static policy/context fixtures. Local
credentials expire after one hour; rerun `./dev up` to rotate them. The API validates
signed JWTs; Development-only trust is not a production identity provider.
STEP_UP/LIMIT are recommendations, with no challenge/rate enforcement yet.

See [environment specification](docs/aegisai/specs/SPEC-010-V01-ENVIRONMENT.md),
[reproduction details](research/README.md), and the
[V0.1 readiness report](docs/aegisai/V01_READINESS.md) for validation and limitations.

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
benchmarks/                  Executable synthetic A–C experiment framework
deployment/
  docker/                    Local API container and development tools
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
Provider deployments require authority and audience configuration. The explicitly
opted-in local Docker environment uses temporary Development-only public-key trust. See
[SPEC-004](docs/aegisai/specs/SPEC-004-AUTHENTICATION.md) for example configuration,
validation rules, tests, and remaining provider integration work.

## Model A — RBAC baseline

`POST /authorization/evaluate` accepts `{"resource":"reports","action":"read"}`
for the authenticated caller and returns ALLOW or DENY with a reason and policy
version. Roles and permissions come from server configuration; the default policy
has no grants. This experimental endpoint evaluates decisions without executing
protected actions. See [SPEC-005](docs/aegisai/specs/SPEC-005-RBAC.md) for configuration,
assumptions, algorithm, test scenarios, and limitations. Model B adds ABAC; Model C adds deterministic contextual risk.

## Model B — RBAC + ABAC

`POST /authorization/evaluate/model-b` accepts the same resource/action request.
It requires an RBAC grant and all applicable ABAC conditions to pass. Subject,
resource, and environment attributes come from server configuration, never request
attribute bags. Missing attributes or applicable rules deny. See
[SPEC-006](docs/aegisai/specs/SPEC-006-ABAC.md) and
[ADR-003](docs/aegisai/adr/ADR-003-POLICY-EVALUATION.md). These are experimental
software baselines; behavioral AI and anomaly detection remain PLANNED. Model C adds deterministic contextual risk.

## Model C — deterministic contextual risk

`POST /authorization/evaluate/model-c` adds a fixed weighted score after RBAC and
ABAC allow. It returns ALLOW, DENY, STEP_UP, or LIMIT with risk level and signal
contributions. Context is server-owned simulation data; missing context denies.
This is a deterministic heuristic, not AI or ML. STEP_UP/LIMIT are recommendations
with obligations, not implemented enforcement. See
[SPEC-007](docs/aegisai/specs/SPEC-007-CONTEXTUAL-RISK.md) and
[ADR-004](docs/aegisai/adr/ADR-004-RISK-ENGINE.md).

## Authorization audit and observability

IMPLEMENTED: one structured decision event per completed Model A/B/C API evaluation,
with generated correlation ID, pseudonymized identifiers, outcome/reasons, optional
risk, UTC timestamp, and processing duration. .NET ActivitySource/Meter instrumentation
is OpenTelemetry-compatible; no exporter or Kafka is configured. Audit failures do
not change experimental decisions. See [SPEC-008](docs/aegisai/specs/SPEC-008-AUDIT-OBSERVABILITY.md)
for data minimization and best-effort delivery limits. Durable audit remains PLANNED.

## Reproducible synthetic experiments

An executable A–C engine-only comparison runner, synthetic scenarios, and generated
smoke artifacts are available. See [research/README](research/README.md) for exact
Release build/test/run commands and [SPEC-009](docs/aegisai/specs/SPEC-009-EXPERIMENT-FRAMEWORK.md)
for measurement boundaries. Model D and detection-quality analysis remain PLANNED.
Smoke measurements do not establish production performance or security effectiveness.

## Research definition

AEGISAI-012 defines the [research program](docs/aegisai/research/RESEARCH_PROGRAM.md),
[primary question and RQ1–RQ4](docs/aegisai/research/RESEARCH_QUESTION.md), and
[unproven H1–H4](docs/aegisai/research/RESEARCH_HYPOTHESES.md). Model D remains
PLANNED RESEARCH. The proposed contribution is subject to literature and prior-art
review; no effectiveness, novelty or patentability is established. Consult the
[claims register](docs/aegisai/research/CLAIMS_REGISTER.md) before public claims.
