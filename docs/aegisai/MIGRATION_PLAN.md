# Foundation and Development Plan

The earlier proposal to upgrade and reuse GloboTicket is superseded by
AEGISAI-001. AegisAI starts with a fresh implementation. Historical source remains
available at `securing-microservices-legacy` only.

## IMPLEMENTED — Repository foundation

Documentation and directory placeholders are established. Legacy application and
project files are removed from the current working tree without changing history.
No .NET solution is created in AEGISAI-001.

## PLANNED — New implementation

- Define specifications, threat model, and architecture decisions.
- Establish a .NET 8 solution using Clean Architecture and SOLID in a later task.
- Implement new behavior with meaningful automated tests and secret-free configuration.
- Introduce authorization contracts, audit events, and an RBAC baseline.
- Evaluate ABAC, contextual risk, behavioral signals, and policy-controlled response.
- Add reproducible deployment and experiment tooling as implementation progresses.

## EXPERIMENTAL

No experiments have run. Future experiments must record methodology, dataset
provenance, environment, measurements, and limitations. No results are assumed.
