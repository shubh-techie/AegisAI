# AegisAI

**Adaptive Intelligence for Secure Distributed Systems**

Observe. Assess. Authorize. Respond.

**Under development — repository foundation only.** No runnable AegisAI
application, .NET solution, or test suite exists yet.

AegisAI is intended to explore adaptive authorization, contextual risk assessment,
behavioral anomaly detection, and policy-controlled response in distributed systems.

## Status

- **IMPLEMENTED**: repository structure, project history, development log, and agent instructions.
- **PLANNED**: fresh .NET 8 implementation using Clean Architecture and SOLID,
  with tests, authorization, risk assessment, telemetry, and deployment support.
- **EXPERIMENTAL**: no experiments have been implemented or executed yet.

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
src/                         Future application source
tests/                       Future automated tests
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

Solution creation and build/run instructions are deferred to a later task.

## License

No root LICENSE file is currently present; no license is asserted here.
