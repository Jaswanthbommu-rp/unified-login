 
using Newtonsoft.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// --- Service Registrations ---

// Add controllers with custom JSON settings
builder.Services.AddControllers(options =>
{
    // Remove XML formatter if present (SystemTextJsonOutputFormatter is default in .NET 8)
})
.AddNewtonsoftJson(options =>
{
    options.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented;
    options.SerializerSettings.DateFormatHandling = Newtonsoft.Json.DateFormatHandling.MicrosoftDateFormat;
    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
});

// Add CORS policy (allow all, as in legacy)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Add Swagger if needed
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- Authentication (IdentityServer4.AccessTokenValidation) ---
var identityConfig = builder.Configuration.GetSection("IdentityConfig");
var authority = identityConfig["IssuerUri"];
var requiredScopes = identityConfig["RequiredScope"]?.Split(' ', System.StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = authority;
        options.RequireHttpsMetadata = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = authority,
            ValidateAudience = false, // Set to true and configure if you have audience
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true
        };
        options.Audience = requiredScopes.FirstOrDefault(); // Use first scope as audience if needed
    });

var app = builder.Build();

// --- Middleware Pipeline ---

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// --- Helper methods/classes (if needed) ---
// If you need to load certificates, use the ASP.NET Core configuration and dependency injection model.
// The GetSigningCertificate method can be adapted as a service if required.
