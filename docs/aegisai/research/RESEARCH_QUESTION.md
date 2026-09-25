# Research question

Date: 2026-09-25 | Baseline: AEGISAI-012
Status: **PLANNED RESEARCH**; questions are unanswered.

## Primary research question

> How does explainable, policy-bounded, closed-loop adaptive authorization perform relative to RBAC, ABAC, and deterministic risk-aware authorization under behavioral and operational anomalies in cloud-native distributed systems?

## RQ1 — Security effectiveness

Does incorporating behavioral risk evidence improve detection and handling of
abnormal or compromised access compared with deterministic authorization baselines?

## RQ2 — Policy safety

Does policy-bounded behavioral intelligence reduce inappropriate security actions
compared with allowing machine-learning predictions to directly control enforcement?

## RQ3 — Resilience

Does incorporating operational-system state into security decisions improve
containment or adaptive protection during degraded or adversarial
distributed-system conditions?

## RQ4 — System cost

What latency, throughput, resource, stability, and false-positive costs are
introduced by adaptive authorization?

[H1–H4](RESEARCH_HYPOTHESES.md) correspond respectively to RQ1–RQ4. In the initial
comparison, ABAC means Model B's RBAC + ABAC composition, not a standalone ABAC
implementation or every possible ABAC system. [Models A–D](RESEARCH_MODELS.md)
define the comparison scope. No question is answered by existing smoke artifacts
or software correctness tests.
