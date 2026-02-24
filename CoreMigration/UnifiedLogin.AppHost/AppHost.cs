using System.Net;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.UnifiedLogin_LandingAPI>("unified-login-coreapiv2")
    .WithEnvironment("TraceIdRatioBasedSampler", "1")
    //.WithEnvironment("OTEL_SERVICE_NAME", "unified-login-coreapiv2")
    .WithEnvironment("OTEL_RESOURCE_ATTRIBUTES", "service.instance.id=" + Dns.GetHostName() + ",deployment.environment=LOCAL")
    //.WithEnvironment("OTEL_EXPORTER_OTLP_ENDPOINT", "https://fde55d4458df447f9c2eb593c0913f89.apm.us-east-2.aws.elastic-cloud.com:443")
    //.WithEnvironment("OTEL_EXPORTER_OTLP_HEADERS", builder.Configuration["CentralLogOTELAuthentication"])
    ;

builder.AddProject<Projects.UnifiedLogin_LandingAPIEnterprise>("unified-login-coreenterpriseapiv2")
    .WithEnvironment("TraceIdRatioBasedSampler", "1")
    //.WithEnvironment("OTEL_SERVICE_NAME", "unified-login-coreenterpriseapiv2")
    .WithEnvironment("OTEL_RESOURCE_ATTRIBUTES", "service.instance.id=" + Dns.GetHostName() + ",deployment.environment=LOCAL")
    //.WithEnvironment("OTEL_EXPORTER_OTLP_ENDPOINT", "https://fde55d4458df447f9c2eb593c0913f89.apm.us-east-2.aws.elastic-cloud.com:443")
    //.WithEnvironment("OTEL_EXPORTER_OTLP_HEADERS", builder.Configuration["CentralLogOTELAuthentication"])
    ;

builder.AddProject<Projects.UnifiedLogin_BatchProcessor>("unifiedlogin-batchprocessor")
    .WithEnvironment("TraceIdRatioBasedSampler", "1");

builder.Build().Run();
