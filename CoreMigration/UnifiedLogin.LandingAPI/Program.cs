
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using UnifiedLogin.Core;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();


builder.Services.AddDistributedMemoryCache(); // used for caching access token for remote api call

builder.Services.AddLaunchDarkly(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddApiProblemDetails();

builder.Services
    .AddMvcCoreWithAddOns()
    .AddVersioning()
    .AddUnifiedPlatformAuthentication(builder.Configuration)
    .AddApiIntegrations(builder.Configuration)
    .AddSwaggerDocumentation()
    .AddRepositories(builder.Configuration)
    .AddBusinessLogic(builder.Configuration);


var app = builder.Build();

app.MapDefaultEndpoints();

//using (var scope = app.Services.CreateScope())
//{
//    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
//    dbContext.Database.Migrate();

//    if (app.Environment.IsDevelopment())
//    {
//        dbContext.CreateFreshSampleData(50);
//    }
//}

var allCorsOrigins = builder.Configuration.GetValue<string>("AllCORSOrigins");
if (!string.IsNullOrEmpty(allCorsOrigins))
{
    var origins = allCorsOrigins.Split(",");
    app.UseCors(bld => bld
           .WithOrigins(origins)
           .AllowAnyHeader()
           .AllowAnyMethod());
}

var apiVersionProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
app
    .UseSwaggerDocumentation(builder.Configuration, apiVersionProvider)
    .UseAuthentication()
    .UseRouting()
    .UseAuthorization()
    //.UseMiddleware<UnifiedLoginUserScopeMiddleware>()
    .UseExceptionHandler() // put here so we can capture logged in user with exceptions
    .UseEndpoints(endpoints =>
    {
        endpoints.MapControllers();
    });

app.Run();

public partial class Program { } // needed for integration testing