import os
import time
import subprocess
import json
import csv
import requests
import traceback

# --- CONFIGURATION ---
ITERATIONS = 1
MODES = ["BASELINE", "EF_SQL", "DAPPER"]
COMPOSE_FILE = "docker-compose.benchmark.yml"
RESULTS_DIR = "./results"
CSV_FILE = f"{RESULTS_DIR}/happy_path_report.csv"
PROMETHEUS_URL = "http://localhost:9095"

def run_cmd(cmd, env=None, suppress_output=False, ignore_codes=None):
    if not suppress_output:
        print(f"[$] {cmd}")
    stdout_dest = subprocess.DEVNULL if suppress_output else None
    cmd_env = os.environ.copy()
    if env:
        cmd_env.update(env)
    
    try:
        subprocess.run(cmd, shell=True, check=True, stdout=stdout_dest, env=cmd_env)
        return True
    except subprocess.CalledProcessError as e:
        if ignore_codes and e.returncode in ignore_codes:
            return False
        print(f"Command failed with exit code {e.returncode}")
        raise e
    
def aggressive_cleanup():
    try:
        subprocess.run(f"docker compose -f {COMPOSE_FILE} kill", shell=True, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
        subprocess.run(f"docker compose -f {COMPOSE_FILE} down -v --remove-orphans", shell=True, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
    except:
        pass

def get_metric_any_of(candidate_names, start, end):
    if isinstance(candidate_names, str):
        candidate_names = [candidate_names]
        
    for query in candidate_names:
        try:
            # Add buffer (+10s) to catch ingestion lag
            params = {'query': query, 'start': start, 'end': end + 10, 'step': 1}
            r = requests.get(f"{PROMETHEUS_URL}/api/v1/query_range", params=params)
            data = r.json()
            
            if data['status'] == 'success' and data['data']['result']:
                values = [float(x[1]) for x in data['data']['result'][0]['values']]
                if values:
                    val = max(values)
                    if val > 0: 
                        return val
        except Exception:
            continue
    return 0.0

def main():
    if not os.path.exists(RESULTS_DIR):
        os.makedirs(RESULTS_DIR)

    with open(CSV_FILE, 'w', newline='') as f:
        writer = csv.writer(f)
        writer.writerow(["Implementation", "Run", "P95_Latency_ms", "Max_Memory_MB", "GC_Count"])

    print(f"=== HAPPY PATH BENCHMARK STARTING ===")
    
    aggressive_cleanup()

    run_cmd(f"docker compose -f {COMPOSE_FILE} build api")

    for mode in MODES:
        print(f"\n>>> MODE: {mode} <<<")        
        
        try:
            aggressive_cleanup()
            
            my_env = {"REPO_IMPLEMENTATION": mode}
            
            print("  Starting Infrastructure...")
            run_cmd(f"docker compose -f {COMPOSE_FILE} up -d api db prometheus", env=my_env, suppress_output=True)
            
            print("  Waiting for DB Seeding & Warmup (45s)...")
            time.sleep(45)

            start_ts = time.time()
            
            print("  Executing k6...")
            run_cmd(
                f"docker compose -f {COMPOSE_FILE} run --rm k6 run --summary-export /results/stats.json /scripts/stress-test.js",
                env=my_env, 
                suppress_output=True,
                ignore_codes=[99]
            )

            end_ts = time.time()

            print("  Collecting Metrics (Waiting 5s)...")
            time.sleep(5)
            
            try:
                subprocess.run(f"sudo chown {os.getuid()}:{os.getgid()} {RESULTS_DIR}/stats.json", shell=True, check=False)
            except:
                pass

            k6_file = f"{RESULTS_DIR}/stats.json"
            p95 = -1
            if os.path.exists(k6_file):
                with open(k6_file) as f:
                    data = json.load(f)
                    try: p95 = data['metrics']['http_req_duration']['p(95)']
                    except: pass
                os.rename(k6_file, f"{RESULTS_DIR}/{mode.lower()}_happy.json")
            
            # 1. TRY MEMORY (Underscore versions are most likely for Prometheus)
            mem_bytes = get_metric_any_of(
                [
                    "process_runtime_dotnet_gc_committed_memory_size_bytes",
                ], 
                start_ts, end_ts
            )
            mem_mb = mem_bytes / (1024*1024)
            
            # 2. TRY GC (Try looking for 'gen' vs 'generation')
            gc_count = get_metric_any_of(
                [
                    "process_runtime_dotnet_gc_collections_count_total{generation=\"gen2\"}"
                ],
                start_ts, end_ts
            )
            
            # 3. DIAGNOSTICS IF FAILED

            print(f"    -> RESULT: P95={p95:.2f}ms | RAM={mem_mb:.2f}MB | GC(Gen2)={int(gc_count)}")
            
            with open(CSV_FILE, 'a', newline='') as f:
                csv.writer(f).writerow([mode, 1, p95, mem_mb, gc_count])
        
        except Exception as e:
            print(f"  ERROR: {e}")
            traceback.print_exc()
        
        finally:
            pass

    print("\n=== HAPPY PATH COMPLETE ===")
    aggressive_cleanup()

if __name__ == "__main__":
    main()