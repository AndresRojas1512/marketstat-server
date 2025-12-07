import pandas as pd
import matplotlib.pyplot as plt
import seaborn as sns
import os

# Configuration
RESULTS_DIR = "./results"
CSV_FILE = f"{RESULTS_DIR}/final_report.csv"
OUTPUT_IMG_DIR = f"{RESULTS_DIR}/plots"

# Ensure output directory exists
if not os.path.exists(OUTPUT_IMG_DIR):
    os.makedirs(OUTPUT_IMG_DIR)

def generate_plots():
    # 1. Load Data
    try:
        if not os.path.exists(CSV_FILE):
            print(f"Error: {CSV_FILE} not found. Run the benchmark first.")
            return
        
        df = pd.read_csv(CSV_FILE)
        print(f"Loaded {len(df)} rows from {CSV_FILE}")
        
        if len(df) == 0:
            print("Dataset is empty. Skipping plots.")
            return

    except Exception as e:
        print(f"Error reading CSV: {e}")
        return

    # Set global style for professional academic look
    sns.set_theme(style="whitegrid", context="paper", font_scale=1.2)
    plt.rcParams.update({'figure.max_open_warning': 0})

    # --- PLOT 1: Latency Stability (Box Plot) ---
    print("Generating: 01_latency_stability_boxplot.png...")
    plt.figure(figsize=(10, 6))
    # Removed 'legend=False' to fix compatibility
    sns.boxplot(
        x="Implementation", 
        y="P95_Latency_ms", 
        data=df, 
        palette="viridis",
        hue="Implementation"
    )
    plt.title("Latency Stability: P95 Distribution (Lower/Tighter is Better)", fontweight='bold')
    plt.ylabel("P95 Latency (ms)")
    plt.xlabel("Repository Implementation")
    plt.tight_layout()
    plt.savefig(f"{OUTPUT_IMG_DIR}/01_latency_stability_boxplot.png", dpi=300)
    plt.close()

    # --- PLOT 2: Latency Histogram (KDE) ---
    print("Generating: 02_latency_histogram.png...")
    plt.figure(figsize=(10, 6))
    sns.histplot(
        data=df, 
        x="P95_Latency_ms", 
        hue="Implementation", 
        kde=True, 
        element="step", 
        palette="viridis"
    )
    plt.title("Latency Frequency Distribution", fontweight='bold')
    plt.xlabel("P95 Latency (ms)")
    plt.ylabel("Frequency (Count of Runs)")
    plt.tight_layout()
    plt.savefig(f"{OUTPUT_IMG_DIR}/02_latency_histogram.png", dpi=300)
    plt.close()

    # --- PLOT 3: Resource Efficiency (Bar Chart - RAM) ---
    print("Generating: 03_memory_efficiency.png...")
    plt.figure(figsize=(8, 6))
    # Removed 'legend=False'
    sns.barplot(
        x="Implementation", 
        y="Max_Memory_MB", 
        data=df, 
        errorbar="sd", 
        palette="magma",
        hue="Implementation"
    )
    plt.title("Resource Cost: Peak Memory Usage (Lower is Better)", fontweight='bold')
    plt.ylabel("Peak Memory (MB)")
    plt.xlabel("Repository Implementation")
    plt.tight_layout()
    plt.savefig(f"{OUTPUT_IMG_DIR}/03_memory_efficiency.png", dpi=300)
    plt.close()

    # --- PLOT 4: CPU Overhead (Bar Chart - GC) ---
    print("Generating: 04_gc_impact.png...")
    plt.figure(figsize=(8, 6))
    # Removed 'legend=False'
    sns.barplot(
        x="Implementation", 
        y="GC_Count", 
        data=df, 
        errorbar="sd", 
        palette="rocket",
        hue="Implementation"
    )
    plt.title("CPU Overhead: Gen 2 Garbage Collections (Lower is Better)", fontweight='bold')
    plt.ylabel("Count of Full GCs per Run")
    plt.xlabel("Repository Implementation")
    plt.tight_layout()
    plt.savefig(f"{OUTPUT_IMG_DIR}/04_gc_impact.png", dpi=300)
    plt.close()

    print(f"\nSuccess! 4 plots saved to: {os.path.abspath(OUTPUT_IMG_DIR)}")

if __name__ == "__main__":
    generate_plots()