# Legacy Inspection Snapshot

**Historical snapshot, superseded by AEGISAI-001 (2026-09-23).** The descriptions
below refer to the legacy checkpoint before removal, not the current working tree.
The current foundation contains documentation and placeholders only; no application
or solution exists. Legacy code will not be reused. See [README](../../README.md).

This document records what is currently present in the repository at the start of the AegisAI foundation branch. It intentionally separates existing implementation from planned AegisAI work.

## Repository Snapshot

- Repository: `AegisAI`
- Current branch: `feature/aegisai-foundation`
- Legacy checkpoint tag: `securing-microservices-legacy`
- Historical commits visible at inspection time:
  - `e2a81c9` - `Initial commit` - authored and committed on February 8, 2022
  - `b9518fa` - `added project` - authored and committed on February 9, 2022

The current codebase is the original Securing-Microservices implementation. It is a .NET Core 3.1 sample-style microservices system named GloboTicket.

## Solution and Projects

The repository contains one Visual Studio solution:

- `GloboTicket.sln`

The solution includes these projects:

- `GloboTicket.Client/GloboTicket.Web.csproj`
- `GloboTicket.Gateway/GloboTicket.Gateway.csproj`
- `GloboTicket.Services.Identity/GloboTicket.Services.Identity.csproj`
- `GloboTicket.Services.EventCatalog/GloboTicket.Services.EventCatalog.csproj`
- `GloboTicket.Services.ShoppingBasket/GloboTicket.Services.ShoppingBasket.csproj`
- `GloboTicket.Services.Discount/GloboTicket.Services.Discount.csproj`
- `GloboTicket.Services.Order/GloboTicket.Services.Ordering.csproj`
- `GloboTicket.Integration.Messages/GloboTicket.Integration.Messages.csproj`
- `GloboTicket.Integration.MessagingBus/GloboTicket.Integration.MessagingBus.csproj`

## Runtime and Frameworks

Existing implementation:

- ASP.NET Core / .NET Core 3.1 web projects
- C#
- Razor MVC client application
- Ocelot API gateway
- IdentityServer4 identity service
- Entity Framework Core with SQL Server LocalDB connection strings
- Azure Service Bus integration code
- Swagger via Swashbuckle on API services
- AutoMapper for DTO mapping
- Polly for selected HTTP retry and circuit breaker behavior

Not currently implemented:

- .NET 8 migration
- PostgreSQL
- Redis
- Kafka
- Kubernetes manifests
- OpenTelemetry instrumentation
- Python or FastAPI services
- ML or anomaly detection components
- AegisAI-specific risk engine, authorization engine, or policy decision engine

## Existing Architecture

The current system is a ticketing-oriented microservices application:

- A Razor MVC web client signs users in and calls backend APIs.
- An Ocelot gateway routes selected upstream paths to downstream services.
- IdentityServer4 issues tokens for interactive, machine-to-machine, and token-exchange scenarios.
- Event catalog, shopping basket, discount, and ordering services store service-owned data in SQL Server databases.
- Shopping basket checkout publishes messages to Azure Service Bus.
- Ordering consumes checkout messages from Azure Service Bus and creates orders after validating the propagated security context.

## Authentication and Authorization

Existing authentication:

- `GloboTicket.Services.Identity` uses IdentityServer4 with in-memory identity resources, API scopes, API resources, clients, and test users.
- `GloboTicket.Client` uses cookie authentication plus OpenID Connect authorization code flow against `https://localhost:5010/`.
- `GloboTicket.Gateway` validates JWT bearer tokens issued by the Identity service.
- `GloboTicket.Services.EventCatalog` and `GloboTicket.Services.ShoppingBasket` validate JWT bearer tokens directly.
- Gateway and shopping basket code include token-exchange flows for downstream service calls.

Existing authorization:

- The client applies a global authenticated-user MVC authorization filter.
- Event catalog and shopping basket apply global authenticated-user controller policies.
- Gateway route configuration includes scope checks for some shopping basket routes.
- Event catalog gateway route scope checks are present but commented out.
- Discount and ordering services call `UseAuthorization`, but no explicit authentication configuration or global authorization policy is present in the inspected startup files.

Not currently implemented:

- RBAC model
- ABAC model
- Context-aware authorization
- Risk-based authorization
- Adaptive policy decisions such as `STEP_UP` or `LIMIT`
- Centralized AegisAI authorization engine

## API Implementation

Existing API surface includes:

- Event catalog:
  - Read categories
  - Read events
  - Read events by category
  - Read event by ID
- Shopping basket:
  - Create and read baskets
  - Add, update, remove basket lines
  - Checkout a basket
  - Record and read basket change events
- Discount:
  - Read discount coupon by user ID
  - Read discount coupon by coupon ID
- Ordering:
  - Read orders for a user
  - Create orders from Azure Service Bus checkout messages

## Database Usage

Existing data access:

- Entity Framework Core DbContext per service.
- SQL Server LocalDB connection strings in service `appsettings.json` files.
- EF migrations are present in service projects.
- Seed data is present in DbContext `OnModelCreating` methods for event catalog, shopping basket event data, and discount coupons.

Current databases by connection string name:

- `GloboTicketEventCatalogDb`
- `GloboTicketShoppingBasketDb`
- `GloboTicketDiscountDb`
- `GloboTicketOrderDb`

## Messaging

Existing messaging:

- `GloboTicket.Integration.Messages` defines a base integration message with security context.
- `GloboTicket.Integration.MessagingBus` publishes messages to Azure Service Bus.
- Shopping basket checkout publishes a checkout message.
- Ordering consumes checkout messages from the `checkoutmessage` topic using subscription `globoticketorder`.
- Ordering validates an exchanged access token before creating an order.

Current concern:

- Azure Service Bus connection information is present in committed code/configuration. The secret value should be rotated and moved to local secret storage or deployment environment variables.

## Docker and Deployment

No Dockerfile, Docker Compose file, Kubernetes manifest, or deployment folder was found during repository inspection.

Some Visual Studio Azure service dependency metadata files are present under service `Properties/` folders.

## Tests

No dedicated test project was found in the solution.

The repository currently has no visible unit, integration, benchmark, or security test suite.

## Documentation

Before this AegisAI foundation work, `README.md` only identified the repository as `Securing-Microservices` and described it as "Securing Microservices in ASP.NET Core."

## Build Artifacts and Local Files

The repository currently contains checked-in or present local build artifacts and IDE/generated files:

- `bin/`
- `obj/`
- `.DS_Store`
- `.idea/`

The `.gitignore` already contains common Visual Studio and .NET ignore patterns for `bin`, `obj`, logs, user files, and build outputs. This foundation work extends it for macOS, JetBrains Rider, local environment files, and secret patterns.

## Superseded Reuse Proposal (not adopted)

The initial draft proposed the following reuse options. AEGISAI-001 rejects these options:

- A concrete distributed-system security baseline.
- A working example of OIDC/JWT flows in a microservice topology.
- A gateway-mediated service routing example.
- A token exchange and delegated downstream access example.
- A service-owned database boundary example.
- An asynchronous message flow with propagated security context.
- A realistic target for future authorization, risk, telemetry, and policy experiments.

## Current Technical Debt and Risks

- The code targets .NET Core 3.1, which is obsolete.
- IdentityServer4 4.x is no longer the recommended long-term foundation for new production systems.
- Secrets and client credentials are committed in source/configuration.
- Azure Service Bus connection information appears in code/configuration.
- Some services lack explicit authentication despite calling `UseAuthorization`.
- Authorization is scope/authenticated-user oriented, not RBAC/ABAC/risk-based.
- Gateway scope enforcement is inconsistent across routes.
- No automated tests are present.
- No Docker or reproducible local orchestration is present.
- Build artifacts and IDE files are present in the working tree.
- Service configuration is localhost-oriented and not yet environment-driven.
