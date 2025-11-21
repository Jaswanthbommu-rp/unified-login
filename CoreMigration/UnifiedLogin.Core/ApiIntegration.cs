using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.AspNetCore;

namespace UnifiedLogin.Core;

public static class ApiIntegration
{
    public static IServiceCollection AddApiIntegrations(this IServiceCollection services, IConfiguration config)
    {
        //const string tokenServerForSampleClient = "demoidserver";
        //const string sampleClientCfg = "SampleClientIntegration";

        //// note: token gets cached in IDistributedCache service
        //services.AddAccessTokenManagement(options =>
        //{
        //    options.Client.Clients.Add(tokenServerForSampleClient, new ClientCredentialsTokenRequest
        //    {
        //        Address = config.GetValue<string>($"{sampleClientCfg}:TokenEndpoint"),
        //        ClientId = config.GetValue<string>($"{sampleClientCfg}:ClientId")!,
        //        ClientSecret = config.GetValue<string>($"{sampleClientCfg}:ClientSecret"),
        //        Scope = config.GetValue<string>($"{sampleClientCfg}:Scopes")
        //    });
        //});

        ////Sample
        ////services.AddHttpClient<ISampleClient, SampleClient>()
        ////    .AddClientAccessTokenHandler(tokenServerForSampleClient);

        return services;
    }
}
