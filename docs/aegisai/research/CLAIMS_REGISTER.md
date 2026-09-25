# Research claims register

Date: 2026-09-25 | Baseline: AEGISAI-012

Future agents must consult this register before writing README claims, website
claims, papers, presentations or patent documentation. Evidence supports only the
scope actually observed. Update claims with dated, inspectable evidence; never
promote a hypothesis because a diagram, title or software test exists.

VERIFIED means supported within the stated repository scope. UNPROVEN identifies
a hypothesis without outcome evidence. UNVERIFIED means the requisite review has
not established the claim. IMPLEMENTED, PLANNED and EXPERIMENTAL remain capability
labels and do not substitute for claim verification.

| Claim | Status | Evidence Required | Current Evidence | Allowed Wording |
| --- | --- | --- | --- | --- |
| Model A is implemented | VERIFIED | Repository implementation and applicable tests | [RBAC engine](../../../src/AegisAI.Application/Authorization/RbacAuthorizationEngine.cs), [tests](../../../tests/AegisAI.Application.Tests/RbacAuthorizationTests.cs), validation in [development log](../DEVELOPMENT_LOG.md) | Implemented deterministic RBAC decision baseline |
| Model B is implemented | VERIFIED | Repository implementation and applicable tests | [Model B](../../../src/AegisAI.Application/Authorization/ModelBAuthorizationEngine.cs), [tests](../../../tests/AegisAI.Application.Tests/AbacEvaluationTests.cs), development log | Implemented RBAC + ABAC decision baseline |
| Model C is implemented | VERIFIED | Repository implementation and applicable tests | [Model C](../../../src/AegisAI.Application/Authorization/ModelCAuthorizationEngine.cs), [tests](../../../tests/AegisAI.Application.Tests/ContextualRiskTests.cs), development log | Implemented deterministic contextual-risk decision baseline |
| Model D improves security | UNPROVEN | Controlled A/B/C/D experiments, independent labels and behavioral ablation | None; D is not implemented | Research hypothesis |
| Policy-bounded AI reduces inappropriate responses | UNPROVEN | Controlled comparison against direct ML enforcement with independent appropriateness labels and utility constraint | None | To be evaluated |
| Operational state improves resilience | UNPROVEN | Controlled degraded-system experiments and matched operational-state ablation | None | Research hypothesis |
| Adaptive authorization provides benefits within performance budgets | UNPROVEN | Joint security and performance evaluation against prespecified thresholds | None; narrow A–C smoke timing does not test H4 | Research hypothesis |
| AegisAI is novel | UNVERIFIED | Literature and prior-art analysis supporting a precisely scoped distinction | Not completed | DO NOT CLAIM |
| Candidate mechanisms are patentable | UNVERIFIED | Prior-art review and patent-counsel review | Not established | Research / invention candidate only; DO NOT CLAIM patentability |
| A–C synthetic engine-only smoke artifacts exist | VERIFIED | Raw observations, manifest, source and environment provenance | [Artifact description](../../../research/README.md), existing results directories | Experimental synthetic execution evidence; no effectiveness inference |
| Model C explains its deterministic score arithmetic | VERIFIED | Contribution records, score recomputation and tests | [Scorer](../../../src/AegisAI.Application/Authorization/ContextualRiskEngine.cs) and contextual risk tests | Implemented arithmetic contributions and reasons; no demonstrated AI explainability |
| Closed-loop adaptive enforcement is implemented | UNVERIFIED | State, feedback, enforcement code and integration/security validation | Absent; decision recommendations and best-effort audit only | PLANNED; DO NOT CLAIM implementation |
| AegisAI is production-ready or superior to authorization systems generally | UNVERIFIED | Representative deployment/security evaluation and appropriately scoped comparisons | None; current single-process synthetic baseline is insufficient | DO NOT CLAIM |

No research effectiveness, originality or patentability claim is verified by
AEGISAI-012. Consult [integrity rules](RESEARCH_INTEGRITY.md) and
[proposed contribution](PROPOSED_CONTRIBUTION.md) before changing allowed wording.
