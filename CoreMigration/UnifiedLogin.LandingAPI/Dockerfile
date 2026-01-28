# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM artifacts.realpage.com/docker-virtual/dotnet/aspnet:10.0-noble AS base
ENV DOTNET_SYSTEM_DRAWING_ENABLE=1

WORKDIR /app
RUN apt-get update 
RUN apt-get install -y --allow-unauthenticated libgdiplus libc6-dev
RUN apt-get clean && rm -rf /var/lib/apt/lists/*
EXPOSE 8080

# This stage is used to build the service project
FROM artifacts.realpage.com/docker-virtual/dotnet/sdk:10.0-noble AS build
WORKDIR /src
RUN dotnet nuget add source https://artifacts.realpage.com/artifactory/api/nuget/nuget-virtual
COPY . .
WORKDIR "/src/UnifiedLogin.LandingAPI"
RUN dotnet build "./UnifiedLogin.LandingAPI.csproj" -c Release -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
RUN dotnet publish "./UnifiedLogin.LandingAPI.csproj" -c Release -o /app/publish

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY certs/*.crt /usr/local/share/ca-certificates/
RUN  update-ca-certificates
ENV ASPNETCORE_URLS http://+:8080
ENTRYPOINT ["dotnet", "UnifiedLogin.LandingAPI.dll"]


## Build stage
#FROM artifacts.realpage.com/docker-virtual/dotnet/sdk:10.0-noble AS base
#WORKDIR /src
#
## Copy central package management file
#COPY ["Directory.Packages.props", "."]
#
### Copy project files
##COPY ["UnifiedLogin.LandingAPI/UnifiedLogin.LandingAPI.csproj", "UnifiedLogin.LandingAPI/"]
##COPY ["UnifiedLogin.BusinessLogic/UnifiedLogin.BusinessLogic.csproj", "UnifiedLogin.BusinessLogic/"]
##COPY ["UnifiedLogin.SharedObjects/UnifiedLogin.SharedObjects.csproj", "UnifiedLogin.SharedObjects/"]
##COPY ["UnifiedLogin.DataAccess/UnifiedLogin.DataAccess.csproj", "UnifiedLogin.DataAccess/"]
##COPY ["UnifiedLogin.Core/UnifiedLogin.Core.csproj", "UnifiedLogin.Core/"]
##COPY ["UnifiedLogin.ServiceDefaults/UnifiedLogin.ServiceDefaults.csproj", "UnifiedLogin.ServiceDefaults/"]
##
## Restore dependencies
#RUN dotnet restore "UnifiedLogin.LandingAPI/UnifiedLogin.LandingAPI.csproj"
#
## Copy all source files
#COPY . .
#
## Build the application
#WORKDIR "/src/UnifiedLogin.LandingAPI"
#RUN dotnet build "UnifiedLogin.LandingAPI.csproj" -c Release -o /app/build
#
## Publish stage
#FROM build AS publish
#RUN dotnet publish "UnifiedLogin.LandingAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false
#
## Runtime stage
#FROM artifacts.realpage.com/docker-virtual/dotnet/aspnet:10.0-noble AS final
#WORKDIR /app
#
## Enable System.Drawing for .NET 10 (if needed)
#ENV DOTNET_SYSTEM_DRAWING_ENABLE=1
#
## Install curl for health checks
#RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*
#
## Create non-root user for security
#RUN groupadd -r appuser && useradd -r -g appuser appuser
#
#EXPOSE 8080
#EXPOSE 8081
#
#COPY --from=publish /app/publish .
#
## Set ownership
#RUN chown -R appuser:appuser /app
#
## Switch to non-root user
#USER appuser
#
#ENTRYPOINT ["dotnet", "UnifiedLogin.LandingAPI.dll"]
#