using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace UnifiedLogin.Core;

public static class KafkaExtensions
{
    public static IServiceCollection AddKafka(this IServiceCollection services, IConfiguration configuration)
    {
        //Add Kafka Producer and Consumer configuration

        return services;
    }
}
