import subprocess
import time
import os
import sys
import signal
import urllib.request
import urllib.parse
import json
import csv
import datetime
import statistics 
import shutil

ITERATIONS = 5
RESULTS_FILE = "benchmark_final_report.csv"
COMPOSE_FILE = "docker-compose.benchmark.yml"
MONITOR_FILE = "docker-compose.monitoring.yml"
PROMETHEUS_URL = "http://localhost:9091" 

DUMMY_ENV = { "REPO_IMPLEMENTATION": "BASELINE", "API_PORT": "5055" }

CONFIGS = [
    {"mode": "BASELINE", "port": 5055},
    {"mode": "EF_SQL",   "port": 5056},
    {"mode": "DAPPER",   "port": 5057},
]

STOP_REQUESTED = False

def signal_handler(sig, frame):
    global STOP_REQUESTED
    print("\n\n>>> CTRL+C DETECTED. Stopping gracefully after cleanup...")
    STOP_REQUESTED = True

signal.signal(signal.SIGINT, signal_handler)

def run_cmd(cmd, env=None, bg=False, suppress_output=False, check=True):
    if STOP_REQUESTED: raise KeyboardInterrupt
    
    cmd_env = os.environ.copy()
    cmd_env.update(DUMMY_ENV) 
    if env: cmd_env.update(env)
    
    if not suppress_output: print(f"[$] {cmd}")
    
    if bg: return subprocess.Popen(cmd, shell=True, env=cmd_env)
    
    result = subprocess.run(
        cmd, shell=True, env=cmd_env,
        stdout=subprocess.PIPE if suppress_output else None,
        stderr=subprocess.PIPE if suppress_output else None
    )
    
    if check and result.returncode != 0:
        if suppress_output:
            print(f"\n[!] Command Failed: {cmd}")
            print(f"[!] STDERR: {result.stderr.decode('utf-8', errors='ignore').strip()}")
        raise subprocess.CalledProcessError(result.returncode, cmd)
    
    return result

def build_locally():
    print(">>> 1. BUILDING APPLICATION LOCALLY (Host Machine)...")
    
    project_path = "../src/MarketStat/MarketStat.csproj"
    output_path = "../published"
    
    if os.path.exists(output_path):
        shutil.rmtree(output_path)
        
    try:
        cmd = f"dotnet publish {project_path} -c Release -o {output_path}"
        run_cmd(cmd, suppress_output=False)
        print(">>> Build Successful. Artifacts ready in 'server/published'.")
    except subprocess.CalledProcessError:
        print(">>> FATAL: Local build failed. Check your dotnet SDK.")
        sys.exit(1)

def wait_for_health(port, mode, timeout=60):
    print(f"    [...] Waiting for {mode}...", end="", flush=True)
    start = time.time()
    url = f"http://localhost:{port}/swagger/v1/swagger.json"
    while time.time() - start < timeout:
        if STOP_REQUESTED: return False
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


def query_range(query, start_t, end_t, step='2s'):
    try:
        params = {'query': query, 'start': start_t, 'end': end_t, 'step': step}
        url = f"{PROMETHEUS_URL}/api/v1/query_range?{urllib.parse.urlencode(params)}"
        with urllib.request.urlopen(url) as r:
            d = json.loads(r.read().decode())
            if d['data']['result']: return d['data']['result'][0]['values']
    except: pass
    return []

def query_scalar(query):
    try:
        url = f"{PROMETHEUS_URL}/api/v1/query?query={urllib.parse.quote(query)}"
        with urllib.request.urlopen(url) as r:
            d = json.loads(r.read().decode())
            if d['data']['result']: return float(d['data']['result'][0]['value'][1])
    except: pass
    return 0.0

def fetch_metrics(mode):
    service = f"MarketStat.API.{mode}"
    window = "16s"
    
    end_t = time.time()
    start_t = end_t - 16

    mem_query = f'process_runtime_dotnet_gc_committed_memory_size_bytes{{service_name="{service}"}}'
    
    cpu_query = f'process_cpu_time_seconds_total{{service_name="{service}"}}'
    
    gc_query = f'process_runtime_dotnet_gc_duration_nanoseconds_total{{service_name="{service}"}}'
    
    lat_sum = f'http_server_request_duration_seconds_sum{{service_name="{service}"}}'
    lat_count = f'http_server_request_duration_seconds_count{{service_name="{service}"}}'
    lat_query = f'rate({lat_sum}[5s]) / rate({lat_count}[5s])'

    mem_series = query_range(mem_query, start_t, end_t)
    cpu_series = query_range(f'rate({cpu_query}[5s])', start_t, end_t)
    gc_series = query_range(f'rate({gc_query}[5s])', start_t, end_t)
    lat_series = query_range(lat_query, start_t, end_t)
    
    def calc_stats(series, factor=1.0):
        if not series: return {"min": 0, "max": 0, "med": 0}
        vals = [float(x[1]) * factor for x in series]
        return {
            "min": round(min(vals), 4),
            "max": round(max(vals), 4),
            "med": round(statistics.median(vals), 4)
        }

    mem_stats = calc_stats(mem_series, factor=1/(1024*1024))
    cpu_stats = calc_stats(cpu_series)
    gc_total_seconds = round(query_scalar(f'increase({gc_query}[{window}])') / 1e9, 4)

    return {
        "summary": { "memory": mem_stats, "cpu": cpu_stats, "gc_seconds": gc_total_seconds },
        "time_series": { "memory_bytes": mem_series, "cpu_rate": cpu_series, "gc_rate": gc_series, "latency_seconds": lat_series }
    }

def init_csv():
    with open(RESULTS_FILE, mode='w', newline='') as file:
        writer = csv.writer(file)
        writer.writerow([
            "Iteration", "Implementation", "Status", "Req/s", 
            "P50_Latency_ms", "P75_Latency_ms", "P90_Latency_ms", "P95_Latency_ms", "P99_Latency_ms", 
            "Max_Memory_MB", "Med_Memory_MB", "Max_CPU_Cores", "GC_Seconds"
        ])

def save_result(iteration, mode, status, metrics=None, summary=None):
    with open(RESULTS_FILE, mode='a', newline='') as file:
        writer = csv.writer(file)
        if status == "CRASHED" or not metrics:
            writer.writerow([iteration, mode, "CRASHED", 0, 0, 0, 0, 0, 0, 0, 0, 0, 0])
        else:
            dur = metrics.get('http_req_duration', {})
            writer.writerow([
                iteration, mode, status,
                round(metrics.get('http_reqs', {}).get('rate', 0), 2),
                round(dur.get('p(50)', 0), 2),
                round(dur.get('p(75)', 0), 2),
                round(dur.get('p(90)', 0), 2),
                round(dur.get('p(95)', 0), 2),
                round(dur.get('p(99)', 0), 2),
                summary.get('memory', {}).get('max', 0),
                summary.get('memory', {}).get('med', 0),
                summary.get('cpu', {}).get('max', 0),
                summary.get('gc_seconds', 0)
            ])

def main():
    try:
        build_locally()
        
        init_csv()
        if not os.path.exists("results"): os.makedirs("results")

        print(f"=== MARKETSTAT ENDURANCE BENCHMARK ({ITERATIONS} Runs/Impl) ===")
        
        print(">>> 2. PACKAGING DOCKER IMAGE...")
        run_cmd(f"docker compose -f {COMPOSE_FILE} build --no-cache api", suppress_output=False)
        
        print(">>> Cleaning previous containers...")
        run_cmd(f"docker compose -f {COMPOSE_FILE} down -v --remove-orphans", suppress_output=True)
        
        print(">>> Starting Monitoring & Database...")
        run_cmd(f"docker compose -f {MONITOR_FILE} up -d", suppress_output=True)
        run_cmd(f"docker compose -f {COMPOSE_FILE} up -d db", suppress_output=True)
        time.sleep(10)

        print(">>> Checking Database Seeding...")
        seed_env = {"REPO_IMPLEMENTATION": "BASELINE", "API_PORT": "5055"}
        run_cmd(f"docker compose -f {COMPOSE_FILE} up -d api", env=seed_env, suppress_output=True)
        if not wait_for_health(5055, "SEEDER", timeout=120): return
        run_cmd(f"docker compose -f {COMPOSE_FILE} stop api", env=seed_env, suppress_output=True)
        run_cmd(f"docker compose -f {COMPOSE_FILE} rm -f api", env=seed_env, suppress_output=True)

        for cfg in CONFIGS:
            if STOP_REQUESTED: break
            
            mode = cfg['mode']
            port = cfg['port']
            env = {"REPO_IMPLEMENTATION": mode, "API_PORT": str(port)}
            
            print(f"\n==================================================")
            print(f">>> STARTING BLOCK: {mode}")
            print(f"==================================================")
            
            run_cmd(f"docker compose -f {COMPOSE_FILE} up -d api", env=env, suppress_output=True)
            
            if not wait_for_health(port, mode):
                print(f"[!] Critical: {mode} failed to start. Skipping.")
                continue
            
            print("    [!] Warming up JIT & Connection Pools...", end="", flush=True)
            run_cmd(f"docker compose -f {COMPOSE_FILE} run --rm -e API_URL=http://api:8080/api k6 run --vus 10 --duration 10s /scripts/stress-test.js", env=env, suppress_output=True, check=False)
            print(" Done.")

            for i in range(1, ITERATIONS + 1):
                if STOP_REQUESTED: break
                
                print(f"    [Run {i}/{ITERATIONS}] {mode}...", end="", flush=True)
                
                json_filename = f"report_{mode}_{i}.json"
                local_path = os.path.join("results", json_filename)
                
                if os.path.exists(local_path): os.remove(local_path)
                
                try:
                    k6_cmd = (
                        f"docker compose -f {COMPOSE_FILE} run --rm "
                        f"-e API_URL=http://api:8080/api "
                        f"k6 run --summary-export=/results/{json_filename} /scripts/stress-test.js"
                    )
                    
                    run_cmd(k6_cmd, env=env, suppress_output=True, check=False)
                    
                    full_metrics = fetch_metrics(mode)
                    summary = full_metrics['summary']
                    
                    retry = 0
                    while not os.path.exists(local_path) and retry < 5:
                        time.sleep(0.5)
                        retry += 1

                    try:
                        with open(local_path, "r+") as f:
                            data = json.load(f)
                            err = data['metrics']['error_rate'].get('rate', 0)
                            status = "SUCCESS" if err < 0.10 else "THRESHOLD_FAIL"
                            
                            data['custom_metrics'] = {
                                'summary': summary,
                                'time_series': full_metrics['time_series'],
                                'timestamp': datetime.datetime.now().isoformat()
                            }
                            f.seek(0)
                            json.dump(data, f, indent=4)
                            f.truncate()
                            
                            save_result(i, mode, status, data['metrics'], summary)
                            print(f" Done. (Mem: {summary['memory']['max']}MB)")
                    except (FileNotFoundError, json.JSONDecodeError):
                        print(" Failed (Report Error)")
                        save_result(i, mode, "CRASHED")
                        
                except Exception as e:
                    print(f" Failed (Exec Error: {e})")
                    save_result(i, mode, "CRASHED")
                
                time.sleep(2)

            print(f">>> Stopping {mode}...")
            run_cmd(f"docker compose -f {COMPOSE_FILE} stop api", env=env, suppress_output=True)
            run_cmd(f"docker compose -f {COMPOSE_FILE} rm -f api", env=env, suppress_output=True)

    except KeyboardInterrupt:
        print("\n\n>>> STOPPING BENCHMARK (Cleanup started)...")
    finally:
        print(">>> Tearing down infrastructure...")
        subprocess.run(f"docker compose -f {COMPOSE_FILE} down -v", shell=True)
        print(">>> Done.")

if __name__ == "__main__":
    main()