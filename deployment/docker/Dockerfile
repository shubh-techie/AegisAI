FROM mcr.microsoft.com/dotnet/sdk:8.0-bookworm-slim AS build
WORKDIR /source
COPY global.json AegisAI.sln ./
COPY src/ src/
RUN dotnet publish src/AegisAI.Api/AegisAI.Api.csproj -c Release -o /publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0-bookworm-slim AS api
WORKDIR /app
COPY --from=build /publish ./
USER app
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "AegisAI.Api.dll"]

FROM mcr.microsoft.com/dotnet/sdk:8.0-bookworm-slim AS tools
RUN apt-get update && apt-get install -y --no-install-recommends git \
    && rm -rf /var/lib/apt/lists/*
COPY deployment/docker/tools-entrypoint.sh /tools-entrypoint.sh
ENV NUGET_PACKAGES=/tmp/nuget DOTNET_CLI_HOME=/tmp/dotnet DOTNET_NOLOGO=1 DOTNET_CLI_TELEMETRY_OPTOUT=1
ENTRYPOINT ["sh", "/tools-entrypoint.sh"]
