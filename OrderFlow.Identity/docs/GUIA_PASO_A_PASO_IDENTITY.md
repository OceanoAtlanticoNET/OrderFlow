# 🎯 Guía Paso a Paso: OrderFlow.Identity

## Microservicio de Autenticación e Identidad con .NET Aspire

---

## 📋 Índice

| Fase | Tema | Conceptos Clave |
|------|------|-----------------|
| 0 | Infraestructura Aspire | AppHost, ServiceDefaults, Orquestación |
| 1 | Proyecto Identity | Estructura, Dependencias |
| 2 | Base de Datos | EF Core, Identity, Database per Service |
| 3 | Autenticación JWT | Tokens, User Secrets |
| 4 | Capa de Servicios | Abstracción, Result Pattern |
| 5 | Endpoints Auth V1 | Minimal APIs, Validación |
| 6 | Gestión de Usuarios | CRUD, Autorización por Rol |
| 7 | Gestión de Roles | RBAC |
| 8 | Controllers V2 | API Versioning |
| 9 | Eventos | MassTransit, RabbitMQ |
| 10 | OpenAPI | Documentación, Scalar |

---

## Fase 0: Infraestructura .NET Aspire

### 🎯 Objetivo
Crear la base de orquestación que gestionará todos los microservicios.

### 📚 Conceptos Clave

| Concepto | Descripción |
|----------|-------------|
| **AppHost** | Proyecto que orquesta todos los contenedores y servicios |
| **ServiceDefaults** | Proyecto compartido con configuraciones comunes (Health Checks, OpenTelemetry, Service Discovery) |
| **Database per Service** | Cada microservicio tiene su propia base de datos aislada |
| **Service Discovery** | Los servicios se descubren automáticamente por nombre |

### 📝 Pasos

1. **Crear solución con Aspire**
   ```bash
   dotnet new aspire -n OrderFlow
   ```

2. **Estructura generada:**
   ```
   OrderFlow/
   ├── OrderFlow.sln
   ├── OrderFlow.AppHost/        ← Orquestador
   └── OrderFlow.ServiceDefaults/ ← Configuraciones compartidas
   ```

3. **Configurar AppHost** (`AppHost.cs`):
   - Agregar PostgreSQL con bases de datos separadas
   - Agregar RabbitMQ para mensajería
   - Agregar Redis para caché (opcional)
   - Definir parámetros compartidos (JWT Secret)

4. **Revisar ServiceDefaults** (`Extensions.cs`):
   - `AddServiceDefaults()` - Agrega Health Checks, OpenTelemetry, Service Discovery
   - `MapDefaultEndpoints()` - Mapea endpoints `/health` y `/alive`

### ✅ Verificación
- [ ] AppHost compila
- [ ] ServiceDefaults tiene OpenTelemetry configurado
- [ ] Entiendes el patrón Database per Service

---

## Fase 1: Crear Proyecto Identity

### 🎯 Objetivo
Crear el microservicio de Identity y conectarlo con Aspire.

### 📚 Conceptos Clave

| Concepto | Descripción |
|----------|-------------|
| **Aspire Integration** | Paquetes `Aspire.*` para conectar con recursos |
| **Connection Strings** | Aspire inyecta automáticamente las cadenas de conexión |

### 📝 Pasos

1. **Crear proyecto Web API**
   ```bash
   dotnet new webapi -n OrderFlow.Identity
   ```

2. **Agregar referencias de proyecto**:
   - `OrderFlow.ServiceDefaults` (Health Checks, Telemetría)
   - `OrderFlow.Shared` (Eventos compartidos)

3. **Agregar paquetes NuGet**:
   - `Aspire.Npgsql.EntityFrameworkCore.PostgreSQL` - Integración PostgreSQL
   - `Microsoft.AspNetCore.Identity.EntityFrameworkCore` - Identity
   - `Microsoft.AspNetCore.Authentication.JwtBearer` - JWT
   - `FluentValidation.DependencyInjectionExtensions` - Validación
   - `MassTransit.RabbitMQ` - Mensajería
   - `Asp.Versioning.Http` - Versionado API

4. **Registrar en AppHost**:
   ```csharp
   var identityDb = postgres.AddDatabase("identitydb");
   
   builder.AddProject<Projects.OrderFlow_Identity>("orderflow-identity")
       .WithReference(identityDb)
       .WithReference(rabbitmq)
       .WaitFor(identityDb);
   ```

5. **Crear estructura de carpetas**:
   ```
   OrderFlow.Identity/
   ├── Controllers/V2/
   ├── Data/
   ├── Dtos/{Auth,Users,Roles,Common}/
   ├── Extensions/
   ├── Features/{Auth,Users,Roles}/V1/
   ├── Services/{Auth,Users,Roles,Common}/
   └── Validators/
   ```

### ✅ Verificación
- [ ] Proyecto referencia ServiceDefaults
- [ ] Proyecto aparece en Aspire Dashboard
- [ ] Estructura de carpetas creada

---

## Fase 2: Configuración de Base de Datos

### 🎯 Objetivo
Configurar Entity Framework Core con ASP.NET Core Identity.

### 📚 Conceptos Clave

| Concepto | Descripción |
|----------|-------------|
| **IdentityDbContext** | DbContext especializado que incluye tablas de Identity |
| **IdentityUser** | Entidad base para usuarios (extensible) |
| **IdentityRole** | Entidad para roles |
| **IDesignTimeDbContextFactory** | Factory para crear contexto durante migraciones |
| **Database per Service** | `identitydb` es exclusiva de este microservicio |

### 📝 Pasos

1. **Crear `Data/ApplicationDbContext.cs`**:
   - Heredar de `IdentityDbContext<IdentityUser>`
   - Sobrescribir `OnModelCreating` si necesitas personalizar

2. **Crear `Data/ApplicationDbContextFactory.cs`**:
   - Implementar `IDesignTimeDbContextFactory<ApplicationDbContext>`
   - Necesario para ejecutar migraciones offline

3. **Crear `Data/Roles.cs`**:
   - Definir constantes: `Admin`, `Customer`
   - Método `GetAll()` para iterar roles

4. **Configurar en Program.cs**:
   ```csharp
   // Aspire inyecta la connection string automáticamente
   builder.AddNpgsqlDbContext<ApplicationDbContext>("identitydb");
   ```

5. **Crear `Extensions/DatabaseExtensions.cs`**:
   - `SeedDevelopmentDataAsync()` para crear roles y admin inicial
   - Aplicar migraciones automáticamente en desarrollo

6. **Generar migración inicial**:
   ```bash
   dotnet ef migrations add InitialIdentity
   ```

### ✅ Verificación
- [ ] Migración generada correctamente
- [ ] DbContext hereda de IdentityDbContext
- [ ] Roles definidos (Admin, Customer)

---

## Fase 3: Autenticación JWT

### 🎯 Objetivo
Configurar autenticación JWT Bearer de forma segura.

### 📚 Conceptos Clave

| Concepto | Descripción |
|----------|-------------|
| **JWT (JSON Web Token)** | Token firmado con claims del usuario |
| **User Secrets** | Almacenamiento seguro de secretos en desarrollo |
| **Claims** | Información embebida en el token (userId, email, roles) |
| **Bearer Authentication** | Esquema donde el token va en header `Authorization: Bearer <token>` |
| **TokenValidationParameters** | Reglas para validar tokens entrantes |

### 📝 Pasos

#### Paso 3.1: Configurar appsettings

**`appsettings.json`** (valores por defecto, sin secretos):
```json
{
  "Jwt": {
    "Issuer": "OrderFlow.Identity",
    "Audience": "OrderFlow.Client",
    "ExpiryInMinutes": 60
  }
}
```

> ⚠️ **IMPORTANTE**: El `Secret` NUNCA va en appsettings.json

#### Paso 3.2: Configurar User Secrets en AppHost

Cuando usas **Aspire**, el secreto JWT se configura **una sola vez** en el proyecto **AppHost** (no en cada microservicio):

**En terminal:**
```bash
cd OrderFlow.AppHost
dotnet user-secrets set "Parameters:jwt-secret" "MiClaveSecretaJWTqueTieneMasDe32CaracteresParaHMAC256"
```

**En `AppHost.cs`:**
```csharp
// Lee el secret de User Secrets del AppHost
var jwtSecret = builder.AddParameter("jwt-secret", secret: true);

// Lo pasa a Identity como variable de entorno
var identityService = builder.AddProject<Projects.OrderFlow_Identity>("orderflow-identity")
    .WithEnvironment("Jwt__Secret", jwtSecret)  // Doble __ = Jwt:Secret
    .WithEnvironment("Jwt__Issuer", "OrderFlow.Identity")
    .WithEnvironment("Jwt__Audience", "OrderFlow.Client");
```

> 📝 **¿Por qué en AppHost?** Porque el mismo secret debe compartirse entre Identity (genera tokens) y otros servicios/Gateway (validan tokens). Centralizarlo evita inconsistencias.

**Flujo de configuración:**
```
User Secrets del AppHost
    ↓ Parameters:jwt-secret
AppHost.cs lee con AddParameter()
    ↓ 
WithEnvironment("Jwt__Secret", jwtSecret)
    ↓
.NET inyecta variable de entorno en Identity
    ↓
Configuration["Jwt:Secret"] disponible en Program.cs
```

#### Paso 3.3: Configurar JWT en Program.cs (SIN extensiones)

Agregar estos `using` al inicio:
```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
```

Agregar **después de configurar Identity** y **antes de `var app = builder.Build()`**:

```csharp
// ============================================
// JWT BEARER AUTHENTICATION
// ============================================

// 1. Leer configuración (Secret viene de AppHost via variable de entorno)
var jwtSecret = builder.Configuration["Jwt:Secret"] 
    ?? throw new InvalidOperationException("Jwt:Secret no está configurado. Configúralo en User Secrets del AppHost.");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] 
    ?? "OrderFlow.Identity";
var jwtAudience = builder.Configuration["Jwt:Audience"] 
    ?? "OrderFlow.Client";

// 2. Configurar Authentication con JWT Bearer como esquema por defecto
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        // Validar que el Issuer del token coincida con el nuestro
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        
        // Validar que el Audience del token coincida
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        
        // Validar que el token no haya expirado
        ValidateLifetime = true,
        
        // Validar la firma del token con nuestra clave secreta
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSecret)),
        
        // Sin tolerancia de tiempo (por defecto son 5 min)
        ClockSkew = TimeSpan.Zero
    };
});
```

#### Paso 3.4: Agregar Middleware de Autenticación

En el pipeline de middleware, **el orden es crítico**:

```csharp
var app = builder.Build();

// ... otros middleware ...

app.UseHttpsRedirection();
app.UseCors();              // CORS antes de Auth (si lo usas)
app.UseAuthentication();    // ← PRIMERO: valida el token JWT
app.UseAuthorization();     // ← SEGUNDO: verifica permisos/roles

// ... MapControllers, etc ...
```

> 📝 **Nota**: El Paso 3.2 ya explicó cómo configurar el secret en AppHost. No es necesario configurar User Secrets en el proyecto Identity directamente cuando usas Aspire.

### 🔍 ¿Cómo funciona el flujo completo?

```
┌─────────────────────────────────────────────────────────────┐
│ 1. POST /api/v1/auth/login  { email, password }             │
└─────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────┐
│ 2. AuthService valida credenciales con UserManager          │
│    - FindByEmailAsync(email)                                │
│    - CheckPasswordSignInAsync(user, password)               │
└─────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────┐
│ 3. TokenService genera JWT con claims:                      │
│    - sub: userId                                            │
│    - email: user@example.com                                │
│    - role: ["Admin", "Customer"]                            │
│    - exp: timestamp de expiración                           │
│    Firma el token con HMAC-SHA256 usando el Secret          │
└─────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────┐
│ 4. Response: { accessToken: "eyJhbG...", expiresIn: 3600 }  │
│    Cliente guarda el token (localStorage, cookie, etc.)     │
└─────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────┐
│ 5. Request a endpoint protegido:                            │
│    GET /api/v1/auth/me                                      │
│    Header: Authorization: Bearer eyJhbG...                  │
└─────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────┐
│ 6. Middleware JWT (UseAuthentication):                      │
│    - Extrae token del header                                │
│    - Valida firma con IssuerSigningKey                      │
│    - Valida Issuer, Audience, Lifetime                      │
│    - Si válido: HttpContext.User = ClaimsPrincipal          │
└─────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────┐
│ 7. Middleware Authorization (UseAuthorization):             │
│    - Verifica [Authorize] o RequireAuthorization()          │
│    - Verifica roles si hay [Authorize(Roles = "Admin")]     │
└─────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────┐
│ 8. Endpoint accede a usuario via ClaimsPrincipal:           │
│    var userId = User.FindFirst(ClaimTypes.NameIdentifier)   │
└─────────────────────────────────────────────────────────────┘
```

### ✅ Verificación
- [ ] User Secrets configurado con `Jwt:Secret` (mínimo 32 caracteres)
- [ ] `appsettings.json` tiene `Issuer`, `Audience`, `ExpiryInMinutes`
- [ ] `AddAuthentication()` con `JwtBearerDefaults.AuthenticationScheme`
- [ ] `AddJwtBearer()` con `TokenValidationParameters` completo
- [ ] `UseAuthentication()` ANTES de `UseAuthorization()` en el pipeline
- [ ] AppHost pasa el secret via `WithEnvironment("Jwt__Secret", ...)`

---

## Fase 4: Capa de Servicios

### 🎯 Objetivo
Abstraer la lógica de negocio en servicios reutilizables.

### 📚 Conceptos Clave

| Concepto | Descripción |
|----------|-------------|
| **Result Pattern** | Encapsular éxito/error sin excepciones |
| **Service Layer** | Capa entre Controllers/Endpoints y Data |
| **Dependency Injection** | Inyectar servicios vía constructor |
| **Interface Segregation** | Interfaces pequeñas y específicas |

### 📝 Pasos

1. **Crear `Services/Common/AuthResult<T>.cs`**:
   - Propiedades: `Succeeded`, `Data`, `Errors`
   - Métodos estáticos: `Success(data)`, `Failure(errors)`

2. **Crear `Services/Auth/ITokenService.cs` y `TokenService.cs`**:
   - `GenerateAccessTokenAsync(user, roles)` - Genera JWT
   - `GetTokenExpiryInSeconds()` - Tiempo de expiración

3. **Crear `Services/Auth/IAuthService.cs` y `AuthService.cs`**:
   - `LoginAsync(email, password)` - Autenticación
   - `RegisterAsync(email, password)` - Registro
   - `GetCurrentUserAsync(userId)` - Usuario actual

4. **Registrar en Program.cs**:
   ```csharp
   builder.Services.AddScoped<ITokenService, TokenService>();
   builder.Services.AddScoped<IAuthService, AuthService>();
   ```

### ✅ Verificación
- [ ] TokenService genera JWT válido
- [ ] AuthService usa Result Pattern
- [ ] Servicios registrados en DI

---

## Fase 5: Endpoints de Autenticación (Minimal APIs V1)

### 🎯 Objetivo
Crear endpoints de autenticación usando Minimal APIs.

### 📚 Conceptos Clave

| Concepto | Descripción |
|----------|-------------|
| **Minimal APIs** | Endpoints sin Controllers, más ligeros |
| **Route Groups** | Agrupar endpoints con prefijo común |
| **FluentValidation** | Validación declarativa con reglas |
| **Records** | DTOs inmutables para request/response |

### 📝 Pasos

1. **Crear `Features/Auth/V1/AuthGroup.cs`**:
   - `RouteGroupBuilder` con prefijo `/api/v{version}/auth`
   - Configurar API Version Set

2. **Crear `Features/Auth/V1/RegisterUser.cs`**:
   - Records: `RegisterUserRequest`, `RegisterUserResponse`
   - Clase `Validator` con reglas (email válido, password seguro)
   - Método `MapRegisterUser()` → POST `/register`

3. **Crear `Features/Auth/V1/LoginUser.cs`**:
   - POST `/login` → retorna JWT
   - Validar credenciales

4. **Crear `Features/Auth/V1/GetCurrentUser.cs`**:
   - GET `/me` con `RequireAuthorization()`
   - Leer userId del ClaimsPrincipal

5. **Registrar en Program.cs**:
   ```csharp
   builder.Services.AddValidatorsFromAssemblyContaining<Program>();
   
   app.MapRegisterUser();
   app.MapLoginUser();
   app.MapGetCurrentUser();
   ```

### ✅ Verificación
- [ ] POST `/api/v1/auth/register` crea usuario
- [ ] POST `/api/v1/auth/login` retorna JWT
- [ ] GET `/api/v1/auth/me` requiere token

---

## Fase 6: Gestión de Usuarios

### 🎯 Objetivo
Endpoints CRUD para administración de usuarios (solo Admin).

### 📚 Conceptos Clave

| Concepto | Descripción |
|----------|-------------|
| **Role-Based Authorization** | Restringir acceso por rol |
| **Pagination** | Retornar datos en páginas |
| **Query Parameters** | Filtros vía URL (?page=1&search=...) |

### 📝 Pasos

1. **Crear `Services/Users/IUserService.cs` y `UserService.cs`**:
   - CRUD: `GetUsersAsync`, `GetUserByIdAsync`, `CreateUserAsync`, `UpdateUserAsync`, `DeleteUserAsync`
   - Bloqueo: `LockUserAsync`, `UnlockUserAsync`
   - Roles: `GetUserRolesAsync`, `AddUserToRoleAsync`, `RemoveUserFromRoleAsync`

2. **Crear DTOs** en `Dtos/Users/`:
   - `Queries/UserQueryParameters`
   - `Requests/CreateUserRequest`, `UpdateUserRequest`
   - `Responses/UserResponse`

3. **Crear `Features/Users/V1/UserManagementGroup.cs`**:
   - Prefijo: `/api/v1/admin/users`
   - `RequireAuthorization(policy => policy.RequireRole("Admin"))`

4. **Crear endpoints** en `Features/Users/V1/`:
   - `GetUsers`, `GetUserById`, `CreateUser`, `UpdateUser`, `DeleteUser`

5. **Crear grupo self-service** (usuario actual):
   - `GetMyProfile`, `UpdateMyProfile`, `ChangeMyPassword`
   - Prefijo: `/api/v1/users/me`

### ✅ Verificación
- [ ] Solo Admin accede a `/admin/users`
- [ ] Paginación funciona
- [ ] Usuario puede cambiar su contraseña

---

## Fase 7: Gestión de Roles

### 🎯 Objetivo
Endpoints CRUD para administración de roles.

### 📚 Conceptos Clave

| Concepto | Descripción |
|----------|-------------|
| **RBAC** | Role-Based Access Control |
| **RoleManager<T>** | Servicio de Identity para gestionar roles |

### 📝 Pasos

1. **Crear `Services/Roles/IRoleService.cs` y `RoleService.cs`**:
   - `GetRolesAsync()`, `GetRoleByIdAsync(id)`
   - `CreateRoleAsync(name)`, `UpdateRoleAsync(id, name)`, `DeleteRoleAsync(id)`
   - `GetUsersInRoleAsync(roleName)`

2. **Crear `Features/Roles/V1/RoleManagementGroup.cs`**:
   - Prefijo: `/api/v1/admin/roles`
   - Requiere rol `Admin`

3. **Crear endpoints** en `Features/Roles/V1/`:
   - `GetRoles`, `GetRoleById`, `CreateRole`, `UpdateRole`, `DeleteRole`

### ✅ Verificación
- [ ] CRUD de roles funciona
- [ ] Solo Admin puede gestionar roles

---

## Fase 8: Controllers (V2)

### 🎯 Objetivo
Crear versión alternativa con Controllers tradicionales.

### 📚 Conceptos Clave

| Concepto | Descripción |
|----------|-------------|
| **API Versioning** | Múltiples versiones coexistiendo |
| **URL Segment Versioning** | Versión en la URL: `/v1/`, `/v2/` |
| **[ApiVersion]** | Atributo para marcar la versión del controller |

### 📝 Pasos

1. **Configurar versionado en Program.cs**:
   ```csharp
   builder.Services.AddApiVersioning(options => {
       options.DefaultApiVersion = new ApiVersion(1, 0);
       options.ReportApiVersions = true;
       options.ApiVersionReader = new UrlSegmentApiVersionReader();
   });
   ```

2. **Crear `Controllers/V2/AuthController.cs`**:
   - `[ApiVersion("2.0")]`
   - `[Route("api/v{version:apiVersion}/auth")]`
   - Endpoints: Login, Register, GetCurrentUser

3. **Mapear controllers**:
   ```csharp
   app.MapControllers();
   ```

### ✅ Verificación
- [ ] V1 (Minimal APIs) y V2 (Controllers) funcionan
- [ ] URLs: `/api/v1/auth/login` y `/api/v2/auth/login`

---

## Fase 9: Integración con MassTransit/RabbitMQ

### 🎯 Objetivo
Publicar eventos cuando se registran usuarios.

### 📚 Conceptos Clave

| Concepto | Descripción |
|----------|-------------|
| **Event-Driven Architecture** | Comunicación asíncrona via eventos |
| **Integration Events** | Eventos que cruzan fronteras de microservicios |
| **MassTransit** | Abstracción sobre RabbitMQ |
| **IPublishEndpoint** | Interfaz para publicar mensajes |

### 📝 Pasos

1. **En `OrderFlow.Shared`, crear evento**:
   ```csharp
   public record UserRegisteredEvent(string UserId, string Email) : IIntegrationEvent;
   ```

2. **Configurar MassTransit en Program.cs**:
   ```csharp
   builder.Services.AddMassTransit(x => {
       x.UsingRabbitMq((context, cfg) => {
           cfg.Host(new Uri(connectionString));
       });
   });
   ```

3. **Inyectar `IPublishEndpoint` en AuthService**

4. **Publicar evento en `RegisterAsync`**:
   ```csharp
   await _publishEndpoint.Publish(new UserRegisteredEvent(user.Id, user.Email));
   ```

5. **Verificar en RabbitMQ Management UI** (puerto 15672)

### ✅ Verificación
- [ ] Evento publicado al registrar usuario
- [ ] Mensaje visible en RabbitMQ Management

---

## Fase 10: Documentación OpenAPI

### 🎯 Objetivo
Documentar la API con OpenAPI/Swagger.

### 📚 Conceptos Clave

| Concepto | Descripción |
|----------|-------------|
| **OpenAPI** | Especificación para documentar APIs REST |
| **Scalar** | UI moderna para explorar APIs |
| **Security Scheme** | Documentar autenticación JWT |

### 📝 Pasos

1. **Configurar documentos por versión**:
   ```csharp
   builder.Services.AddOpenApi("v1");
   builder.Services.AddOpenApi("v2");
   ```

2. **Agregar security scheme JWT** mediante document transformers

3. **Mapear UIs**:
   ```csharp
   app.MapOpenApi();
   app.MapScalarApiReference();
   app.UseSwaggerUI();
   ```

### ✅ Verificación
- [ ] `/scalar` muestra documentación
- [ ] Botón "Authorize" para JWT

---

## 📊 Resumen de Endpoints

| Método | Ruta | Descripción | Auth |
|--------|------|-------------|------|
| POST | `/api/v1/auth/register` | Registrar usuario | ❌ |
| POST | `/api/v1/auth/login` | Login → JWT | ❌ |
| GET | `/api/v1/auth/me` | Usuario actual | ✅ |
| GET | `/api/v1/admin/users` | Listar usuarios | Admin |
| POST | `/api/v1/admin/users` | Crear usuario | Admin |
| GET | `/api/v1/admin/roles` | Listar roles | Admin |

---

## 🎓 Conceptos Aprendidos

| Categoría | Conceptos |
|-----------|-----------|
| **Aspire** | AppHost, ServiceDefaults, Service Discovery, Health Checks, OpenTelemetry |
| **Arquitectura** | Database per Service, Event-Driven, Microservicios |
| **Seguridad** | JWT, Identity, RBAC, User Secrets |
| **APIs** | Minimal APIs, Controllers, API Versioning, OpenAPI |
| **Datos** | EF Core, Migrations, PostgreSQL |
| **Mensajería** | MassTransit, RabbitMQ, Integration Events |
| **Patrones** | Result Pattern, Service Layer, DI |

---

## ✅ Checklist Final (Rúbrica)

- [ ] Proyecto arranca con Aspire Dashboard
- [ ] PostgreSQL con `identitydb` (Database per Service)
- [ ] Registro de usuario funciona
- [ ] Login retorna JWT válido
- [ ] Endpoints protegidos requieren token
- [ ] API versionada (`/v1/`, `/v2/`)
- [ ] Evento `UserRegistered` publicado en RabbitMQ
- [ ] Sin secretos hardcodeados (User Secrets)
- [ ] Health Checks en `/health`
- [ ] Documentación en `/scalar`
