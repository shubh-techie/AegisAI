# Project History

## 2022 — Securing-Microservices

The repository originated as Securing-Microservices. Its historical GloboTicket
application targeted .NET Core 3.1 and demonstrated microservice security.

Observed Git commits:

- 2022-02-08: `e2a81c9` — Initial commit.
- 2022-02-09: `b9518fa` — added project.

The `securing-microservices-legacy` tag points to `b9518fa` and preserves the
legacy application. AegisAI did not exist as this project in 2022.

## Between 2022 and 2026

The inspected Git history contains no intervening development commits. No work,
releases, experiments, or other activity is invented for that interval.

## 2026 — AegisAI development begins

In September 2026, development begins on AegisAI — Adaptive Intelligence for
Secure Distributed Systems. Its tagline is: Observe. Assess. Authorize. Respond.

AEGISAI-001 establishes documentation and directory placeholders on
`feature/aegisai-foundation`. The legacy application/source/project files are
removed from the current working tree only. The application will not be reused;
AegisAI implementation is PLANNED as fresh .NET 8 work using Clean Architecture
and SOLID. No .NET solution is created in this task.

## Historical preservation and recovery

Historical commits, dates, authors, and the legacy tag must remain unchanged.
Never rewrite history, rebase/squash/amend historical commits, delete or move the
legacy tag, or force push. The 2022 project must remain recoverable.

Inspect the legacy solution without changing the working tree:

```sh
git show securing-microservices-legacy:GloboTicket.sln
```

Export the complete historical project into an archive outside the working tree:

```sh
git archive --format=tar --output=/tmp/securing-microservices-legacy.tar securing-microservices-legacy
```

These commands read the checkpoint; they do not rewrite history.
