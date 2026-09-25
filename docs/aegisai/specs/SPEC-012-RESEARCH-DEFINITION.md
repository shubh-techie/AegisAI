# SPEC-012 — Research Definition and Hypothesis Baseline

Date: 2026-09-25

Task: AEGISAI-012-DOC-COMPLETION

Research baseline: AEGISAI-012 — Research Definition and Hypothesis Baseline

Status: **IMPLEMENTED** documentation completion in working tree; pending review.

## Purpose and scope

Establish the formal pre-experiment research baseline for AegisAI before
literature-gap analysis, Model D implementation and controlled experimentation.
This specification completes the record of the already established baseline;
it does not redefine its research content.

Research umbrella: **AI-Driven Automation for Resilient & Secure Cloud/Distributed Systems**.
Experimental platform: **AegisAI — Adaptive Intelligence for Secure Distributed Systems**.
The [research program](../research/RESEARCH_PROGRAM.md) defines their relationship.

## Canonical research references

The following existing documents remain authoritative. This specification records
traceability and acceptance criteria; it does not duplicate or rewrite their
definitions. [ADR-006](../adr/ADR-006-POLICY-BOUNDED-ADAPTIVE-SECURITY.md) records the
policy-authority decision without changing the research question or hypotheses.

| Baseline artifact | Canonical source |
| --- | --- |
| Research problem | [RESEARCH_PROBLEM](../research/RESEARCH_PROBLEM.md) |
| Primary question and RQ1–RQ4 | [RESEARCH_QUESTION](../research/RESEARCH_QUESTION.md) |
| H1–H4 | [RESEARCH_HYPOTHESES](../research/RESEARCH_HYPOTHESES.md) |
| Models A–D | [RESEARCH_MODELS](../research/RESEARCH_MODELS.md) |
| Proposed contribution | [PROPOSED_CONTRIBUTION](../research/PROPOSED_CONTRIBUTION.md) |
| Conceptual model | [CONCEPTUAL_MODEL](../research/CONCEPTUAL_MODEL.md) |
| Variables and candidate metrics | [RESEARCH_VARIABLES](../research/RESEARCH_VARIABLES.md) |
| Claims register | [CLAIMS_REGISTER](../research/CLAIMS_REGISTER.md) |
| Invention candidates | [INVENTION_CANDIDATES](../research/INVENTION_CANDIDATES.md) |
| Research integrity | [RESEARCH_INTEGRITY](../research/RESEARCH_INTEGRITY.md) |

## Status and evidence boundaries

| Item | Status |
| --- | --- |
| Model A — RBAC | **IMPLEMENTED** |
| Model B — RBAC + ABAC | **IMPLEMENTED** |
| Model C — Deterministic Contextual Risk | **IMPLEMENTED** |
| Model D — Policy-Bounded Behavioral Adaptive Authorization | **PLANNED RESEARCH** |
| H1–H4 | **PRE-EXPERIMENT HYPOTHESES — NOT VALIDATED** |
| Proposed contribution | **PLANNED RESEARCH — UNVERIFIED** |
| Novelty | **NOT ESTABLISHED** |
| Patentability | **NOT ESTABLISHED** |
| Experimental superiority | **NOT ESTABLISHED** |

IMPLEMENTED denotes repository behavior with supporting validation, not proven
research effectiveness. A–C decision engines and existing synthetic smoke usage
are **EXPERIMENTAL** baselines. They do not enforce protected operations;
STEP_UP/LIMIT are recommendations with obligations. Behavioral intelligence,
operational-state ingestion and closed-loop feedback remain planned. See
[SPEC-007](SPEC-007-CONTEXTUAL-RISK.md) and
[SPEC-009](SPEC-009-EXPERIMENT-FRAMEWORK.md) for implementation/measurement limits.

## Acceptance criteria

These are documentation-baseline criteria, not findings that validate H1–H4.

| ID | Criterion | Verification source or boundary |
| --- | --- | --- |
| AC1 | Research problem documented | Canonical research problem |
| AC2 | Primary research question and RQ1–RQ4 established | Canonical research question |
| AC3 | H1–H4 established before Model D experimentation | Canonical hypotheses and provenance below |
| AC4 | Models A–D status explicitly documented | Status table and canonical model definitions |
| AC5 | Proposed contribution documented conservatively | Canonical proposed contribution; planned/unverified only |
| AC6 | No novelty claim made | Claims register and contribution disclaimers |
| AC7 | Research variables and candidate metrics documented | Canonical variables; unresolved operational definitions retained |
| AC8 | Claims register exists | Canonical claims register |
| AC9 | Research-integrity rules exist | Canonical research integrity |
| AC10 | Invention concepts are candidates only | Canonical invention candidates; prior-art and patent-counsel review required |
| AC11 | No Model D implementation introduced | Documentation-only diff; Model D remains planned |
| AC12 | No experimental results introduced | No new experiments, measurements or result artifacts in either research-definition task or this completion |
| AC13 | Existing production behavior remains unchanged | Source/configuration/tests unchanged; Release build and test checks |
| AC14 | Literature/prior-art analysis is the next research gate | Review must examine overlap and supportable gaps before stronger contribution claims or Model D implementation/controlled experiments |

AC14 establishes the next gate; it does not claim that literature/prior-art review
has been completed. Any subsequent revision must follow the canonical integrity
rules and preserve this baseline. This completion adds no algorithms, thresholds,
datasets, research results or expanded contribution.

## Research provenance

The research definitions were established in commit `c8e6112`
(`research: establish AegisAI research definition and hypotheses`), incorporated
by merge commit `ff37d4b`, and preserved by `research-baseline-v0.1` at that merge.
The annotated tag object is `e34b35e5198835a82e08e6008df713697d769e14`.

Those definitions precede literature-gap analysis, Model D implementation,
A/B/C/D comparative experiments and experimental results from those investigations.
This does not mean they precede all repository experiments: synthetic A–C
engine-only smoke artifacts already existed under SPEC-009/010. Those artifacts
are not Model D results and do not validate H1–H4.

SPEC-012 and ADR-006 were absent from both the original research commit and the
tagged state. Their completion occurs **after** that baseline commit/tag in
AEGISAI-012-DOC-COMPLETION. This intentional distinction preserves provenance;
neither file is represented as part of the original tagged contents. The tag,
historical commits, dates and authors remain unchanged. The original
[development-log entry](../DEVELOPMENT_LOG.md) remains a historical task record;
a new completion entry records this follow-up.

## Validation

Validate local Markdown links, status/acceptance-criteria consistency, canonical
research-file preservation and Git diffs. Run from the repository root:

```sh
git status --short
git diff --stat
git diff --check
dotnet build --configuration Release
dotnet test --configuration Release
```

Check new untracked documents separately because ordinary git diff excludes them.
Record actual outcomes in the completion entry of the development log. Passing
software checks does not validate the proposed contribution or H1–H4.
