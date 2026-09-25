# ADR-006 — Policy-Bounded Adaptive Security Decision Architecture

Date: 2026-09-25

Status: Accepted as research architecture / planned Model D boundary

Recorded by: AEGISAI-012-DOC-COMPLETION; documentation pending review

## Context

AegisAI currently implements deterministic authorization Models A–C. Future Model D
proposes behavioral intelligence. Allowing probabilistic models to directly grant
or deny access could create false-positive enforcement, false-negative enforcement,
policy bypass, explainability problems, unstable autonomous actions and operational
disruption. The canonical [research problem](../research/RESEARCH_PROBLEM.md) and
[research question](../research/RESEARCH_QUESTION.md) define the investigation.

[ADR-003](ADR-003-POLICY-EVALUATION.md) records RBAC/ABAC composition;
[ADR-004](ADR-004-RISK-ENGINE.md) records deterministic Model C and deny precedence.
Neither records the authority of future probabilistic behavioral components.
[ADR-005](ADR-005-LOCAL-DEVELOPMENT-TRUST.md), the preceding ADR, concerns local JWT
trust. A separate ADR is warranted for this planned boundary; it does not change
those accepted baseline decisions or introduce a different research model.

## Decision

Behavioral/AI components will produce risk evidence. They will **not** possess
unrestricted enforcement authority. Deterministic policy remains the enforcement
authority: it evaluates risk evidence through explicit policy constraints and
selects permitted actions. Probabilistic output cannot directly authorize or
execute a grant, denial or restriction, override mandatory RBAC/ABAC rejection,
or rewrite policy. Enforcement adapters must apply the policy decision and its
obligations at the protected-resource boundary.

Permitted bounded outcomes may include ALLOW, STEP-UP, LIMIT and DENY, depending
on explicit policy. STEP-UP corresponds to STEP_UP in existing code/JSON. Its
assurance/reevaluation obligations and LIMIT restrictions require actual enforcement;
a recommendation alone is not a protected action. Missing or stale evidence must
receive explicit policy handling rather than silently becoming low risk.

Runtime outcomes may later participate in closed-loop risk-state evaluation.
That functionality remains **PLANNED RESEARCH** until implemented and validated
within its stated scope. It does not imply automatic retraining or policy rewriting.
The [conceptual model](../research/CONCEPTUAL_MODEL.md) and
[research models](../research/RESEARCH_MODELS.md) remain authoritative for scope;
this ADR specifies no new feature, scoring method, threshold or contribution.

## Implemented and planned behavior

**IMPLEMENTED**: A–C deterministic decision engines, Model C score contributions
and mandatory baseline-denial precedence. The endpoints are decision queries;
STEP_UP/LIMIT obligations are not enforced. **EXPERIMENTAL**: existing A–C
synthetic fixtures and engine-only smoke measurements.

**PLANNED RESEARCH**: probabilistic behavioral evidence, Model D arbitration,
operational-state integration, protected-resource enforcement and runtime feedback.
No Model D or production behavior is implemented or changed by this ADR.

## Consequences

Intended positive consequences, to be evaluated rather than claimed as measured:

- An explicit deterministic enforcement boundary.
- Improved auditability through recorded evidence and policy decisions.
- Clearer separation between AI evidence and policy authority.
- Safer experimentation through bounded responses.
- Explicit control over autonomous actions.

Potential trade-offs:

- Additional policy complexity.
- Potential latency.
- Policy/AI disagreement.
- Risk of overly conservative policies.
- Additional observability requirements.

These are design expectations and risks. They do not establish that the approach
improves security, resilience, performance or safety. H1–H4 remain unvalidated;
see [canonical hypotheses](../research/RESEARCH_HYPOTHESES.md).

## Alternatives considered

1. Direct ML enforcement: rejected as the research architecture because probabilistic
   output would directly control security enforcement. The separately named,
   isolated experimental comparator already proposed for H2 remains planned;
   its use for comparison would not grant Model D unrestricted authority.
2. Deterministic-only authorization: retained as Models A–C baselines but insufficient
   for testing the behavioral-adaptive research hypothesis.
3. Human-only intervention: useful for high-impact responses but insufficient for
   evaluating bounded automated responses. This decision does not exclude human
   review where explicit policy requires it.

## Research status and provenance

This is a research architecture decision, not evidence validating the
[proposed contribution](../research/PROPOSED_CONTRIBUTION.md). That contribution
remains planned and unverified. Novelty, patentability and experimental superiority
are not established. The [claims register](../research/CLAIMS_REGISTER.md) restricts
claims, and [research integrity](../research/RESEARCH_INTEGRITY.md) governs later
protocols, revisions and evidence preservation.

The canonical AEGISAI-012 definitions were established in `c8e6112`, merged in
`ff37d4b`, and preserved by `research-baseline-v0.1` (annotated tag object
`e34b35e5198835a82e08e6008df713697d769e14`). They precede literature-gap analysis,
Model D implementation, A/B/C/D comparative experiments and results from those
investigations. Existing synthetic A–C smoke results predate that definition;
they are not evidence validating H1–H4.

ADR-006 and [SPEC-012](../specs/SPEC-012-RESEARCH-DEFINITION.md) are completed
**after** the original baseline commit/tag. Neither existed in that commit or
tagged state. This records the existing research boundary after the milestone;
it does not backdate the ADR or alter the canonical research definitions.
Literature/prior-art analysis remains the next research gate before stronger
claims or Model D implementation/controlled experimentation. The baseline tag
and all historical commits remain untouched.
