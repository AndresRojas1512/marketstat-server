import pandas as pd
import matplotlib.pyplot as plt
import seaborn as sns
import os

# --- CONFIGURATION CHANGES ---
CSV_FILE = 'serialization_benchmark.csv'      # Targets the new CSV
OUTPUT_DIR = 'charts_serialization'           # Targets a new folder
os.makedirs(OUTPUT_DIR, exist_ok=True)

# Consistent Color Palette matching Grafana
PALETTE = {
    "BASELINE": "#d62728", # Red (Danger/Heavy)
    "EF_SQL":   "#1f77b4", # Blue (Standard)
    "DAPPER":   "#2ca02c"  # Green (Fast)
}

# Set global style
sns.set_theme(style="whitegrid")
plt.rcParams.update({
    'figure.figsize': (12, 7), 
    'font.size': 12,
    'axes.titlesize': 14,
    'axes.labelsize': 12
})

def generate_charts():
    try:
        df = pd.read_csv(CSV_FILE)
    except FileNotFoundError:
        print(f"Error: {CSV_FILE} not found. Run 'runner_serialization.py' first.")
        return

    # Filter out crashes
    df_valid = df[df['Status'].isin(['SUCCESS', 'THRESHOLD_FAIL'])]
    print(f"Loaded {len(df_valid)} valid runs for analysis.")
    
    if df_valid.empty:
        print("No valid data to plot.")
        return

    # ---------------------------------------------------------
    # CHART 1: LATENCY BOX PLOT
    # ---------------------------------------------------------
    plt.figure()
    sns.boxplot(x='Implementation', y='P95_Latency_ms', data=df_valid, palette=PALETTE)
    plt.title('Serialization Latency (P95) - Mapping 1000 Objects')
    plt.ylabel('Response Time (ms)')
    plt.xlabel('Repository Implementation')
    plt.tight_layout()
    plt.savefig(f'{OUTPUT_DIR}/1_serial_latency_box.png')
    print(f"Generated: {OUTPUT_DIR}/1_serial_latency_box.png")

    # ---------------------------------------------------------
    # CHART 2: MEMORY USAGE (Allocations)
    # ---------------------------------------------------------
    plt.figure()
    # Using barplot here is often cleaner for short benchmarks (5 runs) than lineplot
    sns.barplot(x='Implementation', y='Max_Memory_MB', data=df_valid, palette=PALETTE, errorbar='sd')
    plt.title('Memory Cost: Peak RAM used to serialize 1000 items')
    plt.ylabel('Peak RAM Usage (MB)')
    plt.xlabel('')
    plt.tight_layout()
    plt.savefig(f'{OUTPUT_DIR}/2_serial_memory_bar.png')
    print(f"Generated: {OUTPUT_DIR}/2_serial_memory_bar.png")

    # ---------------------------------------------------------
    # CHART 3: STABILITY HISTOGRAM
    # ---------------------------------------------------------
    plt.figure()
    sns.histplot(data=df_valid, x="P95_Latency_ms", hue="Implementation", 
                 palette=PALETTE, element="step", bins=10, kde=True)
    plt.title('Stability Histogram: Consistency of Serialization Speed')
    plt.xlabel('P95 Latency (ms)')
    plt.ylabel('Count of Runs')
    plt.tight_layout()
    plt.savefig(f'{OUTPUT_DIR}/3_serial_latency_histogram.png')
    print(f"Generated: {OUTPUT_DIR}/3_serial_latency_histogram.png")

    # ---------------------------------------------------------
    # CHART 4: THE "TAX" BILL (GC Cost)
    # ---------------------------------------------------------
    plt.figure()
    sns.barplot(x='Implementation', y='GC_Seconds', data=df_valid, errorbar='sd', palette=PALETTE)
    plt.title('The Cost of Mapping: Time Lost to Garbage Collection')
    plt.ylabel('Total Seconds Paused (GC)')
    plt.xlabel('')
    plt.tight_layout()
    plt.savefig(f'{OUTPUT_DIR}/4_serial_gc_cost.png')
    print(f"Generated: {OUTPUT_DIR}/4_serial_gc_cost.png")

    # ---------------------------------------------------------
    # CHART 5: CPU EFFICIENCY
    # ---------------------------------------------------------
    # Now valid because runner_serialization.py captures CPU!
    plt.figure()
    sns.barplot(x='Implementation', y='Max_CPU_Cores', data=df_valid, errorbar='sd', palette=PALETTE)
    plt.title('Serialization CPU Efficiency (Peak Cores)')
    plt.ylabel('Cores Used (Max 2.0)')
    plt.xlabel('')
    plt.tight_layout()
    plt.savefig(f'{OUTPUT_DIR}/5_serial_cpu_efficiency.png')
    print(f"Generated: {OUTPUT_DIR}/5_serial_cpu_efficiency.png")

if __name__ == "__main__":
    generate_charts()