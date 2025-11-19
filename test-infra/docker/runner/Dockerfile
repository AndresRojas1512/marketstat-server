FROM mcr.microsoft.com/dotnet/sdk:8.0

WORKDIR /src

RUN apt-get update && \
    DEBIAN_FRONTEND=noninteractive apt-get install -y tshark docker.io && \
    rm -rf /var/lib/apt/lists/*

COPY src/. .

COPY test-infra/. ./test-infra

RUN dotnet restore MarketStat.sln

RUN dotnet build MarketStat.sln -c Release --no-restore

RUN chmod +x test-infra/docker/runner/entrypoint.sh

ENTRYPOINT ["test-infra/docker/runner/entrypoint.sh"]