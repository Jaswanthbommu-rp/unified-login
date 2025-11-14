using LaunchDarkly.Logging;
using LaunchDarkly.Sdk.Server;
using LaunchDarkly.Sdk.Server.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace UnifiedLogin.Core;

public static class LaunchDarklyExtensions
{
    /// <summary>
    /// Make sure to add this at the beginning of the services collection, since it needs to build a service provider to get the logger factory.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="config"></param>
    /// <returns></returns>
    public static IServiceCollection AddLaunchDarkly(this IServiceCollection services, IConfiguration config)
    {
        var sdkKey = config.GetValue<string>("LaunchDarkly:SdkKey");

        var sp = services.BuildServiceProvider();
        var loggerFactory = sp.GetService<ILoggerFactory>();

        var ldConfig = Configuration.Builder(sdkKey)
                            .Logging(LdMicrosoftLogging.Adapter(loggerFactory)).Build();

        services.AddSingleton<ILdClient>(_ => new LdClient(ldConfig));

        return services;
    }
}
