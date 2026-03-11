using System.Net;

var builder = DistributedApplication.CreateBuilder(args);

var unityEnvironment = "dev";

var useCentralLogAPM = false;

var redis = builder.AddRedis("redis-core")
    .WithRedisInsight();

var apiv2 = builder.AddProject<Projects.UnifiedLogin_LandingAPI>("unified-login-coreapiv2")
    .WithEnvironment("TraceIdRatioBasedSampler", "1")
    .WithEnvironment("OTEL_RESOURCE_ATTRIBUTES", "service.instance.id=" + Dns.GetHostName() + ",deployment.environment=LOCAL")
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", unityEnvironment)
    .WithReference(redis, "RedisConnection")
    .WaitFor(redis)
    ;

var apientv2 = builder.AddProject<Projects.UnifiedLogin_LandingAPIEnterprise>("unified-login-coreenterpriseapiv2")
    .WithEnvironment("TraceIdRatioBasedSampler", "1")
    .WithEnvironment("OTEL_RESOURCE_ATTRIBUTES", "service.instance.id=" + Dns.GetHostName() + ",deployment.environment=LOCAL")
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", unityEnvironment)
    .WithReference(redis, "RedisConnection")
    .WaitFor(redis)
    ;

var batchv2 = builder.AddProject<Projects.UnifiedLogin_BatchProcessor>("unifiedlogin-batchprocessorv2")
    .WithEnvironment("TraceIdRatioBasedSampler", "1")
    .WithEnvironment("OTEL_RESOURCE_ATTRIBUTES", "service.instance.id=" + Dns.GetHostName() + ",deployment.environment=LOCAL")
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", unityEnvironment)
    ;

if (useCentralLogAPM)
{
    apiv2.WithEnvironment("OTEL_EXPORTER_OTLP_ENDPOINT", "https://fde55d4458df447f9c2eb593c0913f89.apm.us-east-2.aws.elastic-cloud.com:443")
        .WithEnvironment("OTEL_EXPORTER_OTLP_HEADERS", builder.Configuration["CentralLogOTELAuthentication"]);

    apientv2.WithEnvironment("OTEL_EXPORTER_OTLP_ENDPOINT", "https://fde55d4458df447f9c2eb593c0913f89.apm.us-east-2.aws.elastic-cloud.com:443")
        .WithEnvironment("OTEL_EXPORTER_OTLP_HEADERS", builder.Configuration["CentralLogOTELAuthentication"]);

    batchv2.WithEnvironment("OTEL_EXPORTER_OTLP_ENDPOINT", "https://fde55d4458df447f9c2eb593c0913f89.apm.us-east-2.aws.elastic-cloud.com:443")
        .WithEnvironment("OTEL_EXPORTER_OTLP_HEADERS", builder.Configuration["CentralLogOTELAuthentication"]);
}

builder.Build().Run();
