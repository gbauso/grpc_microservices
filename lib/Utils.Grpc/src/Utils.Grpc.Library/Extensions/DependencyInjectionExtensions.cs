using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Utils.Grpc.DiscoveryClient;
using Utils.Grpc.Factory;
using Utils.Grpc.GrpcClients;
using Utils.Grpc.GrpcClients.Interceptors;
using Utils.Grpc.Mediator;
using Utils.Grpc.Metrics;

namespace Utils.Grpc.Extensions
{
    public static class DependencyInjectionExtensions
    {
        public static void RegisterGrpcMediator(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<DiscoveryConfiguration>(configuration.GetSection("DiscoveryService"));

            services.AddScoped<IDiscoveryServiceClient, DiscoveryServiceClient>();
            services.AddScoped<IMetricsProvider, PrometheusMetrics>();

            services.AddSingleton<ChannelFactory>();
            services.AddSingleton<ClientFactory>();

            services.AddScoped<MetricsInterceptor>();

            services.AddScoped<Operation>();

            services.AddScoped<UnaryGrpcClientSingle>();

            services.AddTransient<IGrpcMediator, GrpcMediator>();
        }
    }
}
