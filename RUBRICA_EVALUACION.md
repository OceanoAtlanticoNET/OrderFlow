# Rúbrica de Evaluación: Proyecto OrderFlow (Microservicios con .NET Aspire)

**Estudiante:** _________________________________________________  
**Fecha:** _______________________  
**Calificación Final:** _______ / 10

---

## 📋 Instrucciones de Evaluación
Cada objetivo se evalúa del 1 al 10. La nota final es el promedio de los objetivos.
*   **1-4 (Insuficiente):** No cumple con los requisitos mínimos.
*   **5-6 (Suficiente):** Cumple lo básico pero con errores o falta de profundidad.
*   **7-8 (Notable):** Buena implementación, sigue buenas prácticas.
*   **9-10 (Sobresaliente):** Implementación excelente, código limpio, características avanzadas.

---

## 🎯 Objetivos de Evaluación

### 1. Arquitectura y Orquestación (.NET Aspire)
**Aspectos a evaluar:**
*   Configuración correcta del `AppHost` y orquestación de contenedores.
*   Uso del Dashboard para monitorización.
*   Service Discovery y gestión de dependencias entre proyectos.
*   Implementación de Health Checks y OpenTelemetry.

### 2. Implementación de Microservicios (APIs REST)
**Aspectos a evaluar:**
*   Estructura de los servicios y separación de responsabilidades.
*   Uso correcto de Controladores o Minimal APIs.
*   Implementación de DTOs y validaciones.
*   **Estrategia de Versionado de API (URL, Header, etc.).**
*   Manejo de errores y códigos de estado HTTP.

### 3. Persistencia de Datos (EF Core & PostgreSQL)
**Aspectos a evaluar:**
*   Aplicación del patrón *Database per Service*.
*   Correcta configuración de Entity Framework Core y migraciones.
*   Modelado de datos y relaciones.
*   Inyección de dependencias de contextos de datos.

### 4. Seguridad e Identidad (Auth & JWT)
**Aspectos a evaluar:**
*   Configuración de ASP.NET Core Identity.
*   Generación y validación de tokens JWT.
*   Gestión de usuarios y roles.
*   Protección de endpoints (Autorización).

### 5. Comunicación e Integración (Gateway & Eventos)
**Aspectos a evaluar:**
*   Configuración de YARP como API Gateway.
*   Enrutamiento de tráfico y Rate Limiting.
*   Comunicación entre microservicios (Síncrona/Asíncrona).
*   **Caso de Uso RabbitMQ (Email):**
    *   **Mínimo:** Publicar evento `UserCreated` en RabbitMQ tras el registro.
    *   **Bien:** Consumir el evento desde un Worker y enviar email vía SMTP.

---

## ✅ Requisitos Mínimos para Aprobar (Nota >= 5)

Para considerar el proyecto como **APROBADO**, debe cumplir estrictamente con lo siguiente:

1.  **Compilación y Ejecución:** La solución debe compilar sin errores y arrancar mediante el perfil de .NET Aspire (`OrderFlow.AppHost`).
2.  **Base de Datos:** Los contenedores de PostgreSQL deben levantarse y las migraciones deben aplicarse automáticamente o mediante script documentado.
3.  **Identidad:** Debe ser posible registrar un usuario y obtener un token JWT mediante el endpoint de Login.
4.  **Gateway:** El Frontend o Postman deben poder consumir al menos un microservicio (ej. Catalog) a través del puerto del API Gateway, no directo al microservicio.
5.  **Funcionalidad Core:** Debe existir al menos un flujo de negocio funcional (ej. Ver productos o Crear un pedido simple).
6.  **Código:** El código no debe tener secretos/contraseñas hardcodeadas (usar `appsettings.json` o variables de entorno).
7.  **Versionado:** Al menos una API debe implementar una estrategia de versionado explícita (ej. `/v1/products`).
8.  **RabbitMQ:** Al registrar un usuario, debe publicarse un evento en RabbitMQ (verificable en la consola de administración de RabbitMQ).

---

## 📝 Comentarios del Profesor

*   **Puntos Fuertes:**
    *   
    *   

*   **Áreas de Mejora:**
    *   
    *   
