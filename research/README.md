# AegisAI authorization experiments

**EXPERIMENTAL — synthetic data only.** Models A (RBAC), B (RBAC + ABAC), and C
(RBAC + ABAC + deterministic contextual risk) are executable. Model D remains
**PLANNED**; no behavioral detector exists. This framework does not measure false
positives, false negatives, or behavioral detection quality yet.

## Reproduce the smoke experiment

Prerequisites: Git and a .NET 8 SDK compatible with global.json. Run from the
repository root, with package restore access on the first build:

```sh
dotnet restore
dotnet build -c Release
dotnet test -c Release
dotnet run --project benchmarks/AegisAI.Experiments -c Release --no-build -- \
  --manifest research/experiments/smoke-v1.json \
  --output research/results/my-smoke-run
```

The output directory must not already exist. Each repeat needs a new name; the
runner refuses to overwrite prior results. Do not run tests/builds concurrently
with timing measurements. No API server, credentials, identity provider, or data
download is needed. The runner calls the actual engines with synthetic identities
and trusted synthetic policy/attribute/risk fixtures in process.

The manifest fixes a seed, eight scenarios, 25 warm-up cycles, 100 measurement
cycles, and three repetitions. This yields 2,400 measured calls per model plus
600 warm-up calls per model. Warm-up is recorded but excluded from reported
latency, throughput, and decision distribution. Model and scenario order are
seeded; timings are not deterministic and will vary by machine, runtime, and load.

## Generated artifacts

Each run produces:

- manifest.json: byte-for-byte input plan.
- observations.jsonl: every warm-up and measured invocation, elapsed Stopwatch
  ticks, scenario, model, repetition, outcome or error type.
- summary.json: actual means and nearest-rank p50/p95/p99 latencies in microseconds,
  successful decisions per second, attempts/errors, decision distribution, model
  ordering, and whole-run process CPU/allocation observations.
- environment.json: SDK/runtime/OS/architecture/processor count/memory availability,
  timer frequency, Release/Debug mode, selected runtime settings, source commit,
  dirty-source status, manifest/source/binary hashes, and explicit future-metric gaps.
- source.zip: exact source/project/solution/global.json snapshot, including
  uncommitted implementation. No .git history, build outputs, credentials, or raw
  environment dump is collected.

To reconstruct an uncommitted implementation elsewhere, extract source.zip into a
new directory, copy manifest.json as research/experiments/replay.json, create
research/results, initialize a local Git repository and make a local snapshot
commit (the runner records HEAD), then run the same build/test/run commands with
that manifest. This is a separate reproduction directory; never rewrite this
repository's history. Git provenance will differ, but source hashes can be compared.
The archive excludes prose documentation and does not install SDKs or guarantee
identical hardware/package caches. Restore uses project-declared package versions.

## Measurement interpretation

These are **engine-only, single-threaded, in-process microbenchmarks**. Latency
measures only the selected engine delegate call and its nested stages. Throughput
uses completed decisions divided by measured loop wall time, including the common
per-call timing/recording overhead. It is not HTTP throughput, offered-load capacity,
or protected-operation throughput. No network, authentication, audit sink, tracing,
rate enforcement, or human step-up time is included. This is narrower than the
future durable-audit boundary in Research Architecture.

All models receive the same scenario sequence per repetition. Engines and fixtures
are constructed outside measured windows. Setup, warm-up, and file I/O are excluded
from those windows; no forced garbage collection is used. Raw warm-ups preserve
startup effects for inspection but do not constitute a formal cold-start study.
Errors are recorded and excluded from completed-decision latency percentiles and
throughput numerators; all attempts remain visible. Any warm-up or measurement
error makes the executable exit nonzero after writing results.

All decision outcomes count as completed evaluations, including DENY/STEP_UP/LIMIT.
None proves an action was executed. CPU/allocation values cover setup, warm-up and
measurement within ExperimentRunner.Run, not just timed delegates; they are process
observations, not per-model resource attribution.

## Synthetic scenarios and future analysis

The manifest exercises low risk, the three outcome thresholds, missing context,
an unassigned role, an attribute mismatch, and a forbidden action. Fixture policy
permits readers to read synthetic-report, ABAC requires department engineering,
and each present risk indicator receives the manifest's normalized value. Engines
compute outcomes; no outcomes are stored as input or fabricated as measurements.
These scenarios are neither production data nor an attack/benign labeled dataset.

IFalsePositiveNegativeAnalysis and IBehavioralAnomalyEvaluation are **PLANNED
interfaces only**. Their contracts require independent labels and detection flags;
they have no implementations. DENY is not assumed to mean malicious. Future rates
must define their denominators and return undefined when no eligible cases exist.
Model D has no runner or result row. No placeholders emit zero-valued quality scores.

See [SPEC-009](../docs/aegisai/specs/SPEC-009-EXPERIMENT-FRAMEWORK.md). Smoke results
only validate execution and artifact generation. They do not establish superiority,
statistical confidence, production representativeness, or security effectiveness.

## Initial generated smoke result

The task's actual run is in [summary.json](results/aegisai009-smoke/summary.json),
with [environment metadata](results/aegisai009-smoke/environment.json) and raw
observations alongside it. This run used uncommitted source captured in source.zip.
Measured windows total less than 5 ms per model; use these values only to verify
that the framework executes and produces inspectable artifacts.
