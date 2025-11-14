var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.UnifiedLogin_LandingAPI>("unifiedlogin-landingapi");

builder.Build().Run();
