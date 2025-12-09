import pandas as pd
import matplotlib.pyplot as plt
import seaborn as sns
import os

# Configuration
CSV_FILE = 'benchmark_final_report.csv'
OUTPUT_DIR = 'charts'
os.makedirs(OUTPUT_DIR, exist_ok=True)

# --- IMPROVEMENT: Define a consistent color palette for all charts ---
# This ensures "Dapper" is always the same color, making the report easier to read.
PALETTE = {
    "BASELINE": "#1f77b4", # Blue
    "EF_SQL":   "#ff7f0e", # Orange
    "DAPPER":   "#2ca02c"  # Green
}

sns.set_theme(style="whitegrid")
plt.rcParams.update({'figure.figsize': (12, 7), 'font.size': 12})

def generate_charts():
    # 1. Load Data
    try:
        df = pd.read_csv(CSV_FILE)
    except FileNotFoundError:
        print(f"Error: {CSV_FILE} not found. Run the benchmark first!")
        return

    # --- FIX: Match the string from runner_sequential.py ("THRESHOLD_FAIL") ---
    # We include these because they are valid data points, just slow ones.
    df_valid = df[df['Status'].isin(['SUCCESS', 'THRESHOLD_FAIL'])]

    print(f"Total rows: {len(df)}")
    print(f"Valid rows for plotting: {len(df_valid)}")
    
    if df_valid.empty:
        print("WARNING: No valid data found to plot. Check your CSV or filters.")
        return

    # ==========================================
    #      GROUP 1: LATENCY & THROUGHPUT
    # ==========================================

    # --- CHART 1: Throughput Comparison (Bar Chart) ---
    plt.figure()
    sns.barplot(x='Implementation', y='Req/s', data=df, errorbar='sd', palette=PALETTE)
    plt.title('Average Throughput (Higher is Better)')
    plt.ylabel('Requests / Second')
    plt.tight_layout()
    plt.savefig(f'{OUTPUT_DIR}/1_throughput_comparison.png')
    print(f"Saved {OUTPUT_DIR}/1_throughput_comparison.png")

    # --- CHART 2: P95 Latency Distribution (Box Plot) ---
    plt.figure()
    sns.boxplot(x='Implementation', y='P95', data=df_valid, palette=PALETTE)
    plt.title('P95 Latency Distribution (Lower is Better)')
    plt.ylabel('Latency (ms)')
    plt.tight_layout()
    plt.savefig(f'{OUTPUT_DIR}/2_latency_p95_distribution.png')
    print(f"Saved {OUTPUT_DIR}/2_latency_p95_distribution.png")

    # --- CHART 3: Percentile Profile (Line Plot) ---
    percentile_cols = ['P50', 'P75', 'P90', 'P95', 'P99']
    df_melted = df_valid.melt(id_vars=['Implementation'], value_vars=percentile_cols, 
                                var_name='Percentile', value_name='Latency')
    plt.figure()
    sns.lineplot(x='Percentile', y='Latency', hue='Implementation', data=df_melted, marker='o', palette=PALETTE)
    plt.title('Latency Scaling by Percentile (Tail Latency)')
    plt.ylabel('Latency (ms)')
    plt.grid(True)
    plt.tight_layout()
    plt.savefig(f'{OUTPUT_DIR}/3_latency_percentiles.png')
    print(f"Saved {OUTPUT_DIR}/3_latency_percentiles.png")

    # ==========================================
    #      GROUP 2: STABILITY (Req 4a)
    # ==========================================

    # --- CHART 4: Stability Over Time (Line Plot) ---
    # This detects "Cold Starts" (high at start) or "Memory Leaks" (rising over time)
    plt.figure()
    sns.lineplot(x='Iteration', y='P95', hue='Implementation', data=df_valid, palette=PALETTE, alpha=0.7)
    plt.title('Performance Stability over 100 Iterations')
    plt.ylabel('P95 Latency (ms)')
    plt.xlabel('Iteration Sequence')
    plt.tight_layout()
    plt.savefig(f'{OUTPUT_DIR}/4_stability_over_time.png')
    print(f"Saved {OUTPUT_DIR}/4_stability_over_time.png")

    # ==========================================
    #      GROUP 3: RESOURCES (Req 6)
    # ==========================================

    # --- CHART 5: Memory Usage (Box Plot) ---
    plt.figure()
    sns.boxplot(x='Implementation', y='Max_Memory_MB', data=df_valid, palette=PALETTE)
    plt.title('Memory Footprint (Lower is Better)')
    plt.ylabel('Peak Committed Memory (MB)')
    plt.tight_layout()
    plt.savefig(f'{OUTPUT_DIR}/5_resource_memory.png')
    print(f"Saved {OUTPUT_DIR}/5_resource_memory.png")

    # --- CHART 6: Garbage Collection Overhead (Bar Chart) ---
    plt.figure()
    sns.barplot(x='Implementation', y='GC_Time_Sec', data=df_valid, errorbar='sd', palette=PALETTE)
    plt.title('CPU Time Spent on Garbage Collection (Lower is Better)')
    plt.ylabel('Total GC Time (Seconds)')
    plt.tight_layout()
    plt.savefig(f'{OUTPUT_DIR}/6_resource_gc_cpu.png')
    print(f"Saved {OUTPUT_DIR}/6_resource_gc_cpu.png")

    # --- CHART 7: Error Rate ---
    plt.figure()
    sns.barplot(x='Implementation', y='Error_Rate', data=df, errorbar=None, palette=PALETTE)
    plt.title('Reliability: Average Error Rate')
    plt.ylabel('Error Rate (%)')
    plt.ylim(0, 100) # Fix Y axis to 0-100%
    plt.tight_layout()
    plt.savefig(f'{OUTPUT_DIR}/7_error_rate.png')
    print(f"Saved {OUTPUT_DIR}/7_error_rate.png")

if __name__ == "__main__":
    generate_charts()