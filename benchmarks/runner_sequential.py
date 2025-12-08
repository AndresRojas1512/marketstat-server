import subprocess
import time
import os
import sys
import traceback
import urllib.request
import urllib.error
import json
import csv

ITERATIONS = 1
RESULTS_FILE = "benchmark_final_report.csv"
COMPOSE_FILE = "docker-compose.benchmark.yml"
MONITOR_FILE = "docker-compose.monitoring.yml"

CONFIGS = [
    {"mode": "BASELINE", "port": 5055},
    {"mode": "EF_SQL",   "port": 5056},
    {"mode": "DAPPER",   "port": 5057},
]

def run_cmd(cmd, env=None, bg=False, suppress_output=False):
    if not suppress_output:
        print(f"[$] {cmd}")
    cmd_env = os.environ.copy()
    if env:
        cmd_env.update(env)
    if bg:
        return subprocess.Popen(cmd, shell=True, env=cmd_env)
    subprocess.run(cmd, shell=True, check=True, env=cmd_env, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)

def free_port(port):
    try:
        cmd = f"lsof -t -i:{port}"
        pid = subprocess.check_output(cmd, shell=True, stderr=subprocess.DEVNULL).decode().strip()
        if pid:
            subprocess.run(f"kill -9 {pid}", shell=True, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
    except:
        pass

def wait_for_health(port, mode, timeout=120):
    print(f"  [...] Waiting for {mode} (Port {port}) to be healthy...")
    start = time.time()
    url = f"http://localhost:{port}/swagger/v1/swagger.json"
    while time.time() - start < timeout:
        try:
            with urllib.request.urlopen(url) as response:
                if response.status == 200:
                    print(f"  [+] API {mode} is UP!")
                    return True
        except:
            pass
        time.sleep(1)
    print(f"  [X] {mode} failed to start within {timeout}s.")
    return False

def stop_stack(project_name):
    print(f"  -> Stopping {project_name}...")
    subprocess.run(
        f"docker compose -f {COMPOSE_FILE} -p {project_name} down -v", 
        shell=True, stderr=subprocess.DEVNULL
    )

def init_csv():
    """Creates the CSV file with headers if it doesn't exist."""
    with open(RESULTS_FILE, mode='w', newline='') as file:
        writer = csv.writer(file)
        writer.writerow([
            "Iteration", "Implementation", "Status", "Req/s", 
            "Avg_Latency", "P50", "P75", "P90", "P95", "P99", "Error_Rate"
        ])

def save_result(iteration, mode, status, metrics=None):
    with open(RESULTS_FILE, mode='a', newline='') as file:
        writer = csv.writer(file)
        
        if status == "CRASHED" or not metrics:
            writer.writerow([iteration, mode, "CRASHED", 0, 0, 0, 0, 0, 0, 0, 100])
        else:
            http_duration = metrics.get('http_req_duration', {})
            http_reqs = metrics.get('http_reqs', {})
            error_rate = metrics.get('error_rate', {})

            raw_error = error_rate.get('rate')
            if raw_error is None:
                raw_error = error_rate.get('value', 0)

            writer.writerow([
                iteration, 
                mode, 
                status,
                round(http_reqs.get('rate', 0), 2),
                round(http_duration.get('avg', 0), 2),
                round(http_duration.get('med', 0), 2),
                round(http_duration.get('p(75)', 0), 2),
                round(http_duration.get('p(90)', 0), 2),
                round(http_duration.get('p(95)', 0), 2),
                round(http_duration.get('p(99)', 0), 2),
                round(raw_error * 100, 2)
            ])

def main():
    try:
        init_csv()
        print(f"=== MARKETSTAT SEQUENTIAL BENCHMARK ({ITERATIONS} RUNS) ===")
        
        print("\n>>> Starting Shared Monitoring Stack...")
        run_cmd(f"docker compose -f {MONITOR_FILE} up -d", suppress_output=False)
        time.sleep(5)

        for i in range(1, ITERATIONS + 1):
            print(f"\n[Run {i}/{ITERATIONS}] ----------------------------------------")
            
            for cfg in CONFIGS:
                mode = cfg['mode']
                port = cfg['port']
                project_name = f"ms_{mode.lower()}"
                
                print(f"  >>> Testing: {mode}")
                
                free_port(port)
                env = os.environ.copy()
                env["REPO_IMPLEMENTATION"] = mode
                env["API_PORT"] = str(port)
                
                stop_stack(project_name) 
                
                run_cmd(f"docker compose -f {COMPOSE_FILE} -p {project_name} up -d api db", env=env, suppress_output=True)

                if not wait_for_health(port, mode):
                    print(f"    [!] Skipping {mode} due to startup failure.")
                    save_result(i, mode, "CRASHED")
                    stop_stack(project_name)
                    continue

                time.sleep(5)

                json_report = f"results/report_{mode}_{i}.json"
                
                try:
                    run_cmd(
                        f"docker compose -f {COMPOSE_FILE} -p {project_name} run --rm "
                        f"-e API_URL=http://api:8080/api "
                        f"k6 run --summary-export=/{json_report} /scripts/stress-test.js",
                        env=env,
                        suppress_output=True
                    )
                    
                    with open(f"results/report_{mode}_{i}.json") as f:
                        data = json.load(f)
                        save_result(i, mode, "SUCCESS", data['metrics'])
                    print(f"    [+] {mode}: Completed successfully.")

                except subprocess.CalledProcessError:
                    print(f"    [!] {mode}: Failed Thresholds (Performance too low).")
                    try:
                        with open(f"results/report_{mode}_{i}.json") as f:
                            data = json.load(f)
                            save_result(i, mode, "FAILED_THRESHOLDS", data['metrics'])
                    except:
                        save_result(i, mode, "CRASHED")
                except Exception as ex:
                    print(f"    [!] Unexpected error: {ex}")
                    save_result(i, mode, "CRASHED")

                stop_stack(project_name)
                time.sleep(2)

    except KeyboardInterrupt:
        print("\n>>> Interrupted by user.")
    except Exception as e:
        traceback.print_exc()
    finally:
        print("\n=== BENCHMARK COMPLETE ===")
        print(f"Results saved to: {RESULTS_FILE}")
        print("To stop monitoring: docker compose -f docker-compose.monitoring.yml down -v")

if __name__ == "__main__":
    main()