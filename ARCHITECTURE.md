ARCHITECTURE.md

# Architectural Design Document

## 🏗 Diseño de Arquitectura: Clean Architecture
Se ha implementado una arquitectura en capas para garantizar el desacoplamiento total de las reglas de negocio de los detalles de infraestructura.

- **Domain:** Contiene las entidades de negocio (`Payment`), enums y lógica de estado. Es una capa pura sin dependencias externas.
- **Application:** Define los contratos (Interfaces) y los casos de uso. Implementa el patrón **CQRS** mediante **MediatR** para separar las operaciones de comando (escritura) y consulta (lectura).
- **Infrastructure:** Implementaciones concretas de persistencia (EF Core), repositorios y el productor/consumidor de Kafka.
- **API:** Capa de presentación que expone los endpoints REST y gestiona el ciclo de vida de los Workers.

## 🔄 Flujo de Datos Event-Driven
El sistema utiliza un modelo de consistencia eventual para mejorar el throughput de la API:

1. **Ingesta:** La API recibe el pago, lo persiste con estado `Evaluating` y publica un evento en el tópico `risk-evaluation-request`.
2. **Evaluación:** El `RiskEngine` consume el evento, consulta el acumulado en la DB y aplica las reglas. Publica el resultado en `risk-evaluation-response`.
3. **Cierre:** Un Background Worker en la API escucha la respuesta y actualiza el estado final del pago (`Accepted` o `Denied`).

## 🛡️ Resiliencia y Estabilidad
- **Transient Fault Handling:** Se configuró `EnableRetryOnFailure` en SQL Server para manejar micro-cortes de red.
- **Startup Resiliency:** Se implementó un bucle de reintento exponencial en el `Program.cs` para la aplicación de migraciones, permitiendo que la API sea resiliente a los tiempos de "Boot-up" de los contenedores de infraestructura.
- **Healthchecks:** El archivo `docker-compose` utiliza verificaciones de salud activas (`sqlcmd` para DB y `broker-api-versions` para Kafka) para orquestar un encendido ordenado de los servicios.

## 📈 Escalabilidad
Gracias al uso de Kafka y el desacoplamiento de capas:
- El **RiskEngine** puede escalarse horizontalmente para procesar grandes volúmenes de eventos sin impactar la latencia de respuesta de la API de entrada.
- La persistencia utiliza **Read Committed Snapshot Isolation (RCSI)** para evitar bloqueos entre las lecturas de Swagger y las escrituras del flujo de Kafka.

## ✒️ Decisiones Técnicas
- **Decimal sobre Float:** Para precisión financiera absoluta.
- **GUID como Clave Primaria:** Para facilitar la generación de IDs de operación únicos en sistemas distribuidos y evitar colisiones en futuras migraciones o integraciones.
- **Logging Estructurado:** Uso de plantillas en logs para facilitar la indexación en herramientas como ELK o Seq.