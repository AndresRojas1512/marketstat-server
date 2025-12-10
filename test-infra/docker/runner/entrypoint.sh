#!/usr/bin/env bash

set -e

# 1. Prepare the shared volume
mkdir -p /allure-results
echo "Cleaning old test results in container..."
# Clean the mounted volume to ensure we don't upload old data
rm -rf /allure-results/*

echo "=================================================="
echo "STAGE 1: UNIT TESTS"
echo "=================================================="
dotnet test MarketStat.Tests/MarketStat.Services.Tests/MarketStat.Services.Tests.csproj -c Release
dotnet test MarketStat.Tests/MarketStat.Repository.Tests/MarketStat.Repository.Tests.csproj -c Release

echo "=================================================="
echo "STAGE 2: INTEGRATION TESTS"
echo "=================================================="
dotnet test MarketStat.Tests/MarketStat.Integration.Tests/MarketStat.Integration.Tests.csproj -c Release

#echo "=================================================="
#echo "STAGE 3: E2E TESTS"
#echo "=================================================="
#dotnet test MarketStat.Tests/MarketStat.Tests.E2E/MarketStat.Tests.E2E.csproj -c Release

echo "=================================================="
echo "TESTS FINISHED. COLLECTING RESULTS..."
echo "=================================================="

# 2. COLLECTION STEP (CRITICAL FIX)
# Find all JSON results hidden in the build folders and copy them to the mounted volume.
# This mimics your working local script.
find . -type f -path "*/allure-results/*.json" -exec cp {} /allure-results/ \;

# Verify we found files
file_count=$(ls /allure-results | wc -l)
echo "Collected $file_count result files."

# 3. Fix permissions so GitHub Actions can upload the files
chmod -R 777 /allure-results

echo "SUCCESS: ALL STAGES PASSED & RESULTS COLLECTED"