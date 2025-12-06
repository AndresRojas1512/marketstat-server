import os
import time
import subprocess
import shutil
import socket
import json
import csv
import requests

ITERATIONS = 2
MODES = ["BASELINE", "EF_SQL", "DAPPER"]
COMPOSE_FILE = "docker-compose.benchmark.yml"
RESULTS_DIR = "./results"
CSV_FILE = f"{RESULTS_DIR}/final_report.csv"
PROMETHEUS_URL = "http://localhost:9095"

def run_cmd(cmd, suppress_output=False):
    if not suppress_output:
        print(f"[$] {cmd}")
    stdout_dest = subprocess.DEVNULL if suppress_output else None
    subprocess.run(cmd, shell=True, check=True, stdout=stdout_dest)

def is_port_open(port):
    with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
        s.settimeout(1)
        return s.connect_ex(('localhost', port)) == 0
    
def aggressive_cleanup():
    try:
        subprocess.run(f"docker compose -f {COMPOSE_FILE} kill", shell=True, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
        subprocess.run(f"docker compose -f {COMPOSE_FILE} down -v --remove-orphans", shell=True, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
    except:
        pass
    
def wait_for_ports_to_clear(ports, timeout=20):
    start = time.time()
    while True:
        busy_ports = [p for p in ports if is_port_open(p)]
        if not busy_ports:
            return True
        if time.time() - start > timeout:
            print(f"  WARNING: Ports still in use after {timeout}s: {busy_ports}")
            return False
        time.sleep(1)

def dump_container_logs():
    print("\n--- API CONTAINER LOGS (START) ---")
    try:
        subprocess.run(f"docker logs ms_benchmark_api", shell=True)
    except:
        print("(Could not fetch logs)")
    print("--- API CONTAINER LOGS (END) ---\n")

def get_max_metric(query, start, end):
    try:
        params = {'query': query, 'start': start, 'end': end, 'step': 1}
        r = requests.get(f"{PROMETHEUS_URL}/api/v1/query_range", params=params)
        data = r.json()
        if data['status'] == 'success' and data['data']['result']:
            values = [float(x[1]) for x in data['data']['result'][0]['values']]
            return max(values) if values else 0.0
        return 0.0
    except:
        return 0.0

def main():
    if os.path.exists(RESULTS_DIR):
        subprocess.run(f"sudo rm -rf {RESULTS_DIR}", shell=True)
    os.makedirs(RESULTS_DIR)
    os.chmod(RESULTS_DIR, 0o777)

    with open(CSV_FILE, 'w', newline='') as f:
        writer = csv.writer(f)
        writer.writerow(["Implementation", "Run", "P95_Latency_ms", "Max_Memory_MB", "GC_Count"])

    print("=== MARKETSTAT BENCHMARK STARTING ===")
    aggressive_cleanup()
    print("Building Docker Image...")
    run_cmd(f"docker compose -f {COMPOSE_FILE} build api")

    for mode in MODES:
        print(f"\n>>> MODE: {mode} <<<")        
        for i in range(1, ITERATIONS + 1):
            print(f"  Run {i}/{ITERATIONS}...")
            try:
                aggressive_cleanup()
                if not wait_for_ports_to_clear([5055, 9095]):
                    print("  CRITICAL: Cannot start run, ports blocked. Skipping...")
                    continue

                my_env = os.environ.copy()
                my_env["REPO_IMPLEMENTATION"] = mode
                
                subprocess.run(f"docker compose -f {COMPOSE_FILE} up -d api db prometheus", shell=True, check=True, env=my_env)
                
                print("  Infrastructure is up. Warming up (30s)...")
                time.sleep(30)

                start_ts = time.time()
                
                print("  Executing k6...")
                run_cmd(f"docker compose -f {COMPOSE_FILE} run --rm k6 run /scripts/stress-test.js", suppress_output=True)

                end_ts = time.time()
                
                current_uid = os.getuid()
                current_gid = os.getgid()
                subprocess.run(f"sudo chown {current_uid}:{current_gid} {RESULTS_DIR}/stats.json", shell=True)

                k6_file = f"{RESULTS_DIR}/stats.json"
                p95 = 0
                if os.path.exists(k6_file):
                    with open(k6_file) as f:
                        data = json.load(f)
                        p95 = data['metrics']['http_req_duration']['values']['p(95)']
                    os.rename(k6_file, f"{RESULTS_DIR}/{mode.lower()}_run_{i}.json")
                
                mem = get_max_metric("process_private_memory_size_bytes", start_ts, end_ts) / (1024*1024)
                gc = get_max_metric("process_runtime_dotnet_gc_collections_count", start_ts, end_ts)
                
                print(f"    -> P95: {p95:.2f}ms | MaxRAM: {mem:.2f}MB | GC: {gc}")
                
                with open(CSV_FILE, 'a', newline='') as f:
                    csv.writer(f).writerow([mode, i, p95, mem, gc])
            
            except Exception as e:
                print(f"  CRITICAL ERROR in Run {i}: {e}")
                # dump_container_logs()
            
            finally:
                aggressive_cleanup()

    print("\n=== ALL BENCHMARKS COMPLETE ===")

if __name__ == "__main__":
    main()