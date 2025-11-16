#!/usr/bin/env bash

set -e

echo "=== Running Unit Tests ==="
dotnet test /src/src/MarketStat.Tests/MarketStat.Services.Tests/MarketStat.Services.Tests.csproj -c Release
dotnet test /src/src/MarketStat.Tests/MarketStat.Repository.Tests/MarketStat.Repository.Tests.csproj -c Release

echo "=== Running Integration Tests ==="
dotnet test /src/src/MarketStat.Tests/MarketStat.Integration.Tests/MarketStat.Integration.Tests.csproj -c Release

echo "=== Running E2E Tests ==="
dotnet test /src/src/MarketStat.Tests/MarketStat.Tests.E2E/MarketStat.Tests.E2E.csproj -c Release


echo "=== All Tests Passed! ==="
