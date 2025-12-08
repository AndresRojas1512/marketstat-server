import subprocess
import time
import os
import sys
import traceback
import urllib.request
import urllib.error

# --- CONFIGURATION ---
ITERATIONS = 1
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
    if env: cmd_env.update(env)
    if bg: return subprocess.Popen(cmd, shell=True, env=cmd_env)
    subprocess.run(cmd, shell=True, check=True, env=cmd_env)

def free_port(port):
    try:
        pid = subprocess.check_output(f"lsof -t -i:{port}", shell=True, stderr=subprocess.DEVNULL).decode().strip()
        if pid:
            subprocess.run(f"kill -9 {pid}", shell=True, stderr=subprocess.DEVNULL)
    except: pass 

def wait_for_health(port, mode, timeout=120):
    print(f"  [...] Waiting for {mode} (Port {port}) to be healthy...")
    start = time.time()
    url = f"http://localhost:{port}/swagger/v1/swagger.json"
    while time.time() - start < timeout:
        try:
            with urllib.request.urlopen(url) as response:
                if response.status == 200:
                    print(f"  [✓] {mode} is UP!")
                    return True
        except: pass
        time.sleep(2)
    print(f"  [X] {mode} failed to start.")
    return False

def stop_stack(project_name):
    print(f"  -> Stopping {project_name}...")
    subprocess.run(
        f"docker compose -f {COMPOSE_FILE} -p {project_name} down -v", 
        shell=True, stderr=subprocess.DEVNULL
    )

def main():
    try:
        print(f"=== MARKETSTAT SEQUENTIAL BENCHMARK ===")
        
        # 1. Start Monitoring Stack (Persistent across all runs)
        print("\n>>> Starting Shared Monitoring Stack...")
        run_cmd(f"docker compose -f {MONITOR_FILE} up -d")
        time.sleep(5)

        # 2. Run Configs Sequentially
        for cfg in CONFIGS:
            mode = cfg['mode']
            port = cfg['port']
            project_name = f"ms_{mode.lower()}"
            
            print(f"\n===========================================")
            print(f">>> TESTING IMPLEMENTATION: {mode}")
            print(f"===========================================")
            
            # A. Clean Start
            free_port(port)
            env = os.environ.copy()
            env["REPO_IMPLEMENTATION"] = mode
            env["API_PORT"] = str(port)
            env["API_URL"] = f"http://localhost:{port}/api" # For K6

            print(f"  -> Starting API Stack...")
            run_cmd(f"docker compose -f {COMPOSE_FILE} -p {project_name} up -d api db", env=env)

            # B. Health Check
            if not wait_for_health(port, mode):
                print(f"  [!] Skipping {mode} due to startup failure.")
                stop_stack(project_name)
                continue

            print("  -> Stabilizing (5s)...")
            time.sleep(5)

            # C. Run Load Test
            print(f"  -> Launching K6 for {mode}...")
            try:
                # We use check=True to raise an error if K6 fails thresholds
                run_cmd(
                    f"docker compose -f {COMPOSE_FILE} -p {project_name} run --rm "
                    f"-e API_URL=http://api:8080/api k6 run /scripts/stress-test.js",
                    env=env
                )
            except subprocess.CalledProcessError:
                print(f"  [!] {mode} FAILED the benchmark thresholds (Performance too low).")
                print(f"  [!] Moving to next implementation...")

            # D. Stop this stack to free resources for the next one
            stop_stack(project_name)
            print(f"  -> Cooldown (5s)...")
            time.sleep(5)

    except KeyboardInterrupt:
        print("\n>>> Interrupted.")
    except Exception as e:
        traceback.print_exc()
    finally:
        print("\n=== BENCHMARK COMPLETE ===")
        print("Don't forget to stop the monitoring stack when done analyzing:")
        print(f"docker compose -f {MONITOR_FILE} down -v")

if __name__ == "__main__":
    main()