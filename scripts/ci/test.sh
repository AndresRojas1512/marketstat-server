#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
cd "$ROOT_DIR"

echo "[test] Running MarketStat.Services.Tests..."
dotnet test src/MarketStat.Tests/MarketStat.Services.Tests/MarketStat.Services.Tests.csproj -c Release

echo "[test] Running MarketStat.Repository.Tests..."
dotnet test src/MarketStat.Tests/MarketStat.Repository.Tests/MarketStat.Repository.Tests.csproj -c Release

echo "[test] Validating nginx configuration on VM1..."
./scripts/ci/test_nginx_remote.sh

echo "[test] Test stage completed successfully."