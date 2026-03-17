# Financial Transfer Platform (.NET 8)

Solución robusta para la gestión de transferencias financieras con evaluación de riesgo asíncrona distribuida. Diseñada bajo principios de **Clean Architecture**, **CQRS** y **Event-Driven Design**.

## 🛠 Tech Stack
- **Runtime:** .NET 8 (LTS)
- **Base de Datos:** SQL Server 2022
- **Broker de Mensajería:** Kafka (Confluent Platform)
- **Orquestación:** Docker & Docker Compose
- **Librerías Clave:** MediatR, Entity Framework Core, Confluent.Kafka, Serilog, xUnit, Moq.

## 🚀 Guía de Inicio Rápido

### Requisitos
- Docker Desktop instalado.

### Despliegue
Desde la raíz del proyecto, ejecute:

docker-compose up --build

El sistema implementa una estrategia de self-healing. La API esperará automáticamente a que SQL Server y Kafka estén saludables antes de aplicar las migraciones e iniciar los servicios.
Documentación de la API

Una vez iniciada, puede acceder a la interfaz de Swagger en:

Swagger UI: http://localhost:5000/swagger

📋 Reglas de Negocio (Risk Engine)

El microservicio de riesgo evalúa cada operación bajo los siguientes criterios:
Límite Transaccional: Rechazo si el amount > 20,000,000 Gs.
Límite Acumulado Diario: Rechazo si la suma de pagos del mismo customerId en el día actual supera los 5,000,000 Gs.