using DiscoveryService.Grpc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Serilog;
using DiscoveryService.Factory;
using DiscoveryService.HealthCheck;
using DiscoveryService.Util;

namespace DiscoveryService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddMassTransit(cfg => ServiceBus.ConfigureMassTransit(cfg, hostContext.Configuration));
                    services.AddSingleton(new EtcdClientWrap(hostContext.Configuration.GetConnectionString("Etcd")));
                    services.AddSingleton<DiscoveryGrpc>();
                    services.Configure<GrpcConfiguration>(hostContext.Configuration.GetSection("Grpc"));
                    services.Configure<MetricsConfiguration>(hostContext.Configuration.GetSection("Metrics"));
                    

                    services.AddSingleton<GrpcServerFactory>();
                    services.AddSingleton<ChannelFactory>();
                    services.AddSingleton<HealthChecker>();
                    services.AddHostedService<Worker>();
                    
                    services.AddLogging(logging =>
                    {
                        var configuration = hostContext.Configuration;
                        var log = new LoggerConfiguration()
                            .WriteTo.Fluentd(configuration.GetValue<string>("Logging:Host"),
                                configuration.GetValue<int>("Logging:Port"),
                                configuration.GetValue<string>("Logging:Tag"))
                            .CreateLogger();
                
                        logging.AddSerilog(log);
                    });
                });
    }
}
