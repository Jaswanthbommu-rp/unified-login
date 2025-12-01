var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.UnifiedLogin_LandingAPI>("unifiedlogin-landingapi");

builder.AddProject<Projects.UnifiedLogin_LandingAPIEnterprise>("unifiedlogin-landingapi-enterprise");


builder.Build().Run();
