# MarketStat High Availability & Monitoring Report

## 1. System Architecture
We have transformed the application into a Distributed High-Availability System:
* **Database:** PostgreSQL Master-Slave Replication (Async Streaming).
* **Compute:** 4 Instances (1 Write-Master, 2 Read-Only Replicas, 1 Mirror).
* **Gateway:** NGINX configured as a Layer 7 Load Balancer with traffic splitting.
* **Observability:** Grafana + Loki + Promtail stack for centralized logging.

## 2. Load Balancing Verification (Req #2)
**Algorithm:** Weighted Round-Robin (2:1:1)

### Test Configuration
* **Tool:** Apache Benchmark (`ab`)
* **Command:** `ab -n 40 -c 5 http://localhost:8080/api/v1/dimdates`
* **Traffic:** 184.58 requests/second

### Distribution Results
| Container Name | Role | Configured Weight | Actual Hits | Deviation |
| :--- | :--- | :--- | :--- | :--- |
| `ms_demo_api` | Write Master | **2** | **22** (55%) | +5% |
| `ms_demo_api_ro_1` | Read Replica | **1** | **10** (25%) | 0% |
| `ms_demo_api_ro_2` | Read Replica | **1** | **8** (20%) | -5% |

*Conclusion:* The load balancer is functioning correctly, respecting the weighted distribution logic.

## 3. Failover & Routing Logic
* **Write Isolation:** Verified that `POST` requests are routed exclusively to `ms_demo_api`. Read-only replicas received 0 write traffic during testing.
* **Mirror Site:** The `/mirror` path successfully routes to the isolated `api_mirror` instance connected to the DB Replica.
* **Optimization:** Gzip compression enabled (verified via `Content-Encoding: gzip`).

## 4. Monitoring Integration
Centralized logging is active. Logs from all 11 containers are aggregated in Loki and visualized via Grafana dashboards at `/monitoring/`.