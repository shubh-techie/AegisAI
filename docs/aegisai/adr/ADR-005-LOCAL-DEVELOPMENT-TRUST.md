# ADR-005 — Explicit local development trust

Date: 2026-09-23  
Status: Accepted for AEGISAI-010; pending review

ADR-002 keeps JWT validation at the API boundary and excludes token issuance from
the application. A clone-and-run local demo needs a credential without requiring
an external provider or adding an identity-server deployment.

Add an offline development console under deployment/docker that generates
short-lived synthetic tokens using an ephemeral RSA key. Only the public key is
installed as trust. The private key is never persisted, and the API never issues
tokens. Keep the standard bearer handler and all existing identity checks.
Require Development, explicit public-key configuration, and absence of production
provider configuration. Reject all other combinations at startup. Restrict this
local path to RS256 and fixed synthetic issuer/audience values.

This is a narrow development-tooling exception to ADR-002's no-local-issuance
statement; production identity, Domain, Application, and external-provider behavior
are unchanged. It is not OAuth/OIDC protocol implementation or an authentication
bypass. Existing identity abstractions need no new dependencies.

An always-authenticated demo handler would bypass signature verification. A full
identity provider would add infrastructure solely for local fixtures. Neither is
needed. The tradeoff is a small explicit development trust surface requiring
security tests and clear operational isolation. Local HTTP is restricted to host
loopback; production HTTPS/provider integration remains PLANNED.

See [SPEC-010](../specs/SPEC-010-V01-ENVIRONMENT.md) for lifecycle and limitations.
