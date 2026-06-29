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

| Rol | Acceso |
| --- | --- |
| Usuario anónimo | Consultas agregadas y catálogos públicos |
| Analista | Resúmenes estadísticos, distribuciones salariales y series temporales |
| Administrador | Gestión de datos y funciones analíticas |

## Arquitectura

Arquitectura modular con separación de responsabilidades:

- Controladores HTTP, autenticación, autorización, configuración, middleware y documentación Swagger en la capa de API.
- Lógica de aplicación, validación y coordinación de operaciones en la capa de servicios.
- Contexto de Entity Framework Core, repositorios PostgreSQL, modelos y migraciones en la capa de persistencia.
- Entidades, DTO, convertidores, enumeraciones y excepciones comunes en los módulos de contratos y dominio.
- Tests unitarios, de repositorios, de integración y de extremo a extremo.

```text
src/
├── MarketStat/                    # API ASP.NET Core
├── MarketStat.Common/             # Dominio, DTO, convertidores y excepciones
├── MarketStat.Database/           # Contexto, modelos y repositorios
├── MarketStat.Services/           # Servicios de aplicación
└── MarketStat.Tests/              # Tests automatizados
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
| Dimensiones | `/api/v1/dimdates`, `/api/v1/dimlocations`, `/api/v1/dimeducations`, `/api/v1/dimemployees`, `/api/v1/dimemployers`, `/api/v1/dimindustryfields` y `/api/v1/dimjobs` | Administrador, con catálogos públicos concretos |

Swagger UI:

```text
http://localhost:5000/swagger
```

Contrato OpenAPI: [`openapi/openapi.yaml`](openapi/openapi.yaml).

## Requisitos

- .NET SDK 8.0
- PostgreSQL 16
- Docker y Docker Compose para ejecutar la infraestructura de tests
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

## Tests

Niveles de tests:

- Tests unitarios de servicios con xUnit y Moq.
- Tests de repositorios con Entity Framework Core InMemory.
- Tests de integración con PostgreSQL mediante Testcontainers.
- Tests E2E de la API con `WebApplicationFactory`, PostgreSQL y Testcontainers.

Ejecución completa en contenedores, equivalente al flujo de GitHub Actions:

```bash
docker compose -f test-infra/compose/docker-compose.ci.yml \
  up --build --abort-on-container-exit
```

Ejecución directa con el SDK de .NET:

```bash
dotnet test src/MarketStat.sln
```

Requisito para los tests de integración y E2E: acceso a un daemon de Docker.

## Integración y despliegue continuos

- Automatización de la compilación de la imagen de tests, ejecución de tests unitarios, de integración y E2E, y publicación del informe de Allure en GitHub Pages mediante GitHub Actions.
- Automatización de las etapas de compilación, tests y despliegue mediante GitLab CI/CD y los scripts de [`scripts/ci`](scripts/ci).

## Implementaciones por rama

| Rama | Alcance |
| --- | --- |
| `develop` | API monolítica modular y CI/CD |
| `feature/microservice` | Arquitectura distribuida y mensajería |
| `test/benchmark` | Comparación de rendimiento de persistencia |
| `test/integration` | Integración con PostgreSQL y tests E2E |
| `feature/ha-scaling-monitoring` | Alta disponibilidad, escalado y observabilidad |
| `test/external` | Exportación de informes a almacenamiento S3 |

### `develop`

Implementación principal de MarketStat como API monolítica modular. La API, la lógica de aplicación y la persistencia forman una única unidad desplegable, con separación interna de responsabilidades.

- Exposición de operaciones REST para autenticación, gestión de dimensiones y análisis salarial, con documentación Swagger/OpenAPI.
- Persistencia en PostgreSQL mediante Entity Framework Core, autenticación JWT, autorización por roles y logs estructurados con Serilog.
- Tests unitarios, de repositorios, de integración y E2E, con generación de informes Allure.
- Automatización mediante GitHub Actions y GitLab CI/CD, con despliegue remoto por SSH, proxy inverso Nginx y ejecución como servicio `systemd`.

### `feature/microservice`

Variante distribuida con separación entre entrada HTTP, lógica de dominio y acceso a datos mediante `MarketStat.Gateway`, `MarketStat.Domain` y `MarketStat.Data`. La comunicación entre servicios se realiza mediante contratos de mensajería con MassTransit y RabbitMQ.

- Procesamiento de escrituras mediante el flujo `HTTP → Gateway → RabbitMQ → Domain → RabbitMQ → Data → PostgreSQL`, con comandos asíncronos y respuesta `202 Accepted`.
- Procesamiento de consultas mediante el patrón solicitud-respuesta de MassTransit.
- Escalado horizontal con tres instancias por servicio, balanceo Nginx, registro con Consul y Registrator, y topología PostgreSQL primaria-réplica.
- Trazabilidad distribuida con OpenTelemetry y Jaeger, y centralización de logs con Serilog, Loki, Promtail y Grafana.
- Compilación matricial de imágenes y ejecución de tests mediante GitHub Actions.

### `test/benchmark`

Entorno de benchmark para comparar el rendimiento de tres estrategias de acceso a datos bajo la misma carga, base de datos y límites de recursos.

| Variante | Acceso a datos |
| --- | --- |
| `BASELINE` | Entity Framework Core y LINQ |
| `EF_SQL` | SQL parametrizado mediante `FromSqlRaw` |
| `DAPPER` | Dapper y Npgsql |

- Preparación de un conjunto común de datos mediante `DbSeeder` y asignación equivalente de CPU y memoria a cada variante.
- Ejecución de escenarios k6 secuenciales, paralelos y orientados a medir la serialización de colecciones.
- Medición de solicitudes por segundo, latencias P50–P99, memoria, CPU y tiempo de recolección de basura mediante OpenTelemetry y Prometheus.
- Persistencia de resultados en JSON y CSV, visualización en Grafana y generación de gráficos con pandas, Matplotlib y Seaborn.

### `test/integration`

Suite para validar repositorios, servicios y endpoints HTTP contra PostgreSQL 16 y un servidor Kestrel ejecutados en entornos aislados.

- Creación de instancias PostgreSQL mediante Testcontainers, aplicación de migraciones de Entity Framework Core y carga de datos de referencia.
- Restablecimiento del estado de PostgreSQL entre casos mediante Respawn.
- Cobertura de operaciones CRUD, filtros multidimensionales, distribuciones, percentiles, agregaciones y series temporales.
- Tests E2E sobre Kestrel con captura del tráfico TCP en formato PCAP mediante TShark.
- Ejecución contenedorizada y publicación de informes Allure mediante GitHub Actions.

### `feature/ha-scaling-monitoring`

Topología para validar alta disponibilidad, separación del tráfico de lectura y escritura, replicación de datos y observabilidad de la API monolítica.

| Tráfico | Destino |
| --- | --- |
| `GET` y `HEAD` | API principal y dos instancias de lectura, ponderación `2:1:1` |
| Escrituras | API principal |
| `/mirror` | Instancia espejo |

- Replicación asíncrona de PostgreSQL mediante streaming y creación de la réplica con `pg_basebackup`.
- Enrutamiento por método HTTP mediante Nginx, con TLS, HTTP/2, HTTP/3 sobre QUIC, compresión Gzip y caché de contenido estático.
- Registro de instancias con Consul y Registrator, y centralización de logs con Promtail, Loki y Grafana.
- Exposición simultánea de las interfaces REST v1 y GraphQL v2 mediante Hot Chocolate.
- Validación de la distribución ponderada del tráfico mediante Apache Benchmark.

### `test/external`

Integración con un servicio externo de almacenamiento de objetos compatible con S3 para exportar y conservar informes salariales.

- Generación de un resumen salarial a partir de filtros mediante `POST /api/reports/salary-summary/export`, serialización en JSON, almacenamiento del archivo y devolución de su URL.
- Acceso autenticado para los roles `Admin` y `Analyst`.
- Abstracción mediante `IReportStorageService` e implementación S3 con AWS SDK for .NET, configurable para Amazon S3 y Yandex Object Storage.
- Validación E2E de autenticación, generación del informe, solicitud `PutObject` y URL resultante mediante WireMock.
- Ejecución del entorno con Docker Compose y captura del tráfico HTTP mediante TShark.
