# ADR-002 — Identity Boundary

Date: 2026-09-23  
Status: Accepted for AEGISAI-004; implementation pending review

## Context

The target system needs authenticated identity before future authorization.
Clean Architecture requires Domain and Application to remain independent of the
HTTP host, token format, and identity provider. The current task establishes only
an authentication baseline.

## Decision

Application owns ICurrentIdentity exposing a nullable AuthenticatedIdentity with
nonblank subject and issuer. Api adapts the validated HTTP principal through a
scoped HttpCurrentIdentity. ClaimsPrincipal, HttpContext, JWT validation, discovery,
and authentication registration remain in Api. Domain remains unchanged.

Use the standard ASP.NET Core JWT bearer scheme and HTTPS discovery rather than
custom token parsing or a provider-specific SDK. Require issuer, audience,
signature, and lifetime validation and an unambiguous subject. Accept no token if
trust configuration is absent. Incomplete/insecure configuration fails startup.
Do not store provider secrets or implement local token issuance.

Use an authenticated-user fallback policy with explicit anonymous health access.
GET /identity demonstrates the Application boundary with only issuer and subject;
it is not a role, attribute, tenant, or risk decision endpoint. Authentication and
future permission evaluation remain separate concerns.

## Alternatives

Passing ClaimsPrincipal into Application would expose framework and token details
inside the use-case boundary. A provider SDK inside Domain would violate dependency
rules. A custom JWT parser or development bypass would add unnecessary trust logic.
A browser OIDC handler is not required for a bearer-token resource server. These
alternatives are not adopted.

## Consequences and limits

IMPLEMENTED: a small provider-neutral identity contract, standard bearer validation,
and endpoint authentication. The HTTP-specific adapter belongs in Api; future
non-HTTP callers must provide identity through a separately authenticated adapter,
never by treating arbitrary caller-supplied subject strings as authentication.

Issuer changes can change identity; mapping identities across authorities requires
a future explicit design. No role, tenant, email, or name is promoted into the
contract. Future required attributes need separate trust/provenance decisions.

PLANNED: actual provider integration, access-token profile, revocation/replay,
assurance and deployment security, authorization, and audit. EXPERIMENTAL: none.
See [SPEC-004](../specs/SPEC-004-AUTHENTICATION.md) for configuration and tests.
