# SPEC-004 — Authentication Baseline

Date: 2026-09-23  
Status: IMPLEMENTED in working tree; pending review

## Boundary and scope

Api uses ASP.NET Core authentication services and the standard JWT bearer handler.
Application owns ICurrentIdentity and the immutable AuthenticatedIdentity value
(subject, issuer). Neither Application nor Domain references ASP.NET Core, claims,
JWT libraries, or an identity-provider SDK. The issuer/subject pair identifies a
principal; subject alone is not globally unique. Api provides a scoped adapter
from the validated request principal. Missing context or unauthenticated claims
produce no identity. No claims or identifiers are taken from custom user headers.

This resource-server baseline validates bearer credentials; it does not issue
tokens, implement browser login, or act as an OIDC provider. No RBAC, ABAC, tenant
authorization, risk scoring, or domain business functionality is implemented.
Authentication alone does not prove permission for future protected resources.

## HTTP behavior

| Route | Behavior |
| --- | --- |
| GET /health | Explicitly anonymous; retains the existing healthy JSON response |
| GET /identity | Requires authentication; returns only subject and issuer as JSON |
| Other routes | Authentication required by fallback policy unless explicitly anonymous; authenticated unmatched routes return 404 |

Missing or rejected credentials receive HTTP 401 with a Bearer challenge. Detailed
validation errors and raw tokens are not returned. Middleware runs authentication
before authorization. The framework authorization policy only requires an
authenticated principal; it implements no role or attribute policy.

## Validation and configuration

IMPLEMENTED: require signed tokens, trusted issuer/audience, valid signature,
expiration and lifetime with 30 seconds clock skew, and exactly one nonblank sub
and iss. Disable inbound claim remapping and token saving. Signing keys and issuer
metadata are obtained through HTTPS authority discovery when configured. No signing
secret or private key is stored in application configuration.

Configure these environment variables using the actual provider and API resource:

```text
Authentication__Authority=https://identity.example.invalid
Authentication__Audience=replace-with-api-audience
```

The values above are placeholders. The authority must be an absolute HTTPS URL
without credentials, query, or fragment. Both settings must be supplied together;
invalid or partial configuration fails startup. appsettings.example.json is an
example only and is not automatically loaded; environment variables or normal
ASP.NET Core configuration can supply the settings.

With neither setting configured, the host starts for local health checks but has
no trusted issuer, audience, or signing keys and accepts no bearer tokens. This
is not a development authentication bypass. Deployments must provide real trust
configuration and HTTPS transport. OIDC discovery and signing-key rotation against
a real provider have not been integration-tested in this task.

## Verification

Application tests cover required identity components and issuer-qualified equality.
Integration tests use the real JWT bearer handler with ephemeral RSA keys and local
trust metadata; they do not substitute an always-successful authentication handler.
They cover valid identity mapping, request isolation, absent/malformed tokens,
wrong key/issuer/audience, unsigned/expired/future tokens, missing expiry or subject,
blank subject, configuration rejection, spoofed headers, anonymous claims, and the
unconfigured host. Existing health and architecture tests continue to run.

The test trust configuration exists only in the test host. No real secrets or live
provider dependencies are used. Domain tests remain empty because Domain has no
behavior. Run dotnet restore, dotnet build, and dotnet test from the repository root.

## Limitations and follow-up

PLANNED: provider selection and live discovery tests, deployment HTTPS, provider
specific access-token profile/algorithm restrictions, token revocation/replay
strategy, assurance checks, tenant/resource authorization, audit events, and any
OIDC interactive client. Generic JWT validation does not alone distinguish an ID
token from an access token if a provider uses overlapping audiences; configure a
distinct API audience and define the provider's access-token contract before deployment.
No claim is made that all ARCH-02 or Threat Model V1 controls are complete.
EXPERIMENTAL: none; no research findings or benchmarks.

See [ADR-002](../adr/ADR-002-IDENTITY-BOUNDARY.md) and
[System Architecture](../architecture/SYSTEM_ARCHITECTURE.md).

Implementation reference: [Microsoft ASP.NET Core JWT bearer documentation](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/configure-jwt-bearer-authentication?view=aspnetcore-8.0).
