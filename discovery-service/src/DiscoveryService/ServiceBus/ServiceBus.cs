using DiscoveryService.Extensions;
using MassTransit;
using MassTransit.ExtensionsDependencyInjectionIntegration;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Security.Authentication;

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

                if(connection.IsCloudAmqp)
                {
                    cfg.Host(connection.Host, 5672, connection.Username, h =>
                    {
                        h.Username(connection.Username);
                        h.Password(connection.Password);

                        h.UseSsl(s =>
                        {
                            s.Protocol = SslProtocols.Tls12;
                        }); 
                    });
                }
                else
                {
                    cfg.Host(connection.Host, c =>
                    {
                        c.Username(connection.Username);
                        c.Password(connection.Password);
                    });
                }


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
