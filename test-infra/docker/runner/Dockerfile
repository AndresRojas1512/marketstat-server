FROM mcr.microsoft.com/dotnet/sdk:8.0

WORKDIR /src

COPY ./../../ /src

RUN apt-get update && \
    DEBIAN_FRONTEND=noninteractive apt-get install -y tshark && \
    rm -rf /var/lib/apt/lists/*

RUN dotnet restore src/MarketStat.sln
RUN dotnet build src/MarketStat.sln -c Release --no-restore

RUN chmod +x /src/test-infra/docker/runner/entrypoint.sh

ENTRYPOINT ["/src/test-infra/docker/runner/entrypoint.sh"]