#!/usr/bin/env bash

set -e

mkdir -p /allure-results
echo "Cleaning old test results in container..."
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

echo "=================================================="
echo "STAGE 3: E2E TESTS"
echo "=================================================="
dotnet test MarketStat.Tests/MarketStat.Tests.E2E/MarketStat.Tests.E2E.csproj -c Release

echo "=================================================="
echo "TESTS FINISHED. COLLECTING RESULTS..."
echo "=================================================="

find . -type f -path "*/allure-results/*.json" -exec cp {} /allure-results/ \;

file_count=$(ls /allure-results | wc -l)
echo "Collected $file_count result files."

chmod -R 777 /allure-results

echo "SUCCESS: ALL STAGES PASSED & RESULTS COLLECTED"