# Research variables and metrics

Date: 2026-09-25 | Baseline: AEGISAI-012
Status: **PLANNED** experimental definitions except the narrow metrics explicitly
implemented by [SPEC-009](../specs/SPEC-009-EXPERIMENT-FRAMEWORK.md).

## Independent variables

| Variable | Initial design / definition still required |
| --- | --- |
| Authorization model | A–D; separately named direct-ML comparator and matched ablations |
| Behavioral condition | Ordinary behavior, controlled deviation, compromised behavior; define sequences, windows and independent labels |
| Request context | Bound action, subject/resource context; freeze fixture distributions and live-source semantics |
| Resource sensitivity | Define sensitivity classes and mappings; C currently uses synthetic normalized values |
| Authentication assurance | Define verified assurance levels and transitions; C's weakness indicator is simulated |
| Operational-system state | Define healthy/degraded/faulted dependency states, severity and timing independently of detector outputs |
| Attack/anomaly scenario | Versioned generator, seed, onset, intensity, duration, affected entities and ground truth |

Hold shared policies, hardware/resource allocation, data splits, measurement
boundaries, offered load/concurrency and instrumentation constant where applicable.
Record intentional changes, run order, warm-up, repetitions and random seeds.
Avoid leakage between training, calibration and held-out evaluation; fit feature
transformations using training data only. Baseline capabilities differ, so a common
workload does not imply every model consumes every input.

## Dependent variables

| Group | Metrics | Initial measurement semantics and remaining definitions |
| --- | --- | --- |
| Security | Precision, recall, F1, false-positive rate, false-negative rate | Precision = TP/(TP+FP); recall = TP/(TP+FN); F1 = 2TP/(2TP+FP+FN); FPR = FP/(FP+TN); FNR = FN/(FN+TP). Freeze request/event unit, independent labels, detection flag, aggregation and unknown handling. **PLANNED** analysis; no current quality result |
| Response | Detection time; response time | Onset to first correlated detection; detection to confirmed mitigation. Define onset, correlation, window and completion evidence; censor undetected/uncompleted events and report coverage. **PLANNED** |
| Response | Incorrect automated action rate | Inappropriate executed actions / eligible executed automated actions; freeze action unit, impact class, independent appropriateness rule and eligibility. Report proposed actions and missed events separately. **PLANNED** |
| Authorization | ALLOW, STEP-UP, LIMIT, DENY frequency | Counts and proportions of completed decisions over a declared window; errors/attempts reported separately. A–C counts **IMPLEMENTED**; D and executed-action distributions **PLANNED**. A/B cannot produce STEP_UP or LIMIT |
| Authorization | Policy/AI disagreement | Disagreements / eligible paired AI recommendations and policy decisions; define common action mapping, unknowns and severity. **PLANNED**; not a baseline A–C metric |
| Performance | P50, P95, P99 latency; throughput | Engine-only nearest-rank percentiles and completed decisions / measured loop seconds **IMPLEMENTED**. Full authorization and end-to-end boundaries **PLANNED**; specify load, concurrency, units, errors/timeouts and audit inclusion |
| Performance | CPU overhead; memory overhead | Define per-model process/container scope, CPU time/utilization, RSS/working set versus allocations, sampling interval and baseline difference/ratio. **PLANNED** per-model attribution; current runner CPU seconds and allocated bytes cover whole-run setup/warm-up/measurement and are not utilization or resident memory |
| Closed-loop behavior | Risk-state evolution; escalation stability; risk decay; recovery behavior | Define state entity, scale, update cadence, transition trace, oscillation/escalation counts, decay law/time, recovery target and window. **PLANNED**; no current stateful loop |

## Additional hypothesis endpoints requiring precise operational definitions

- H2: unnecessary DENY/STEP-UP rates require independently justified required
  actions and declared denominators. Missed high-risk events need a fixed event
  severity rule. Explainability requires a prespecified fidelity/completeness and
  comprehension rubric, assessors and scoring procedure.
- H3: exposure duration requires a defined exposure predicate and start/end
  events. Blast-radius proxy requires affected entity/resource units, causality,
  time window and denominator before experiments. Inappropriate restriction rate,
  recovery and system stability require independent targets and fault attribution.
- H4: decision-processing overhead requires matched timing boundaries and baseline
  subtraction/ratios. Numeric performance budgets and a security-benefit criterion
  must be declared before any acceptable-performance claim.

A zero denominator is undefined, not zero. Distinguish absent capability from
measured zero events. A/B lack risk scores and detectors; do not synthesize scores
or award zero detection time. For cross-model access handling, independently label
allowed harmful actions separately from anomaly detection quality. A benign but
unauthorized request can correctly receive DENY without being anomalous.

Existing smoke throughput includes loop/timer/recording overhead and excludes
HTTP, authentication, audit and enforcement; it cannot be pooled with the future
audit-inclusive boundary in [Research Architecture](../architecture/RESEARCH_ARCHITECTURE.md).
Distributed timings need clock synchronization/error bounds or runner-controlled
clocks. Human STEP-UP delay is separate. Preserve raw observations and report
uncertainty, missingness, exclusions and per-scenario support. These initial
variables are not a finalized dataset, threshold selection or statistical protocol.
