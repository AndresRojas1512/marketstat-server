#!/usr/bin/env bash

set -e

echo "Cleaning ALL old test results..."
rm -rf allure-results || true
rm -rf allure-report || true

find . -type d -name "allure-results" -exec rm -rf {} +

mkdir -p allure-results

echo "Starting test run..."

echo "Running Service tests..."
dotnet test MarketStat.Tests/MarketStat.Services.Tests/MarketStat.Services.Tests.csproj

echo "Running Repository tests..."
dotnet test MarketStat.Tests/MarketStat.Repository.Tests/MarketStat.Repository.Tests.csproj

echo "Collecting results from build folders..."
find . -type f -path "*/bin/*/allure-results/*.json" -exec cp {} allure-results/ \;

echo "Generating Allure report..."
allure generate allure-results -o allure-report --clean

echo "Test run complete."

echo "Opening Allure report..."
allure open allure-report