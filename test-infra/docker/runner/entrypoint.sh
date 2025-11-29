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
echo "Starting TShark capture on port 5050..."
tshark -i lo -f "tcp port 5050" -w /allure-results/e2e_traffic.pcap &
TSHARK_PID=$!

sleep 2

dotnet test MarketStat.Tests/MarketStat.Tests.E2E/MarketStat.Tests.E2E.csproj -c Release --logger "trx;LogFileName=e2e_tests.trx"

echo "Stopping TShark..."
kill $TSHARK_PID || true

echo "=================================================="
echo "TESTS FINISHED. COLLECTING RESULTS..."
echo "=================================================="

find . -type f -path "*/allure-results/*.json" -exec cp {} /allure-results/ \;

if [ -f /allure-results/e2e_traffic.pcap ]; then
    echo "SUCCESS: Traffic capture generated (e2e_traffic.pcap)."
else
    echo "WARNING: No traffic capture file found."
fi

chmod -R 777 /allure-results

echo "SUCCESS: ALL STAGES PASSED & RESULTS COLLECTED"