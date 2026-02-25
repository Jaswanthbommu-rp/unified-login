# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM artifacts.realpage.com/docker-virtual/dotnet/aspnet:10.0-noble AS base
ENV DOTNET_SYSTEM_DRAWING_ENABLE=1

WORKDIR /app
RUN apt-get update && \
    apt-get install -y --allow-unauthenticated libgdiplus libc6-dev && \
    apt-get clean && rm -rf /var/lib/apt/lists/*
EXPOSE 8080

# This stage is used to build the service project
FROM artifacts.realpage.com/docker-virtual/dotnet/sdk:10.0-noble AS publish
WORKDIR /src
RUN dotnet nuget add source https://artifacts.realpage.com/artifactory/api/nuget/nuget-virtual
COPY . .
WORKDIR "/src/UnifiedLogin.LandingAPIEnterprise"
##RUN dotnet build "./UnifiedLogin.LandingAPIEnterprise.csproj" -c Release -o /app/build

# This stage is used to publish the service project to be copied to the final stage
#FROM build AS publish
RUN dotnet publish "./UnifiedLogin.LandingAPIEnterprise.csproj" -c Release -r linux-x64 --no-self-contained -o /app/publish

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
#COPY certs/*.crt /usr/local/share/ca-certificates/
#RUN  update-ca-certificates
ENV ASPNETCORE_URLS http://+:8080
ENTRYPOINT ["dotnet", "UnifiedLogin.LandingAPIEnterprise.dll"]