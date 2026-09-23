# SPEC-008 — Authorization Audit and Observability

Date: 2026-09-23  
Status: IMPLEMENTED best-effort decision observability; pending review

## Scope

Models A/B/C emit one structured audit event per completed authenticated, valid
HTTP evaluation. The API wraps the selected engine call; nested stages do not emit
duplicate events. Application owns AuthorizationAuditEvent and IAuthorizationAuditSink.
Api owns timing, tracing, pseudonymization, and the logging adapter. Domain and the
authorization algorithms remain unchanged. No Kafka, external transport, exporter,
database, durable queue, or additional dependency is introduced.

IMPLEMENTED: structured JSON console logging, an audit sink abstraction, tracing,
and counters/histograms. PLANNED: durable audit acceptance, retention controls,
protected action outcome/reconciliation, and deployment collection. EXPERIMENTAL:
authorization software baselines only; no performance measurements or research
claims are produced by this task.

## Event contract

| Field | Meaning |
| --- | --- |
| EventId | New generated UUID in compact form for this emission |
| CorrelationId | New generated UUID for this evaluation; returned as X-Correlation-ID |
| TraceId / SpanId | Current authorization activity identifiers, or hosting activity identifiers when no child is sampled; nullable with no activity |
| SubjectId | SHA-256 pseudonym of the issuer-qualified authenticated subject |
| ResourceId / ActionId | SHA-256 pseudonyms of the requested opaque identifiers |
| Model | A, B, or C |
| Decision | ALLOW, DENY, STEP_UP, or LIMIT |
| RiskScore / RiskLevel | Model C assessment when evaluated; otherwise null; Unknown retains its conservative score |
| Reasons | RBAC reason, ABAC reason when evaluated, and Model C reason when applicable |
| Timestamp | UTC DateTimeOffset captured after the decision completes |
| ProcessingDurationMs | Monotonic elapsed milliseconds around the engine call |

Duration excludes HTTP binding/authentication, audit serialization/sink work,
response serialization, and protected action enforcement. The trace span surrounds
evaluation plus observation, so its duration may exceed ProcessingDurationMs.
These are operational measurements, not published benchmark claims. Sink latency
can add overhead; no zero-overhead or performance guarantee is asserted.

Correlation is generated internally, never copied from X-Correlation-ID or other
caller headers. Hosting W3C trace context can correlate services, but is metadata,
not an authentication or authorization input. Event IDs are not persisted for
retry/deduplication; there is one write attempt and no automatic retry.

## Data minimization

Only explicitly selected event fields are sent to the audit sink. Do not serialize
HttpContext, claims, request bodies, headers, tokens, cookies, credentials, risk
signal values, attribute bags, policy content, or exception messages. Known reasons
are stable engine codes; unknown Model C reason text becomes Other.

Subject identifiers are issuer-qualified before hashing. Pseudonyms are lowercase
SHA-256 hex prefixed with `sha256:` over UTF-8 length-prefixed components. Subject
components are issuer then subject ID; resource/action each have one component.
Lengths are .NET string lengths. This also prevents raw secrets accidentally placed
in arbitrary resource/action strings from entering audit logs. Raw identifiers are
not included alongside their pseudonyms. Pseudonyms are deterministic for correlation,
not anonymization: low-entropy identifiers remain susceptible to guessing. Restrict
log access and define retention before handling real identities.

JSON console formatting is enabled with scopes disabled. EventId 8001 / name
AuthorizationDecision uses named structured fields; Reasons is comma-joined in the
logging adapter while the sink contract retains a list. There are no custom free-text
messages containing request data. Tests use canaries in headers, tokens, identifiers,
and exceptions to verify the fields emitted by this instrumentation.

This task's exclusion guarantee applies to the new audit/authorization instrumentation.
Existing framework hosting/authentication logs and future third-party exporters must
be reviewed independently before deployment; do not enable raw request/header or
PII logging. The code does not propagate baggage into custom tags or events.

## OpenTelemetry-compatible instrumentation

ActivitySource and Meter name: `AegisAI.Authorization`, version `1.0.0`.
The custom span is `authorization.evaluate` (Internal), parented to the current
hosting activity when available. Tags are model, decision, risk level, and generated
correlation ID. No subject/resource/action values or pseudonyms appear in metric
tags. Authorization DENY is a successful evaluation, so its span status is Ok;
an engine exception marks Error without recording its message or stack.

| Instrument | Type / unit | Dimensions |
| --- | --- | --- |
| aegisai.authorization.decisions | Counter / decision | model, decision |
| aegisai.authorization.duration | Histogram / ms | model, decision |
| aegisai.authorization.audit.failures | Counter / failure | model |
| aegisai.authorization.errors | Counter / error | model |

No identifiers, versions, reasons, or arbitrary strings are metric dimensions.
These low-cardinality dimensions are bounded by the implemented models/outcomes.
An OpenTelemetry collector host can subscribe with AddSource("AegisAI.Authorization")
and AddMeter("AegisAI.Authorization") in its SDK configuration. No SDK/exporter or
remote destination is installed/configured by this task. Activities require a
listener/sampler; metrics require collection. Console audit emission is independent
of trace sampling. See the official [.NET observability documentation](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/observability-with-otel)
and [OpenTelemetry instrumentation documentation](https://opentelemetry.io/docs/languages/dotnet/instrumentation/).

## Behavior and failure semantics

Observation returns the original decision object. It cannot override engine
outcomes, add permissions, or satisfy STEP_UP/LIMIT obligations. Audit sink or
telemetry listener exceptions are isolated; a failed sink attempts to increment
an audit-failure counter without logging exception contents. Operational engine
exceptions propagate unchanged and emit no fabricated decision event. A separate
error counter/span status records that an evaluation failed.

Invalid input (400/415), unauthenticated requests (401), health checks, and identity
queries do not produce authorization decision events because no engine decision
exists. Authentication rejection auditing is separate future work. In-process
engine calls outside the HTTP observation wrapper do not automatically emit events.
The caller-facing response body/status stays unchanged; evaluated requests receive
an additional generated correlation header.

The logging sink is synchronous and best-effort. Logger filtering, process failure,
console buffer loss, disabled collectors, or a failing sink may lose events; a
blocking custom sink can delay responses. There is no durable acceptance, delivery
acknowledgement, retry, bounded queue, or guarantee of exactly-once storage.
This deliberately preserves behavior of decision-query endpoints, which execute
no protected actions. It does not fulfill the target architecture's future durable
pre-action audit requirement. Before implementing real resource execution, define
a durable sink and an explicit fail-closed enforcement policy as separate work.

## Validation

Tests verify one event per selected model across ALLOW/DENY/STEP_UP/LIMIT, optional
risk fields, reason codes, subject/resource/action pseudonyms, UTC timestamp bounds,
nonnegative monotonic duration, generated correlation response headers, and absence
of tokens/canaries. Listener tests collect real ActivitySource/Meter output and
verify trace correlation and bounded metric tags. Logger tests inspect structured
state. Fault tests confirm a throwing sink preserves the HTTP decision and an
engine exception propagates unchanged without fabricating an event. Existing
policy/identity/architecture suites verify unchanged authorization behavior.

Build and run all tests with dotnet build and dotnet test. No tests are presented
as evidence of throughput, latency improvement, durable delivery, or production
security effectiveness.
