import subprocess
import time
import os
import sys
import signal
import urllib.request
import shutil

ITERATIONS = 10
COMPOSE_FILE = "docker-compose.benchmark.yml"
MONITOR_FILE = "docker-compose.monitoring.yml"

K6_SCRIPT = "/scripts/stress-test-serialization.js"
# K6_SCRIPT = "/scripts/stress-test.js"

TARGET_BASELINE = {"name": "BASELINE", "port": 5055, "k6_service": "k6-baseline"}
TARGET_EFSQL    = {"name": "EF_SQL",   "port": 5056, "k6_service": "k6-efsql"}
TARGET_DAPPER   = {"name": "DAPPER",   "port": 5057, "k6_service": "k6-dapper"}

ALL_TARGETS = [TARGET_BASELINE, TARGET_EFSQL, TARGET_DAPPER]

STOP_REQUESTED = False

def signal_handler(sig, frame):
    global STOP_REQUESTED
    print("\n\n>>> CTRL+C DETECTED. Stopping gracefully...")
    STOP_REQUESTED = True

signal.signal(signal.SIGINT, signal_handler)

def run_cmd(cmd, suppress_output=False, bg=False):
    if STOP_REQUESTED:
        raise KeyboardInterrupt
    
    if not suppress_output:
        print(f"[$] {cmd}")
    
    if bg:
        return subprocess.Popen(cmd, shell=True)
        
    result = subprocess.run(
        cmd, shell=True, 
        stdout=subprocess.PIPE if suppress_output else None,
        stderr=subprocess.PIPE if suppress_output else None
    )
    if result.returncode != 0 and not suppress_output:
        print(f"[!] Command failed: {cmd}")
        sys.exit(1)
    return result

def wait_for_targets(targets, timeout=120):
    """Waits for a specific list of targets to become healthy"""
    print(f"    [...] Waiting for {[t['name'] for t in targets]}...")
    start_time = time.time()
    
    while time.time() - start_time < timeout:
        if STOP_REQUESTED: return False
        
        healthy_count = 0
        for t in targets:
            try:
                url = f"http://localhost:{t['port']}/swagger/v1/swagger.json"
                with urllib.request.urlopen(url, timeout=1) as response:
                    if response.status == 200: healthy_count += 1
            except: pass
        
        sys.stdout.write(f"\r    Status: {healthy_count}/{len(targets)} ready...   ")
        sys.stdout.flush()
        
        if healthy_count == len(targets):
            print("\n    [OK] Ready.")
            return True
        time.sleep(2)
        
    print(f"\n    [X] Timeout waiting for {[t['name'] for t in targets]}.")
    return False

def main():
    try:
        print(">>> 1. BUILDING...")
        run_cmd(f"docker compose -f {COMPOSE_FILE} build api-baseline")
        
        print(">>> 2. CLEANUP...")
        run_cmd(f"docker compose -f {COMPOSE_FILE} down -v --remove-orphans", suppress_output=True)
        
        print(">>> 3. INFRASTRUCTURE INIT...")
        run_cmd(f"docker compose -f {MONITOR_FILE} up -d", suppress_output=True)
        
        run_cmd(f"docker compose -f {COMPOSE_FILE} up -d db")
        time.sleep(5)

        print("\n>>> [STAGE 1] Starting Master (BASELINE) for DB Migration...")
        run_cmd(f"docker compose -f {COMPOSE_FILE} up -d api-baseline")
        
        if not wait_for_targets([TARGET_BASELINE], timeout=120):
            print("Master container failed to start. Aborting.")
            return

        print("\n>>> [STAGE 2] Starting Peers (EF_SQL, DAPPER)...")
        run_cmd(f"docker compose -f {COMPOSE_FILE} up -d api-efsql api-dapper")
        
        if not wait_for_targets([TARGET_EFSQL, TARGET_DAPPER], timeout=120):
            print("Peer containers failed to start. Aborting.")
            return

        print("\n>>> ALL SYSTEMS STARTED. Starting Serialization Benchmark Loop...")

        for i in range(1, ITERATIONS + 1):
            if STOP_REQUESTED: break
            print(f"\n==================================================")
            print(f">>> [ITERATION {i}/{ITERATIONS}] PARALLEL LOAD (SERIALIZATION)")
            print(f"==================================================")
            
            procs = []
            for t in ALL_TARGETS:
                json_report = f"/results/report_serialization_{t['name']}_run{i}.json"
                
                cmd = (
                    f"docker compose -f {COMPOSE_FILE} run --rm "
                    f"{t['k6_service']} run --summary-export={json_report} {K6_SCRIPT}"
                )
                
                procs.append(run_cmd(cmd, suppress_output=True, bg=True))
            
            print(f"    -> {len(procs)} K6 Generators running...")
            
            for p in procs:
                p.wait()
            
            print(f"    -> Iteration {i} Complete.")
            time.sleep(5) 

    except KeyboardInterrupt:
        print("\nStopping...")
    finally:
        print(">>> TEARING DOWN...")
        run_cmd(f"docker compose -f {COMPOSE_FILE} down -v")

if __name__ == "__main__":
    main()