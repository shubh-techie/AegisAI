# ADR-001 — Clean Architecture

Date: 2026-09-23  
Status: Accepted for AEGISAI-002; implementation pending review

## Context

AegisAI development begins in 2026 with fresh source. The 2022
Securing-Microservices application remains recoverable through its legacy Git tag
and is not reused. Future security decisions and research components need clear
boundaries and independently testable behavior.

## Decision

Use .NET 8, nullable reference types, Clean Architecture, and SOLID principles.
Split production code into Domain, Application, Infrastructure, and Api projects.

- Domain has no dependencies on other projects or external packages/frameworks.
- Application references Domain and owns future use cases and required abstractions.
- Infrastructure references Application and Domain and will implement adapters.
- Api references Application and Infrastructure and acts as the composition root.

Keep dependencies pointing inward. Api may reference Infrastructure to compose
implementations; domain and application logic must not depend on hosting,
persistence, or transport details. Introduce abstractions when actual behavior
requires them; do not invent business entities or interfaces for this foundation.

Use separate Domain, Application, Integration, and Architecture test projects.
Test the health endpoint through an in-memory ASP.NET Core host and enforce direct
project dependency boundaries through architecture tests.

## Alternatives considered

A single project would reduce initial setup but would not enforce the requested
compile-time boundaries. Reusing or upgrading GloboTicket conflicts with the
fresh-implementation decision. Neither option is adopted.

## Consequences

IMPLEMENTED: project boundaries and a minimal health endpoint. Additional projects
add setup overhead but support isolated future testing and clear responsibility.
Domain and Application test suites have no cases until behavior is implemented.
Project-reference tests guard declared dependencies; deeper architectural checks
may be needed when substantive code exists.

PLANNED: business behavior, infrastructure services, and deployment configuration.
EXPERIMENTAL: none. This decision makes no performance or research claims.
