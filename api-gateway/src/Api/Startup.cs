using Api.Filter;
using Api.Middleware;
using Application;
using Application.DiscoveryClient;
using Application.Factory;
using Application.GrpcClients;
using Application.GrpcClients.Interceptors;
using Application.Metrics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Serilog;
using System;
using System.Threading;

namespace Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers(cfg =>
            {
                cfg.Filters.Add(typeof(ErrorHandlerFilter));
            });

            services.Configure<DiscoveryConfiguration>(Configuration.GetSection("DiscoveryService"));
            services.Configure<MetricsConfiguration>(Configuration.GetSection("Metrics"));
            services.AddSingleton((provider) => {
                var candidate = provider.GetService<IOptions<MetricsConfiguration>>()?.Value;
                if (candidate.IsValid()) return candidate;

                var config = Newtonsoft.Json.JsonConvert.DeserializeObject<MetricsConfiguration>(
                                    Configuration.GetValue<string>("Metrics")
                                    );
                return config;
            });

            services.AddSingleton<IDiscoveryServiceClient, DiscoveryServiceClient>();
            services.AddSingleton<ChannelFactory>();
            services.AddSingleton<ClientFactory>();

            services.AddSingleton<IMetricsProvider, InfluxDb>();
            services.AddSingleton<ServerMetricsCollector>();

            //services.AddSingleton<MetricsInterceptor>();

            services.AddScoped<Operation>();

            services.AddScoped<IGrpcClient, UnaryGrpcClientSingle>();

            services.AddLogging(logging =>
            {
                var log = new LoggerConfiguration()
                                    .WriteTo.Fluentd(Configuration.GetValue<string>("Logging:Host"),
                                        Configuration.GetValue<int>("Logging:Port"),
                                        Configuration.GetValue<string>("Logging:Tag"))
                                    .CreateLogger();

                logging.AddSerilog(log);
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseSwagger();

            app.UseMiddleware<ContextMiddleware>();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "API");
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            //new Timer((state) =>
            //{
            //    app.ApplicationServices
            //     .GetRequiredService<IMetricsProvider>()
            //     .CollectServerMetrics(app.ApplicationServices
            //                            .GetRequiredService<ServerMetricsCollector>()
            //                            .GetMetrics()
            //                        );
            //}
            //, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));
        }

    }
}
