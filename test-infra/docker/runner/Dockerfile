FROM mcr.microsoft.com/dotnet/sdk:8.0

WORKDIR /src

# 1. Copy the 'src' folder contents into our WORKDIR '/src'
# This places MarketStat.sln at /src/MarketStat.sln
COPY src/. .

# 2. Copy the 'test-infra' folder contents into '/src/test-infra'
# This is the single, correct line for test-infra
COPY test-infra/. ./test-infra

# 3. Install dependencies
RUN apt-get update && \
    DEBIAN_FRONTEND=noninteractive apt-get install -y tshark && \
    rm -rf /var/lib/apt/lists/*

# 4. Restore the .sln file (now at /src/MarketStat.sln)
RUN dotnet restore MarketStat.sln

# 5. Build the .sln file (now at /src/MarketStat.sln)
RUN dotnet build MarketStat.sln -c Release --no-restore

# 6. Chmod the entrypoint (now at /src/test-infra/docker/runner/entrypoint.sh)
RUN chmod +x test-infra/docker/runner/entrypoint.sh

# 7. Run the entrypoint
ENTRYPOINT ["test-infra/docker/runner/entrypoint.sh"]