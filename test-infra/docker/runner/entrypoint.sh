#!/usr/bin/env bash

set -e

mkdir -p /allure-results

echo "Cleaning old test results in container..."
find /allure-results -mindepth 1 -delete

echo "=================================================="
echo "STAGE 1: UNIT TESTS (Lab 1)"
echo "=================================================="
dotnet test MarketStat.Tests/MarketStat.Services.Tests/MarketStat.Services.Tests.csproj -c Release
dotnet test MarketStat.Tests/MarketStat.Repository.Tests/MarketStat.Repository.Tests.csproj -c Release

echo "=================================================="
echo "STAGE 2: INTEGRATION TESTS (Lab 2)"
echo "=================================================="
dotnet test MarketStat.Tests/MarketStat.Integration.Tests/MarketStat.Integration.Tests.csproj -c Release

echo "=================================================="
echo "STAGE 3: E2E TESTS (Lab 2)"
echo "=================================================="
dotnet test MarketStat.Tests/MarketStat.Tests.E2E/MarketStat.Tests.E2E.csproj -c Release

echo "=================================================="
echo "SUCCESS: ALL TEST STAGES PASSED"
echo "=================================================="

if [ -d "/allure-results" ]; then
    chown -R $(stat -c "%u:%g" /src) /allure-results
else
    echo "Warning: /allure-results does not exist. Skipping chown."
fi