# MarketStat Server

API REST para el análisis multidimensional de salarios en el mercado laboral ruso, con filtrado por ubicación, sector, puesto, nivel profesional y periodo, y generación de indicadores estadísticos, distribuciones salariales y series temporales.

## Funcionalidades

- Registro e inicio de sesión de usuarios con autenticación mediante JWT.
- Autorización basada en los roles de administrador y analista.
- Gestión de fechas, ubicaciones, niveles educativos, empleados, empresas, sectores profesionales, puestos y registros salariales.
- Filtrado de salarios por las dimensiones disponibles en el modelo de datos.
- Cálculo del salario medio, de los percentiles 25, 50 y 75, y de un percentil configurable.
- Generación de distribuciones salariales mediante intervalos.
- Generación de series temporales con distintas granularidades.
- Consultas públicas agregadas para usuarios no autenticados.
- Documentación interactiva de la API mediante Swagger.
- Registro estructurado de eventos y solicitudes mediante Serilog.

## Roles de acceso

- **Usuario anónimo:** acceso a consultas agregadas y catálogos públicos.
- **Analista:** acceso a resúmenes estadísticos, distribuciones salariales y series temporales.
- **Administrador:** acceso a las operaciones de gestión de datos y a las funciones analíticas.

## Arquitectura

Arquitectura modular con separación de responsabilidades:

- **API:** controladores HTTP, autenticación, autorización, configuración, middleware y documentación Swagger.
- **Servicios:** lógica de aplicación, validación y coordinación de operaciones.
- **Persistencia:** contexto de Entity Framework Core, repositorios PostgreSQL, modelos y migraciones.
- **Contratos y dominio:** entidades, DTO, convertidores, enumeraciones y excepciones comunes.
- **Pruebas:** pruebas unitarias, de repositorios, de integración y de extremo a extremo.

```text
src/
├── MarketStat/                    # API ASP.NET Core
├── MarketStat.Common/             # Dominio, DTO, convertidores y excepciones
├── MarketStat.Database/           # Contexto, modelos y repositorios
├── MarketStat.Services/           # Servicios de aplicación
└── MarketStat.Tests/              # Pruebas automatizadas
```

## Tecnologías

- C# y .NET 8
- ASP.NET Core Web API
- PostgreSQL y Entity Framework Core
- JWT Bearer Authentication
- AutoMapper y FluentValidation
- Serilog
- Swagger y OpenAPI
- xUnit, Moq, FluentAssertions, Testcontainers y Respawn
- Docker, Docker Compose y Nginx
- Allure Report
- GitHub Actions y GitLab CI/CD

## API

Prefijo base: `/api/v1`.

| Recurso | Ruta | Acceso |
| --- | --- | --- |
| Autenticación | `/api/v1/auth` | Público |
| Análisis salarial | `/api/v1/factsalaries/summary` | Analista y administrador |
| Distribución salarial | `/api/v1/factsalaries/distribution` | Analista y administrador |
| Series temporales | `/api/v1/factsalaries/timeseries` | Analista y administrador |
| Consulta agregada de puestos | `/api/v1/factsalaries/public/roles` | Público |
| Registros salariales | `/api/v1/factsalaries` | Administrador |
| Dimensiones | `/api/v1/dimdates`, `/dimlocations`, `/dimeducations`, `/dimemployees`, `/dimemployers`, `/dimindustryfields` y `/dimjobs` | Administrador, con catálogos públicos concretos |

Swagger UI:

```text
http://localhost:5000/swagger
```

Contrato OpenAPI: [`openapi/openapi.yaml`](openapi/openapi.yaml).

## Requisitos

- .NET SDK 8.0
- PostgreSQL 16
- Docker y Docker Compose para ejecutar la infraestructura de pruebas
- `dotnet-ef` para aplicar migraciones desde la línea de comandos

## Configuración

Configuración mediante `appsettings`, secretos de usuario o variables de entorno.

| Clave | Descripción |
| --- | --- |
| `ConnectionStrings__MarketStat` | Conexión operativa con PostgreSQL |
| `ConnectionStrings__MarketStatAdmin` | Conexión utilizada para aplicar migraciones |
| `JwtSettings__Key` | Clave de firma de los tokens JWT |
| `JwtSettings__Issuer` | Emisor de los tokens |
| `JwtSettings__Audience` | Audiencia de los tokens |
| `AllowedOrigins__AngularClient` | Origen permitido por la política CORS |
| `RunMigrations` | Aplicación automática de migraciones durante el arranque |

Ejemplo de configuración local con secretos de usuario:

```bash
dotnet user-secrets set "ConnectionStrings:MarketStat" "Host=localhost;Port=5432;Database=marketstat;Username=marketstat_analyst;Password=<contraseña>" --project src/MarketStat/MarketStat.csproj
dotnet user-secrets set "ConnectionStrings:MarketStatAdmin" "Host=localhost;Port=5432;Database=marketstat;Username=marketstat_administrator;Password=<contraseña>" --project src/MarketStat/MarketStat.csproj
dotnet user-secrets set "JwtSettings:Key" "<clave-de-al-menos-32-caracteres>" --project src/MarketStat/MarketStat.csproj
dotnet user-secrets set "JwtSettings:Issuer" "MarketStatAPI" --project src/MarketStat/MarketStat.csproj
dotnet user-secrets set "JwtSettings:Audience" "MarketStatUsers" --project src/MarketStat/MarketStat.csproj
```

Scripts SQL para la creación y configuración de roles de PostgreSQL: [`database/sql/roles`](database/sql/roles).

## Ejecución local

Inicialización y ejecución:

```bash
dotnet restore src/MarketStat.sln
dotnet ef database update \
  --project src/MarketStat.Database/MarketStat.Database.Context/MarketStat.Database.Context.csproj \
  --startup-project src/MarketStat/MarketStat.csproj
dotnet run --project src/MarketStat/MarketStat.csproj
```

API: `http://localhost:5000`.

## Pruebas

Niveles de pruebas:

- pruebas unitarias de servicios con xUnit y Moq
- pruebas de repositorios con Entity Framework Core InMemory
- pruebas de integración con PostgreSQL mediante Testcontainers
- pruebas E2E de la API con `WebApplicationFactory`, PostgreSQL y Testcontainers

Ejecución completa en contenedores, equivalente al flujo de GitHub Actions:

```bash
docker compose -f test-infra/compose/docker-compose.ci.yml \
  up --build --abort-on-container-exit
```

Ejecución directa con el SDK de .NET:

```bash
dotnet test src/MarketStat.sln
```

Requisito para las pruebas de integración y E2E: acceso a un daemon de Docker.

## Integración y despliegue continuos

- **GitHub Actions:** automatización de la compilación de la imagen de pruebas, ejecución de pruebas unitarias, de integración y E2E, y publicación del informe de Allure en GitHub Pages.
- **GitLab CI/CD:** automatización de las etapas de compilación, pruebas y despliegue mediante los scripts de [`scripts/ci`](scripts/ci).

## Implementaciones por rama

### `develop`

API monolítica modular sobre ASP.NET Core, con capas de API, servicios, dominio y persistencia. Operaciones REST, PostgreSQL mediante Entity Framework Core, autenticación JWT, autorización por roles, Swagger/OpenAPI y Serilog. Pruebas unitarias, de repositorios, de integración y E2E. Automatización con GitHub Actions y GitLab CI/CD, despliegue remoto mediante SSH, Nginx y `systemd`, e informes Allure.

### `feature/microservice`

Separación en `MarketStat.Gateway`, `MarketStat.Domain` y `MarketStat.Data`. Flujo de escritura: `HTTP → Gateway → RabbitMQ → Domain → RabbitMQ → Data → PostgreSQL`. Comandos asíncronos con respuesta `202 Accepted` y consultas mediante solicitud-respuesta de MassTransit.

Tres instancias por servicio, balanceo con Nginx, registro con Consul y Registrator, y PostgreSQL primaria-réplica. Trazabilidad con OpenTelemetry y Jaeger. Logs centralizados con Serilog, Loki, Promtail y Grafana. Compilación matricial de imágenes y pruebas mediante GitHub Actions.

### `test/benchmark`

Comparación de tres implementaciones de persistencia: Entity Framework Core con LINQ, Entity Framework Core con SQL parametrizado y Dapper con Npgsql. Misma base de datos, datos generados con `DbSeeder` y límites equivalentes de CPU y memoria.

Escenarios k6 secuenciales, paralelos y de serialización. Medición de solicitudes por segundo, latencias P50–P99, memoria, CPU y recolección de basura mediante OpenTelemetry, Prometheus y Grafana. Resultados en JSON y CSV, con gráficos generados mediante pandas, Matplotlib y Seaborn.

### `test/integration`

Pruebas de repositorios y servicios contra una instancia efímera de PostgreSQL 16 mediante Testcontainers. Migraciones de Entity Framework Core, datos de referencia y restablecimiento entre casos mediante Respawn. Cobertura de CRUD, filtros, distribuciones, percentiles, agregaciones y series temporales.

Pruebas E2E sobre Kestrel y una base de datos aislada. Captura PCAP con TShark, ejecución contenedorizada e informes Allure mediante GitHub Actions.

### `feature/ha-scaling-monitoring`

Topología con una API principal, dos instancias de lectura y una instancia espejo. Replicación asíncrona de PostgreSQL mediante streaming y `pg_basebackup`. Enrutamiento Nginx de `GET` y `HEAD` con ponderación `2:1:1`, escrituras hacia la instancia principal y acceso independiente mediante `/mirror`.

TLS, HTTP/2, HTTP/3 sobre QUIC, Gzip y caché estática. Registro de servicios con Consul y Registrator. Logs centralizados con Promtail, Loki y Grafana. Interfaces REST v1 y GraphQL v2 con Hot Chocolate. Validación del balanceo mediante Apache Benchmark.

### `test/external`

Exportación autenticada de resúmenes salariales en JSON mediante `POST /api/reports/salary-summary/export`. Abstracción `IReportStorageService` e implementación S3 con AWS SDK for .NET, compatible con Amazon S3 y Yandex Object Storage.

Validación E2E de autenticación, generación, carga `PutObject` y URL resultante. Simulación del servicio S3 mediante WireMock, ejecución con Docker Compose y captura de tráfico con TShark.
