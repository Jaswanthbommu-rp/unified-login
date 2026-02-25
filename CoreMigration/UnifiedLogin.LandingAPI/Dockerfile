# Stage 1: runtime base — installs only what's needed at runtime
FROM artifacts.realpage.com/docker-virtual/dotnet/aspnet:10.0-noble AS base
ENV DOTNET_SYSTEM_DRAWING_ENABLE=1
WORKDIR /app
RUN apt-get update && \
    apt-get install -y --no-install-recommends libgdiplus libc6-dev && \
    apt-get clean && rm -rf /var/lib/apt/lists/*
EXPOSE 8080

# Stage 2: restore — cached unless .csproj or Directory.Packages.props changes
FROM artifacts.realpage.com/docker-virtual/dotnet/sdk:10.0-noble AS restore
WORKDIR /src
RUN dotnet nuget add source https://artifacts.realpage.com/artifactory/api/nuget/nuget-virtual

# Copy Central Package Management manifest and project files only
COPY Directory.Packages.props .
COPY UnifiedLogin.LandingAPI/UnifiedLogin.LandingAPI.csproj UnifiedLogin.LandingAPI/
COPY UnifiedLogin.BusinessLogic/UnifiedLogin.BusinessLogic.csproj UnifiedLogin.BusinessLogic/
COPY UnifiedLogin.DataAccess/UnifiedLogin.DataAccess.csproj UnifiedLogin.DataAccess/
COPY UnifiedLogin.ServiceDefaults/UnifiedLogin.ServiceDefaults.csproj UnifiedLogin.ServiceDefaults/
COPY UnifiedLogin.SharedObjects/UnifiedLogin.SharedObjects.csproj UnifiedLogin.SharedObjects/
COPY UnifiedLogin.Core/UnifiedLogin.Core.csproj UnifiedLogin.Core/

RUN dotnet restore "UnifiedLogin.LandingAPI/UnifiedLogin.LandingAPI.csproj" -r linux-x64

# Stage 3: publish — source changes only rebuild from here
FROM restore AS publish
COPY . .
RUN dotnet publish "UnifiedLogin.LandingAPI/UnifiedLogin.LandingAPI.csproj" \
    -c Release \
    -r linux-x64 \
    --no-self-contained \
    --no-restore \
    -o /app/publish

# Stage 4: final — lean runtime image with only published output
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "UnifiedLogin.LandingAPI.dll"]