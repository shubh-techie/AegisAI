# Research problem

Date: 2026-09-25 | Baseline: AEGISAI-012
Status: **IMPLEMENTED** research-definition documentation; investigation **PLANNED**.

The research umbrella is **AI-Driven Automation for Resilient & Secure
Cloud/Distributed Systems**. AegisAI — Adaptive Intelligence for Secure
Distributed Systems — is the experimental research platform within that program.
Its focus is **explainable, policy-bounded, adaptive security for cloud-native
distributed systems**.

Traditional authorization approaches such as RBAC and ABAC provide important
deterministic access-control mechanisms. Runtime conditions in distributed
cloud-native environments can nevertheless change, including:

- Behavioral deviations and unusual API sequences.
- Abnormal request velocity and changes in authentication assurance.
- Sensitive-resource access.
- Service degradation, dependency instability, and operational anomalies.

A valid identity and valid static authorization state therefore do not necessarily
imply that runtime risk remains unchanged. This does not imply that RBAC or ABAC
cannot incorporate changing evidence; AegisAI's current implementations use static
snapshots and are deliberately limited experimental baselines.

Allowing probabilistic AI/ML predictions to directly control authorization also
raises questions about false positives, false negatives, explainability, unstable
decisions, inappropriate autonomous actions, security-policy bypass, and
operational disruption. More restrictive decisions alone do not establish better
security: legitimate access and recovery can also suffer.

AegisAI investigates whether behavioral and operational intelligence can augment
deterministic authorization while retaining explicit policy boundaries. This
problem is not claimed to be an invention or an established research contribution.
The [proposed contribution](PROPOSED_CONTRIBUTION.md) is subject to literature and
prior-art review. No effectiveness result follows from defining the problem.

See the [research question](RESEARCH_QUESTION.md), [model definitions](RESEARCH_MODELS.md),
and [claims register](CLAIMS_REGISTER.md). The current platform is a single API
and an in-process runner, not an evaluated distributed enforcement deployment.
