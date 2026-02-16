using UnifiedLogin.BatchProcessor.Configuration;

var builder = Host.CreateApplicationBuilder(args);
builder.AddKeyedSqlServerClient("DBConnection");

builder.Services.AddRequiredServices(builder.Configuration);
builder.Services.AddHostedServices();

var host = builder.Build();
await host.RunAsync();
