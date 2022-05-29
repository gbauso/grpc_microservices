using GrpcGateway.Services;
using Serilog;
using Serilog.Formatting.Json;
using Utils.Grpc.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Additional configuration is required to successfully run gRPC on macOS.
// For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

var serilog = new LoggerConfiguration().
    WriteTo.Console()
    .WriteTo.File(new JsonFormatter(), builder.Configuration.GetValue<string>("Logging:Path"))
    .CreateLogger();

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();
builder.Services.UseChannelFactory(builder.Configuration);
builder.Services.AddLogging(builder => builder.AddSerilog(serilog));

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<CityInformationService>();
app.MapGrpcReflectionService();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
