# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["UnifiedLogin.UserNotification.csproj", "./"]
RUN dotnet restore "UnifiedLogin.UserNotification.csproj"

# Copy everything else and build
COPY . .
RUN dotnet build "UnifiedLogin.UserNotification.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "UnifiedLogin.UserNotification.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Create non-root user
RUN groupadd -r notificationservice && useradd -r -g notificationservice notificationservice

# Copy published files
COPY --from=publish /app/publish .

# Create logs directory and set permissions
RUN mkdir -p /app/logs && chown -R notificationservice:notificationservice /app

# Switch to non-root user
USER notificationservice

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=10s --retries=3 \
  CMD dotnet --version || exit 1

ENTRYPOINT ["dotnet", "UnifiedLogin.UserNotification.dll"]
