#!/usr/bin/env bash

# Keep set -e for setup, but we will handle test failures manually
set -e

mkdir -p /allure-results
echo "Cleaning old test results in container..."
rm -rf /allure-results/*

echo "=================================================="
echo "STAGE 1: UNIT TESTS"
echo "=================================================="
# We use '|| TEST_EXIT_CODE=$?' to capture failure but keep running
dotnet test MarketStat.Tests/MarketStat.Services.Tests/MarketStat.Services.Tests.csproj -c Release --logger "trx;LogFileName=unit_tests.trx"
dotnet test MarketStat.Tests/MarketStat.Repository.Tests/MarketStat.Repository.Tests.csproj -c Release --logger "trx;LogFileName=repo_tests.trx"

echo "=================================================="
echo "STAGE 2: INTEGRATION TESTS"
echo "=================================================="
dotnet test MarketStat.Tests/MarketStat.Integration.Tests/MarketStat.Integration.Tests.csproj -c Release --logger "trx;LogFileName=integration_tests.trx"

echo "=================================================="
echo "STAGE 3: E2E TESTS & TRAFFIC CAPTURE"
echo "=================================================="

echo "Starting TShark capture on port 5050..."
# Start in background
tshark -i lo -f "tcp port 5050" -w /allure-results/e2e_traffic.pcap &
TSHARK_PID=$!

sleep 3

# Disable set -e temporarily so a test failure doesn't crash the script before chmod
set +e 

dotnet test MarketStat.Tests/MarketStat.Tests.E2E/MarketStat.Tests.E2E.csproj -c Release --logger "trx;LogFileName=e2e_tests.trx"
TEST_EXIT_CODE=$?

echo "Stopping TShark..."
kill $TSHARK_PID || true

# Re-enable strict error checking
set -e

echo "=================================================="
echo "TESTS FINISHED. COLLECTING RESULTS..."
echo "=================================================="

find . -type f -path "*/allure-results/*.json" -exec cp {} /allure-results/ \;

if [ -f /allure-results/e2e_traffic.pcap ]; then
    echo "SUCCESS: Traffic capture generated."
else
    echo "WARNING: No traffic capture file found."
fi

# CRITICAL: Fix permissions so GitHub Actions can upload artifacts
echo "Fixing permissions..."
chmod -R 777 /allure-results

# Return the exit code of the tests. 
# If tests failed, this makes the CI pipeline fail (correctly), but AFTER artifacts are processed.
exit $TEST_EXIT_CODE