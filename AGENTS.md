# AegisAI Agent Instructions

Read this AGENTS.md before every future task and follow it throughout the work.

## Project identity

AegisAI — Adaptive Intelligence for Secure Distributed Systems

Observe. Assess. Authorize. Respond.

Securing-Microservices is the historical 2022 project. AegisAI development begins
in 2026. The legacy application is not reused; new implementation starts fresh.

## Git history rules

- Never rewrite Git history.
- Never rebase, squash, or amend historical commits.
- Never change historical dates or authors.
- Never delete, move, or replace `securing-microservices-legacy`.
- Never force push.
- Never imply AegisAI existed in 2022 or invent activity between 2022 and 2026.
- Keep the 2022 project recoverable from Git and the legacy tag.
- Remove or replace legacy files only through current-branch changes; never alter historical commits.
- Do not commit automatically. Stop for review when requested.

## Engineering requirements

- Target .NET 8 for new .NET implementation.
- Follow Clean Architecture: dependencies point inward; domain and application
  logic remain independent of infrastructure and presentation concerns.
- Apply SOLID principles and keep responsibilities and interfaces focused.
- Tests are required for implemented behavior and bug fixes. Include meaningful
  unit tests and integration/security tests where relevant; run applicable checks
  and report their actual outcomes. For documentation-only work, validate structure,
  links, and diffs; explain when no executable test suite exists.
- Never commit secrets, credentials, private keys, or sensitive production data.
  Use environment variables, local secret storage, or a secret manager.
- Do not create a .NET solution as part of AEGISAI-001.

## Evidence and status

Clearly label capabilities and research work:

- **IMPLEMENTED**: exists in the repository, with supporting validation stated.
- **PLANNED**: intended future work; no claim of implementation.
- **EXPERIMENTAL**: exploratory work with limitations and reproducibility details.

Never fabricate research results, benchmarks, datasets, or citations. Report only
observed results; record methodology, provenance, environment, and limitations.
Label synthetic datasets explicitly. Verify citations against actual sources.
Keep project history and the chronological development log truthful and current.
