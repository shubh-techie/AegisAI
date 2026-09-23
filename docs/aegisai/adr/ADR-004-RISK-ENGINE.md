# ADR-004 — Deterministic Contextual Risk Engine

Date: 2026-09-23  
Status: Accepted for AEGISAI-007; implementation pending review

## Context

Model C adds contextual risk to independently testable RBAC/ABAC baselines.
The project has no trusted live device/network/history sources or measured training
data. The next research baseline needs transparent, reproducible calculations and
explicit handling of incomplete context, not invented evidence.

## Decision

Implement an Application risk engine behind IContextualRiskEngine. Use five
normalized indicators and a fixed decimal weighted sum, with versioned weights
and thresholds documented in [SPEC-007](../specs/SPEC-007-CONTEXTUAL-RISK.md).
This is a deterministic heuristic, not AI or ML. Report every weighted contribution,
including zero and missing values, plus risk score, level, and version metadata.

Use trusted static simulation contexts bound to issuer/subject/resource/action
behind IRiskContextProvider. Domain validates [0,1] inputs. Infrastructure holds
snapshots; Api owns binding and HTTP serialization. No caller risk inputs are
accepted. Do not use unavailable recent authorization history.

Model C invokes the existing Model B abstraction first. A non-ALLOW baseline
short-circuits risk assessment and denies. Missing context is conservatively
imputed for explainable arithmetic but remains explicitly Unknown and denies.
Provider failure is an error, never an implicit low score.

Extend authorization outcomes with STEP_UP and LIMIT while retaining Model A/B
behavior. Fixed score thresholds map to ALLOW, STEP_UP, LIMIT, DENY. Return explicit
stronger-authentication/reevaluation or rate-cap obligations. No protected operation
is executed and no challenge/rate enforcement is claimed. A future enforcement
adapter must reject unsupported or unsatisfied obligations.

## Alternatives

A learned model would require evidence and evaluation outside this task. Treating
missing values as zero would incorrectly suggest low risk. Allowing a risk score
to override a role/attribute denial would weaken mandatory policy. A single shared
mutable context would undermine request binding and reproducibility. These options
are not adopted.

## Consequences

IMPLEMENTED: explainable normalized scores, conservative missing-context handling,
independent risk tests, and four outcome recommendations in the experimental
Model C endpoint. Fixed weights are easy to reproduce but uncalibrated; no efficacy
or performance claim follows from correctness tests. Scores with Unknown level
are imputed assessments, not measurements. Explicit fixture values must not be
misrepresented as verified authentication or device posture.

PLANNED: trusted live evidence/provenance and freshness, empirical threshold
assessment, history collection if justified, enforcement, audit, and research
instrumentation. EXPERIMENTAL: Models A/B/C software only. Changes to this version's
weights, signal meanings, or outcome thresholds require a versioned specification
and updated boundary tests rather than silent baseline changes.
