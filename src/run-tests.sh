#!/usr/bin/env bash

set -e

echo "Starting test run..."

echo "Cleaning old test results..."

rm -rf allure-results || true
rm -rf allure-report || true

echo "Running Service tests..."
dotnet test MarketStat.Tests/MarketStat.Services.Tests/MarketStat.Services.Tests.csproj

echo "Running Repository tests..."
dotnet test MarketStat.Tests/MarketStat.Repository.Tests/MarketStat.Repository.Tests.csproj

echo "Generating Allure report..."
allure generate allure-results -o allure-report --clean

echo "Test run complete."

echo "Opening Allure report..."
allure open allure-report