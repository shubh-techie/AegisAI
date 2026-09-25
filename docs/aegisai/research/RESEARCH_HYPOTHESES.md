# Research hypotheses

Date: 2026-09-25 | Hypothesis baseline: AEGISAI-012 v1
Status: **PLANNED RESEARCH**. H1–H4 are **UNPROVEN**; none is asserted true.

These directional hypotheses precede Model D experiments. Existing A–C synthetic
smoke runs predate this baseline and are exploratory execution evidence only.
Each future confirmatory protocol must freeze its primary endpoint, minimum effect,
uncertainty method, sample/repetition plan, exclusions and decision rule before
viewing evaluation results. No significance, power or effect size is assumed here.
A null, adverse or inconclusive result must remain reportable.

## H1 — Behavioral security effectiveness

Adding behavioral risk evidence to deterministic authorization will improve
detection of abnormal or compromised access relative to Models A–C under
controlled behavioral-anomaly scenarios.

Required measurements should eventually include precision, recall, F1,
false-positive rate, false-negative rate, and detection time. Use independent
benign/attack labels, a fixed detection definition, and paired scenarios. Report
per-baseline comparisons and a matched behavioral-evidence ablation so that
additional restrictions or operational features do not masquerade as detection
improvement. Predetermine how baselines without detectors are evaluated; never
invent detection outputs. Absence of improvement against the prespecified
comparators would not support H1; lower false negatives at uncontrolled false
positive cost do not automatically support it either.

## H2 — Policy-bounded safety

Policy-bounded behavioral intelligence will produce fewer inappropriate
high-impact security actions than direct ML-based enforcement while preserving
useful anomaly-response capability.

Potential measurements: incorrect automated action rate, policy/AI disagreement
rate, unnecessary DENY rate, unnecessary STEP-UP rate, missed high-risk events,
and decision explainability. Compare the same frozen predictor, evidence and
workload under bounded arbitration and a separately named direct-enforcement
variant in an isolated synthetic environment. Define high-impact actions and
independently adjudicated appropriateness in advance. Prespecify a response-utility
floor: preventing every action must not count as useful safety. Failure to reduce
inappropriate actions, or failure to retain that utility, would not support H2.
Disagreement alone is neither a policy defect nor a safety improvement.

## H3 — Operational resilience

Including distributed-system operational state in adaptive security decisions
will improve containment or reduce security exposure during degraded or
adversarial operating conditions compared with authorization models that do not
incorporate operational state.

Potential measurements: exposure duration, blast-radius proxy, response time,
inappropriate restriction rate, recovery behavior, and system stability. Compare
matched variants with/without operational state under the same controlled faults
and adversarial scenarios; also retain A–C reference comparisons. Degradation is
not itself proof of malicious behavior.

The exact definition of **blast radius** must be formalized before experimentation,
including affected entities/resources, causal attribution, observation window,
and denominator. A proxy is not a measured real-world damage claim. No improvement
in the prespecified containment/exposure endpoint, or unacceptable restriction
under an explicitly frozen constraint, would not support H3.

## H4 — Performance trade-off

Adaptive authorization can provide measurable security benefits while maintaining
bounded authorization latency, throughput, and resource overhead relative to
deterministic baselines.

Potential measurements: P50, P95 and P99 latency, throughput, CPU utilization,
memory utilization, and decision-processing overhead. Measure all variants at
matched load, concurrency, hardware and timing boundaries. Report stability,
errors, timeouts and false-positive costs alongside performance.

Before confirmatory experiments, specify numeric absolute/relative latency,
throughput and resource thresholds, baseline denominators, and a security-benefit
criterion. No performance is called acceptable without those explicit thresholds.
A security improvement with breached budgets, or bounded overhead without a
security improvement, would not support the joint hypothesis. Current smoke
measurements cannot establish H4.

Metric gaps are in [Research variables](RESEARCH_VARIABLES.md); protocol changes
must follow [Research integrity](RESEARCH_INTEGRITY.md). These definitions are not
a completed confirmatory protocol or authorization to implement Model D.
