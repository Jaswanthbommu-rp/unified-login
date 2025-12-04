var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.UnifiedLogin_LandingAPI>("unifiedlogin-landingapi");

builder.AddProject<Projects.UnifiedLogin_LandingAPIEnterprise>("unifiedlogin-landingapi-enterprise");

builder.AddProject<Projects.UnifiedLogin_BatchProcessor>("unifiedlogin-batchprocessor");

builder.AddProject<Projects.UnifiedLogin_UserNotification>("unifiedlogin-usernotification");

builder.Build().Run();
