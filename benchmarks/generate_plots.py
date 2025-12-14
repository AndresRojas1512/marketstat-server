import pandas as pd
import matplotlib.pyplot as plt
import seaborn as sns
import os

# Configuration
CSV_FILE = 'benchmark_final_report.csv'
OUTPUT_DIR = 'charts'
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
        print(f"Error: {CSV_FILE} not found. Run the benchmark first.")
        return

    # Filter out crashes
    df_valid = df[df['Status'].isin(['SUCCESS', 'THRESHOLD_FAIL'])]
    print(f"Loaded {len(df_valid)} valid runs for analysis.")
    
    if df_valid.empty:
        print("No valid data to plot.")
        return

    # ---------------------------------------------------------
    # CHART 1: LATENCY BOX PLOT (Requirement 4b - Distribution)
    # ---------------------------------------------------------
    # We use P95 as the standard "User Experience" metric
    plt.figure()
    sns.boxplot(x='Implementation', y='P95_Latency_ms', data=df_valid, palette=PALETTE)
    plt.title('Latency Distribution (P95) - Lower & Tighter is Better')
    plt.ylabel('Response Time (ms)')
    plt.xlabel('Repository Implementation')
    plt.tight_layout()
    plt.savefig(f'{OUTPUT_DIR}/1_latency_distribution_box.png')
    print("Generated: 1_latency_distribution_box.png")

    # ---------------------------------------------------------
    # CHART 2: MEMORY ENDURANCE (Requirement 2 - Degradation)
    # ---------------------------------------------------------
    plt.figure()
    # This chart proves if the app "leaks" over time (Requirement 2)
    sns.lineplot(x='Iteration', y='Max_Memory_MB', hue='Implementation', data=df_valid, palette=PALETTE, linewidth=2)
    plt.title('Memory Endurance Test (100 Sequential Runs)')
    plt.ylabel('Peak RAM Usage (MB)')
    plt.xlabel('Run Number (1-100)')
    plt.legend(title='Implementation', loc='upper left')
    plt.tight_layout()
    plt.savefig(f'{OUTPUT_DIR}/2_memory_endurance_trend.png')
    print("Generated: 2_memory_endurance_trend.png")

    # ---------------------------------------------------------
    # CHART 3: LATENCY HISTOGRAM (Requirement 4c - Histogram)
    # ---------------------------------------------------------
    plt.figure()
    # A histogram of the P95 latencies shows "Stability Probability"
    sns.histplot(data=df_valid, x="P95_Latency_ms", hue="Implementation", 
                 palette=PALETTE, element="step", bins=20, kde=True)
    plt.title('Stability Histogram: Frequency of P95 Latencies')
    plt.xlabel('P95 Latency (ms)')
    plt.ylabel('Count of Runs')
    plt.tight_layout()
    plt.savefig(f'{OUTPUT_DIR}/3_latency_histogram.png')
    print("Generated: 3_latency_histogram.png")

    # ---------------------------------------------------------
    # CHART 4: THE "TAX" BILL (GC Cost)
    # ---------------------------------------------------------
    plt.figure()
    sns.barplot(x='Implementation', y='GC_Seconds', data=df_valid, errorbar='sd', palette=PALETTE)
    plt.title('The Cost of Abstraction: Time Lost to Garbage Collection')
    plt.ylabel('Avg Seconds Paused per Run')
    plt.xlabel('')
    plt.tight_layout()
    plt.savefig(f'{OUTPUT_DIR}/4_gc_cost_bar.png')
    print("Generated: 4_gc_cost_bar.png")

    # ---------------------------------------------------------
    # CHART 5: CPU EFFICIENCY
    # ---------------------------------------------------------
    plt.figure()
    sns.barplot(x='Implementation', y='Max_CPU_Cores', data=df_valid, errorbar='sd', palette=PALETTE)
    plt.title('Computational Efficiency (Peak CPU Cores)')
    plt.ylabel('Cores Used (Max 2.0)')
    plt.xlabel('')
    plt.tight_layout()
    plt.savefig(f'{OUTPUT_DIR}/5_cpu_efficiency.png')
    print("Generated: 5_cpu_efficiency.png")

if __name__ == "__main__":
    generate_charts()