# 📚 OrderFlow - Ruta de Aprendizaje e Implementación

**Ruta de Aprendizaje Progresiva para Construir un Sistema de Microservicios**

Este documento proporciona una guía paso a paso para implementar OrderFlow, diseñada para que los estudiantes aprendan arquitectura de microservicios con retroalimentación visual inmediata en cada paso.

---

## ✅ FASE 1: COMPLETADA - Configuración Base

### Lo que Tenemos Ahora:
- ✅ Orquestación de .NET Aspire funcionando
- ✅ Base de datos PostgreSQL ejecutándose
- ✅ Servicio Identity con ASP.NET Core Identity (7 tablas: Users, Roles, Claims, etc.)
- ✅ Frontend React conectado vía Vite
- ✅ Todos los servicios visibles en Aspire Dashboard (http://localhost:15888)
- ✅ Documentación API con Scalar (https://localhost:7264/scalar/v1)

### Progreso Visible:
- Los estudiantes pueden ver todos los servicios ejecutándose en Aspire Dashboard
- Tablas de base de datos creadas automáticamente
- Aplicación React carga en http://localhost:5173
- Documentación API accesible vía Scalar

---

## 🎯 FASE 2: Hacer Funcional el Servicio Identity

**Objetivo:** Los estudiantes pueden registrar, iniciar sesión y autenticar usuarios

### Paso 2.1: Crear Endpoints de API de Autenticación ⏭️ SIGUIENTE

**Archivos a Crear:**
- `OrderFlow.Identity/Controllers/AuthController.cs`
- `OrderFlow.Identity/DTOs/RegisterRequest.cs`
- `OrderFlow.Identity/DTOs/LoginRequest.cs`
- `OrderFlow.Identity/DTOs/AuthResponse.cs`

**Endpoints de API a Implementar:**
```csharp
POST /api/auth/register
POST /api/auth/login  
GET /api/auth/me
```

**Por Qué es Importante:**
- Los estudiantes ven autenticación REAL funcionando
- Pueden probar inmediatamente con la UI de Scalar
- Base para todos los demás servicios

**Resultado Visible:** 
✅ Registrar usuario vía Scalar → Login → Ver datos de usuario en PostgreSQL

**Objetivos de Aprendizaje:**
- Controllers de ASP.NET Core
- Data Transfer Objects (DTOs)
- Validación de modelos
- API de Identity UserManager

---

### Paso 2.2: Agregar Generación de Tokens JWT

**Paquetes a Instalar:**
```bash
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
```

**Archivos a Crear:**
- `OrderFlow.Identity/Services/TokenService.cs`
- `OrderFlow.Identity/Models/TokenSettings.cs`

**Configuración a Agregar:**
- Configuración JWT en `appsettings.json`
- Middleware de autenticación JWT en `Program.cs`

**Resultado Visible:**
✅ Login retorna token JWT → Decodificar en jwt.io → Ver claims del usuario

**Objetivos de Aprendizaje:**
- Estructura de tokens JWT
- Autenticación basada en claims
- Gestión de configuración
- Inyección de dependencias

---

### Paso 2.3: Construir UI de Login/Registro en React

**Componentes a Crear:**
- `src/components/Auth/LoginForm.tsx`
- `src/components/Auth/RegisterForm.tsx`
- `src/services/authService.ts`
- `src/contexts/AuthContext.tsx`

**Características:**
- Formulario de login con email/contraseña
- Formulario de registro con validación
- Almacenar JWT en localStorage
- Ejemplo de rutas protegidas
- Mostrar perfil de usuario

**Resultado Visible:**
✅ 🎨 Los estudiantes pueden iniciar sesión desde la app React → Ver su nombre → Cerrar sesión funciona

**Objetivos de Aprendizaje:**
- Formularios React y manejo de estado
- Context API para estado global
- Cliente HTTP (fetch/axios)
- Almacenamiento y gestión de tokens
- Enrutamiento protegido

---

## 🎯 FASE 3: Construir Servicio de Catálogo (¡El Más Visual!)

**Por Qué Este Orden:** Catálogo es el servicio más independiente y proporciona retroalimentación visual inmediata - ¡perfecto para aprender!

### Paso 3.1: Crear Proyecto del Servicio Catálogo

**Comandos:**
```bash
dotnet new webapi -n OrderFlow.Catalog
dotnet sln add OrderFlow.Catalog
```

**Tareas de Configuración:**
- Agregar proyecto a la solución
- Agregar a Aspire AppHost
- Crear base de datos `catalogdb` en AppHost
- Agregar paquetes Npgsql y EF Core
- Crear DbContext

**Modelos a Crear:**
```csharp
Product (Id, Name, Description, Price, Stock, CategoryId, ImageUrl, IsActive)
Category (Id, Name, Description, IsActive)
```

**Resultado Visible:**
✅ Nuevo servicio aparece en Aspire Dashboard → Base de datos creada → Migraciones aplicadas

**Objetivos de Aprendizaje:**
- Crear nuevo microservicio
- Patrón Database per Service
- Relaciones en Entity Framework

---

### Paso 3.2: Implementar API CRUD de Productos

**Controllers a Crear:**
- `ProductsController.cs` - Operaciones CRUD
- `CategoriesController.cs` - Gestión de categorías

**Características:**
- GET todos los productos (con paginación)
- GET producto por ID
- POST crear producto (solo Admin)
- PUT actualizar producto (solo Admin)
- DELETE eliminación suave (solo Admin)
- GET todas las categorías

**Datos Iniciales:**
- Categoría Electrónica con laptops, teléfonos
- Categoría Ropa con camisas, zapatos
- Al menos 10-15 productos de ejemplo

**Resultado Visible:**
✅ Navegar productos vía Scalar → Ver hermosa documentación API → Consultar base de datos

**Objetivos de Aprendizaje:**
- Diseño de API RESTful
- Patrón Repository
- Implementación de paginación
- Estrategias de datos iniciales

---

### Paso 3.3: Construir UI de Catálogo de Productos en React

**Componentes a Crear:**
- `src/pages/ProductList.tsx` - Cuadrícula de tarjetas de productos
- `src/pages/ProductDetail.tsx` - Vista de producto individual
- `src/components/ProductCard.tsx` - Tarjeta reutilizable
- `src/components/SearchBar.tsx` - Funcionalidad de búsqueda
- `src/components/CategoryFilter.tsx` - Filtrar por categoría

**Características:**
- Tarjetas de productos hermosas con imágenes
- Búsqueda por nombre
- Filtrar por categoría
- Ordenar por precio
- Click en producto → Página de detalle
- Diseño responsive

**Resultado Visible:**
✅ 🎨 ¡Los estudiantes ven un catálogo de e-commerce real! ¡Muy motivador!

**Objetivos de Aprendizaje:**
- Composición de componentes
- Integración con API
- Enrutamiento con React Router
- Gestión de estado
- CSS/estilos
- Estados de carga y manejo de errores

---

## 🎯 FASE 4: Agregar Caché con Redis

**Por Qué Ahora:** Los estudiantes entienden el rendimiento y pueden ver métricas de caché

### Paso 4.1: Agregar Redis a la Infraestructura

**Configuración:**
- Agregar Redis a AppHost con `AddRedis()`
- Agregar Aspire.StackExchange.Redis al servicio Catalog
- Configurar distributed cache

**Resultado Visible:**
✅ Redis aparece en Aspire Dashboard → Conexión verificada

---

### Paso 4.2: Implementar Caché de Productos

**Estrategia de Caché:**
- Cachear productos populares (GET por ID)
- Cachear lista de productos (expiración de 5 minutos)
- Invalidación de caché en actualizaciones

**Métricas a Rastrear:**
- Tasa de aciertos de caché
- Mejora en tiempo de respuesta

**Resultado Visible:**
✅ Ver aciertos/fallos de caché en métricas de Aspire → Tiempos de respuesta más rápidos

**Objetivos de Aprendizaje:**
- Caché distribuido
- Estrategias de invalidación de caché
- Optimización de rendimiento
- Observabilidad

---

## 🎯 FASE 5: Construir Servicio de Clientes

**Objetivo:** Almacenar perfiles de clientes y direcciones de envío

### Paso 5.1: Crear Servicio de Clientes

**Configuración:**
- Nuevo proyecto ASP.NET Core Web API
- Crear base de datos `customersdb`
- Agregar a Aspire AppHost

**Modelos:**
```csharp
Customer (Id, UserId, FirstName, LastName, Phone, CompanyName, TaxId)
Address (Id, CustomerId, Street, City, State, PostalCode, Country, IsDefault, Type)
```

**Controllers:**
- `CustomersController.cs`
- `AddressesController.cs`

**Resultado Visible:**
✅ Nuevo servicio en Dashboard → Crear perfil de cliente vía API

**Objetivos de Aprendizaje:**
- Relaciones entre servicios
- Clave foránea al servicio Identity (UserId)
- Relaciones uno a muchos

---

### Paso 5.2: UI de Gestión de Perfil

**Páginas a Crear:**
- `src/pages/Profile.tsx` - Ver/editar perfil
- `src/pages/Addresses.tsx` - Gestionar direcciones
- `src/components/AddressForm.tsx` - Agregar/editar dirección

**Características:**
- Ver perfil de cliente
- Editar información de perfil
- Agregar dirección de envío
- Establecer dirección predeterminada
- Eliminar dirección

**Resultado Visible:**
✅ Los estudiantes pueden gestionar su perfil y direcciones desde la UI

**Objetivos de Aprendizaje:**
- Formularios con múltiples campos
- Operaciones CRUD desde la UI
- Datos específicos del usuario

---

## 🎯 FASE 6: Construir Servicio de Pedidos (¡El Complejo!)

**Por Qué Al Final de los Servicios Core:** Requiere que Catálogo + Clientes funcionen correctamente

### Paso 6.1: Crear Servicio de Pedidos

**Configuración:**
- Nuevo proyecto ASP.NET Core Web API
- Crear base de datos `ordersdb`
- Agregar a Aspire AppHost

**Modelos:**
```csharp
Order (Id, OrderNumber, UserId, CustomerId, OrderDate, Status, TotalAmount, Notes)
OrderItem (Id, OrderId, ProductId, ProductName, Quantity, UnitPrice, Subtotal)
OrderStatus enum (Pending, Confirmed, Shipped, Delivered, Cancelled)
```

**Lógica de Negocio:**
- Validar stock vía llamada HTTP a Catalog
- Obtener dirección del cliente vía llamada HTTP a Customers
- Calcular monto total
- Crear pedido con items (transacción)
- Usar snapshots para nombre/precio del producto

**Resultado Visible:**
✅ Crear pedido vía API → Ver pedido en base de datos con items

**Objetivos de Aprendizaje:**
- Comunicación HTTP entre servicios
- Transacciones y consistencia de datos
- Patrón Snapshot (almacenar detalles de producto)
- Capa de lógica de negocio

---

### Paso 6.2: Carrito de Compras y UI de Checkout

**Componentes a Crear:**
- `src/components/Cart/CartIcon.tsx` - Icono de carrito con contador
- `src/components/Cart/CartDrawer.tsx` - Carrito deslizante
- `src/components/Cart/CartItem.tsx` - Item en carrito
- `src/pages/Checkout.tsx` - Flujo de checkout
- `src/pages/OrderConfirmation.tsx` - Página de éxito
- `src/pages/OrderHistory.tsx` - Ver pedidos pasados

**Características:**
- Agregar al carrito desde página de producto
- Ver carrito con items
- Actualizar cantidad
- Eliminar items
- Flujo de checkout:
  1. Revisar items
  2. Seleccionar dirección de envío
  3. Confirmar pedido
  4. Ver confirmación
- Ver historial de pedidos

**Resultado Visible:**
✅ 🛒 ¡Flujo completo de e-commerce! ¡Los estudiantes pueden comprar y hacer pedidos!

**Objetivos de Aprendizaje:**
- Gestión de estado complejo
- Formularios multi-paso
- UX de flujo de pedidos
- Local storage para carrito
- Actualizaciones optimistas de UI

---

## 🎯 FASE 7: Agregar Arquitectura Orientada a Eventos con RabbitMQ

**Por Qué Ahora:** Los estudiantes ven la necesidad de comunicación asíncrona entre servicios

### Paso 7.1: Agregar Infraestructura RabbitMQ

**Configuración:**
- Agregar RabbitMQ a AppHost con `AddRabbitMQ()`
- Agregar Aspire.RabbitMQ.Client a servicios
- Configurar exchanges y queues

**Eventos a Definir:**
```csharp
OrderCreatedEvent
OrderStatusChangedEvent
StockUpdatedEvent
CustomerRegisteredEvent
```

**Resultado Visible:**
✅ RabbitMQ aparece en Dashboard → UI de gestión accesible

---

### Paso 7.2: Publicar Eventos desde Servicios

**Publicadores:**
- Servicio Orders → `OrderCreatedEvent`, `OrderStatusChangedEvent`
- Servicio Catalog → `StockUpdatedEvent`
- Servicio Customers → `CustomerRegisteredEvent`

**Resultado Visible:**
✅ Hacer pedido → Ver evento publicado en UI de gestión de RabbitMQ

**Objetivos de Aprendizaje:**
- Arquitectura orientada a eventos
- Patrón Publisher
- Serialización de mensajes

---

### Paso 7.3: Crear Servicio de Notificaciones

**Configuración:**
- Nuevo ASP.NET Core Worker Service
- Suscribirse a eventos desde RabbitMQ
- Sin base de datos (servicio stateless)

**Manejadores de Eventos:**
- `OrderCreatedEvent` → Log "Email de confirmación de pedido enviado"
- `OrderStatusChangedEvent` → Log "Email de actualización de estado"
- `StockUpdatedEvent` → Log si el stock está bajo
- `CustomerRegisteredEvent` → Log "Email de bienvenida"

**Mejora Posterior:**
- Reemplazar logs de consola con emails reales (MailKit)
- Plantillas de email con Razor

**Resultado Visible:**
✅ Hacer pedido → Ver notificación en logs del servicio → Mensaje "Email enviado"

**Objetivos de Aprendizaje:**
- Worker services
- Consumidores de eventos
- Procesamiento asíncrono
- Arquitectura desacoplada

---

## 🎯 FASE 8: API Gateway con YARP (Avanzado)

**Por Qué Al Final:** Los estudiantes entienden POR QUÉ se necesita después de trabajar con múltiples servicios

### Paso 8.1: Crear API Gateway

**Configuración:**
- Nuevo proyecto ASP.NET Core Web API (minimal)
- Agregar YARP (Yet Another Reverse Proxy)
- Configurar rutas a todos los servicios

**Rutas:**
```
/api/auth/* → Servicio Identity
/api/products/* → Servicio Catalog
/api/orders/* → Servicio Orders
/api/customers/* → Servicio Customers
```

**Características:**
- Punto de entrada único para frontend
- Validación JWT en gateway
- Transformación de request/response

**Resultado Visible:**
✅ Frontend llama a una URL → Gateway enruta al servicio correcto

---

### Paso 8.2: Agregar Rate Limiting

**Configuración:**
- Agregar rate limiting basado en Redis
- Configurar límites por endpoint
- Retornar 429 Too Many Requests

**Ejemplo de Límites:**
- Endpoints públicos: 100 requests/minuto
- Autenticados: 1000 requests/minuto
- Admin: Ilimitado

**Resultado Visible:**
✅ Spamear API → Ser limitado → Ver throttling en acción

**Objetivos de Aprendizaje:**
- Patrón API Gateway
- Estrategias de rate limiting
- Protección DDoS
- Redis para estado distribuido

---

## 📊 Seguimiento del Progreso

### Indicadores Visuales de Progreso:

1. **Aspire Dashboard** (http://localhost:15888)
   - Conteo de servicios: 1 → 2 → 3 → 4 → 5 → 6 → 7
   - Todos los servicios saludables ✅
   - Logs y traces visibles

2. **Tablas de Base de Datos**
   - Fase 1: 7 tablas de Identity
   - Fase 3: +3 tablas de Catalog
   - Fase 5: +2 tablas de Customers
   - Fase 6: +2 tablas de Orders

3. **Evolución de la UI React**
   - Fase 1: Básico "Hola OrderFlow"
   - Fase 2: Formularios de Login/Registro
   - Fase 3: Catálogo de productos 🎨
   - Fase 5: Página de perfil
   - Fase 6: Experiencia completa de compras 🛍️

4. **Documentación de API**
   - Cada servicio tiene documentación Scalar
   - Los estudiantes pueden probar cada endpoint
   - Ver ejemplos de request/response

---

## 🎓 Resultados de Aprendizaje

Al completar esta ruta, los estudiantes aprenderán:

### Arquitectura y Patrones
- ✅ Arquitectura de microservicios
- ✅ Patrón Database per Service
- ✅ Patrón API Gateway
- ✅ Arquitectura orientada a eventos
- ✅ Conceptos básicos de CQRS (queries vs commands)

### Tecnologías Backend
- ✅ ASP.NET Core Web API
- ✅ Entity Framework Core
- ✅ ASP.NET Core Identity
- ✅ Autenticación JWT
- ✅ PostgreSQL
- ✅ Caché con Redis
- ✅ Mensajería con RabbitMQ

### Tecnologías Frontend
- ✅ React con TypeScript
- ✅ Context API
- ✅ React Router
- ✅ Integración con API
- ✅ Manejo de formularios
- ✅ Gestión de estado

### DevOps y Herramientas
- ✅ Orquestación con .NET Aspire
- ✅ Contenedores Docker
- ✅ Service discovery
- ✅ Health checks
- ✅ Distributed tracing
- ✅ Logging y monitoreo

### Mejores Prácticas
- ✅ Clean architecture
- ✅ Separación de responsabilidades
- ✅ Manejo de errores
- ✅ Validación
- ✅ Seguridad (autenticación, autorización)
- ✅ Documentación de API

---

## 💡 Consejos para el Éxito

### Para Instructores:
1. **Mostrar Resultados Primero** - Demostrar la fase completada antes de que los estudiantes comiencen
2. **Programación en Parejas** - Los estudiantes trabajan en parejas en partes complejas
3. **Revisiones de Código** - Revisar cada fase antes de avanzar
4. **Ramas Git** - Cada fase en su propia rama
5. **Celebrar Hitos** - ¡Cuando la UI funcione, celebrar! 🎉

### Para Estudiantes:
1. **Probar Frecuentemente** - Usar Scalar para probar APIs inmediatamente
2. **Leer Logs** - Aspire Dashboard muestra todo
3. **Hacer Preguntas** - Si algo no funciona, revisar logs primero
4. **Experimentar** - Intentar romper cosas para entender cómo funcionan
5. **Documentar** - Tomar notas sobre lo aprendido

### Errores Comunes a Evitar:
- ⚠️ No saltarse fases - cada una construye sobre la anterior
- ⚠️ No copiar/pegar sin entender
- ⚠️ No olvidar ejecutar migraciones
- ⚠️ No ignorar errores en Aspire Dashboard
- ⚠️ No olvidar configuración CORS para frontend

---

## 🚀 Comenzar

### Prerequisitos:
- .NET 9 SDK
- Node.js 20+
- Docker Desktop
- Visual Studio Code o Visual Studio 2022
- Git

### Ejecutar el Proyecto:
```bash
# Desde el directorio OrderFlow (AppHost)
dotnet run

# O usar Visual Studio
# Establecer OrderFlow (AppHost) como proyecto de inicio
# Presionar F5
```

### Puntos de Acceso:
- 📊 Aspire Dashboard: http://localhost:15888
- 🔐 Identity API: https://localhost:7264/scalar/v1
- 🎨 React App: http://localhost:5173

---

## 📝 Próximos Pasos

**➡️ Comenzar con Fase 2, Paso 2.1: Crear Endpoints de API de Autenticación**

¡Esto hará que el servicio Identity sea inmediatamente útil y los estudiantes podrán comenzar a probar autenticación real!

¡Buena suerte y feliz codificación! 🚀
