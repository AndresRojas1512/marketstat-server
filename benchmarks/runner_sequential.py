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

# --- CONFIGURATION ---
ITERATIONS = 100
RESULTS_FILE = "benchmark_final_report.csv"
COMPOSE_FILE = "docker-compose.benchmark.yml"
MONITOR_FILE = "docker-compose.monitoring.yml"
PROMETHEUS_URL = "http://localhost:9091" 

CONFIGS = [
    {"mode": "BASELINE", "port": 5055},
    {"mode": "EF_SQL",   "port": 5056},
    {"mode": "DAPPER",   "port": 5057},
]

def run_cmd(cmd, env=None, bg=False, suppress_output=False, check=True):
    if not suppress_output: print(f"[$] {cmd}")
    cmd_env = os.environ.copy()
    if env: cmd_env.update(env)
    if bg: return subprocess.Popen(cmd, shell=True, env=cmd_env)
    
    # Run the command
    subprocess.run(
        cmd, 
        shell=True, 
        check=check, 
        env=cmd_env, 
        stdout=subprocess.DEVNULL if suppress_output else None, 
        stderr=subprocess.DEVNULL if suppress_output else None
    )

def free_port(port):
    try:
        cmd = f"lsof -t -i:{port}"
        pid = subprocess.check_output(cmd, shell=True, stderr=subprocess.DEVNULL).decode().strip()
        if pid: subprocess.run(f"kill -9 {pid}", shell=True, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
    except: pass

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

# --- METRIC FETCHING ---
def query_prometheus(query):
    try:
        url = f"{PROMETHEUS_URL}/api/v1/query?query={urllib.parse.quote(query)}"
        req = urllib.request.Request(url)
        with urllib.request.urlopen(req) as response:
            data = json.loads(response.read().decode())
            if data['status'] == 'success' and data['data']['result']:
                return float(data['data']['result'][0]['value'][1])
    except: pass
    return 0.0

def fetch_metrics(mode):
    service = f"MarketStat.API.{mode}"
    # [45s] window to capture this run without overlap
    mem = query_prometheus(f'max_over_time(process_runtime_dotnet_gc_committed_memory_size_bytes{{service_name="{service}"}}[45s])')
    gc = query_prometheus(f'increase(process_runtime_dotnet_gc_duration_nanoseconds_total{{service_name="{service}"}}[45s])')
    return round(mem / (1024 * 1024), 2), round(gc / 1e9, 4)

def init_csv():
    # Only write header if starting fresh
    if not os.path.exists(RESULTS_FILE):
        with open(RESULTS_FILE, mode='w', newline='') as file:
            writer = csv.writer(file)
            writer.writerow([
                "Iteration", "Implementation", "Status", "Req/s", 
                "Avg_Latency", "P50", "P75", "P90", "P95", "P99", 
                "Error_Rate", "Max_Memory_MB", "GC_Time_Sec"
            ])

def save_result(iteration, mode, status, metrics=None, mem_mb=0, gc_sec=0):
    with open(RESULTS_FILE, mode='a', newline='') as file:
        writer = csv.writer(file)
        if status == "CRASHED" or not metrics:
            writer.writerow([iteration, mode, "CRASHED", 0, 0, 0, 0, 0, 0, 0, 100, 0, 0])
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
                mem_mb,
                gc_sec
            ])

def main():
    try:
        init_csv()
        print(f"=== MARKETSTAT ROBUST BENCHMARK ({ITERATIONS} RUNS) ===")
        
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
                
                # 1. CLEAN START
                free_port(port)
                # Force cleanup of the specific project
                subprocess.run(f"docker compose -f {COMPOSE_FILE} -p {project_name} down -v", shell=True, stderr=subprocess.DEVNULL)
                
                env = os.environ.copy()
                env.update({"REPO_IMPLEMENTATION": mode, "API_PORT": str(port)})
                
                # Start API and DB together (Fresh Volume)
                run_cmd(f"docker compose -f {COMPOSE_FILE} -p {project_name} up -d api db", env=env, suppress_output=True)

                # 2. Health Check (Includes Seeding Time)
                if not wait_for_health(port, mode, timeout=120): # Increased timeout for seeding
                    save_result(i, mode, "CRASHED")
                    subprocess.run(f"docker compose -f {COMPOSE_FILE} -p {project_name} down -v", shell=True, stderr=subprocess.DEVNULL)
                    continue
                
                # 3. Stabilization
                time.sleep(5)

                # 4. Run Load Test
                json_report = f"results/report_{mode}_{i}.json"
                try:
                    run_cmd(
                        f"docker compose -f {COMPOSE_FILE} -p {project_name} run --rm "
                        f"-e API_URL=http://api:8080/api "
                        f"k6 run --summary-export=/{json_report} /scripts/stress-test.js",
                        env=env, suppress_output=True, check=False
                    )
                    
                    # 5. Capture & Inject Metrics
                    mem_mb, gc_sec = fetch_metrics(mode)
                    
                    try:
                        with open(f"results/report_{mode}_{i}.json", "r+") as f:
                            data = json.load(f)
                            
                            err_rate = data['metrics']['error_rate'].get('rate', 0)
                            status = "SUCCESS" if err_rate < 0.10 else "THRESHOLD_FAIL"
                            
                            data['custom_metrics'] = {
                                'max_memory_mb': mem_mb,
                                'gc_time_sec': gc_sec,
                                'timestamp': datetime.datetime.now().isoformat()
                            }
                            f.seek(0)
                            json.dump(data, f, indent=4)
                            f.truncate()
                            
                            save_result(i, mode, status, data['metrics'], mem_mb, gc_sec)
                            print(f"    [+] {mode}: {status} | P95={round(data['metrics']['http_req_duration']['p(95)'],1)}ms | Mem={mem_mb}MB")
                            
                    except FileNotFoundError:
                        print(f"    [!] JSON report not found for {mode}")
                        save_result(i, mode, "CRASHED")

                except Exception as e:
                    try:
                        mem_mb, gc_sec = fetch_metrics(mode)
                        save_result(i, mode, "FAILED", None, mem_mb, gc_sec)
                        print(f"    [!] {mode}: FAILED")
                    except:
                        save_result(i, mode, "CRASHED")
                        print(f"    [!] {mode}: CRASHED")

                # 6. Full Cleanup
                subprocess.run(f"docker compose -f {COMPOSE_FILE} -p {project_name} down -v", shell=True, stderr=subprocess.DEVNULL)

    except KeyboardInterrupt:
        print("\n>>> Interrupted.")
    finally:
        print(f"\nDone. Results saved to {RESULTS_FILE}")

if __name__ == "__main__":
    main()