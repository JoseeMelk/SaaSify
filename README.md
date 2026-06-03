# SaaSify

**Subscription & Entitlement Infrastructure for Modern SaaS**

> A side project built with curiosity and a lot of coffee. I'm a Laravel developer exploring the .NET ecosystem — doing my best to apply Clean Architecture principles while learning along the way.

---

## What is SaaSify?

SaaSify is an open-source platform that handles **subscription management and feature access control** for SaaS applications.

Instead of building billing infrastructure from scratch, developers integrate SaaSify and get:

- Plan and subscription management
- Feature access control (entitlements)
- Customer lifecycle management
- API key authentication for runtime checks

The idea is simple: **a developer should be able to protect a feature in their app with a single API call.**

```http
GET /api/v1/entitlements/check?customerId=user_123&feature=export_csv
X-Api-Key: sk_live_...

{
  "allowed": true,
  "plan": "pro",
  "feature": "export_csv",
  "expiresAt": "2025-07-01T00:00:00Z"
}
```

---

## What SaaSify is NOT

- ❌ A payment gateway — we don't process money
- ❌ A Stripe replacement — we complement it
- ❌ A replacement for your app's auth system
- ❌ A CRUD for payments

---

## How it Works

SaaSify has two separate worlds:

### World A — Developer Setup
The developer registers their SaaS project, defines plans and features, and gets an API key.

### World B — Customer Runtime
When a user of the developer's app makes a request, the developer's backend calls SaaSify to check access. The user never interacts with SaaSify directly.

```
User makes request
   ↓
Developer's backend
   ↓
GET /api/v1/entitlements/check?customerId=user_123&feature=export_csv
   ↓
{ "allowed": true }
   ↓
Show or hide the feature
```

---

## Current State — v1 (In Development)

### What's built

| Feature | Status |
|---------|--------|
| Developer auth (JWT + refresh tokens) | ✅ Done |
| Projects with API key generation | ✅ Done |
| Plans with pricing and billing cycle | ✅ Done |
| Feature flags per plan | ✅ Done |
| Customer registration via API | ✅ Done |
| Subscription assignment and lifecycle | ✅ Done |
| Entitlement check endpoint | ✅ Done |
| API key middleware for runtime auth | ✅ Done |

### Subscription lifecycle

SaaSify manages the subscription lifecycle without processing payments:

- The developer charges their customer via Stripe, MercadoPago, or whatever they use
- When payment succeeds, the developer notifies SaaSify: `POST /subscriptions/renew`
- SaaSify updates `currentPeriodEnd` and `renewsAt`
- The entitlement check verifies the period is still valid

| Status | Entitlement Check |
|--------|-------------------|
| Active | ✅ Allowed |
| PastDue | ❌ Blocked |
| Cancelled | ❌ Blocked |
| Expired | ❌ Blocked |

---

## API Reference

### Auth
```
POST /api/auth/register
POST /api/auth/login
POST /api/auth/refresh
POST /api/auth/logout
```

### Projects
```
POST   /api/projects
GET    /api/projects
GET    /api/projects/{id}
POST   /api/projects/{id}/rotate-api-key
```

### Plans
```
POST   /api/projects/{projectId}/plans
GET    /api/projects/{projectId}/plans
GET    /api/projects/{projectId}/plans/{slug}
POST   /api/projects/{projectId}/plans/{planId}/deactivate
```

### Customers
```
POST   /api/projects/{projectId}/customers
GET    /api/projects/{projectId}/customers
GET    /api/projects/{projectId}/customers/{externalId}
```

### Subscriptions
```
GET    /api/projects/{projectId}/customers/{externalId}/subscriptions/active
POST   /api/projects/{projectId}/customers/{externalId}/subscriptions/assign
POST   /api/projects/{projectId}/customers/{externalId}/subscriptions/renew
POST   /api/projects/{projectId}/customers/{externalId}/subscriptions/cancel
```

### Entitlements ⭐
```
GET    /api/v1/entitlements/check?customerId={id}&feature={slug}
       X-Api-Key: sk_live_...
```

---

## Roadmap

### v1 — Core Platform (Current)
- [x] Domain entities with business logic
- [x] Clean Architecture with CQRS + MediatR
- [x] EF Core + PostgreSQL
- [x] JWT authentication + API key middleware
- [x] Full subscription lifecycle
- [x] Entitlement check
- [ ] Global exception middleware
- [ ] Features management endpoints (add/remove features to plans via API)
- [ ] Basic health checks

### v2 — Stability & Operations
- [ ] Background job for subscription expiration (Hangfire)
- [ ] Structured logging with Serilog
- [ ] Rate limiting on entitlement check
- [ ] Redis cache for entitlement check
- [ ] Integration tests

### v3 — Developer Experience
- [ ] Improved error messages
- [ ] Request/response logging
- [ ] API versioning
- [ ] Basic usage metrics per project

### Further ahead
- [ ] Webhooks (subscription_expired, payment_failed, etc.)
- [ ] Trial periods
- [ ] Numeric limits / quotas (max_seats: 10, api_calls: 1000)
- [ ] MercadoPago integration (LATAM friendly)
- [ ] Stripe integration

---

## Architecture

**Clean Architecture** with a **Modular Monolith** approach.

```
SaaSify.Api           → Controllers, Middleware, HTTP
SaaSify.Application   → Commands, Queries, Handlers (CQRS + MediatR)
SaaSify.Domain        → Entities, Business Logic (pure C#, no frameworks)
SaaSify.Infrastructure → EF Core, Repositories, Services
SaaSify.Shared        → Shared types, Results
```

### Key decisions

**Shared schema multi-tenancy** — all tables have a `project_id` column. EF Core Global Query Filters handle tenant isolation automatically.

**Entitlements are a derived query, not a table** — the check is: does this customer have an active subscription to a plan that has this feature enabled? No extra table needed.

**API key authentication for runtime** — the entitlement check uses `X-Api-Key` instead of JWT because it's called from the developer's backend, not from a user session.

---

## Tech Stack

| Component | Technology |
|-----------|-----------|
| Framework | ASP.NET Core 10 |
| ORM | Entity Framework Core |
| Database | PostgreSQL |
| Auth | JWT + BCrypt + SHA-256 API keys |
| Validation | FluentValidation |
| Mediator | MediatR |
| Logging | Serilog |
| API Docs | OpenAPI (.NET 10 native) |

---

## Getting Started

### Prerequisites
- .NET 10 SDK
- PostgreSQL (local or Docker container)

### Setup

```bash
git clone https://github.com/your-username/saasify
cd saasify

# Configure your connection string
# Edit src/SaaSify.Api/appsettings.Development.json

dotnet restore
dotnet build

# Run migrations
dotnet ef database update \
  --project src/SaaSify.Infrastructure \
  --startup-project src/SaaSify.Api

# Start the API
dotnet run --project src/SaaSify.Api
```

### Quick test

```bash
# Register a developer account
POST http://localhost:5138/api/auth/register
{
  "email": "dev@example.com",
  "password": "Secret123",
  "name": "Dev"
}

# Create a project
POST http://localhost:5138/api/projects
Authorization: Bearer {accessToken}
{
  "name": "My SaaS"
}
# Save the apiKey from the response — it's shown only once

# Create a plan
POST http://localhost:5138/api/projects/{projectId}/plans
Authorization: Bearer {accessToken}
{
  "name": "Pro",
  "slug": "pro",
  "price": 29.00,
  "currency": "USD",
  "billingCycle": "Monthly",
  "isPublic": true
}

# Register a customer with a plan
POST http://localhost:5138/api/projects/{projectId}/customers
Authorization: Bearer {accessToken}
{
  "externalId": "user_001",
  "planSlug": "pro"
}

# Check entitlement
GET http://localhost:5138/api/v1/entitlements/check?customerId=user_001&feature=export_csv
X-Api-Key: sk_live_...
```

---

## Notes

This is a personal side project. It's not production-ready yet. Things might break, APIs might change, and some features are still rough around the edges. Contributions, feedback, and issues are welcome.

Built with .NET 10, PostgreSQL, and a lot of learning.

---

*Open-source. No strings attached.*# SaaSify

**Subscription & Entitlement Infrastructure for Modern SaaS**

> A side project built with curiosity and a lot of coffee. I'm a Laravel developer exploring the .NET ecosystem — doing my best to apply Clean Architecture principles while learning along the way.

---

## What is SaaSify?

SaaSify is an open-source platform that handles **subscription management and feature access control** for SaaS applications.

Instead of building billing infrastructure from scratch, developers integrate SaaSify and get:

- Plan and subscription management
- Feature access control (entitlements)
- Customer lifecycle management
- API key authentication for runtime checks

The idea is simple: **a developer should be able to protect a feature in their app with a single API call.**

```http
GET /api/v1/entitlements/check?customerId=user_123&feature=export_csv
X-Api-Key: sk_live_...

{
  "allowed": true,
  "plan": "pro",
  "feature": "export_csv",
  "expiresAt": "2025-07-01T00:00:00Z"
}
```

---

## What SaaSify is NOT

- ❌ A payment gateway — we don't process money
- ❌ A Stripe replacement — we complement it
- ❌ A replacement for your app's auth system
- ❌ A CRUD for payments

---

## How it Works

SaaSify has two separate worlds:

### World A — Developer Setup
The developer registers their SaaS project, defines plans and features, and gets an API key.

### World B — Customer Runtime
When a user of the developer's app makes a request, the developer's backend calls SaaSify to check access. The user never interacts with SaaSify directly.

```
User makes request
   ↓
Developer's backend
   ↓
GET /api/v1/entitlements/check?customerId=user_123&feature=export_csv
   ↓
{ "allowed": true }
   ↓
Show or hide the feature
```

---

## Current State — v1 (In Development)

### What's built

| Feature | Status |
|---------|--------|
| Developer auth (JWT + refresh tokens) | ✅ Done |
| Projects with API key generation | ✅ Done |
| Plans with pricing and billing cycle | ✅ Done |
| Feature flags per plan | ✅ Done |
| Customer registration via API | ✅ Done |
| Subscription assignment and lifecycle | ✅ Done |
| Entitlement check endpoint | ✅ Done |
| API key middleware for runtime auth | ✅ Done |

### Subscription lifecycle

SaaSify manages the subscription lifecycle without processing payments:

- The developer charges their customer via Stripe, MercadoPago, or whatever they use
- When payment succeeds, the developer notifies SaaSify: `POST /subscriptions/renew`
- SaaSify updates `currentPeriodEnd` and `renewsAt`
- The entitlement check verifies the period is still valid

| Status | Entitlement Check |
|--------|-------------------|
| Active | ✅ Allowed |
| PastDue | ❌ Blocked |
| Cancelled | ❌ Blocked |
| Expired | ❌ Blocked |

---

## API Reference

### Auth
```
POST /api/auth/register
POST /api/auth/login
POST /api/auth/refresh
POST /api/auth/logout
```

### Projects
```
POST   /api/projects
GET    /api/projects
GET    /api/projects/{id}
POST   /api/projects/{id}/rotate-api-key
```

### Plans
```
POST   /api/projects/{projectId}/plans
GET    /api/projects/{projectId}/plans
GET    /api/projects/{projectId}/plans/{slug}
POST   /api/projects/{projectId}/plans/{planId}/deactivate
```

### Customers
```
POST   /api/projects/{projectId}/customers
GET    /api/projects/{projectId}/customers
GET    /api/projects/{projectId}/customers/{externalId}
```

### Subscriptions
```
GET    /api/projects/{projectId}/customers/{externalId}/subscriptions/active
POST   /api/projects/{projectId}/customers/{externalId}/subscriptions/assign
POST   /api/projects/{projectId}/customers/{externalId}/subscriptions/renew
POST   /api/projects/{projectId}/customers/{externalId}/subscriptions/cancel
```

### Entitlements ⭐
```
GET    /api/v1/entitlements/check?customerId={id}&feature={slug}
       X-Api-Key: sk_live_...
```

---

## Roadmap

### v1 — Core Platform (Current)
- [x] Domain entities with business logic
- [x] Clean Architecture with CQRS + MediatR
- [x] EF Core + PostgreSQL
- [x] JWT authentication + API key middleware
- [x] Full subscription lifecycle
- [x] Entitlement check
- [ ] Global exception middleware
- [ ] Features management endpoints (add/remove features to plans via API)
- [ ] Basic health checks

### v2 — Stability & Operations
- [ ] Background job for subscription expiration (Hangfire)
- [ ] Structured logging with Serilog
- [ ] Rate limiting on entitlement check
- [ ] Redis cache for entitlement check
- [ ] Integration tests

### v3 — Developer Experience
- [ ] Improved error messages
- [ ] Request/response logging
- [ ] API versioning
- [ ] Basic usage metrics per project

### Further ahead
- [ ] Webhooks (subscription_expired, payment_failed, etc.)
- [ ] Trial periods
- [ ] Numeric limits / quotas (max_seats: 10, api_calls: 1000)
- [ ] MercadoPago integration (LATAM friendly)
- [ ] Stripe integration

---

## Architecture

**Clean Architecture** with a **Modular Monolith** approach.

```
SaaSify.Api           → Controllers, Middleware, HTTP
SaaSify.Application   → Commands, Queries, Handlers (CQRS + MediatR)
SaaSify.Domain        → Entities, Business Logic (pure C#, no frameworks)
SaaSify.Infrastructure → EF Core, Repositories, Services
SaaSify.Shared        → Shared types, Results
```

### Key decisions

**Shared schema multi-tenancy** — all tables have a `project_id` column. EF Core Global Query Filters handle tenant isolation automatically.

**Entitlements are a derived query, not a table** — the check is: does this customer have an active subscription to a plan that has this feature enabled? No extra table needed.

**API key authentication for runtime** — the entitlement check uses `X-Api-Key` instead of JWT because it's called from the developer's backend, not from a user session.

---

## Tech Stack

| Component | Technology |
|-----------|-----------|
| Framework | ASP.NET Core 10 |
| ORM | Entity Framework Core |
| Database | PostgreSQL |
| Auth | JWT + BCrypt + SHA-256 API keys |
| Validation | FluentValidation |
| Mediator | MediatR |
| Logging | Serilog |
| API Docs | OpenAPI (.NET 10 native) |

---

## Getting Started

### Prerequisites
- .NET 10 SDK
- PostgreSQL (local or Docker container)

### Setup

```bash
git clone https://github.com/JoseeMelk/SaaSify.git
cd saasify

# Configure your connection string
# Edit src/SaaSify.Api/appsettings.Development.json or appsettings.json

dotnet restore
dotnet build

# Run migrations
dotnet ef database update \
  --project src/SaaSify.Infrastructure \
  --startup-project src/SaaSify.Api

# Start the API
dotnet run --project src/SaaSify.Api
```

### Quick test

```bash
# Register a developer account
POST http://localhost:5138/api/auth/register
{
  "email": "dev@example.com",
  "password": "Secret123",
  "name": "Dev"
}

# Create a project
POST http://localhost:5138/api/projects
Authorization: Bearer {accessToken}
{
  "name": "My SaaS"
}
# Save the apiKey from the response — it's shown only once

# Create a plan
POST http://localhost:5138/api/projects/{projectId}/plans
Authorization: Bearer {accessToken}
{
  "name": "Pro",
  "slug": "pro",
  "price": 29.00,
  "currency": "USD",
  "billingCycle": "Monthly",
  "isPublic": true
}

# Register a customer with a plan
POST http://localhost:5138/api/projects/{projectId}/customers
Authorization: Bearer {accessToken}
{
  "externalId": "user_001",
  "planSlug": "pro"
}

# Check entitlement
GET http://localhost:5138/api/v1/entitlements/check?customerId=user_001&feature=export_csv
X-Api-Key: sk_live_...
```

---

## Notes

This is a personal side project. It's not production-ready yet. Things might break, APIs might change, and some features are still rough around the edges. Contributions, feedback, and issues are welcome.

Built with .NET 10, PostgreSQL, and a lot of learning.

---

*Open-source. No strings attached.*