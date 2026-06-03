# SaaSify

**Infraestructura de Suscripciones y Entitlements para SaaS Modernos**

> Un proyecto personal construido con curiosidad y bastante café. Soy desarrollador de Laravel explorando el ecosistema .NET — haciendo mi mejor esfuerzo para aplicar principios de Clean Architecture mientras aprendo en el camino.

---

## ¿Qué es SaaSify?

SaaSify es una plataforma open-source que gestiona **suscripciones y control de acceso a features** para aplicaciones SaaS.

En vez de construir infraestructura de billing desde cero, los developers integran SaaSify y obtienen:

- Gestión de planes y suscripciones
- Control de acceso por feature (entitlements)
- Gestión del ciclo de vida de customers
- Autenticación por API key para verificaciones en runtime

La idea es simple: **un developer debería poder proteger una feature de su app con una sola llamada a la API.**

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

## Lo que SaaSify NO es

- ❌ Una pasarela de pago — no procesamos dinero
- ❌ Un reemplazo de Stripe — lo complementamos
- ❌ Un reemplazo del sistema de auth de tu app
- ❌ Un CRUD de pagos

---

## Cómo Funciona

SaaSify tiene dos mundos separados:

### Mundo A — Setup del Developer
El developer registra su proyecto SaaS, define planes y features, y obtiene una API key.

### Mundo B — Runtime del Customer
Cuando un usuario de la app del developer hace una request, el backend del developer llama a SaaSify para verificar el acceso. El usuario nunca interactúa con SaaSify directamente.

```
El usuario hace una request
   ↓
Backend del developer
   ↓
GET /api/v1/entitlements/check?customerId=user_123&feature=export_csv
   ↓
{ "allowed": true }
   ↓
Mostrar u ocultar la feature
```

---

## Estado Actual — v1 (En Desarrollo)

### Lo que está construido

| Feature | Estado |
|---------|--------|
| Auth de developer (JWT + refresh tokens) | ✅ Listo |
| Proyectos con generación de API key | ✅ Listo |
| Planes con precios y ciclo de facturación | ✅ Listo |
| Feature flags por plan | ✅ Listo |
| Registro de customers via API | ✅ Listo |
| Asignación y ciclo de vida de suscripciones | ✅ Listo |
| Endpoint de entitlement check | ✅ Listo |
| Middleware de API key para auth en runtime | ✅ Listo |

### Ciclo de vida de suscripciones

SaaSify gestiona el ciclo de vida de las suscripciones sin procesar pagos:

- El developer cobra a su customer via Stripe, MercadoPago, o lo que use
- Cuando el pago es exitoso, el developer notifica a SaaSify: `POST /subscriptions/renew`
- SaaSify actualiza `currentPeriodEnd` y `renewsAt`
- El entitlement check verifica que el período siga vigente

| Estado | Entitlement Check |
|--------|-------------------|
| Active | ✅ Permitido |
| PastDue | ❌ Bloqueado |
| Cancelled | ❌ Bloqueado |
| Expired | ❌ Bloqueado |

---

## Referencia de la API

### Auth
```
POST /api/auth/register
POST /api/auth/login
POST /api/auth/refresh
POST /api/auth/logout
```

### Proyectos
```
POST   /api/projects
GET    /api/projects
GET    /api/projects/{id}
POST   /api/projects/{id}/rotate-api-key
```

### Planes
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

### Suscripciones
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

### v1 — Core Platform (Actual)
- [x] Entidades del dominio con lógica de negocio
- [x] Clean Architecture con CQRS + MediatR
- [x] EF Core + PostgreSQL
- [x] JWT + middleware de API key
- [x] Ciclo de vida completo de suscripciones
- [x] Entitlement check
- [ ] Middleware global de excepciones
- [ ] Endpoints de gestión de features (agregar/quitar features a planes via API)
- [ ] Health checks básicos

### v2 — Estabilidad Operacional
- [ ] Background job para expiración de suscripciones (Hangfire)
- [ ] Logging estructurado con Serilog
- [ ] Rate limiting en el entitlement check
- [ ] Cache en Redis para el entitlement check
- [ ] Tests de integración

### v3 — Experiencia del Developer
- [ ] Mensajes de error mejorados
- [ ] Logging de requests y responses
- [ ] Versionado de la API
- [ ] Métricas básicas de uso por proyecto

### Más adelante
- [ ] Webhooks (subscription_expired, payment_failed, etc.)
- [ ] Períodos de prueba (trials)
- [ ] Límites numéricos / quotas (max_seats: 10, api_calls: 1000)
- [ ] Integración con MercadoPago (friendly para LATAM)
- [ ] Integración con Stripe

---

## Arquitectura

**Clean Architecture** con un enfoque de **Modular Monolith**.

```
SaaSify.Api            → Controllers, Middleware, HTTP
SaaSify.Application    → Commands, Queries, Handlers (CQRS + MediatR)
SaaSify.Domain         → Entidades, Lógica de Negocio (C# puro, sin frameworks)
SaaSify.Infrastructure → EF Core, Repositorios, Servicios
SaaSify.Shared         → Tipos compartidos, Results
```

### Decisiones clave

**Multi-tenancy con shared schema** — todas las tablas tienen una columna `project_id`. Los Global Query Filters de EF Core manejan el aislamiento por tenant automáticamente.

**Los entitlements son una consulta derivada, no una tabla** — la verificación es: ¿tiene este customer una suscripción activa a un plan que tiene esta feature habilitada? No se necesita una tabla extra.

**Autenticación por API key en runtime** — el entitlement check usa `X-Api-Key` en vez de JWT porque lo llama el backend del developer, no una sesión de usuario.

---

## Stack Tecnológico

| Componente | Tecnología |
|------------|-----------|
| Framework | ASP.NET Core 10 |
| ORM | Entity Framework Core |
| Base de datos | PostgreSQL |
| Auth | JWT + BCrypt + API keys con SHA-256 |
| Validación | FluentValidation |
| Mediator | MediatR |
| Logging | Serilog |
| API Docs | OpenAPI (.NET 10 nativo) |

---

## Primeros Pasos

### Requisitos
- .NET 10 SDK
- PostgreSQL (local o contenedor Docker)

### Setup

```bash
git clone https://github.com/tu-usuario/saasify
cd saasify

# Configura tu connection string
# Edita src/SaaSify.Api/appsettings.Development.json

dotnet restore
dotnet build

# Ejecutar migraciones
dotnet ef database update \
  --project src/SaaSify.Infrastructure \
  --startup-project src/SaaSify.Api

# Iniciar la API
dotnet run --project src/SaaSify.Api
```

### Prueba rápida

```bash
# Registrar una cuenta de developer
POST http://localhost:5138/api/auth/register
{
  "email": "dev@example.com",
  "password": "Secret123",
  "name": "Dev"
}

# Crear un proyecto
POST http://localhost:5138/api/projects
Authorization: Bearer {accessToken}
{
  "name": "Mi SaaS"
}
# Guarda el apiKey de la respuesta — solo se muestra una vez

# Crear un plan
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

# Registrar un customer con un plan
POST http://localhost:5138/api/projects/{projectId}/customers
Authorization: Bearer {accessToken}
{
  "externalId": "user_001",
  "planSlug": "pro"
}

# Verificar entitlement
GET http://localhost:5138/api/v1/entitlements/check?customerId=user_001&feature=export_csv
X-Api-Key: sk_live_...
```

---

## Notas

Este es un proyecto personal. No está listo para producción todavía. Las cosas pueden fallar, las APIs pueden cambiar y algunas features están incompletas. Contribuciones, feedback e issues son bienvenidos.

Construido con .NET 10, PostgreSQL y muchas horas de aprendizaje.

---

*Open-source. Sin compromisos.*