using GrpcComposition.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Prometheus;
using Serilog;
using Utils.Grpc.Extensions;

namespace GrpcComposition
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
            services.UseChannelFactory(Configuration);

            services.AddLogging(logging =>
            {
                var log = new LoggerConfiguration()
                                    .WriteTo.Fluentd(Configuration.GetValue<string>("Logging:Host"),
                                        Configuration.GetValue<int>("Logging:Port"),
                                        Configuration.GetValue<string>("Logging:Tag"))
                                    .CreateLogger();

                logging.AddSerilog(log);
            });

            services.AddGrpc();

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
            app.UseGrpcMetrics();

            new KestrelMetricServer(port: Configuration.GetValue<int>("Metrics:Port")).Start();

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "API");
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<CityInformationService>();
            });

        }

    }
}
