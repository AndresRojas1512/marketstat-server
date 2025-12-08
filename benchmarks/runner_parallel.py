import subprocess
import time
import os
import signal
import sys
import traceback
import urllib.request
import urllib.error

# --- CONFIGURATION ---
ITERATIONS = 1
COMPOSE_FILE = "docker-compose.benchmark.yml"
MONITOR_FILE = "docker-compose.monitoring.yml"

# Configuration for parallel stacks
CONFIGS = [
    {"mode": "BASELINE", "port": 5055},
    {"mode": "EF_SQL",   "port": 5056},
    {"mode": "DAPPER",   "port": 5057},
]

def run_cmd(cmd, env=None, bg=False, suppress_output=False):
    """Runs a shell command. Can be blocking or background."""
    if not suppress_output:
        print(f"[$] {cmd}")
    
    cmd_env = os.environ.copy()
    if env:
        cmd_env.update(env)

    if bg:
        # Returns the process object so we can wait() on it later
        return subprocess.Popen(cmd, shell=True, env=cmd_env)
    
    # Blocking call
    subprocess.run(cmd, shell=True, check=True, env=cmd_env)

def free_port(port):
    """Kills any process occupying the target port to ensure clean startup."""
    try:
        cmd = f"lsof -t -i:{port}"
        pid = subprocess.check_output(cmd, shell=True, stderr=subprocess.DEVNULL).decode().strip()
        if pid:
            print(f"  [!] Port {port} is busy (PID {pid}). Killing...")
            subprocess.run(f"kill -9 {pid}", shell=True, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
            time.sleep(1)
    except:
        pass 

def wait_for_health(port, mode, timeout=300):
    """Polls the API until it returns a 200 OK on the Swagger endpoint."""
    print(f"  [...] Waiting for {mode} (Port {port}) to be healthy...")
    start = time.time()
    # We use the swagger definition as a health check because it requires the app to be fully booted
    url = f"http://localhost:{port}/swagger/v1/swagger.json"
    
    while time.time() - start < timeout:
        try:
            with urllib.request.urlopen(url) as response:
                if response.status == 200:
                    print(f"  [✓] {mode} is UP!")
                    return True
        except (urllib.error.URLError, ConnectionResetError):
            pass # Connection failed, wait and retry
        except Exception as e:
            print(f"  [?] unexpected error checking health: {e}")
        
        time.sleep(2)
    
    print(f"  [X] {mode} failed to start within {timeout}s.")
    return False

def cleanup():
    """Stops all containers (Benchmark + Monitoring)."""
    print("\n>>> CLEANUP INITIATED...")
    
    # Stop the 3 Benchmark Stacks
    for cfg in CONFIGS:
        project_name = f"ms_{cfg['mode'].lower()}"
        print(f"  -> Stopping {project_name}...")
        subprocess.run(
            f"docker compose -f {COMPOSE_FILE} -p {project_name} down -v", 
            shell=True, stderr=subprocess.DEVNULL
        )
    
    # Stop the Monitoring Stack
    print("  -> Stopping Monitoring Stack...")
    subprocess.run(
        f"docker compose -f {MONITOR_FILE} down -v", 
        shell=True, stderr=subprocess.DEVNULL
    )

def main():
    try:
        print(f"=== MARKETSTAT PARALLEL BENCHMARK ({ITERATIONS} ITERATIONS) ===")
        
        # 1. Clean previous runs (Ports & Docker)
        cleanup()
        free_port(3001)  # Grafana
        free_port(9095)  # Prometheus
        for cfg in CONFIGS:
            free_port(cfg['port'])

        # 2. Start Shared Monitoring Stack (Prometheus + Grafana)
        print("\n>>> Starting Shared Monitoring Stack (Host Mode)...")
        run_cmd(f"docker compose -f {MONITOR_FILE} up -d")
        print(">>> Waiting 10s for Monitoring to initialize...")
        time.sleep(10)

        # 3. Start All 3 Benchmark Stacks in Parallel
        print("\n>>> Starting 3 Parallel API Stacks...")
        for cfg in CONFIGS:
            env = os.environ.copy()
            env["REPO_IMPLEMENTATION"] = cfg["mode"]
            env["API_PORT"] = str(cfg["port"])
            project_name = f"ms_{cfg['mode'].lower()}"
            
            # Start API and DB only (k6 runs later)
            print(f"  -> Starting {cfg['mode']} on Port {cfg['port']}...")
            run_cmd(f"docker compose -f {COMPOSE_FILE} -p {project_name} up -d api db", env=env)

        # 4. Smart Health Check
        print("\n>>> Verifying API Health...")
        all_healthy = True
        for cfg in CONFIGS:
            if not wait_for_health(cfg['port'], cfg['mode']):
                all_healthy = False
        
        if not all_healthy:
            raise Exception("One or more API stacks failed to start. Aborting benchmark.")

        print("\n>>> APIs are Healthy. Stabilizing for 10s...")
        time.sleep(10)

        # 5. Run Benchmark Loops
        print("\n>>> STARTING STRESS TESTS")
        for i in range(1, ITERATIONS + 1):
            print(f"\n=== ITERATION {i}/{ITERATIONS} ===")
            
            k6_processes = []
            
            # Launch k6 for all 3 stacks simultaneously
            for cfg in CONFIGS:
                env = os.environ.copy()
                env["REPO_IMPLEMENTATION"] = cfg["mode"]
                env["API_PORT"] = str(cfg["port"])
                env["API_URL"] = "http://api:8080/api" 
                project_name = f"ms_{cfg['mode'].lower()}"
                
                print(f"  -> Launching k6 for {cfg['mode']}...")
                
                p = run_cmd(
                    f"docker compose -f {COMPOSE_FILE} -p {project_name} run --rm k6 run /scripts/stress-test.js",
                    env=env,
                    bg=True,
                    suppress_output=True 
                )
                k6_processes.append(p)
            
            # Wait for all 3 k6 runs to finish before starting next iteration
            print("  -> Waiting for all tests to complete...")
            for p in k6_processes:
                p.wait()
            
            print(f"  -> Iteration {i} Complete. Cooling down (10s)...")
            time.sleep(10)

    except KeyboardInterrupt:
        print("\n>>> Benchmark interrupted by user.")
    except Exception as e:
        print(f"\n>>> CRITICAL ERROR: {e}")
        traceback.print_exc()
    finally:
        print("\n=== BENCHMARK COMPLETE ===")
        print(f"Grafana is available at: http://localhost:3001")
        try:
            input("Press [ENTER] to stop the stack and clean up.")
        except:
            pass
        cleanup()

if __name__ == "__main__":
    main()