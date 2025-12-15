KAHOOT: ARQUITECTURA DE MICROSERVICIOS CON .NET ASPIRE
=======================================================

INSTRUCCIONES: Copiar cada pregunta en Kahoot.
Tiempo sugerido: 20 segundos por pregunta.


PREGUNTA 1
Orders debe esperar a que Identity y la BD esten listos. ¿Como?

Usar WaitFor en AppHost
Retry en connection string
Thread.Sleep en Program.cs
Health check manual en Orders

Correcta: 1


PREGUNTA 2
Gateway valida JWT pero NO los genera. ¿Que configurar?

UserManager y SignInManager
Solo AddAuthentication JwtBearer
Identity con DbContext completo
AddIdentity con tablas usuarios

Correcta: 2


PREGUNTA 3
Catalog quiere notificar a Notifications. ¿Patron correcto?

Llamada HTTP directa
Escribir en BD de Notifications
Evento via MassTransit/RabbitMQ
Polling cada segundo

Correcta: 3


PREGUNTA 4
JWT Secret compartido entre Identity y Gateway. ¿Donde guardarlo?

Duplicar en appsettings de cada uno
Hardcodear en codigo fuente
Archivo secrets.json en raiz
User Secrets del AppHost

Correcta: 4


PREGUNTA 5
Identity usa PostgreSQL y RabbitMQ. Gateway solo Redis. ¿Como configurar?

Cada servicio recibe solo lo que usa
Todos reciben todas las referencias
Proyecto compartido con conexiones
Se resuelve automaticamente

Correcta: 1


PREGUNTA 6
Frontend llama a /api/identity/login. ¿Cual es el flujo?

Conecta directo a Identity
Gateway enruta via YARP
ServiceDefaults redirige
AppHost balancea la carga

Correcta: 2


PREGUNTA 7
Se configura UseAuthorization() ANTES de UseAuthentication(). ¿Problema?

Funciona correctamente
Peticiones duplicadas
Autorizacion falla sin usuario
Servidor no arranca

Correcta: 3


PREGUNTA 8
Orders consulta si producto existe en Catalog. ¿Mejor practica?

Acceder a BD de Catalog directo
Sincronizar BDs cada minuto
Copia local de productos
API de Catalog con Service Discovery

Correcta: 4


PREGUNTA 9
¿Por que cada microservicio tiene su propia BD?

Despliegue independiente
Reducir coste licencias
Mas velocidad en consultas
Esquema mas simple

Correcta: 1


PREGUNTA 10
Notifications consume eventos de RabbitMQ. ¿Patron MassTransit?

Request-Response sincrono
Publish-Subscribe Consumer
Saga con persistencia
Compensating Transaction

Correcta: 2


PREGUNTA 11
Usuario no autenticado accede a /api/orders. ¿Como rechazar en YARP?

AuthorizationPolicy en la ruta
Dejarlo abierto por defecto
Bloquear por IP en firewall
CORS rechaza origen

Correcta: 1


PREGUNTA 12
UserRegisteredEvent lo usan Identity y Notifications. ¿Donde definirlo?

Solo en Identity
Solo en Notifications
En AppHost global
En proyecto Shared

Correcta: 4


PREGUNTA 13
DbContext de Identity con usuarios, roles y tokens. ¿Clase base?

DbContext estandar EF Core
ApplicationDbContext generico
IdentityDbContext tipado
UserStoreContext Membership

Correcta: 3


PREGUNTA 14
¿Beneficio principal de ServiceDefaults?

Reduce tamaño compilado
Comparte logica negocio
Genera OpenAPI automatico
Centraliza HealthChecks y Telemetria

Correcta: 4


PREGUNTA 15
1000 peticiones/segundo al login. ¿Como mitigar?

Mas memoria RAM
Rate Limiting con Redis
Mas replicas de Identity
Comprimir con GZIP

Correcta: 2


PREGUNTA 16
Orders arranca antes que PostgreSQL. ¿Como prevenir fallo?

Delay 30 segundos
Reintentos infinitos EF
Desactivar validacion conexion
WaitFor en Aspire

Correcta: 4


PREGUNTA 17
¿Que claim necesita JWT para autorizar por rol?

Solo el email
Hash de contraseña
Rol en claim especifico
Direccion IP cliente

Correcta: 3


PREGUNTA 18
YARP resuelve orderflow-identity a localhost:5001. ¿Como?

Service Discovery de Aspire
DNS externo manual
Archivo hosts del SO
Tabla estatica appsettings

Correcta: 1


PREGUNTA 19
V1 con Minimal APIs y V2 con Controllers. ¿Es valido?

No se pueden mezclar
Si con API Versioning
Requiere dos proyectos
Solo con puertos distintos

Correcta: 2


PREGUNTA 20
¿Orden correcto para crear proyectos desde cero?

Microservicios primero
Gateway primero
AppHost y ServiceDefaults primero
Base de datos primero

Correcta: 3


RESUMEN DE RESPUESTAS
=====================
1-1    6-2    11-1   16-4
2-2    7-3    12-4   17-3
3-3    8-4    13-3   18-1
4-4    9-1    14-4   19-2
5-1   10-2    15-2   20-3


DISTRIBUCION VERIFICADA (5 de cada)
===================================
1: preguntas 1, 5, 9, 11, 18 = 5
2: preguntas 2, 6, 10, 15, 19 = 5
3: preguntas 3, 7, 13, 17, 20 = 5
4: preguntas 4, 8, 12, 14, 16 = 5


FORMATO CSV PARA IMPORTAR EN KAHOOT
===================================
Question,Answer 1,Answer 2,Answer 3,Answer 4,Time,Correct
Orders debe esperar a Identity y BD. ¿Como?,Usar WaitFor en AppHost,Retry en connection string,Thread.Sleep en Program.cs,Health check manual en Orders,20,1
Gateway valida JWT pero NO los genera. ¿Que configurar?,UserManager y SignInManager,Solo AddAuthentication JwtBearer,Identity con DbContext completo,AddIdentity con tablas usuarios,20,2
Catalog quiere notificar a Notifications. ¿Patron?,Llamada HTTP directa,Escribir en BD de Notifications,Evento via MassTransit/RabbitMQ,Polling cada segundo,20,3
JWT Secret compartido Identity y Gateway. ¿Donde?,Duplicar en appsettings cada uno,Hardcodear en codigo fuente,Archivo secrets.json en raiz,User Secrets del AppHost,20,4
Identity usa PostgreSQL/RabbitMQ. Gateway solo Redis. ¿Config?,Cada servicio recibe solo lo que usa,Todos reciben todas las referencias,Proyecto compartido con conexiones,Se resuelve automaticamente,20,1
Frontend llama a /api/identity/login. ¿Flujo?,Conecta directo a Identity,Gateway enruta via YARP,ServiceDefaults redirige,AppHost balancea la carga,20,2
UseAuthorization() ANTES de UseAuthentication(). ¿Problema?,Funciona correctamente,Peticiones duplicadas,Autorizacion falla sin usuario,Servidor no arranca,20,3
Orders consulta producto en Catalog. ¿Mejor practica?,Acceder a BD de Catalog directo,Sincronizar BDs cada minuto,Copia local de productos,API Catalog con Service Discovery,20,4
¿Por que cada microservicio tiene su propia BD?,Despliegue independiente,Reducir coste licencias,Mas velocidad en consultas,Esquema mas simple,20,1
Notifications consume eventos RabbitMQ. ¿Patron MassTransit?,Request-Response sincrono,Publish-Subscribe Consumer,Saga con persistencia,Compensating Transaction,20,2
Usuario no autenticado accede a /api/orders. ¿Rechazar en YARP?,AuthorizationPolicy en la ruta,Dejarlo abierto por defecto,Bloquear por IP en firewall,CORS rechaza origen,20,1
UserRegisteredEvent lo usan Identity y Notifications. ¿Donde?,Solo en Identity,Solo en Notifications,En AppHost global,En proyecto Shared,20,4
DbContext Identity con usuarios roles tokens. ¿Clase base?,DbContext estandar EF Core,ApplicationDbContext generico,IdentityDbContext tipado,UserStoreContext Membership,20,3
¿Beneficio principal de ServiceDefaults?,Reduce tamaño compilado,Comparte logica negocio,Genera OpenAPI automatico,Centraliza HealthChecks Telemetria,20,4
1000 peticiones/segundo al login. ¿Mitigar?,Mas memoria RAM,Rate Limiting con Redis,Mas replicas de Identity,Comprimir con GZIP,20,2
Orders arranca antes que PostgreSQL. ¿Prevenir fallo?,Delay 30 segundos,Reintentos infinitos EF,Desactivar validacion conexion,WaitFor en Aspire,20,4
¿Que claim necesita JWT para autorizar por rol?,Solo el email,Hash de contraseña,Rol en claim especifico,Direccion IP cliente,20,3
YARP resuelve orderflow-identity a localhost. ¿Como?,Service Discovery de Aspire,DNS externo manual,Archivo hosts del SO,Tabla estatica appsettings,20,1
V1 Minimal APIs y V2 Controllers. ¿Es valido?,No se pueden mezclar,Si con API Versioning,Requiere dos proyectos,Solo con puertos distintos,20,2
¿Orden correcto para crear proyectos?,Microservicios primero,Gateway primero,AppHost y ServiceDefaults primero,Base de datos primero,20,3
