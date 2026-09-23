# ADR-003 — Independent RBAC and ABAC Evaluation

Date: 2026-09-23  
Status: Accepted for AEGISAI-006; implementation pending review

## Context

Model A establishes independently testable RBAC. Model B must add attribute checks
without weakening role permissions, binding Application to HTTP, or inventing
research outcomes. Existing request identity is issuer-qualified and authenticated
outside Application. The evaluation endpoints perform no protected operations.

## Decision

Keep IAuthorizationEngine as Model A and introduce IAbacEvaluator for pure attribute
policy evaluation. ModelBAuthorizationEngine composes the two through abstractions:
RBAC first, ABAC only after RBAC allows. Both must allow. A denial is final.

Use an immutable AbacContext containing rules, subject/resource/environment
attributes, and a snapshot version. A separate IAbacContextProvider supplies trusted
evidence for the request. Infrastructure retains a static startup snapshot;
configuration binding and HTTP mapping stay in Api. Domain holds immutable attribute
sets, conditions, and targeted rules. No external dependencies enter the inner layers.

Start with exact ordinal string equality against literals. Rule targets match
resource/action exactly. Every condition of every applicable rule must pass.
No applicable rules and missing attributes deny. Reject empty conditions and invalid
policies. Missing evidence has stable precedence over mismatches. Requested action
is read from the request, never overridden by a supplied attribute dictionary.

Expose Model B on a separate authenticated decision endpoint. Keep Model A's contract
unchanged to support paired tests. Neither endpoint accepts caller-supplied attributes.
Return stage evidence and separate RBAC/ABAC versions; skipped ABAC is explicitly null.
Do not output sensitive attribute values. Preserve operator-owned configurations
for reproducibility; version labels alone do not prove identical policy contents.

## Alternatives considered

Combining RBAC and ABAC into one engine would obscure independent testing and
comparison. Accepting subject/environment attributes from clients would weaken
trust. Adopting a general expression engine or external policy service adds scope
and dependencies before requirements exist. Treating absent ABAC rules as ALLOW
would silently reduce Model B to Model A. These alternatives are not adopted.

## Consequences

IMPLEMENTED: deterministic deny-preserving composition and independently tested
stages. The language is intentionally restricted, and adding mandatory rules can
only constrain access further. Static configuration makes fixtures reproducible but
cannot demonstrate live context accuracy or revocation. All configured snapshots
are validated at startup; malformed ABAC configuration prevents the host from serving.

PLANNED: dynamic evidence sources with provenance/freshness, richer policy semantics,
audit, enforcement, and research instrumentation. Future semantics must receive
explicit specifications and tests rather than changing the baseline silently.
EXPERIMENTAL: software baselines A/B; no AI, anomaly detection, contextual risk
scoring, or research results. See [SPEC-006](../specs/SPEC-006-ABAC.md).
