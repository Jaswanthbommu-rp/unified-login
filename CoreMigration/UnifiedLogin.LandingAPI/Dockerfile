# Build stage
FROM artifacts.realpage.com/docker-virtual/dotnet/sdk:10.0-noble AS build
WORKDIR /src

# Copy central package management file
COPY ["Directory.Packages.props", "."]

# Copy project files
COPY ["UnifiedLogin.LandingAPI/UnifiedLogin.LandingAPI.csproj", "UnifiedLogin.LandingAPI/"]
COPY ["UnifiedLogin.BusinessLogic/UnifiedLogin.BusinessLogic.csproj", "UnifiedLogin.BusinessLogic/"]
COPY ["UnifiedLogin.SharedObjects/UnifiedLogin.SharedObjects.csproj", "UnifiedLogin.SharedObjects/"]
COPY ["UnifiedLogin.DataAccess/UnifiedLogin.DataAccess.csproj", "UnifiedLogin.DataAccess/"]
COPY ["UnifiedLogin.Core/UnifiedLogin.Core.csproj", "UnifiedLogin.Core/"]
COPY ["UnifiedLogin.ServiceDefaults/UnifiedLogin.ServiceDefaults.csproj", "UnifiedLogin.ServiceDefaults/"]

# Restore dependencies
RUN dotnet restore "UnifiedLogin.LandingAPI/UnifiedLogin.LandingAPI.csproj"

# Copy all source files
COPY . .

# Build the application
WORKDIR "/src/UnifiedLogin.LandingAPI"
RUN dotnet build "UnifiedLogin.LandingAPI.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "UnifiedLogin.LandingAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM artifacts.realpage.com/docker-virtual/dotnet/aspnet:10.0-noble AS final
WORKDIR /app

# Enable System.Drawing for .NET 10 (if needed)
ENV DOTNET_SYSTEM_DRAWING_ENABLE=1

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Create non-root user for security
RUN groupadd -r appuser && useradd -r -g appuser appuser

EXPOSE 8080
EXPOSE 8081

COPY --from=publish /app/publish .

# Set ownership
RUN chown -R appuser:appuser /app

# Switch to non-root user
USER appuser

ENTRYPOINT ["dotnet", "UnifiedLogin.LandingAPI.dll"]
