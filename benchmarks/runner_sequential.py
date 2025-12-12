import subprocess
import time
import os
import sys
import traceback
import urllib.request
import urllib.error
import urllib.parse
import json
import csv
import datetime

ITERATIONS = 100
RESULTS_FILE = "benchmark_final_report.csv"
COMPOSE_FILE = "docker-compose.benchmark.yml"
MONITOR_FILE = "docker-compose.monitoring.yml"
PROMETHEUS_URL = "http://localhost:9091" 

DUMMY_ENV = {
    "REPO_IMPLEMENTATION": "BASELINE", "API_PORT": "5055"
}

CONFIGS = [
    {"mode": "BASELINE", "port": 5055},
    {"mode": "EF_SQL",   "port": 5056},
    {"mode": "DAPPER",   "port": 5057},
]

def run_cmd(cmd, env=None, bg=False, suppress_output=False, check=True):
    cmd_env = os.environ.copy()
    cmd_env.update(DUMMY_ENV) 
    if env:
        cmd_env.update(env)
    
    if not suppress_output:
        print(f"[$] {cmd}")
    
    if bg:
        return subprocess.Popen(cmd, shell=True, env=cmd_env)
    
    subprocess.run(
        cmd, 
        shell=True, 
        check=check, 
        env=cmd_env, 
        stdout=subprocess.DEVNULL if suppress_output else None, 
        stderr=subprocess.DEVNULL if suppress_output else None
    )
    

def wait_for_health(port, mode, timeout=60):
    print(f"    [...] Waiting for {mode}...", end="", flush=True)
    start = time.time()
    url = f"http://localhost:{port}/swagger/v1/swagger.json"
    while time.time() - start < timeout:
        try:
            with urllib.request.urlopen(url, timeout=1) as response:
                if response.status == 200: 
                    print(" OK")
                    return True
        except: pass
        time.sleep(1)
        print(".", end="", flush=True)
    print(f"\n    [X] {mode} failed to start.")
    return False

# --- PASSPORT METRIC FETCHING ---

def query_prometheus_range(query, start_time, end_time, step='1s'):
    """Fetch raw time-series arrays for plotting"""
    try:
        params = {'query': query, 'start': start_time, 'end': end_time, 'step': step}
        url = f"{PROMETHEUS_URL}/api/v1/query_range?{urllib.parse.urlencode(params)}"
        with urllib.request.urlopen(url) as response:
            data = json.loads(response.read().decode())
            if data['status'] == 'success' and data['data']['result']:
                # Return the list of [timestamp, value]
                return data['data']['result'][0]['values'] 
    except: pass
    return []

def query_scalar(query):
    """Fetch a single number (max/sum/avg)"""
    try:
        url = f"{PROMETHEUS_URL}/api/v1/query?query={urllib.parse.quote(query)}"
        with urllib.request.urlopen(url) as r:
            d = json.loads(r.read().decode())
            if d['data']['result']: return float(d['data']['result'][0]['value'][1])
    except: pass
    return 0.0

def fetch_metrics(mode):
    service = f"MarketStat.API.{mode}"
    end_t = time.time()
    start_t = end_t - 60 # Look back 60s
    
    # --- 1. Define Queries for "The Passport" ---
    
    # A. MEMORY (Heap Size)
    mem_query = f'process_runtime_dotnet_gc_committed_memory_size_bytes{{service_name="{service}"}}'
    
    # B. ALLOCATIONS (Churn - Total Bytes Allocated) - KEY FOR MENTOR
    alloc_query = f'process_runtime_dotnet_gc_allocations_size_bytes_total{{service_name="{service}"}}'
    
    # C. CPU (Total CPU Time consumed by process)
    cpu_query = f'process_cpu_seconds_total{{service_name="{service}"}}'
    
    # D. GC EFFICIENCY (Time spent paused)
    gc_pause_query = f'process_runtime_dotnet_gc_duration_nanoseconds_total{{service_name="{service}"}}'
    
    # E. THREADS (Queue Length - Starvation indicator)
    thread_query = f'process_runtime_dotnet_thread_pool_queue_length{{service_name="{service}"}}'

    # --- 2. Fetch Summaries (Scalars for Quick Check) ---
    mem_max_bytes = query_scalar(f'max_over_time({mem_query}[60s])')
    total_alloc_bytes = query_scalar(f'increase({alloc_query}[60s])')
    total_cpu_sec = query_scalar(f'increase({cpu_query}[60s])')
    total_gc_pause_ns = query_scalar(f'increase({gc_pause_query}[60s])')
    
    # --- 3. Fetch Raw Time Series (For Future Plotting) ---
    # We fetch the RATE (per second) for counters to make graphs readable
    
    # Memory Usage Over Time
    series_mem = query_prometheus_range(mem_query, start_t, end_t)
    
    # Allocation Rate (Bytes/sec)
    series_alloc_rate = query_prometheus_range(f'rate({alloc_query}[1m])', start_t, end_t)
    
    # CPU Usage % (approximate)
    series_cpu = query_prometheus_range(f'rate({cpu_query}[1m])', start_t, end_t)
    
    # GC Pause Rate (Seconds paused per second)
    series_gc = query_prometheus_range(f'rate({gc_pause_query}[1m])', start_t, end_t)

    return {
        "summary": {
            "max_memory_mb": round(mem_max_bytes / (1024 * 1024), 2),
            "total_allocated_mb": round(total_alloc_bytes / (1024 * 1024), 2),
            "total_cpu_sec": round(total_cpu_sec, 4),
            "total_gc_pause_sec": round(total_gc_pause_ns / 1e9, 4)
        },
        "time_series": {
            "memory_committed_bytes": series_mem,
            "allocation_rate_bytes_sec": series_alloc_rate,
            "cpu_usage_rate": series_cpu,
            "gc_pause_rate": series_gc
        }
    }

def init_csv():
    if not os.path.exists(RESULTS_FILE):
        with open(RESULTS_FILE, mode='w', newline='') as file:
            writer = csv.writer(file)
            writer.writerow([
                "Iteration", "Implementation", "Status", "Req/s", 
                "Avg_Latency", "P50", "P75", "P90", "P95", "P99", 
                "Error_Rate", "Max_Memory_MB", "Total_Alloc_MB", "GC_Time_Sec", "CPU_Time_Sec"
            ])

def save_result(iteration, mode, status, metrics=None, summary=None):
    with open(RESULTS_FILE, mode='a', newline='') as file:
        writer = csv.writer(file)
        if status == "CRASHED" or not metrics:
            writer.writerow([iteration, mode, "CRASHED", 0, 0, 0, 0, 0, 0, 0, 100, 0, 0, 0, 0])
        else:
            http_dur = metrics.get('http_req_duration', {})
            http_reqs = metrics.get('http_reqs', {})
            err = metrics.get('error_rate', {})
            err_val = err.get('rate') if err.get('rate') is not None else err.get('value', 0)

            writer.writerow([
                iteration, mode, status,
                round(http_reqs.get('rate', 0), 2),
                round(http_dur.get('avg', 0), 2),
                round(http_dur.get('med', 0), 2),
                round(http_dur.get('p(75)', 0), 2),
                round(http_dur.get('p(90)', 0), 2),
                round(http_dur.get('p(95)', 0), 2),
                round(http_dur.get('p(99)', 0), 2),
                round(err_val * 100, 2),
                summary.get('max_memory_mb', 0),
                summary.get('total_allocated_mb', 0),
                summary.get('total_gc_pause_sec', 0),
                summary.get('total_cpu_sec', 0)
            ])

def main():
    try:
        init_csv()
        print(f"=== MARKETSTAT PASSPORT BENCHMARK ({ITERATIONS} RUNS) ===")
        
        # Initial Cleanup
        print(">>> Cleaning previous containers...")
        subprocess.run(f"docker compose -f {COMPOSE_FILE} down -v --remove-orphans", shell=True, stderr=subprocess.DEVNULL)
        
        print(">>> Starting Monitoring Stack...")
        run_cmd(f"docker compose -f {MONITOR_FILE} up -d", suppress_output=False)
        time.sleep(5) 

        for i in range(1, ITERATIONS + 1):
            print(f"\n[Run {i}/{ITERATIONS}]")
            
            for cfg in CONFIGS:
                mode = cfg['mode']
                port = cfg['port']
                project_name = f"ms_{mode.lower()}"
                
                free_port(port)
                subprocess.run(f"docker compose -f {COMPOSE_FILE} -p {project_name} down -v", shell=True, stderr=subprocess.DEVNULL)
                
                env = os.environ.copy()
                env.update({"REPO_IMPLEMENTATION": mode, "API_PORT": str(port)})
                run_cmd(f"docker compose -f {COMPOSE_FILE} -p {project_name} up -d api db", env=env, suppress_output=True)

                if not wait_for_health(port, mode, timeout=90): 
                    save_result(i, mode, "CRASHED")
                    subprocess.run(f"docker compose -f {COMPOSE_FILE} -p {project_name} down -v", shell=True, stderr=subprocess.DEVNULL)
                    continue
                
                print(f"    [+] {mode} is UP!")
                time.sleep(5) # Stabilization

                # Run K6
                json_report = f"results/report_{mode}_{i}.json"
                try:
                    run_cmd(
                        f"docker compose -f {COMPOSE_FILE} -p {project_name} run --rm "
                        f"-e API_URL=http://api:8080/api "
                        f"k6 run --summary-export=/{json_report} /scripts/stress-test.js",
                        env=env, suppress_output=True, check=False
                    )
                    
                    # Capture PASSPORT METRICS
                    full_metrics = fetch_metrics(mode)
                    summary = full_metrics["summary"]
                    
                    try:
                        with open(f"results/report_{mode}_{i}.json", "r+") as f:
                            data = json.load(f)
                            
                            err_rate = data['metrics']['error_rate'].get('rate', 0)
                            status = "SUCCESS" if err_rate < 0.10 else "THRESHOLD_FAIL"
                            
                            # SAVE THE PASSPORT (Time Series + Summaries)
                            data['custom_metrics'] = {
                                'summary': summary,
                                'time_series': full_metrics["time_series"],
                                'timestamp': datetime.datetime.now().isoformat()
                            }
                            f.seek(0)
                            json.dump(data, f, indent=4)
                            f.truncate()
                            
                            save_result(i, mode, status, data['metrics'], summary)
                            print(f"    [+] {mode}: {status} | P95={round(data['metrics']['http_req_duration']['p(95)'],1)}ms | Alloc={summary['total_allocated_mb']}MB")
                            
                    except FileNotFoundError:
                        print(f"    [!] JSON report not found for {mode}")
                        save_result(i, mode, "CRASHED")

                except Exception:
                    try:
                        # Try to save partial resource data even if K6 crashed
                        full_metrics = fetch_metrics(mode)
                        save_result(i, mode, "FAILED", None, full_metrics["summary"])
                        print(f"    [!] {mode}: FAILED")
                    except:
                        save_result(i, mode, "CRASHED")
                        print(f"    [!] {mode}: CRASHED")

                # Cleanup
                subprocess.run(f"docker compose -f {COMPOSE_FILE} -p {project_name} down -v", shell=True, stderr=subprocess.DEVNULL)

    except KeyboardInterrupt:
        print("\n>>> Interrupted.")
    finally:
        print(f"\nDone. Results saved to {RESULTS_FILE}")

if __name__ == "__main__":
    main()