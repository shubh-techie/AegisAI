# Target Architecture

This document describes the intended AegisAI architecture. It is a target direction, not the current implementation.

## Target Flow

```text
Client
  |
  v
API Gateway
  |
  v
OIDC / JWT Authentication
  |
  v
Authorization Engine
  |
  +-- RBAC
  +-- ABAC
  +-- Context
  |
  v
Risk Engine
  |
  +-- Contextual Risk
  +-- Behavioral Signals
  +-- Anomaly Detection
  |
  v
Policy Decision Engine
  |
  +-- ALLOW
  +-- DENY
  +-- STEP_UP
  +-- LIMIT
  |
  v
Audit / Telemetry / Security Events
```

## IMPLEMENTED — Repository foundation

AEGISAI-002 provides a .NET 8 Clean Architecture solution and minimal health API. The legacy application has
been removed from the current working tree and will not be reused. New .NET 8
code must follow Clean Architecture and SOLID, with meaningful automated tests.

## PLANNED — AegisAI Components

Planned components:

- Authorization Engine:
  - Centralizes authorization decisions.
  - Starts with RBAC and adds ABAC and contextual inputs.
  - Produces structured authorization evidence.
- Risk Engine:
  - Computes contextual risk from request, identity, service, and environment signals.
  - Incorporates behavioral signals when available.
  - Provides risk classifications for policy evaluation.
- Policy Decision Engine:
  - Combines identity, authorization, risk, and policy inputs.
  - Produces `ALLOW`, `DENY`, `STEP_UP`, or `LIMIT`.
  - Emits explainable decision records.
- Audit / Telemetry / Security Events:
  - Captures decision inputs and outcomes.
  - Feeds research datasets and operational dashboards.
  - Supports incident analysis and automated response.

None of these AegisAI-specific engines currently exist in source code.

## Potential Technology Direction

Target technologies:

- .NET 8 / ASP.NET Core
- C#
- Python / FastAPI for ML experiments
- OIDC / OAuth2
- PostgreSQL
- Redis
- Kafka
- OpenTelemetry
- Docker Compose
- Kubernetes later

These technologies are PLANNED candidates; .NET 8 is the required .NET target. Only the .NET 8 solution and health endpoint are implemented; infrastructure integrations remain PLANNED.

## Target Service Boundaries

Potential future services and modules:

- `AegisAI.Gateway`
- `AegisAI.Identity` or external OIDC provider integration
- `AegisAI.Authorization`
- `AegisAI.Risk`
- `AegisAI.Policy`
- `AegisAI.Telemetry`
- `AegisAI.Experiments`
- Python experiment services or notebooks under `research/`

New service boundaries will be decided through specifications and ADRs. GloboTicket services will not be reused.

## Decision Model

Target policy decisions:

- `ALLOW`: Request proceeds.
- `DENY`: Request is rejected.
- `STEP_UP`: User or service must satisfy stronger authentication or verification.
- `LIMIT`: Request proceeds under reduced capability, rate, scope, or data exposure.

The repository currently does not implement this decision model.

## Observability Goals

Future observability should capture:

- Request identity and subject.
- Client and service context.
- Authorization attributes.
- Risk features and risk score.
- Policy decision and reason codes.
- Latency at each decision stage.
- Security event correlation IDs.
- Response action and outcome.

OpenTelemetry is a target direction, not an existing implementation.
