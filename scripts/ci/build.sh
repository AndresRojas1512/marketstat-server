#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
cd "$ROOT_DIR"

echo "[build] Cleaning publish directory..."
rm -rf artifacts/publish
mkdir -p artifacts/publish

echo "[build] Restoring solution..."
dotnet restore src/MarketStat.sln

echo "[build] Building solution..."
dotnet build src/MarketStat.sln -c Release --no-restore

echo "[build] Publishing MarketStat..."
dotnet publish src/MarketStat/MarketStat.csproj -c Release --no-build -o artifacts/publish

echo "[build] Publish completed successfully."
