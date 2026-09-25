# Research Scope

AegisAI explores adaptive intelligence for secure distributed systems.

Research theme:

> AI-Driven Automation for Resilient & Secure Cloud/Distributed Systems

Tagline:

> Observe. Assess. Authorize. Respond.

## Purpose

AegisAI is intended to become an engineering and research platform for:

- Adaptive authorization
- Contextual risk assessment
- Behavioral anomaly detection
- Zero Trust security
- Policy-controlled automated threat response
- Secure telemetry and audit trails for distributed systems

The legacy GloboTicket application is preserved in Git only and will not be reused.
**IMPLEMENTED**: Models A/B/C, authentication, best-effort audit, and supporting tests.
Model C is deterministic contextual risk, not learned AI; see
[SPEC-007](specs/SPEC-007-CONTEXTUAL-RISK.md). Behavioral/AI Model D remains
**PLANNED**. The research runner and generated synthetic smoke artifacts are
**EXPERIMENTAL**; see [SPEC-009](specs/SPEC-009-EXPERIMENT-FRAMEWORK.md).

## Research Questions

AEGISAI-012 establishes the primary question:

> How does explainable, policy-bounded, closed-loop adaptive authorization perform relative to RBAC, ABAC, and deterministic risk-aware authorization under behavioral and operational anomalies in cloud-native distributed systems?

[RQ1–RQ4](research/RESEARCH_QUESTION.md) cover security effectiveness, policy safety,
resilience and system cost. [H1–H4](research/RESEARCH_HYPOTHESES.md) remain unproven.
The [research program](research/RESEARCH_PROGRAM.md) indexes the problem, proposed
contribution, conceptual model, variables, invention candidates and integrity rules.
Consult the [claims register](research/CLAIMS_REGISTER.md) before public claims.

## Planned Experimental Models

AegisAI should eventually support reproducible experiments comparing:

- Model A: RBAC
- Model B: RBAC + ABAC
- Model C: RBAC + ABAC + contextual risk
- Model D: policy-bounded behavioral adaptive authorization — PLANNED RESEARCH

Model A is implemented in [SPEC-005](specs/SPEC-005-RBAC.md), Model B in
[SPEC-006](specs/SPEC-006-ABAC.md), and Model C in
[SPEC-007](specs/SPEC-007-CONTEXTUAL-RISK.md). Model D remains PLANNED.
Existing A–C smoke measurements validate the experimental framework only.

## Candidate Measurements

Potential measurements:

- Authorization latency
- Throughput
- False-positive rate
- False-negative rate
- Anomaly detection effectiveness
- Risk classification quality
- Incident detection time
- Incident response time
- Policy decision explainability
- Operational overhead

Synthetic engine-only smoke results exist under research/results with methodology,
environment metadata, and source provenance. They do not establish superiority,
production performance, or security effectiveness. Future results must retain the
same evidence and clearly state limitations.

## Experiment Hygiene

Future experiments should be reproducible:

- Keep datasets versioned or clearly referenced.
- Separate synthetic data from real telemetry.
- Never commit secrets or sensitive production logs.
- Record runtime versions, configuration, workload shape, and hardware/cloud environment.
- Keep raw results under `research/results/` or `benchmarks/` with metadata.
- Document known threats to validity.

## Ethical and Security Boundaries

AegisAI should avoid overstating AI capability. Research outputs should identify:

- Where a model is heuristic rather than learned.
- Where a signal is noisy or biased.
- Where automated response could disrupt legitimate users.
- Where human review is required.
- Where test data does not represent production behavior.

## Detailed experimental plan

[Research Architecture](architecture/RESEARCH_ARCHITECTURE.md) defines controlled
comparisons, metric boundaries, independent labels, and future behavioral analysis.
The full research protocol remains planned; the narrower engine-only smoke
framework and its generated artifacts are EXPERIMENTAL.
