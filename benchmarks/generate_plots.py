import pandas as pd
import matplotlib.pyplot as plt
import seaborn as sns
import os
import json
import glob
import numpy as np

# Configuration
CSV_FILE = 'benchmark_final_report.csv'
JSON_DIR = 'results'
OUTPUT_DIR = 'charts'
os.makedirs(OUTPUT_DIR, exist_ok=True)

# Consistent Color Palette
PALETTE = {
    "BASELINE": "#1f77b4", # Blue
    "EF_SQL":   "#ff7f0e", # Orange
    "DAPPER":   "#2ca02c"  # Green
}

sns.set_theme(style="whitegrid")
plt.rcParams.update({'figure.figsize': (12, 7), 'font.size': 12})

def load_and_plot_summaries():
    """Generates bar/box plots from the CSV summary data."""
    try:
        df = pd.read_csv(CSV_FILE)
    except FileNotFoundError:
        print(f"Error: {CSV_FILE} not found.")
        return

    # Filter valid runs
    df_valid = df[df['Status'].isin(['SUCCESS', 'THRESHOLD_FAIL'])]
    print(f"Summary Data: {len(df_valid)} valid runs found.")

    if df_valid.empty: return

    # 1. Throughput
    plt.figure()
    sns.barplot(x='Implementation', y='Req/s', data=df, errorbar='sd', palette=PALETTE)
    plt.title('Throughput (Requests/Sec)')
    plt.tight_layout()
    plt.savefig(f'{OUTPUT_DIR}/1_throughput.png')

    # 2. P95 Latency
    plt.figure()
    sns.boxplot(x='Implementation', y='P95', data=df_valid, palette=PALETTE)
    plt.title('P95 Latency Distribution (ms)')
    plt.tight_layout()
    plt.savefig(f'{OUTPUT_DIR}/2_latency_p95.png')

    # 3. Memory Footprint (Max)
    plt.figure()
    sns.boxplot(x='Implementation', y='Max_Memory_MB', data=df_valid, palette=PALETTE)
    plt.title('Peak Memory Usage (MB)')
    plt.tight_layout()
    plt.savefig(f'{OUTPUT_DIR}/3_memory_peak.png')

    # 4. Total Allocations (CRITICAL FOR MENTOR)
    plt.figure()
    sns.barplot(x='Implementation', y='Total_Alloc_MB', data=df_valid, errorbar='sd', palette=PALETTE)
    plt.title('Total Memory Allocated per Run (MB) - "Memory Churn"')
    plt.ylabel('MegaBytes Allocated')
    plt.tight_layout()
    plt.savefig(f'{OUTPUT_DIR}/4_allocations_total.png')

    # 5. GC Time (CPU Overhead)
    plt.figure()
    sns.barplot(x='Implementation', y='GC_Time_Sec', data=df_valid, errorbar='sd', palette=PALETTE)
    plt.title('CPU Time Lost to Garbage Collection (Seconds)')
    plt.tight_layout()
    plt.savefig(f'{OUTPUT_DIR}/5_gc_overhead.png')

    print(">>> Summary charts generated.")

def parse_time_series_from_jsons():
    """Reads all JSONs to build time-series datasets."""
    print(">>> Parsing JSON time-series data (this may take a moment)...")
    
    memory_data = []
    allocation_data = []
    
    # Find all JSON reports
    files = glob.glob(os.path.join(JSON_DIR, "report_*.json"))
    
    for fpath in files:
        try:
            # Extract implementation name from filename
            filename = os.path.basename(fpath)
            
            if "EF_SQL" in filename:
                impl = "EF_SQL"
            elif "DAPPER" in filename:
                impl = "DAPPER"
            elif "BASELINE" in filename:
                impl = "BASELINE"
            else:
                continue

            with open(fpath, 'r') as f:
                data = json.load(f)
                
            # Get the vectors
            ts_data = data.get('custom_metrics', {}).get('time_series', {})
            mem_points = ts_data.get('memory_committed_bytes', [])
            alloc_points = ts_data.get('allocation_rate_bytes_sec', [])
            
            if not mem_points: continue

            # Normalize Time: Start at T=0
            start_time = float(mem_points[0][0])
            
            # Resample Memory Data
            for point in mem_points:
                t_rel = float(point[0]) - start_time
                val_mb = float(point[1]) / (1024 * 1024)
                # Filter out long tails > 30s (benchmark duration)
                if t_rel <= 30:
                    memory_data.append({'Time': t_rel, 'MB': val_mb, 'Implementation': impl})

            # Resample Allocation Rate Data
            if alloc_points:
                start_time_alloc = float(alloc_points[0][0])
                for point in alloc_points:
                    t_rel = float(point[0]) - start_time_alloc
                    val_mb_s = float(point[1]) / (1024 * 1024)
                    if t_rel <= 30:
                        allocation_data.append({'Time': t_rel, 'MB_Sec': val_mb_s, 'Implementation': impl})
                        
        except Exception:
            continue

    return pd.DataFrame(memory_data), pd.DataFrame(allocation_data)

def plot_time_series_curves(df_mem, df_alloc):
    """Plots confidence band curves using Seaborn."""
    if df_mem.empty:
        print("Warning: No time-series data found. (Did you run the new runner?)")
        return

    # 6. Memory Curve (The "Leak" Graph)
    plt.figure()
    sns.lineplot(data=df_mem, x='Time', y='MB', hue='Implementation', palette=PALETTE)
    plt.title('Memory Usage Over Time (Mean ± 95% CI)')
    plt.xlabel('Time (Seconds)')
    plt.ylabel('Committed Memory (MB)')
    plt.tight_layout()
    plt.savefig(f'{OUTPUT_DIR}/6_curve_memory.png')

    # 7. Allocation Rate (The "Pressure" Graph)
    if not df_alloc.empty:
        plt.figure()
        sns.lineplot(data=df_alloc, x='Time', y='MB_Sec', hue='Implementation', palette=PALETTE)
        plt.title('Allocation Rate Over Time (GC Pressure)')
        plt.xlabel('Time (Seconds)')
        plt.ylabel('Allocations (MB/sec)')
        plt.tight_layout()
        plt.savefig(f'{OUTPUT_DIR}/7_curve_allocation_rate.png')

    print(">>> Time-series curves generated.")

if __name__ == "__main__":
    # 1. Plot the basic CSV stats
    load_and_plot_summaries()
    
    # 2. Parse JSONs and plot the advanced curves
    mem_df, alloc_df = parse_time_series_from_jsons()
    plot_time_series_curves(mem_df, alloc_df)