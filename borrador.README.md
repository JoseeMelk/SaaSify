# SaaSify — Subscription & Entitlement Infrastructure

**Versión:** 0.1.0 MVP  
**Estado:** En desarrollo — Architecture & Domain completado  
**Última actualización:** Mayo 2025

---

## Tabla de Contenidos

1. [Visión y Filosofía](#visión-y-filosofía)
2. [Problema que Resuelve](#problema-que-resuelve)
3. [¿Qué NO es SaaSify?](#qué-no-es-saasify)
4. [Público Objetivo](#público-objetivo)
5. [MVP — Definición](#mvp--definición)
6. [Arquitectura General](#arquitectura-general)
7. [Los Dos Mundos](#los-dos-mundos)
8. [Requerimientos Funcionales MVP](#requerimientos-funcionales-mvp)
9. [Requerimientos No Funcionales](#requerimientos-no-funcionales)
10. [Modelo de Datos](#modelo-de-datos)
11. [Gestión de Suscripciones — Sin Cobrar](#gestión-de-suscripciones--sin-cobrar)
12. [Multi-Tenancy](#multi-tenancy)
13. [Stack Tecnológico](#stack-tecnológico)
14. [Roadmap](#roadmap)
15. [Modelo de Monetización](#modelo-de-monetización)

---

## Visión y Filosofía

### Misión

**Convertirse en la infraestructura open-source de monetización para SaaS modernos.**

Hacer que agregar planes, suscripciones y control de acceso a un SaaS tome **minutos, no semanas**.

### Principios Fundamentales

#### 1. Developer First

El sistema debe sentirse:
- **Simple** — API intuitiva, sin sorpresas
- **Limpio** — código mantenible y extensible
- **Rápido de integrar** — 10 minutos desde cero a primera feature controlada
- **Bien documentado** — cada decisión explicada
- **Predecible** — sin comportamientos mágicos

#### 2. API First

- Todo debe poder hacerse mediante API REST
- El dashboard web es secundario — decorativo
- Los SDKs en otros lenguajes lo hacen más fácil, pero la API es lo fundamental

#### 3. Open Core

- El núcleo es open-source — hospedable en cualquier lado
- La monetización viene del SaaS cloud y funcionalidades premium
- Los clientes confían porque pueden auditar el código

#### 4. Infrastructure, Not CRUD

❌ **No somos** una pasarela de pago simple o un CRUD de suscripciones

✅ **Somos** infraestructura — orquestamos el ciclo completo de monetización

#### 5. Self-Host Friendly (Futuro)

Eventualmente, el self-hosting será parte de la estrategia. Por ahora, el enfoque es el SaaS cloud.

#### 6. LATAM Friendly

- Integraciones con MercadoPago, transferencias locales
- Soporte para múltiples monedas desde el día 1
- No asumimos que todos usan Stripe

---

## Problema que Resuelve

### La Realidad del SaaS Indie

Los desarrolladores **saben construir productos**. Lo que **no quieren hacer** es construir infraestructura de billing:

**Problemas típicos:**
- Gestión de planes y suscripciones
- Control de acceso por plan
- Cálculo de límites de uso
- Renovación automática de períodos
- Sincronización con proveedores de pago
- Estados de pago y reintentos
- Webhooks y notificaciones
- Auditoría y cumplimiento normativo

Todo eso consume **semanas de desarrollo**, es **propenso a bugs** y **difícil de mantener**.

### La Solución

SaaSify asume toda esa carga. El developer:

1. Se registra en SaaSify
2. Define sus planes
3. Registra sus clientes
4. Consulta una API para saber si pueden acceder a una feature
5. El resto, SaaSify lo maneja

---

## ¿Qué NO es SaaSify?

### No Procesamos Pagos

- ❌ No cobro tarjetas
- ❌ No manejo PCI compliance
- ❌ No gestiono reembolsos
- ❌ No hago cumplimiento fiscal

**¿Por qué?** Eso es trabajo de Stripe, MercadoPago, etc. Nosotros no competimos, complementamos.

### No es una Pasarela de Pago

SaaSify está un nivel arriba. Las pasarelas procesan dinero. Nosotros **gestionamos el acceso basado en lo que pagaron**.

### No Reemplaza la Auth del Developer

- El developer mantiene su propia autenticación
- SaaSify solo sabe de sus usuarios por un ID externo (`externalId`)
- Cero complejidad de SSO o migración

---

## Público Objetivo

### Primario (MVP enfocado aquí)

- **Indie hackers** — desarrolladores individuales
- **Micro SaaS** — startups con <50 clientes
- **Early-stage startups** — equipo técnico, presupuesto limitado
- **Equipos pequeños** — <5 developers

### Secundario (Futuro)

- **Agencias** — para vender SaaS a sus clientes
- **Empresas pequeñas** — <100 empleados
- **Productos internos** — para monetizar herramientas internas
- **Plataformas multi-tenant** — que venden acceso a terceros

---

## MVP — Definición

### El Aha Moment (10 minutos)

Un developer puede:

1. **Minuto 1-2:** Crear cuenta en SaaSify
2. **Minuto 3-5:** Definir sus planes (Free, Pro, Enterprise) con features
3. **Minuto 6-7:** Registrar un cliente de prueba
4. **Minuto 8-9:** Asignar el cliente a un plan
5. **Minuto 10:** Una llamada API le dice si ese cliente puede acceder a una feature

### Endpoint Central del MVP

```http
GET /api/v1/entitlements/check?customerId=user_123&feature=export_csv
X-Api-Key: sk_live_...

{
  "allowed": true,
  "plan": "pro",
  "feature": "export_csv"
}
```

Este endpoint es la fuente de verdad. Todo lo demás es infraestructura para que esta llamada sea posible.

### Incluido en el MVP

| Feature | Estado |
|---------|--------|
| Registro de developer | ✅ MVP |
| Login con JWT | ✅ MVP |
| Crear proyectos | ✅ MVP |
| Generar API keys | ✅ MVP |
| Definir planes | ✅ MVP |
| Agregar features a planes | ✅ MVP |
| Registrar customers | ✅ MVP |
| Asignar suscripciones | ✅ MVP |
| Entitlement check | ✅ MVP |
| Dashboard básico | ✅ MVP |

### Excluido del MVP (Fase 2+)

| Feature | Fase |
|---------|------|
| Stripe / MercadoPago | v2 |
| Webhooks | v2 |
| Usage tracking | v2 |
| Trials y expiración auto | v2 |
| Límites numéricos (quotas) | v2 |
| Analytics / MRR | v3 |
| SDKs (.NET, JS, PHP) | v4 |
| Cloud hosted SaaS | v5 |
| Self-hosting (Docker) | Futuro |

---

## Arquitectura General

### Estilo Arquitectónico

**Clean Architecture** con **Modular Monolith**

```
┌─────────────────────────────────────┐
│         Presentación (Api)          │
├─────────────────────────────────────┤
│      Aplicación (Use Cases)         │
├─────────────────────────────────────┤
│   Dominio (Lógica de Negocio)       │
├─────────────────────────────────────┤
│    Infraestructura (EF Core, DB)    │
└─────────────────────────────────────┘
```

### Principios Aplicados

- **Separation of Concerns** — cada capa tiene una responsabilidad clara
- **SOLID** — especialmente Dependency Inversion
- **Domain-Driven Design** — el negocio está primero
- **No Frameworks en el Domain** — puro C#

---

## Los Dos Mundos

SaaSify tiene **dos contextos completamente separados**:

### Mundo A: Developer Setup (Una sola vez)

El developer **configura su producto mediante API y consulta dashboard**:

```
Developer (Setup inicial)
   ↓
[SaaSify API] — Crear proyecto, planes, features
   ↓
[Dashboard web] — Ver estadísticas, gestionar excepciones

Developer (Durante operación)
   ↓
[SaaS Backend del developer] — Crea customers via SaaSify API en masa
```

**SaaSify maneja:**
- Registro y login del developer
- Proyectos y API keys
- Definición de planes
- Definición de features por plan
- **Creación de customers (programaticamente)**
- **Asignación de suscripciones**

### Mundo B: Customer Runtime

Los usuarios finales del SaaS del developer **nunca tocan SaaSify**:

```
Customer (usuario final)
   ↓
[SaaS del developer] ← Consulta
   ↓
[SaaSify API] — ¿Puede acceder a X?
   ↓
[SaaS del developer] — Muestra/oculta feature
```

**Flujo:**
1. El backend del developer recibe una request de su usuario
2. Busca ese usuario en su base de datos
3. Obtiene su ID (`user_123`)
4. Llama a SaaSify: `GET /entitlements/check?customerId=user_123&feature=export`
5. SaaSify devuelve `allowed: true/false`
6. El backend del developer permite o bloquea la acción

**Punto clave:** El developer maneja completamente la autenticación de sus usuarios. SaaSify solo sabe de ellos por un ID externo.

---

## Requerimientos Funcionales MVP

### Autenticación del Developer

```
POST /auth/register
POST /auth/login
POST /auth/refresh
POST /auth/logout
```

- JWT con refresh tokens
- Password hash con BCrypt
- Recuperación de contraseña (fuera del MVP)

### Proyectos

```
POST /projects
GET /projects/{id}
GET /projects
PATCH /projects/{id}
```

Cada proyecto del developer es un universo separado de planes, clientes, suscripciones.

### Planes

```
POST /projects/{projectId}/plans
GET /projects/{projectId}/plans
GET /projects/{projectId}/plans/{slug}
PATCH /projects/{projectId}/plans/{id}
DELETE /projects/{projectId}/plans/{id}
```

Planes: Free, Pro, Enterprise, etc. Cada uno con features y precio.

### Features

```
POST /projects/{projectId}/plans/{planId}/features
PATCH /projects/{projectId}/plans/{planId}/features/{id}
DELETE /projects/{projectId}/plans/{planId}/features/{id}
```

Features: booleanas en el MVP (v2 agrega límites numéricos).

### Customers — Creados por la API del Developer

```
POST /projects/{projectId}/customers
  → Crea un customer nuevo con suscripción inicial al plan

GET /projects/{projectId}/customers/{externalId}
  → Obtiene un customer existente (para verificar estado)

GET /projects/{projectId}/customers
  → Lista todos los customers (para dashboard/analytics)

PATCH /projects/{projectId}/customers/{customerId}
  → Actualiza info del customer (email, name)
```

**Punto crítico:** Los customers se crean **programáticamente via API**, no manualmente en el dashboard.

**Flujo típico:**
1. Usuario se registra en el SaaS del developer
2. Backend del developer llama: `POST /customers` con `externalId`, email, nombre y plan inicial
3. SaaSify crea el customer y lo asigna al plan
4. Devuelve el customer creado
5. Listo — el usuario ya tiene acceso a sus features inmediatamente

El `externalId` es el ID único del usuario en la base de datos del SaaS del developer. SaaSify no autentica a estos usuarios, solo los identifica y gestiona su acceso.

### Subscripciones

```
POST /projects/{projectId}/customers/{customerId}/subscriptions
GET /projects/{projectId}/customers/{customerId}/subscriptions
POST /projects/{projectId}/customers/{customerId}/subscriptions/{id}/renew
POST /projects/{projectId}/customers/{customerId}/subscriptions/{id}/cancel
```

Asignación de planes a customers y ciclo de vida.

### El Endpoint Crítico: Entitlement Check

```
GET /api/v1/entitlements/check?customerId=user_123&feature=export_csv
```

**Respuesta:**
```json
{
  "allowed": true,
  "plan": "pro",
  "feature": "export_csv",
  "expiresAt": "2025-06-26T14:30:00Z"
}
```

Este endpoint es consultado **miles de veces por segundo**. Debe ser:
- **Rápido** — <10ms con cache
- **Fiable** — nunca debe fallar
- **Simple** — sin parámetros complejos

---

## Requerimientos No Funcionales

### Escalabilidad

- Soportar 1000+ developers
- Soportar 100k+ customers por developer
- Miles de entitlement checks por segundo
- Cache agresivo en Redis

### Seguridad

- JWT seguro con expiración corta
- API keys hasheadas (SHA-256)
- Rate limiting por IP/API key
- CORS configurado
- SQL injection: prevenido por EF Core

### Performance

- Entitlement check <10ms
- Índices en todas las búsquedas frecuentes
- Connection pooling en PostgreSQL
- Redis para cache de planes y features

### Observabilidad

- Logs estructurados con Serilog
- Health checks
- Tracing básico de requests
- Métricas de uso por developer

### Mantenibilidad

- Clean Architecture
- SOLID principles
- Unit tests para lógica crítica
- Integration tests para repositorios
- Documentación API con Swagger

---

## Modelo de Datos

### 6 Entidades MVP

#### 1. User
```csharp
Id: Guid (PK)
Email: string (UNIQUE)
PasswordHash: string
Name: string
Status: enum [Active, Suspended, Deleted]
CreatedAt: DateTime
UpdatedAt: DateTime?
DeletedAt: DateTime?
```

El developer que se registra en SaaSify.

#### 2. Project
```csharp
Id: Guid (PK)
OwnerId: Guid (FK → User)
Name: string
Slug: string (UNIQUE)
ApiKeyHash: string
ApiKeyPrefix: string (UNIQUE)
Status: enum [Active, Suspended]
CreatedAt: DateTime
UpdatedAt: DateTime?
DeletedAt: DateTime?
```

El SaaS que el developer registra.

#### 3. Plan
```csharp
Id: Guid (PK)
ProjectId: Guid (FK → Project)
Name: string
Slug: string (UNIQUE per project)
Price: decimal?
Currency: string?
BillingCycle: enum [Monthly, Yearly]?
IsActive: bool
IsPublic: bool
CreatedAt: DateTime
UpdatedAt: DateTime?
DeletedAt: DateTime?
```

Planes: Free, Pro, Enterprise, etc.

#### 4. Feature
```csharp
Id: Guid (PK)
PlanId: Guid (FK → Plan)
Slug: string
IsEnabled: bool
CreatedAt: DateTime
UpdatedAt: DateTime?
DeletedAt: DateTime?
```

Features owned por Plan. No existen independientemente.

#### 5. Customer
```csharp
Id: Guid (PK)
ProjectId: Guid (FK → Project)
ExternalId: string (unique per project)
Email: string?
Name: string?
CreatedAt: DateTime
UpdatedAt: DateTime?
DeletedAt: DateTime?
```

Usuario final del SaaS del developer. SaaSify solo sabe su ID externo.

#### 6. Subscription
```csharp
Id: Guid (PK)
CustomerId: Guid (FK → Customer)
PlanId: Guid (FK → Plan)
Status: enum [Active, PastDue, Cancelled, Expired]
BillingCycle: enum [Monthly, Yearly]
PaymentMethod: enum? [Card, Transfer, Cash, Other]
ExternalPaymentRef: string?
StartedAt: DateTime
CurrentPeriodStart: DateTime
CurrentPeriodEnd: DateTime (← la fuente de verdad)
RenewsAt: DateTime?
CancelAtPeriodEnd: bool
CancelledAt: DateTime?
CreatedAt: DateTime
UpdatedAt: DateTime?
DeletedAt: DateTime?
```

Relación Customer ↔ Plan. El ciclo de vida de la suscripción.

### Índices Críticos

| Tabla | Índice | Tipo | Razón |
|-------|--------|------|-------|
| users | (email) | UNIQUE | Login rápido |
| projects | (slug) | UNIQUE | Lookup por nombre |
| projects | (api_key_prefix) | UNIQUE | Auth rápida |
| plans | (project_id, slug) | UNIQUE | Feature lookup |
| customers | (project_id, external_id) | UNIQUE | Búsqueda principal |
| subscriptions | (customer_id) WHERE status='Active' | PARTIAL UNIQUE | Solo 1 activa por customer |
| subscriptions | (current_period_end) | INDEX | Job de expiración |

---

## Gestión de Suscripciones — Sin Cobrar

### Lo que SaaSify Gestiona

✅ Fechas de inicio y vencimiento  
✅ Próxima fecha de renovación  
✅ Duración del ciclo (mensual/anual)  
✅ Método de pago registrado  
✅ Estado de la suscripción  
✅ Historial de renovaciones  

### Lo que SaaSify NO Hace

❌ Cobrar tarjetas  
❌ Crear cargos en Stripe  
❌ Emitir facturas  
❌ Manejar reembolsos  
❌ Reintentar pagos fallidos  
❌ Cumplimiento fiscal/PCI  

### El Flujo

1. **Developer cobra al customer en su sistema** (Stripe, MercadoPago, efectivo)
2. **Pago es exitoso**
3. **Developer notifica a SaaSify:**
   ```
   POST /subscriptions/{id}/renew
   {
     "externalPaymentRef": "pi_stripe_abc123"
   }
   ```
4. **SaaSify actualiza:**
   - `CurrentPeriodStart` = hoy
   - `CurrentPeriodEnd` = hoy + 1 mes (o 1 año)
   - `RenewsAt` = nuevo `CurrentPeriodEnd`
   - `Status` = Active
5. **Background job cada noche:**
   - Detecta suscripciones donde `CurrentPeriodEnd` < hoy
   - Las marca como `PastDue` si no fueron renovadas

### Estados de Suscripción

| Estado | Significado | Entitlement Check |
|--------|-------------|-------------------|
| Active | Activa y vigente | ✅ Allowed |
| PastDue | Venció pero dev no confirmó pago | ❌ Blocked |
| Cancelled | Cancelada por el customer | ❌ Blocked |
| Expired | Venció sin renovación | ❌ Blocked |

---

## Multi-Tenancy

### Estrategia: Shared Schema

**Un solo PostgreSQL con columna `project_id` en cada tabla.**

```
┌─────────────────────────────┐
│      PostgreSQL             │
├─────────────────────────────┤
│ customers:                  │
│  - id, project_id ← filter  │
│  - external_id              │
│  - ...                      │
└─────────────────────────────┘
```

### Global Query Filters (EF Core)

```csharp
// Automáticamente se inyecta en cada query
WHERE project_id = {currentProjectId}
AND deleted_at IS NULL
```

**Ventajas:**
- Simple de implementar
- Self-host fácil
- Difícil de romper accidentalmente
- Escalable hasta millones de records

**Desventaja:**
- Un bug de filtro expone datos de otro tenant
- Aislamiento lógico, no físico

---

## Stack Tecnológico

### Backend

| Componente | Tecnología | Razón |
|------------|-----------|-------|
| Framework | ASP.NET Core 10 | Moderno, performante, C# |
| ORM | Entity Framework Core | Migrations, LINQ, productividad |
| Database | PostgreSQL (managed) | Open-source, confiable, índices parciales |
| Cache | Redis (managed) | Rápido, simple, en memoria |
| Background Jobs | Hangfire | Reliable, persistente |
| Validation | FluentValidation | Declarativo, reutilizable |
| Logging | Serilog | Estructurado, múltiples sinks |
| API Docs | Swagger/OpenAPI | Generado automáticamente |
| Auth | JWT + Bearer | Stateless, escalable |
| Deployment | Cloud managed (Heroku/Railway/Render) | Simplificado para MVP |

### Arquitectura

| Capa | Proyecto | Responsabilidad |
|------|----------|-----------------|
| Api | SaaSify.Api | Controllers, Middleware, HTTP |
| Application | SaaSify.Application | Commands, Queries, Use Cases |
| Domain | SaaSify.Domain | Entidades, Lógica de Negocio |
| Infrastructure | SaaSify.Infrastructure | EF Core, Repositorios, Servicios |
| Shared | SaaSify.Shared | DTOs, Results, Helpers |

### Patrón de Aplicación

**CQRS + MediatR**

```
Request → Command/Query → Handler → Use Case → Repository → Response
```

---

## Roadmap

### Fase 1: Core Platform (MVP) ✅ En progreso

- [x] Domain entities y lógica
- [x] EF Core + PostgreSQL
- [ ] Application commands/queries
- [ ] API endpoints básicos
- [ ] Dashboard web simple
- [ ] Deploy a cloud (Heroku/Railway/similar)

### Fase 2: Entitlements & Advanced Features

- [ ] Usage tracking
- [ ] Límites numéricos (quotas)
- [ ] Trials
- [ ] Expiración automática
- [ ] Webhooks
- [ ] Email notifications

### Fase 3: Payment Providers

- [ ] Stripe integration
- [ ] MercadoPago integration
- [ ] Coinbase/cripto (stretch goal)

### Fase 4: SDKs

- [ ] .NET SDK
- [ ] JavaScript/TypeScript SDK
- [ ] PHP SDK

### Fase 5: Self-Hosting (Futuro)

- [ ] Docker Compose para desarrollo
- [ ] Documentación de deployment en servidor propio
- [ ] Scripts de migración
- [ ] Guías de operación

---

## Modelo de Monetización

### Filosofía: Open Source Primero

SaaSify es **fundamentalmente open-source y siempre lo será**.

El código está disponible públicamente. No hay features "cerradas" o secretas. Cualquiera puede auditar, fork, y self-hostear sin restricciones.

### Estrategia (En Exploración)

La monetización es **secundaria** a la misión de resolver el problema. Se explorará cuando haya producto estable y comunidad real.

Opciones consideradas:

#### 1. Cloud Hosting (Probable)

```
Self-hosted (Gratis):
  ✅ Código completo
  ✅ Sin límites
  ✅ Control total
  ❌ Administras todo (DB, backups, escala, seguridad)

Cloud SaaS (Pago):
  ✅ Zero-ops hosting
  ✅ Backups automáticos
  ✅ Monitoreo y alertas
  ✅ Actualizaciones automáticas
  ✅ 99.9% uptime SLA
```

**Modelo freemium cloud:**
- **Free:** 1 proyecto, 100 customers
- **Pro:** $29/mes → 5 proyectos, 10k customers, advanced analytics
- **Enterprise:** Custom → SSO, SLA 99.99%, on-premise, soporte dedicado

**Lógica:** Los desarrolladores pagan por **no tener que pensar en infraestructura**, no por features.

#### 2. Premium Features (Posible)

Analytics, dashboards, email automation, audit logs que mejoren UX.

**Pero:** El código estará disponible. Un developer con self-hosted puede implementar lo mismo si lo necesita. No es un "lock-in".

**Diferenciador:** UI polida, soporte, mantenimiento.

#### 3. Professional Services (Futuro)

- Integración con sistemas legacy
- Consultoría de billing
- Custom development
- Training para equipos

---

### Lo que NO haremos

❌ **Nunca cerraremos features en open-source**

El core de SaaSify (API, entitlements, suscripciones) será siempre gratis y open.

❌ **Nunca habrá "nagware" o limitaciones artificiales**

Si alguien self-hostea, tiene acceso a TODO sin interrupciones.

❌ **Nunca dependeremos de "vendor lock-in"**

Los datos del developer son suyos. Exportable. Migrables.

---

### Timeline

- **Ahora (Fase 1-2):** Construir producto sólido, ganar comunidad
- **Después (Fase 3):** Versión cloud estable, considerar precios
- **Largo plazo:** Modelo que soporte el proyecto sin comprometer valores

---

### Por Qué Este Modelo Funciona

Ejemplos reales de open-source que monetiza sin cerrar código:

| Proyecto | Modelo | Ingresos |
|----------|--------|----------|
| Supabase | Cloud hosting de Postgres | $M's/año |
| GitLab | Cloud + Enterprise Services | $100M+/año |
| Mattermost | Cloud + Self-hosted Enterprise | $M's/año |
| Stripe (CLI) | Open-source, servicios API pagan | $10B+ valuación |

**Patrón común:** El software es gratis, pero la **comodidad, escala y soporte** se pagan.

---

### Compromiso con la Comunidad

Si SaaSify escala y genera ingresos, estos se reinvertirán en:

- Mantenimiento del código open-source
- Mejoras de seguridad
- Documentación
- Soporte comunitario
- Investigación de nuevas features

SaaSify nunca será un proyecto abandonado que "extrae valor" sin reinvertir.

---

## Conclusión

SaaSify es **infraestructura, no una herramienta de pago**. Está diseñada para que los developers se enfoquen en su producto y nosotros manejemos la parte de "quién puede acceder a qué".

**Éxito = un developer en 10 minutos tiene su primer entitlement check funcionando.**

---

*Documento de arquitectura y visión. Sujeto a cambios según feedback durante desarrollo.*
