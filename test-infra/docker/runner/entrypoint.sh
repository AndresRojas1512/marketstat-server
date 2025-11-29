#!/usr/bin/env bash

# Keep set -e for setup, but we will handle test failures manually
set -e

mkdir -p /allure-results
echo "Cleaning old test results in container..."
rm -rf /allure-results/*

echo "=================================================="
echo "STAGE 1: UNIT TESTS"
echo "=================================================="
dotnet test MarketStat.Tests/MarketStat.Services.Tests/MarketStat.Services.Tests.csproj -c Release --logger "trx;LogFileName=unit_tests.trx" || TEST_EXIT_CODE=1
dotnet test MarketStat.Tests/MarketStat.Repository.Tests/MarketStat.Repository.Tests.csproj -c Release --logger "trx;LogFileName=repo_tests.trx" || TEST_EXIT_CODE=1

echo "=================================================="
echo "STAGE 2: INTEGRATION TESTS"
echo "=================================================="
dotnet test MarketStat.Tests/MarketStat.Integration.Tests/MarketStat.Integration.Tests.csproj -c Release --logger "trx;LogFileName=integration_tests.trx" || TEST_EXIT_CODE=1

echo "=================================================="
echo "STAGE 3: E2E TESTS & TRAFFIC CAPTURE"
echo "=================================================="

echo "Starting TShark capture on port 5050..."
# -l: Flush output after each packet (Line buffering)
# -i lo: Loopback
tshark -i lo -f "tcp port 5050" -l -w /allure-results/e2e_traffic.pcap &
TSHARK_PID=$!

# INCREASE WAIT: Give TShark 5 seconds to fully hook the interface
echo "Waiting for TShark to initialize..."
sleep 5

# Run E2E tests
dotnet test MarketStat.Tests/MarketStat.Tests.E2E/MarketStat.Tests.E2E.csproj -c Release --logger "trx;LogFileName=e2e_tests.trx" || TEST_EXIT_CODE=1

echo "Stopping TShark..."
# Send SIGINT (CTRL+C) instead of kill, to allow TShark to close the file gracefully
kill -SIGINT $TSHARK_PID || true

# Wait for TShark to flush and exit
wait $TSHARK_PID || true
sleep 2src/MarketStat.Tests/MarketStat.Tests.E2E/MarketStat.Tests.E2E.csproj

echo "=================================================="
echo "TESTS FINISHED. COLLECTING RESULTS..."
echo "=================================================="

find . -type f -path "*/allure-results/*.json" -exec cp {} /allure-results/ \;

if [ -f /allure-results/e2e_traffic.pcap ]; then
    echo "SUCCESS: Traffic capture generated."
    ls -l /allure-results/e2e_traffic.pcap
else
    echo "WARNING: No traffic capture file found."
fi

echo "Fixing permissions on /allure-results..."
chmod -R 777 /allure-results

echo "Exiting with code: ${TEST_EXIT_CODE:-0}"
exit ${TEST_EXIT_CODE:-0}