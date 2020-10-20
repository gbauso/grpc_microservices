using DiscoveryService.Extensions;
using MassTransit;
using MassTransit.ExtensionsDependencyInjectionIntegration;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace DiscoveryService
{
    public class ServiceBus
    {
        public static void ConfigureMassTransit(IServiceCollectionConfigurator configurator, IConfiguration configuration)
        {
            configurator.AddConsumer<DiscoveryConsumer>();

            configurator.AddBus(provider => Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                var connection = configuration.GetSection("ServiceBus").FromSection<ServiceBusConfiguration>();

                cfg.Host(connection.Host, c =>
                {
                    c.Username(connection.Username);
                    c.Password(connection.Password);
                });


                cfg.ReceiveEndpoint("discovery", e =>
                {
                    e.ClearMessageDeserializers();
                    e.UseRawJsonSerializer();
                    e.ConfigureConsumer<DiscoveryConsumer>(provider,
                        c => c.UseConcurrentMessageLimit(1));

                    EndpointConvention.Map<DiscoveryConsumer>(e.InputAddress);
                });
            }));
        }
    }
}
