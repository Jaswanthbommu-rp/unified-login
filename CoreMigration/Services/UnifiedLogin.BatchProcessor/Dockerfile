# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM artifacts.realpage.com/docker-virtual/dotnet/aspnet:10.0-noble AS base
RUN apt-get update
RUN apt-get -y install gss-ntlmssp
WORKDIR /app

# This stage is used to build the service project
FROM artifacts.realpage.com/docker-virtual/dotnet/sdk:10.0-noble AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
RUN dotnet nuget add source https://artifacts.realpage.com/artifactory/api/nuget/nuget-virtual
COPY . .
WORKDIR "/src/Services/UnifiedLogin.BatchProcessor"
RUN dotnet build "UnifiedLogin.BatchProcessor.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "UnifiedLogin.BatchProcessor.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
#COPY certs/*.crt /usr/local/share/ca-certificates/
#RUN update-ca-certificates
ENTRYPOINT ["dotnet", "UnifiedLogin.BatchProcessor.dll"]

## Build stage
#FROM artifacts.realpage.com/docker-virtual/dotnet/sdk:10.0 AS base
#WORKDIR /src
#
## Copy central package management file
#COPY ["../../Directory.Packages.props", "."]
#
## Copy csproj and restore dependencies
#COPY ["UnifiedLogin.BatchProcessor.csproj", "./"]
#RUN dotnet restore "UnifiedLogin.BatchProcessor.csproj"
#
## Copy everything else and build
#COPY . .
#RUN dotnet build "UnifiedLogin.BatchProcessor.csproj" -c Release -o /app/build
#
## Publish stage
#FROM base AS publish
#RUN dotnet publish "UnifiedLogin.BatchProcessor.csproj" -c Release -o /app/publish /p:UseAppHost=false
#
## Runtime stage
#FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
#WORKDIR /app
#
## Create non-root user
#RUN groupadd -r batchprocessor && useradd -r -g batchprocessor batchprocessor
#
## Copy published files
#COPY --from=publish /app/publish .
#
## Create logs directory and set permissions
#RUN mkdir -p /app/logs && chown -R batchprocessor:batchprocessor /app
#
## Switch to non-root user
#USER batchprocessor
#
## Health check
#HEALTHCHECK --interval=30s --timeout=10s --start-period=10s --retries=3 \
  #CMD dotnet --version || exit 1
#
#ENTRYPOINT ["dotnet", "UnifiedLogin.BatchProcessor.dll"]
#