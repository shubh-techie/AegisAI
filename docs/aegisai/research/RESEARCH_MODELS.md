# Research models

Date: 2026-09-25 | Baseline: AEGISAI-012

| Model | Definition | Current status |
| --- | --- | --- |
| A — RBAC | Role-based deterministic authorization baseline | **IMPLEMENTED** |
| B — RBAC + ABAC | Role authorization augmented with resource/request/context attributes | **IMPLEMENTED** |
| C — Deterministic contextual risk | RBAC + ABAC + deterministic contextual risk evaluation | **IMPLEMENTED** |
| D — Policy-bounded behavioral adaptive authorization | Behavioral and operational risk evidence, evolving state, deterministic policy constraints, bounded responses and runtime feedback | **PLANNED RESEARCH** |

IMPLEMENTED denotes repository behavior supported by tests, not demonstrated
research effectiveness or production readiness. A–C and their synthetic smoke
usage are **EXPERIMENTAL** software baselines.

## Model A

[RbacAuthorizationEngine](../../../src/AegisAI.Application/Authorization/RbacAuthorizationEngine.cs)
uses issuer-qualified subjects, server-owned role assignments and exact
resource/action permissions. Role grants combine by union; absent grants deny.
Outcomes are ALLOW/DENY. See [SPEC-005](../specs/SPEC-005-RBAC.md) and
[unit tests](../../../tests/AegisAI.Application.Tests/RbacAuthorizationTests.cs).

## Model B

[ModelBAuthorizationEngine](../../../src/AegisAI.Application/Authorization/ModelBAuthorizationEngine.cs)
requires RBAC to allow before evaluating ABAC. All applicable equality conditions
must pass; missing attributes or rules deny. Subject/resource/environment values
are static server snapshots; the requested action is request-bound. This limited
ABAC language does not represent every ABAC implementation. See
[SPEC-006](../specs/SPEC-006-ABAC.md) and
[unit tests](../../../tests/AegisAI.Application.Tests/AbacEvaluationTests.cs).

## Model C

[ModelCAuthorizationEngine](../../../src/AegisAI.Application/Authorization/ModelCAuthorizationEngine.cs)
requires Model B to allow before contextual scoring. The fixed weighted heuristic
uses AuthenticationWeakness, ResourceSensitivity, OperationSensitivity,
NetworkExposure, and DeviceExposure. These are configured simulation indicators,
not verified live measurements or operational dependency-health inputs.

Missing indicators yield Unknown and DENY; weights and thresholds are uncalibrated.
Complete scores map to ALLOW below 0.25, STEP_UP below 0.50, LIMIT below 0.75,
and DENY otherwise. STEP_UP and LIMIT carry obligations, not enforcement.
See [SPEC-007](../specs/SPEC-007-CONTEXTUAL-RISK.md),
[ContextualRiskEngine](../../../src/AegisAI.Application/Authorization/ContextualRiskEngine.cs),
and [unit tests](../../../tests/AegisAI.Application.Tests/ContextualRiskTests.cs).

## Model D — proposed future scope

Model D is intended to incorporate behavioral evidence, anomaly detection,
evolving risk state, operational-system state, deterministic policy constraints,
bounded adaptive responses, and feedback from runtime outcomes. Probabilistic
intelligence informs risk evaluation without unrestricted enforcement authority.
Mandatory authorization denials must remain final. Missing or stale evidence
must have explicit policy handling; it must not silently become low risk.

AEGISAI-012 supersedes the earlier research-only definition that required D to
replace C's contextual scorer and separately name every combined scorer. D now
names the proposed combined policy-bounded adaptive architecture. The exact
contextual/behavioral combination, algorithm, features, state updates and thresholds
remain to be specified and versioned before experiments. C's implementation and
existing artifacts retain their original meaning; they are not relabeled as D.

## Comparison boundaries

The [current runner](../../../benchmarks/AegisAI.Experiments/Experiment.cs) executes
only A–C with paired seeded synthetic workloads. It has no D implementation,
independent attack labels, dynamic behavior or operational-state injection. The
API endpoints evaluate decisions only and do not perform protected operations.
STEP-UP is research prose for the existing code/JSON outcome STEP_UP.

Future protocols must hold identity, resource semantics, shared policies,
workload, instrumentation and resource budgets constant where applicable, and
record intentionally different inputs. H1 requires a behavioral-evidence ablation
in addition to A–D comparisons to isolate the behavioral effect. H2 requires a
separately named direct-ML-enforcement comparator using the same predictor and
inputs with an explicit action mapping. That comparator is **PLANNED**, outside
Model D, and limited to an isolated synthetic test environment. H3 requires
matched D variants with and without operational state. A–D comparison alone cannot
attribute a difference to one added component. H4 requires matched measurement
boundaries and declared budgets. No comparator is implemented by this definition.
