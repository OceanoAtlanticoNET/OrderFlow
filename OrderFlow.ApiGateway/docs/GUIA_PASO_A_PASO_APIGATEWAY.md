# 🌐 Guía Paso a Paso: OrderFlow.ApiGateway

## API Gateway con YARP para Microservicios

---

## 📋 Índice

| Fase | Tema | Conceptos Clave |
|------|------|-----------------|
| 0 | ¿Qué es un API Gateway? | Patrón, beneficios, YARP |
| 1 | Crear Proyecto | Estructura, dependencias |
| 2 | Configurar YARP | Routes, Clusters, Service Discovery |
| 3 | Autenticación JWT | Validar tokens (no generarlos) |
| 4 | Políticas de Autorización | Anonymous, Authenticated, Admin |
| 5 | Rate Limiting | Redis, políticas por usuario/IP |
| 6 | Integración Aspire | AppHost, referencias |

---

## Fase 0: ¿Qué es un API Gateway?

### 📚 Conceptos Clave

| Concepto | Descripción |
|----------|-------------|
| **API Gateway** | Punto de entrada único para todas las peticiones de clientes |
| **Reverse Proxy** | Recibe peticiones y las reenvía a servicios internos |
| **YARP** | "Yet Another Reverse Proxy" - librería de Microsoft para .NET |
| **Route** | Regla que define qué peticiones van a qué servicio |
| **Cluster** | Grupo de destinos (servicios) que pueden manejar una ruta |

### 🔍 ¿Por qué usar un API Gateway?

```
SIN Gateway:                          CON Gateway:
                                      
┌─────────┐                           ┌─────────┐
│ Cliente │──┬──► Identity            │ Cliente │
└─────────┘  │                        └────┬────┘
             ├──► Catalog                  │
             │                             ▼
             └──► Orders              ┌─────────┐
                                      │ Gateway │──┬──► Identity
El cliente necesita                   └─────────┘  ├──► Catalog
conocer TODOS los                          │       └──► Orders
servicios y sus URLs                       │
                                      El cliente solo
                                      conoce el Gateway
```

### ✅ Beneficios del Gateway

| Beneficio | Descripción |
|-----------|-------------|
| **Punto único de entrada** | Clientes solo conocen una URL |
| **Autenticación centralizada** | JWT se valida una vez en el Gateway |
| **Rate Limiting** | Control de tráfico en un solo lugar |
| **Service Discovery** | Gateway descubre servicios automáticamente |
| **Abstracción** | Clientes no saben cómo están organizados los microservicios |

---

## Fase 1: Crear Proyecto ApiGateway

### 🎯 Objetivo
Crear el proyecto que actuará como punto de entrada.

### 📝 Pasos

#### Paso 1.1: Crear proyecto Web vacío

```bash
dotnet new web -n OrderFlow.ApiGateway
```

#### Paso 1.2: Agregar paquetes NuGet

```xml
<ItemGroup>
  <!-- YARP Reverse Proxy -->
  <PackageReference Include="Yarp.ReverseProxy" />
  
  <!-- Service Discovery con YARP -->
  <PackageReference Include="Microsoft.Extensions.ServiceDiscovery.Yarp" />
  
  <!-- JWT para validar tokens -->
  <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" />
  
  <!-- Redis para Rate Limiting -->
  <PackageReference Include="Aspire.StackExchange.Redis" />
  <PackageReference Include="RedisRateLimiting" />
  <PackageReference Include="RedisRateLimiting.AspNetCore" />
</ItemGroup>

<ItemGroup>
  <!-- Referencias a proyectos compartidos -->
  <ProjectReference Include="..\OrderFlow.ServiceDefaults\OrderFlow.ServiceDefaults.csproj" />
</ItemGroup>
```

#### Paso 1.3: Estructura de carpetas

```
OrderFlow.ApiGateway/
├── Extensions/           ← Métodos de extensión para configuración
│   ├── YarpExtensions.cs
│   ├── JwtAuthenticationExtensions.cs
│   ├── AuthorizationPoliciesExtensions.cs
│   └── RedisRateLimitingExtensions.cs
├── appsettings.json      ← Configuración de rutas YARP
├── appsettings.Development.json
└── Program.cs
```

### ✅ Verificación
- [ ] Proyecto creado
- [ ] Paquetes instalados
- [ ] Referencia a ServiceDefaults agregada

---

## Fase 2: Configurar YARP

### 🎯 Objetivo
Configurar el reverse proxy para enrutar peticiones a Identity.

### 📚 Conceptos Clave

| Concepto | Descripción |
|----------|-------------|
| **Route** | Define el patrón de URL y a qué cluster enviarlo |
| **Cluster** | Define los destinos (servicios) disponibles |
| **Match.Path** | Patrón para hacer match con URLs entrantes |
| **{**catch-all}** | Captura todo el resto de la URL |
| **Service Discovery** | Resuelve `https://orderflow-identity` a la IP real |

### 📝 Pasos

#### Paso 2.1: Configurar rutas en appsettings.json

```json
{
  "ReverseProxy": {
    "Routes": {
      "identity-public": {
        "ClusterId": "identity-cluster",
        "AuthorizationPolicy": "anonymous",
        "Match": {
          "Path": "/api/v{version}/auth/{**catch-all}"
        }
      },
      "identity-admin-users": {
        "ClusterId": "identity-cluster",
        "AuthorizationPolicy": "admin",
        "Match": {
          "Path": "/api/v{version}/admin/users/{**catch-all}"
        }
      },
      "identity-admin-roles": {
        "ClusterId": "identity-cluster",
        "AuthorizationPolicy": "admin",
        "Match": {
          "Path": "/api/v{version}/admin/roles/{**catch-all}"
        }
      },
      "identity-protected": {
        "ClusterId": "identity-cluster",
        "AuthorizationPolicy": "authenticated",
        "Match": {
          "Path": "/api/v{version}/users/{**catch-all}"
        }
      }
    },
    "Clusters": {
      "identity-cluster": {
        "Destinations": {
          "identity-service": {
            "Address": "https://orderflow-identity"
          }
        }
      }
    }
  }
}
```

#### 🔍 Explicación de las rutas

| Ruta | Path | Política | Descripción |
|------|------|----------|-------------|
| `identity-public` | `/api/v{version}/auth/*` | anonymous | Login, Register (públicos) |
| `identity-admin-users` | `/api/v{version}/admin/users/*` | admin | Gestión de usuarios |
| `identity-admin-roles` | `/api/v{version}/admin/roles/*` | admin | Gestión de roles |
| `identity-protected` | `/api/v{version}/users/*` | authenticated | Perfil del usuario actual |

#### Paso 2.2: Crear extensión YARP

Crear `Extensions/YarpExtensions.cs`:

```csharp
namespace OrderFlow.ApiGateway.Extensions;

public static class YarpExtensions
{
    public static IServiceCollection AddYarpReverseProxy(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Service Discovery para resolver nombres de servicio
        services.AddServiceDiscovery();

        // Configurar YARP desde appsettings.json
        services.AddReverseProxy()
            .LoadFromConfig(configuration.GetSection("ReverseProxy"))
            .AddServiceDiscoveryDestinationResolver(); // Resuelve "https://orderflow-identity"

        return services;
    }
}
```

#### 🔍 ¿Cómo funciona Service Discovery?

```
1. appsettings.json define:
   "Address": "https://orderflow-identity"
                         ↓
2. AddServiceDiscoveryDestinationResolver() intercepta
                         ↓
3. Consulta a Aspire el IP:Puerto real del servicio
                         ↓
4. YARP reenvía la petición a: https://10.0.0.5:8081
```

### ✅ Verificación
- [ ] `ReverseProxy` configurado en appsettings.json
- [ ] Rutas definidas para Identity
- [ ] Service Discovery habilitado

---

## Fase 3: Autenticación JWT en el Gateway

### 🎯 Objetivo
Configurar el Gateway para VALIDAR tokens JWT (no generarlos).

### 📚 Conceptos Clave

| Concepto | Descripción |
|----------|-------------|
| **Gateway NO genera tokens** | Solo Identity genera tokens |
| **Gateway VALIDA tokens** | Verifica firma, issuer, audience, expiración |
| **Misma configuración JWT** | Gateway e Identity comparten Secret, Issuer, Audience |

### 📝 Pasos

#### Paso 3.1: Configurar JWT en appsettings.Development.json

```json
{
  "Jwt": {
    "Issuer": "OrderFlow.Identity",
    "Audience": "OrderFlow.Client"
  }
}
```

> 📝 **Nota**: El `Secret` NO va en appsettings. Viene inyectado como variable de entorno desde AppHost, que lo lee de sus User Secrets.

#### Paso 3.2: Configurar User Secrets en AppHost (NO en ApiGateway)

El secreto JWT se configura en el proyecto **AppHost**, no en cada microservicio:

```bash
cd OrderFlow.AppHost
dotnet user-secrets set "Parameters:jwt-secret" "MiClaveSecretaJWTqueTieneMasDe32CaracteresParaHMAC256"
```

Luego en `AppHost.cs`:
```csharp
// Lee el secret de User Secrets del AppHost
var jwtSecret = builder.AddParameter("jwt-secret", secret: true);

// Lo pasa a cada servicio como variable de entorno
.WithEnvironment("Jwt__Secret", jwtSecret)
```

Cuando Aspire inicia, automáticamente:
1. Lee `Parameters:jwt-secret` de User Secrets del AppHost
2. Lo inyecta como variable de entorno `Jwt__Secret` en cada servicio
3. .NET Configuration lo mapea a `Configuration["Jwt:Secret"]`

#### Paso 3.3: Configurar JWT en Program.cs (sin extensiones)

```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ... otros servicios ...

// ============================================
// JWT AUTHENTICATION (VALIDACIÓN)
// ============================================
var jwtSecret = builder.Configuration["Jwt:Secret"]
    ?? throw new InvalidOperationException("Jwt:Secret requerido");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "OrderFlow.Identity";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "OrderFlow.Client";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSecret)),
        ClockSkew = TimeSpan.Zero
    };
    
    // Opcional: logging de eventos
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"Auth failed: {context.Exception.Message}");
            return Task.CompletedTask;
        }
    };
});
```

### 🔍 Flujo de autenticación

```
┌─────────────────────────────────────────────────────────────┐
│ 1. Cliente hace login a través del Gateway:                 │
│    POST /api/v1/auth/login → Gateway → Identity             │
│    Identity genera JWT y responde                           │
└─────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────┐
│ 2. Cliente guarda el token                                  │
└─────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────┐
│ 3. Cliente hace petición protegida:                         │
│    GET /api/v1/users/me                                     │
│    Header: Authorization: Bearer eyJhbG...                  │
└─────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────┐
│ 4. Gateway valida el token:                                 │
│    - ¿Firma válida? (usa el mismo Secret que Identity)      │
│    - ¿Issuer correcto? (OrderFlow.Identity)                 │
│    - ¿Audience correcto? (OrderFlow.Client)                 │
│    - ¿No expirado?                                          │
└─────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────┐
│ 5. Si válido: Gateway reenvía a Identity con el token       │
│    Si inválido: Gateway responde 401 Unauthorized           │
└─────────────────────────────────────────────────────────────┘
```

### ✅ Verificación
- [ ] JWT configurado con MISMO Secret, Issuer, Audience que Identity
- [ ] Gateway solo VALIDA, no genera tokens

---

## Fase 4: Políticas de Autorización

### 🎯 Objetivo
Definir qué usuarios pueden acceder a qué rutas.

### 📚 Conceptos Clave

| Política | Descripción | Uso |
|----------|-------------|-----|
| `anonymous` | Sin autenticación requerida | Login, Register, endpoints públicos |
| `authenticated` | Token JWT válido requerido | Perfil de usuario, acciones del cliente |
| `admin` | Token válido + Rol "Admin" | Gestión de usuarios y roles |

### 📝 Pasos

#### Paso 4.1: Configurar políticas en Program.cs

```csharp
builder.Services.AddAuthorization(options =>
{
    // Política para usuarios autenticados
    options.AddPolicy("authenticated", policy =>
        policy.RequireAuthenticatedUser());

    // Política para administradores
    options.AddPolicy("admin", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireRole("Admin");
    });

    // "anonymous" es built-in en YARP - NO definirla aquí
    
    // Sin política por defecto (YARP decide por ruta)
    options.FallbackPolicy = null;
});
```

> ⚠️ **IMPORTANTE**: `anonymous` es una política especial de YARP que permite acceso sin autenticación. NO la definas con `AddPolicy`.

#### 🔍 Cómo YARP usa las políticas

```json
// En appsettings.json
{
  "Routes": {
    "identity-public": {
      "AuthorizationPolicy": "anonymous",  // ← YARP permite sin token
      "Match": { "Path": "/api/v{version}/auth/{**catch-all}" }
    },
    "identity-admin-users": {
      "AuthorizationPolicy": "admin",      // ← YARP exige token + rol Admin
      "Match": { "Path": "/api/v{version}/admin/users/{**catch-all}" }
    }
  }
}
```

### ✅ Verificación
- [ ] Política `authenticated` definida
- [ ] Política `admin` requiere rol "Admin"
- [ ] `FallbackPolicy = null` para dejar que YARP decida

---

## Fase 5: Rate Limiting con Redis

### 🎯 Objetivo
Limitar peticiones para prevenir abusos.

### 📚 Conceptos Clave

| Concepto | Descripción |
|----------|-------------|
| **Rate Limiting** | Limitar número de peticiones por tiempo |
| **Redis** | Almacena contadores de forma distribuida |
| **Fixed Window** | Límite fijo por ventana de tiempo (ej: 100 req/min) |
| **Partition Key** | Identificador para agrupar límites (IP o userId) |

### 📝 Pasos

#### Paso 5.1: Configurar Redis en Program.cs

```csharp
// Aspire inyecta Redis automáticamente
builder.AddRedisClient("cache");
```

#### Paso 5.2: Configurar Rate Limiting

```csharp
using RedisRateLimiting;
using StackExchange.Redis;

builder.Services.AddRateLimiter(options =>
{
    // Política para endpoints anónimos: 100 req/min por IP
    options.AddPolicy("anonymous", context =>
    {
        var redis = context.RequestServices.GetRequiredService<IConnectionMultiplexer>();
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        return RedisRateLimitPartition.GetFixedWindowRateLimiter(
            $"anon:{ip}",
            _ => new RedisFixedWindowRateLimiterOptions
            {
                ConnectionMultiplexerFactory = () => redis,
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            });
    });

    // Política para usuarios autenticados: 250 req/min por usuario
    options.AddPolicy("authenticated", context =>
    {
        var redis = context.RequestServices.GetRequiredService<IConnectionMultiplexer>();
        var userId = context.User.FindFirst("sub")?.Value ?? "unknown";

        return RedisRateLimitPartition.GetFixedWindowRateLimiter(
            $"user:{userId}",
            _ => new RedisFixedWindowRateLimiterOptions
            {
                ConnectionMultiplexerFactory = () => redis,
                PermitLimit = 250,
                Window = TimeSpan.FromMinutes(1)
            });
    });

    // Respuesta cuando se excede el límite
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = 429;
        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            error = "Too many requests",
            retryAfter = "60 seconds"
        }, token);
    };
});
```

#### Paso 5.3: Asignar políticas de Rate Limit en rutas

```json
{
  "Routes": {
    "identity-public": {
      "AuthorizationPolicy": "anonymous",
      "RateLimiterPolicy": "anonymous",    // ← 100 req/min por IP
      "Match": { "Path": "/api/v{version}/auth/{**catch-all}" }
    },
    "identity-protected": {
      "AuthorizationPolicy": "authenticated",
      "RateLimiterPolicy": "authenticated", // ← 250 req/min por usuario
      "Match": { "Path": "/api/v{version}/users/{**catch-all}" }
    }
  }
}
```

### ✅ Verificación
- [ ] Redis configurado
- [ ] Políticas de rate limit definidas
- [ ] Rutas tienen `RateLimiterPolicy` asignado

---

## Fase 6: Integración con Aspire

### 🎯 Objetivo
Registrar el Gateway en AppHost con todas sus dependencias.

### 📝 Pasos

#### Paso 6.1: Configurar en AppHost.cs

```csharp
// ============================================
// INFRASTRUCTURE
// ============================================
var redis = builder.AddRedis("cache")
    .WithLifetime(ContainerLifetime.Persistent);

// JWT Secret compartido
var jwtSecret = builder.AddParameter("jwt-secret", secret: true);
var jwtIssuer = "OrderFlow.Identity";
var jwtAudience = "OrderFlow.Client";

// ============================================
// MICROSERVICES
// ============================================
var identityService = builder.AddProject<Projects.OrderFlow_Identity>("orderflow-identity")
    .WithEnvironment("Jwt__Secret", jwtSecret)
    .WithEnvironment("Jwt__Issuer", jwtIssuer)
    .WithEnvironment("Jwt__Audience", jwtAudience);

// ============================================
// API GATEWAY
// ============================================
var apiGateway = builder.AddProject<Projects.OrderFlow_ApiGateway>("orderflow-apigateway")
    .WithReference(redis)           // Para rate limiting
    .WithReference(identityService) // Service Discovery encuentra Identity
    .WithEnvironment("Jwt__Secret", jwtSecret)
    .WithEnvironment("Jwt__Issuer", jwtIssuer)
    .WithEnvironment("Jwt__Audience", jwtAudience)
    .WaitFor(identityService);      // Esperar a que Identity esté listo
```

#### 🔍 ¿Qué hace `WithReference`?

```
.WithReference(identityService)
        ↓
1. Aspire agrega variable: services__orderflow-identity__https__0 = https://localhost:5001
        ↓
2. Service Discovery en Gateway resuelve "https://orderflow-identity"
        ↓
3. YARP enruta peticiones a la dirección real
```

### ✅ Verificación
- [ ] Gateway referencia Redis e Identity
- [ ] JWT Secret compartido entre Gateway e Identity
- [ ] `WaitFor` asegura orden de arranque

---

## 🏁 Program.cs Completo

```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using RedisRateLimiting;
using StackExchange.Redis;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ============================================
// ASPIRE SERVICE DEFAULTS
// ============================================
builder.AddServiceDefaults();

// ============================================
// REDIS (para Rate Limiting)
// ============================================
builder.AddRedisClient("cache");

// ============================================
// CORS
// ============================================
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// ============================================
// JWT AUTHENTICATION
// ============================================
var jwtSecret = builder.Configuration["Jwt:Secret"]
    ?? throw new InvalidOperationException("Jwt:Secret requerido");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "OrderFlow.Identity";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "OrderFlow.Client";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSecret)),
            ClockSkew = TimeSpan.Zero
        };
    });

// ============================================
// AUTHORIZATION POLICIES
// ============================================
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("authenticated", policy =>
        policy.RequireAuthenticatedUser());

    options.AddPolicy("admin", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireRole("Admin");
    });

    options.FallbackPolicy = null;
});

// ============================================
// RATE LIMITING
// ============================================
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("anonymous", context =>
    {
        var redis = context.RequestServices.GetRequiredService<IConnectionMultiplexer>();
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RedisRateLimitPartition.GetFixedWindowRateLimiter(
            $"anon:{ip}",
            _ => new RedisFixedWindowRateLimiterOptions
            {
                ConnectionMultiplexerFactory = () => redis,
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            });
    });

    options.AddPolicy("authenticated", context =>
    {
        var redis = context.RequestServices.GetRequiredService<IConnectionMultiplexer>();
        var userId = context.User.FindFirst("sub")?.Value ?? "unknown";
        return RedisRateLimitPartition.GetFixedWindowRateLimiter(
            $"user:{userId}",
            _ => new RedisFixedWindowRateLimiterOptions
            {
                ConnectionMultiplexerFactory = () => redis,
                PermitLimit = 250,
                Window = TimeSpan.FromMinutes(1)
            });
    });

    options.OnRejected = async (ctx, token) =>
    {
        ctx.HttpContext.Response.StatusCode = 429;
        await ctx.HttpContext.Response.WriteAsJsonAsync(
            new { error = "Too many requests" }, token);
    };
});

// ============================================
// YARP REVERSE PROXY
// ============================================
builder.Services.AddServiceDiscovery();
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddServiceDiscoveryDestinationResolver();

var app = builder.Build();

// ============================================
// MIDDLEWARE PIPELINE (orden crítico)
// ============================================
app.MapDefaultEndpoints();          // Health checks
app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();            // Valida JWT
app.UseAuthorization();             // Aplica políticas
app.UseRateLimiter();               // Aplica rate limiting
app.MapReverseProxy();              // YARP enruta peticiones

app.Run();
```

---

## 📊 Resumen de Rutas (Solo Identity)

| Ruta | Cluster | Auth | Rate Limit | Descripción |
|------|---------|------|------------|-------------|
| `/api/v{v}/auth/*` | identity | anonymous | 100/min IP | Login, Register |
| `/api/v{v}/admin/users/*` | identity | admin | 250/min user | Gestión usuarios |
| `/api/v{v}/admin/roles/*` | identity | admin | 250/min user | Gestión roles |
| `/api/v{v}/users/*` | identity | authenticated | 250/min user | Perfil usuario |

---

## 🎓 Conceptos Aprendidos

| Categoría | Conceptos |
|-----------|-----------|
| **API Gateway** | Reverse Proxy, punto único de entrada, YARP |
| **YARP** | Routes, Clusters, Match patterns, Service Discovery |
| **Seguridad** | Validación JWT centralizada, políticas de autorización |
| **Rate Limiting** | Redis, Fixed Window, partición por IP/userId |
| **Aspire** | WithReference, Service Discovery, variables de entorno |

---

## ✅ Checklist Final

- [ ] YARP configurado con rutas para Identity
- [ ] Service Discovery resuelve `https://orderflow-identity`
- [ ] JWT validado en Gateway (mismo Secret que Identity)
- [ ] Políticas: `anonymous`, `authenticated`, `admin`
- [ ] Rate Limiting con Redis
- [ ] Gateway registrado en AppHost con `WithReference(identityService)`
- [ ] Middleware en orden: Auth → Authorization → RateLimiter → ReverseProxy
