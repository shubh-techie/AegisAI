# SPEC-010 — V0.1 reproducible development environment

Date: 2026-09-23  
Status: **IMPLEMENTED** workflow and local trust adapter; **EXPERIMENTAL** synthetic demo.
Container runtime validation status is recorded in the development log and readiness report.

## Scope and review of AEGISAI-001 through AEGISAI-009

The 2022 project remains at securing-microservices-legacy; AegisAI begins in 2026.
The current .NET 8 solution has inward project dependencies, standard JWT
validation, provider-neutral identity, independently testable RBAC/ABAC, a
transparent deterministic risk scorer, best-effort structured audit, and a seeded
engine-only experiment runner. Earlier task specifications are historical records;
this document and the readiness report describe the combined V0.1 state.

This task packages those components for local use. No database, gateway, Kafka,
identity-provider server, telemetry collector, or Kubernetes deployment is needed.
Behavioral AI/ML, Model D, protected resource enforcement, durable audit, and
production identity-provider integration remain **PLANNED**.

## Commands and prerequisites

Use a normal Git clone (not a linked worktree or source archive), a POSIX shell,
and Docker Engine/Desktop with Compose and a running Linux container daemon.
Initial builds require access to Microsoft images, Debian packages, and NuGet.
No host .NET SDK, Python, curl, signing tool, or production credentials are needed.

From the repository root:

```sh
./dev up
./dev health
./dev demo
./dev logs
# Ctrl-C stops log following only.
./dev test
./dev experiment my-v01-smoke
./dev down
```

`up` builds both images, generates new synthetic trust, recreates the API, and
polls /health with a bounded retry loop. It invalidates previous local credentials.
Credentials expire after one hour; rerun `up` to regenerate them and restart the
API. `AEGISAI_PORT=5081 ./dev up` changes the default host port 5080. Internal tools
use api:8080. Host port binding is IPv4 loopback only.

`health` sends GET /health and verifies HTTP success. An optional host call is
`curl --fail http://127.0.0.1:5080/health`; expected body is {"status":"healthy"}.
This is process liveness, not production readiness.

`demo` first checks that anonymous evaluation gets 401, then uses the generated
credential to POST {"resource":"reports","action":"read"} to:

| Endpoint | Synthetic fixture assertion |
| --- | --- |
| /authorization/evaluate | A: ALLOW |
| /authorization/evaluate/model-b | B: ALLOW |
| /authorization/evaluate/model-c | C: STEP_UP, score 0.300 |

It also checks that Model A denies writing reports. The score follows the existing
example's five configured indicators and SPEC-007 weights; these expectations are
correctness assertions, not research results. Full response bodies show stage
reasons, policy versions, risk contributions, and obligations where applicable.
The helper prints X-Correlation-ID but never the token. `logs` shows JSON audit
EventId 8001 / AuthorizationDecision, with corresponding correlation, model,
decision, reason codes, pseudonyms, timestamps, and processing durations.
No token, private key, or request body is added to audit events.

`test` runs the entire solution in Release with restore/build. `experiment` runs
the existing Release runner separately with the synthetic smoke-v1 manifest;
choose a new name for every run. Generated files persist under research/results
on the host. Models A/B/C and measurement boundaries are unchanged; see
[research/README](../../../research/README.md). Containers add scheduling/resource
variability. Do not compare their tiny smoke timings as evidence of superiority.

`down` removes the local service/network; generated credentials remain in ignored
deployment/docker/.local. Delete that directory after stopping to discard them.
Experiment results remain available for review. No command commits or rewrites Git.

## Local identity trust

[ADR-005](../adr/ADR-005-LOCAL-DEVELOPMENT-TRUST.md) defines this explicit extension
to ADR-002. The offline DevTools console generates an ephemeral RSA-2048 key and
one RS256 token for synthetic-developer with fixed issuer
https://aegisai-local.example.invalid and audience aegisai-local-development.
It discards the private key without writing it. Only public trust/configuration
is mounted into the API; the token remains in the tools mount with owner-only
file permissions. This is test credential issuance, not an OIDC/OAuth server.

The API accepts LocalDevelopment:PublicKeyPem only in environment Development,
with no provider authority/audience settings. Other environments, mixed trust,
private keys, malformed keys, and short RSA keys fail startup. The existing real
JWT handler still validates signature, issuer, audience, expiration, lifetime,
and unique subject/issuer; local trust restricts algorithms to RS256. No client
headers or unsigned identities can bypass authentication. Without local trust,
the existing provider/unconfigured behavior remains unchanged. Domain and
Application do not reference the development adapter or credential tooling.

All policy, attribute, and risk values derive from the existing synthetic example;
only the issuer and subject are replaced. They are not production data or verified
posture. The public-key field is explicit administrative configuration, never HTTP
input. This environment is for local decision exploration only and must not be
exposed publicly or used to protect actual production operations.

## Container design and reproducibility limits

The API uses a multi-stage .NET 8 build and ASP.NET runtime image, runs as the
image's non-root app user, drops capabilities, disallows privilege escalation,
and has a read-only root filesystem plus /tmp tmpfs. It publishes only port 8080
through the loopback mapping. No SDK or credential generator ships in the API image.
Docker context exclusions keep Git metadata, local credentials, and build/results
artifacts out of images.

The optional tools image contains .NET SDK and Git. It runs as the invoking host
UID/GID, reads a repository bind mount, and copies source to a fresh temporary
workspace excluding host bin/obj, local credentials, Git metadata, and old results.
Git metadata is linked read-only from the original normal clone. Generated results
and local fixture files use separate writable mounts. Builds do not pollute host
bin/obj; results are owned by the invoking user. No host Docker socket is mounted.

Images use .NET 8 bookworm-slim tags, which receive servicing updates, and SDK
selection follows global.json. They are not digest-pinned and package restore is
not locked/transitive-offline: this is reproducible workflow/source/fixture
semantics, not byte-identical image provenance. Record image IDs and daemon/host
resources for serious container comparisons. The runner records its own runtime,
source, manifest and binary hashes; snapshots now include deployment C# projects
because the solution includes DevTools. Credential files are excluded.

## Validation and failure handling

Security integration tests cover local trust isolation, mixed provider rejection,
private-key rejection, real signature/issuer/audience/lifetime checks, and algorithm
restriction. Existing tests retain authorization, audit, architecture, and runner
coverage. The demo exits nonzero on unexpected statuses/outcomes or absent correlation.

If Docker cannot connect, start its daemon and repeat `./dev up`. For startup or
port errors inspect `docker compose -f deployment/docker/compose.yaml logs api`.
For expired credentials regenerate with `up`. A result directory collision requires
a new experiment name; existing results are never overwritten. See the
[V0.1 readiness report](../V01_READINESS.md) for actual checks and remaining gaps.
