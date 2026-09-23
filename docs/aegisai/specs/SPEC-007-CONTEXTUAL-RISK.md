# SPEC-007 — Model C: Deterministic Contextual Risk

Date: 2026-09-23  
Status: IMPLEMENTED experimental software; pending review

## Scope

Model C = RBAC + ABAC + contextual risk. A separate deterministic engine adds a
weighted risk assessment only after Model B allows. Models A/B retain their
existing algorithms and endpoints. This is a hand-specified heuristic, not AI,
machine learning, learned behavior, or a calibrated probability of attack.

IMPLEMENTED: normalized scoring, explanations, four outcome recommendations, a
trusted static context provider, and comparison tests. EXPERIMENTAL: this software
baseline only. No research runs, performance measurements, or effectiveness claims
are made. [ADR-004](../adr/ADR-004-RISK-ENGINE.md) records the design decision.

## Inputs and trust

RiskContext is an immutable Domain snapshot containing a version and normalized
decimal indicators. Infrastructure resolves a context for the exact authenticated
issuer/subject, resource, and action tuple. No client risk scores, attribute bags,
headers, or token role claims are accepted as risk evidence. A missing tuple
returns an empty context, never another subject's or action's context.

Current values are explicit server-owned simulation fixtures. They are not
measurements of authentication assurance, resource metadata, network safety, or
device posture. Each value lies in [0, 1], where 0 is the least contribution and 1
the maximum contribution for that indicator. Values outside the interval or invalid
signal IDs fail context construction; they are not silently clamped. Intermediate
values are permitted for deterministic scenario and boundary testing.

| Signal | Weight | Meaning of fixture endpoints (0 → 1) |
| --- | --- | --- |
| AuthenticationWeakness | 0.25 | Stronger → weaker authentication assurance |
| ResourceSensitivity | 0.25 | Lower → higher resource sensitivity |
| OperationSensitivity | 0.20 | Lower → higher operation sensitivity |
| NetworkExposure | 0.15 | Lower → higher simulated network exposure |
| DeviceExposure | 0.15 | Lower → higher simulated device exposure |

There is no hidden conversion from actual JWT claims or telemetry to these values.
The configuration example is synthetic and must not be represented as verified
context. A future production adapter must specify and test that conversion,
provenance, freshness, and assurance validation. Recent authorization behavior is
not used because there is no trusted history store; no history is fabricated.

## Exact scoring algorithm

Algorithm version: `contextual-weighted-v1`.

For each of the five signals in table order:

1. Use its supplied decimal value x in [0, 1].
2. If it is absent, use x = 1 only for conservative arithmetic and mark the actual
   value null with reason `MissingContextConservativeMaximum`.
3. Compute contribution c = weight × x. Supplied values have reason
   `ConfiguredIndicator`, including zero contributions.
4. Score = sum of the five contributions. Weights sum to exactly 1; therefore the
   decimal score is normalized to [0, 1]. No rounding, rescaling, stochastic step,
   training, or model fitting is performed.

Every assessment includes score, level, algorithm version, context version, and
all five contributions (signal name, actual nullable value, weight, contribution,
and reason). This allows the score to be recomputed from the response.

If any signal is absent, level = Unknown regardless of numeric score. The score
then represents conservative imputation, not known risk. Unknown is never Low and
always causes DENY. Entirely missing context scores 1 with five missing reasons.
For complete context:

| Score interval | Level |
| --- | --- |
| 0 ≤ score < 0.25 | Low |
| 0.25 ≤ score < 0.50 | Medium |
| 0.50 ≤ score ≤ 1 | High |

Illustrative arithmetic only: authentication 1, resource 0.5, operation 0, network
1, and device 0 gives 0.25 + 0.125 + 0 + 0.15 + 0 = 0.525 (High). This is not a
research result. The chosen weights and thresholds are design parameters, not
empirically validated security claims.

## Model C composition and outcomes

1. Evaluate Model B once for the original request.
2. If it does not allow, return DENY / BaselineDenied, with risk null (not
   evaluated). Do not call the risk context provider or engine. RBAC/ABAC rejection
   can never be overridden by a low score.
3. Otherwise resolve one risk context and assess it.
4. Unknown context returns DENY / MissingRiskContext.
5. For complete context apply these exact fixed thresholds:

| Score interval | Outcome | Reason / obligation |
| --- | --- | --- |
| [0, 0.25) | ALLOW | LowContextualRisk; no additional obligation |
| [0.25, 0.50) | STEP_UP | AdditionalAssuranceRequired; VerifyStrongerAuthenticationAndReevaluate |
| [0.50, 0.75) | LIMIT | RateRestrictionRequired; EnforcePerSubjectResourceActionRateLimit, maximum 10 requests/minute |
| [0.75, 1] | DENY | HighContextualRisk; no access |

At exactly 0.25, 0.50, and 0.75 the more restrictive interval applies. High is
split between LIMIT and DENY; the outcome mapping is separate from the three
known risk levels. The 10 requests/minute limit is an explicit experimental
obligation, not a measured capacity or recommended production setting.

STEP_UP requires verified stronger authentication and a new evaluation before
access; no challenge is implemented. LIMIT requires a future enforcement adapter
to apply the stated rate cap to the authenticated subject/resource/action before
access. Unsupported or failed obligations must deny. These decision endpoints
execute no protected action, and no rate limiter, challenge handler, or access
capability is implemented. Callers must never treat every non-DENY result as an
unconditional grant. Provider errors propagate as operational failures, not ALLOW
and not a completed risk result. No fabricated result is emitted after an error.

## API

`POST /authorization/evaluate/model-c` uses the same authenticated request contract
as Models A/B:

```json
{"resource":"reports","action":"read"}
```

Only resource/action are allowed, each nonblank and at most 256 characters. Missing
or invalid credentials yield 401; invalid/extra body fields yield 400. Completed
evaluations return 200 for every outcome because this is a decision query.
Unsupported content type yields 415; operational failure is a 5xx/error, not an
ALLOW. `/health`, `/identity`, and the Model A/B endpoints retain their behavior.

The response contains model C, outcome, reason, resource/action, RBAC and ABAC
stage evidence and versions, a nullable risk assessment, and a nullable obligation.
Risk is null only when the baseline blocks evaluation. Evaluated missing context
produces an Unknown risk result with a numeric conservative score and reasons.
Enum values serialize as named strings. Signal values are exposed for controlled
research explainability; use only nonsensitive simulated fixtures on this endpoint.
Do not deploy it as a public context-disclosure or production enforcement API.

## Configuration and layers

Domain owns RiskSignal and validated RiskContext. Application owns
IContextualRiskEngine, IRiskContextProvider, the risk result/contribution types,
and Model C orchestration. Infrastructure stores immutable request-bound contexts;
Api binds configuration, composes services, and serializes responses. Inner layers
have no HTTP or identity-provider dependency.

`src/AegisAI.Api/appsettings.example.json` includes a synthetic ContextualRisk
section. As before, the example file is not automatically loaded. Configure:

- ContextualRisk:Version — required when configured; covers all fixture inputs.
- ContextualRisk:Contexts — entries with Subject, Issuer, Resource, Action, Signals.
- Signals — a dictionary of the five named indicators and decimal values.

Example environment key:
`ContextualRisk__Contexts__0__Signals__NetworkExposure`.
Omitted, null, or blank signal values remain unknown. Nonblank numeric values use
invariant decimal syntax; malformed numbers fail startup. Absent ContextualRisk configuration
uses an empty `unconfigured` snapshot and Model C denies after any baseline grant.
Invalid numeric values, defined out-of-range indicators, duplicate request tuples,
and malformed configuration fail startup; no partial serving occurs. Contexts are
frozen after host configuration is finalized and before requests are served.
Changing them requires restart. There is no atomic distributed update with RBAC/ABAC;
record all reported versions and exact configuration for reproducibility.

## Verification

Tests cover zero/maximum risk; low, medium, and high levels; exact thresholds and
values immediately below them; weighted explanations and repeatability; each
missing indicator and entirely missing context; invalid ranges and identifiers;
immutable inputs; provider failures; and RBAC/ABAC denial short-circuiting.
JWT integration tests cover all four outcomes, unchanged Model B grants, missing
and null context, issuer/action binding, baseline rejection, input injection,
authentication, and invalid configuration. These are correctness tests, not
benchmarks or experimental findings.

## Limitations

This heuristic is deliberately transparent and uncalibrated. It cannot establish
attack probability, detect behavioral anomalies, or verify real devices/networks.
No historical behavior collector, live metadata adapter, telemetry freshness,
challenge execution, rate enforcement, audit persistence, or research measurement
harness exists. Model D and production enforcement remain PLANNED. Existing
[system architecture](../architecture/SYSTEM_ARCHITECTURE.md) controls must not be
inferred to be complete from these software tests.
