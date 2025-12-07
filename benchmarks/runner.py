import os
import time
import subprocess
import json
import csv
import requests
import traceback

ITERATIONS = 3
MODES = ["BASELINE", "EF_SQL", "DAPPER"]
COMPOSE_FILE = "docker-compose.benchmark.yml"
RESULTS_DIR = "./results"
CSV_FILE = f"{RESULTS_DIR}/final_report.csv"
PROMETHEUS_URL = "http://localhost:9095"
GRAFANA_URL = "http://localhost:3001" 

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

def free_port(port):
    try:
        cmd = f"lsof -t -i:{port}"
        pid = subprocess.check_output(cmd, shell=True, stderr=subprocess.DEVNULL).decode().strip()
        if pid:
            print(f"  [!] Port {port} is busy (PID {pid}). Killing...")
            subprocess.run(f"kill -9 {pid}", shell=True, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
            time.sleep(1)
    except:
        pass 

def start_monitoring_stack():
    print(">>> Starting Persistent Monitoring Stack...")
    
    try:
        subprocess.run(f"docker compose -f {COMPOSE_FILE} --profile monitor down", shell=True, stderr=subprocess.DEVNULL)
    except: pass

    free_port(3001) 
    free_port(9095) 
    
    run_cmd(f"docker compose -f {COMPOSE_FILE} --profile monitor up -d")
    print(">>> Waiting for Grafana/Prometheus (15s)...")
    time.sleep(15)

def stop_monitoring_stack():
    print(">>> Stopping Monitoring Stack...")
    run_cmd(f"docker compose -f {COMPOSE_FILE} --profile monitor down -v")

def cycle_test_stack(mode):
    run_cmd(f"docker compose -f {COMPOSE_FILE} --profile test stop api db k6", suppress_output=True)
    run_cmd(f"docker compose -f {COMPOSE_FILE} --profile test rm -f -v api db k6", suppress_output=True)
    
    free_port(5055)

    my_env = {"REPO_IMPLEMENTATION": mode}
    run_cmd(f"docker compose -f {COMPOSE_FILE} --profile test up -d api db", env=my_env, suppress_output=True)

def grafana_annotate(text, tags):
    try:
        payload = {
            "text": text,
            "tags": tags
        }
        requests.post(
            f"{GRAFANA_URL}/api/annotations", 
            json=payload, 
            headers={"Content-Type": "application/json"},
            auth=('admin', 'admin'),
            timeout=2
        )
    except Exception:
        pass 

def get_prometheus_metric(query, start, end):
    try:
        params = {'query': query, 'start': start, 'end': end + 10, 'step': 1}
        r = requests.get(f"{PROMETHEUS_URL}/api/v1/query_range", params=params)
        data = r.json()
        if data['status'] == 'success' and data['data']['result']:
            values = [float(x[1]) for x in data['data']['result'][0]['values']]
            if values:
                return max(values)
    except:
        pass
    return 0.0

def main():
    if not os.path.exists(RESULTS_DIR):
        os.makedirs(RESULTS_DIR)

    with open(CSV_FILE, 'w', newline='') as f:
        writer = csv.writer(f)
        writer.writerow(["Implementation", "Run", "P95_Latency_ms", "Max_Memory_MB", "GC_Count"])

    print(f"=== MARKETSTAT FULL BENCHMARK (GRAFANA MODE - {ITERATIONS} RUNS) ===")
    
    start_monitoring_stack()
    
    print(">>> Building API Image...")
    run_cmd(f"docker compose -f {COMPOSE_FILE} build api")

    try:
        for mode in MODES:
            print(f"\n>>> MODE: {mode} <<<")        
            
            for i in range(1, ITERATIONS + 1):
                print(f"  Run {i}/{ITERATIONS}...")
                
                grafana_annotate(f"Start Run {i} ({mode})", ["start", mode])

                cycle_test_stack(mode)
                
                time.sleep(60) 

                start_ts = time.time()
                
                my_env = {"REPO_IMPLEMENTATION": mode}
                run_cmd(
                    f"docker compose -f {COMPOSE_FILE} --profile test run --rm k6 run --summary-export /results/stats.json /scripts/stress-test.js",
                    env=my_env, 
                    suppress_output=True,
                    ignore_codes=[99] 
                )

                end_ts = time.time()
                time.sleep(5) 
                
                grafana_annotate(f"End Run {i}", ["end"])
                
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
                    os.rename(k6_file, f"{RESULTS_DIR}/{mode.lower()}_run_{i}.json")
                                
                mem_bytes = get_prometheus_metric(
                    "process_runtime_dotnet_gc_committed_memory_size_bytes", 
                    start_ts, end_ts
                )
                mem_mb = mem_bytes / (1024*1024)
                
                gc_count = get_prometheus_metric(
                    "process_runtime_dotnet_gc_collections_count_total{generation=\"gen2\"}",
                    start_ts, end_ts
                )
                
                print(f"    -> Run {i}: P95={p95:.2f}ms | RAM={mem_mb:.2f}MB | GC(Gen2)={int(gc_count)}")
                
                with open(CSV_FILE, 'a', newline='') as f:
                    csv.writer(f).writerow([mode, i, p95, mem_mb, gc_count])

    except KeyboardInterrupt:
        print("\n>>> Benchmark interrupted by user.")
    except Exception as e:
        print(f"\n>>> CRITICAL ERROR: {e}")
        traceback.print_exc()
    finally:
        print("\n=== BENCHMARK COMPLETE ===")
        print(f"Grafana is available at: {GRAFANA_URL}")
        print("Username: admin / Password: admin")
        print("Check the dashboard now. Press [ENTER] to stop the stack and clean up.")
        try:
            input()
        except:
            pass
        
        stop_monitoring_stack()
        run_cmd(f"docker compose -f {COMPOSE_FILE} --profile test down -v")

if __name__ == "__main__":
    main()