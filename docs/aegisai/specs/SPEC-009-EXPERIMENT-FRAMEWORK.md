# SPEC-009 — Reproducible authorization experiments

Date: 2026-09-23  
Status: IMPLEMENTED experimental runner; synthetic fixtures only

## Scope

Compare actual Model A/B/C implementations using a standalone net8.0 console
project under benchmarks/AegisAI.Experiments. The runner references Application and
Infrastructure; production projects do not depend on it. Tests live in a separate
Experiments.Tests project included in AegisAI.sln. Models and policies remain
unchanged. Model D is PLANNED and cannot be selected/executed by this runner.

Exact commands and artifact interpretation are in [research/README](../../../research/README.md).
This is an engine-only protocol, not the full future pipeline in
[Research Architecture](../architecture/RESEARCH_ARCHITECTURE.md). Authentication,
enforcement, audit durability, external services, and observational instrumentation
are uniformly absent across variants. Consequently its values must not be pooled
with end-to-end or audit-inclusive measurements.

## Manifest and fixture contract

The JSON manifest must declare DataKind SYNTHETIC and a nonblank version. It sets
seed, warm-up cycles, measurement cycles, repetitions, and uniquely named scenarios.
Each scenario supplies AssignedRole, Department (nullable), requested Action, and
RiskIndicator (nullable normalized decimal). No field accepts expected outcomes.
Unknown manifest fields fail binding. Invalid values, duplicate scenario IDs,
empty workloads, and workloads over two million total calls are rejected.

The fixture generator creates an issuer-qualified synthetic actor and report.
Its RBAC role grants only read; ABAC requires department engineering; all five
risk signals use the supplied normalized indicator, or are absent when null.
Each scenario owns fixed engine/provider instances reused throughout the run.
Policy/scorer definitions are the existing implementations in SPEC-005/006/007;
fixture versions are synthetic-rbac-v1, synthetic-abac-v1, synthetic-risk-v1.
Source hashes and the source archive bind those definitions to the generated run.

## Execution and measurement

Construct fixtures before timing. For each repetition, create a balanced sequence
containing every scenario MeasurementCycles times and shuffle it with the seeded
.NET Random. Shuffle model order using the same generator and preserve both model
orders and raw scenario sequences. All models in that repetition receive identical
scenario order. Perform WarmupCycles balanced cycles before each model's measured
window; warm-up order is fixed. Run sequentially with concurrency one and no fixed
arrival rate, timeouts, cache resets, or forced GC.

Per-call latency is elapsed Stopwatch ticks around the selected engine delegate,
converted to microseconds using recorded Stopwatch.Frequency. Reports include mean
and nearest-rank p50/p95/p99 over completed measured calls: sorted index
ceil(p * n) - 1. Zero samples produce null percentiles. Exceptions record an error
type, not an invented decision or a secret-bearing message; raw records include
failed-call durations. No outliers are removed.

Throughput = completed measured evaluations / sum of measured loop seconds for
that model. The loop includes common timer/record allocation overhead. Completion
includes ALLOW, DENY, STEP_UP, and LIMIT. Warm-up and output serialization are
excluded. Decision counts derive from those same completed observations; errors
and attempts are reported independently. Aggregation pools repetitions but retains
raw repetition membership; it does not calculate confidence intervals.

The executable writes artifacts once into a new directory under research/results.
It retains every run rather than overwriting or selecting favorable results. A
nonzero exit indicates invalid setup or an error; completed error-containing runs
still write their observations. Interrupted/setup-failed runs can leave incomplete
artifacts and must not be presented as successful measurements.

## Provenance

Record actual runtime/SDK, build mode, OS, architecture, logical processors,
GC/runtime settings, timer frequency, source HEAD, dirty-source flag, source and
binary SHA-256 hashes, exact manifest, timestamps, and a source archive. Host model,
thermal state, background load, CPU allocation/governor, and package-cache effects
are not controlled. AvailableMemoryBytes is runtime-reported availability, not a
claim about dedicated physical RAM. Process CPU and allocation totals cover runner
setup/warm-up/measurement, not isolated per-model resource use.

Reproduction means the same source, plan, scenario order, and decision semantics;
wall-clock measurements will vary. Do not claim bit-for-bit timing reproduction.
The source archive supports review of runs made before committing this task.

## Future metric interfaces — PLANNED

IFalsePositiveNegativeAnalysis consumes independently labeled malicious/benign
observations and detection flags and returns nullable error rates.
IBehavioralAnomalyEvaluation represents a future labeled detector evaluation.
Neither interface has an implementation. Quality metrics are null/not implemented
in metadata; Model D is explicitly PLANNED. Authorization outcomes must not be used
to manufacture independent labels or detection results.

## Tests and limitations

Tests confirm real engine decisions, Model D exclusion, validation, paired seeded
workloads, warm-up exclusion, summary count consistency, error accounting,
nearest-rank percentiles, and throughput arithmetic. Run the entire solution tests
before a separate Release smoke experiment. An external validation of generated
raw observations can recompute decision counts and latency summaries using metadata.

Tiny smoke windows are susceptible to JIT/tiering, timer overhead, scheduling,
allocation, caches, and host load. Neither synthetic representativeness nor causal
performance improvement is established. No broad performance/security claims,
statistical significance, or production dataset claims are permitted from this run.
