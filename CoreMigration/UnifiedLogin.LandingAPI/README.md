# UnifiedLogin.LandingAPI

A modern ASP.NET Core 8.0 Web API project for Unified Login, migrated from the legacy .NET Framework 4.8 WebAPI.

## Overview

This project is a complete rewrite of the LandingAPI using .NET Core 8.0 conventions:
- **Minimal Hosting Model**: Uses top-level statements in Program.cs
- **Modern Dependency Injection**: Built-in DI container configuration
- **Integrated Logging**: Serilog for structured logging
- **API Documentation**: Swagger/OpenAPI integration
- **Health Checks**: Built-in health monitoring endpoints

## Key Features

- **.NET Core 8.0**: Latest LTS version of .NET
- **RESTful API**: Following REST best practices
- **CORS Support**: Configurable cross-origin resource sharing
- **Structured Logging**: Serilog with file and console outputs
- **API Documentation**: Interactive Swagger UI for testing

## Project Structure

```
UnifiedLogin.LandingAPI/
├── Controllers/          # API controllers
│   └── TestController.cs # Sample test controller
├── Properties/           # Launch settings
├── appsettings.json      # Configuration settings
├── Program.cs            # Application entry point
└── UnifiedLogin.LandingAPI.csproj
```

## Getting Started

### Prerequisites

- .NET 8.0 SDK or later
- Visual Studio 2022 or Visual Studio Code

### Running the Application

1. **Restore packages**:
   ```bash
   dotnet restore
   ```

2. **Build the project**:
   ```bash
   dotnet build
   ```

3. **Run the application**:
   ```bash
   dotnet run
   ```

4. **Access the API**:
   - Swagger UI: `http://localhost:5000/swagger`
   - Test endpoint: `http://localhost:5000/api/test/hello`
   - Health check: `http://localhost:5000/health`

## API Endpoints

### Test Controller

- **GET** `/api/test/hello`
  - Description: Returns a simple greeting message
  - Response: JSON object with message, timestamp, and environment info
  - Status: 200 OK

## Configuration

Configuration is managed through `appsettings.json` and `appsettings.Development.json`:

- **Logging**: Configured for both console and file outputs
- **CORS**: Currently set to allow all origins (configure as needed for production)
- **Swagger**: Enabled in development environment

## Logging

The application uses Serilog for structured logging:
- Console output for development
- Rolling file logs in the `logs/` directory
- Configurable log levels per namespace

## Migration Notes

This project represents a modernized version of the original LandingAPI with the following improvements:

1. **Simplified Configuration**: No more Web.config, using appsettings.json
2. **Built-in DI**: No need for external IoC containers
3. **Middleware Pipeline**: Clean, explicit middleware configuration
4. **Async/Await**: Full async support throughout
5. **Performance**: Better performance with Kestrel web server
6. **Cross-platform**: Runs on Windows, Linux, and macOS

## Development

### Adding New Controllers

1. Create a new class in the `Controllers` folder
2. Inherit from `ControllerBase`
3. Add `[ApiController]` and `[Route]` attributes
4. Implement your endpoints

Example:
```csharp
[ApiController]
[Route("")]
public class MyController : ControllerBase
{
    [HttpGet]
    public ActionResult<string> Get()
    {
        return Ok("Hello World");
    }
}
```

## Next Steps

- Add authentication/authorization (JWT, OAuth2, etc.)
- Implement business logic services
- Add database connectivity (Entity Framework Core)
- Configure production-ready logging and monitoring
- Add unit and integration tests
- Configure CI/CD pipelines
