# AegisAI V0.1 readiness report

Date: 2026-09-23  
Assessment: **Ready for code review as an experimental local development baseline.**
Docker startup and /health are now validated as recorded below. Full container
workflow acceptance remains separate. This is not a production-readiness or
security certification.

## Implemented capabilities and review

| Task | Current capability |
| --- | --- |
| 001 | Fresh repository foundation, agent rules, truthful 2022/2026 history and preserved legacy checkpoint |
| 002 | .NET 8 solution, nullable types, health endpoint, inward project references |
| 003 | Target system/research architecture, threat model and separate future behavioral pipeline |
| 004 | Standard signed JWT validation and Application-owned issuer-qualified identity |
| 005 | Model A: default-deny RBAC and authenticated decision endpoint |
| 006 | Model B: independent ABAC evaluator and deny-preserving composition |
| 007 | Model C: deterministic normalized risk with explanations and four outcomes |
| 008 | Structured minimized audit, correlation headers, ActivitySource/Meter instrumentation |
| 009 | Seeded synthetic A–C experiments, raw observations, summaries and source provenance |
| 010 | Docker files, root ./dev workflow, temporary Development-only credentials, Quick Start |
| 011 | CI for PRs targeting main and pushes to main; repository security review and local Release validation |

**IMPLEMENTED** describes repository code and documented validation. Models A–C,
static context fixtures, and measurements remain **EXPERIMENTAL**. Behavioral
AI/ML, Model D, live anomaly detection and detection-quality analysis are **PLANNED**.

## Validation evidence

- `dotnet build` (single MSBuild node, node reuse disabled): succeeded with zero
  warnings and errors on macOS arm64, SDK 8.0.204.
- `dotnet test`: **168 passed**, zero failures/skips: 14 Domain, 53 Application,
  93 integration, 4 architecture and 4 experiment tests. Twelve new cases cover
  local trust separation and real token validation. An initial valid-token test
  exposed early configuration capture; trust now resolves finalized configuration
  and is validated during startup. The complete suite passed after the fix.
- The generated credential and the actual API were exercised over loopback HTTP:
  health 200, anonymous evaluation 401, A/B ALLOW, C STEP_UP with score 0.300,
  forbidden write DENY. Four JSON authorization events matched the requests and
  correlation IDs; the generated token was absent from captured server logs.
  This was a local .NET host, not a container test. The temporary host was stopped.
- At AEGISAI-010, shell/Compose checks passed but Docker startup could not be
  validated because the daemon was unavailable. At AEGISAI-011 final verification,
  the user reported successful API container startup, listening on port 8080,
  and GET /health HTTP 200. A fresh ./dev health also passed with healthy JSON.
  The tools container reported a workload-verification warning. Full container
  demo/test/experiment acceptance was not rerun in this final task.
- AEGISAI-011 final verification ran the exact dotnet restore, dotnet build
  --configuration Release, and dotnet test --configuration Release commands:
  restore/build passed, zero build warnings/errors, 168 tests passed, zero
  failed/skipped. See [SPEC-011](specs/SPEC-011-CI-SECURITY.md).
- A separate local Release smoke experiment generated
  [actual artifacts](../../research/results/aegisai010-local-smoke/summary.json).
  This validates execution/provenance, not performance or production effectiveness.

## Architecture

Domain has no external dependencies; Application references Domain; Infrastructure
references Application/Domain; Api composes Application/Infrastructure. Authorization
rules remain in Application. Local JWT trust stays in Api, with offline issuance
in deployment tooling. No production project references DevTools or experiments.
The service topology is one API, with an on-demand SDK/Git tools container. No
broker, database, identity server, collector, or gateway was introduced.

## Known limitations

The user observed development Data Protection warnings about ephemeral/in-memory
key storage. No durable Data Protection key store is configured; protected data
depending on these keys may not survive a restart. Production key persistence,
protection, and sharing require separate design. This is distinct from JWT signing
trust and does not make the local Docker configuration production-ready.

Decision queries do not execute protected actions. STEP_UP and LIMIT obligations
are recommendations; no challenge or rate-cap enforcement exists. Policies and
attributes are startup snapshots; risk indicators are synthetic and thresholds
uncalibrated. There is no verified live posture, revocation, historical behavior
source, or behavioral model. Local credentials are short-lived synthetic bearer
tokens, not an OIDC deployment. Public exposure and production use are out of scope.

Audit is best-effort console output; it can be lost and does not gate actions.
Pseudonyms are not anonymization. Traces/metrics need a collector/exporter before
remote observation. Rejected authentication has no decision audit because evaluation
never ran. /health is liveness only. Architecture tests inspect declared dependencies,
not all possible semantic coupling. Docker images are servicing tags, not immutable
digests; NuGet restore is not fully locked. Normal clones are supported by the
container experiment metadata workflow; linked worktrees are not supported.

## Research capabilities

The same executable A–C engines run against eight explicitly synthetic scenarios,
with seeded order, warm-up exclusion, measured latency/throughput, decision counts,
error records and reproducible source/manifest hashes. Container and host timings
must be interpreted separately. Tiny smoke measurements support no broad performance
claims. No production data, trained model, independent benign/malicious labels,
FPR/FNR result, detection time or response effectiveness is claimed. Future analysis
interfaces do not constitute implemented detection or quality measurement.

## Next recommended work

1. Complete and record the remaining container demo/test/experiment acceptance;
   startup and /health alone establish liveness only.
2. Observe the updated CI on GitHub; container smoke CI remains future work.
   Consider reviewed image digests and package locks for stronger provenance.
3. Specify protected-resource enforcement with mandatory STEP_UP/LIMIT handling
   and durable audit acceptance before implementing real resource operations.
4. Validate a real identity provider and context provenance/freshness adapters.
5. Develop independently labeled research protocols before evaluating Model D;
   keep AI/ML **PLANNED** until implemented and supported by measured evidence.

No commit or push was performed. The legacy tag and historical commits are preserved.
