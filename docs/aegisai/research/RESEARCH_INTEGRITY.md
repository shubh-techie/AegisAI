# Research integrity

Date: 2026-09-25 | Baseline: AEGISAI-012
Status: **IMPLEMENTED** documentation requirements; future experiments **PLANNED**.

1. Define hypotheses before Model D experiments. Preserve this H1–H4 baseline
   with its date/version; it is not retrospective confirmation from A–C smoke runs.
2. Preserve raw experimental results, including failed or unfavorable runs.
   Unsuccessful experiments must not be silently removed. Record interrupted runs,
   missing outputs and setup failures without inventing observations.
3. Never fabricate benchmark data, datasets, measurements, results or citations.
   Verify citations against actual sources before use.
4. Never retrospectively modify hypotheses without documenting why. Record old/new
   wording, date, rationale, evidence already viewed and implications for analysis.
   Keep the earlier version recoverable; do not rewrite Git history.
5. Distinguish exploratory experiments from confirmatory experiments. Freeze the
   confirmatory protocol, metrics, thresholds, comparisons, sample/repetition plan,
   exclusions and analysis rules before viewing held-out outcomes. Label deviations
   and post-hoc analyses as such; tuning on held-out results does not confirm H1–H4.
6. Distinguish illustrative examples, correctness-test assertions and measured
   results. Existing synthetic smoke artifacts verify execution/provenance only.
7. Document dataset/scenario generation: source, permissions where applicable,
   generator version, schema, seeds, labels, splits and hashes. Label synthetic
   datasets explicitly. Ground truth must be independent of evaluated decisions.
8. Record model/configuration versions, source commit, dirty state/exact source,
   policies, features, thresholds, state/feedback rules and dependency versions.
   Preserve exact experiment configuration, commands, environment, instrumentation,
   workload/order, timing boundaries, warm-up and run identifiers.
9. Preserve Git history, authors/dates and securing-microservices-legacy. Do not
   invent activity between the 2022 legacy project and AegisAI development in 2026.
10. Document limitations, uncertainty, missingness and negative results where
    relevant. Report denominators and exclusions; zero eligible cases are undefined.
    Do not generalize synthetic engine-only observations to production deployments.
11. Preserve unsuccessful runs alongside successful runs; exclusions require a
    recorded reason and sensitivity analysis where relevant, not silent deletion.
    Retain reproducible analysis scripts and links from summaries to raw artifacts.
12. Never commit secrets or sensitive production data. Preserve raw evidence in
    appropriate controlled storage when it cannot safely be stored in the repository;
    record provenance/access references without exposing protected data.
13. Consult [CLAIMS_REGISTER.md](CLAIMS_REGISTER.md) before README, website, paper,
    presentation or patent claims. Separate IMPLEMENTED, PLANNED and EXPERIMENTAL
    capabilities from verified, unproven and unverified research claims.

No Model D implementation, ML functionality or new research experiment is part
of AEGISAI-012. Passing restore/build/tests validates the unchanged software
baseline; it does not validate the [research hypotheses](RESEARCH_HYPOTHESES.md).
