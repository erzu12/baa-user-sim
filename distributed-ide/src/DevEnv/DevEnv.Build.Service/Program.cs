using DevEnv.Base.Ioc;
using DevEnv.Build.Service;
using DevEnv.Build.Service.Services;

Console.WriteLine("Build Service");

var builder = WebApplication.CreateBuilder(args);

// Additional configuration is required to successfully run gRPC on macOS.
// For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

// Add services to the container.
builder.Services.AddGrpc(options =>
{
    options.MaxReceiveMessageSize = 100 * 1024 * 1024;
    options.MaxSendMessageSize = 100 * 1024 * 1024;
});

IocRegistrations.DoRegistrations(new IocContainerWrapper(
    (abstractionType, implementationType) => builder.Services.Add(new ServiceDescriptor(abstractionType, implementationType, ServiceLifetime.Singleton)),
    (abstractionType, instance) => builder.Services.Add(new ServiceDescriptor(abstractionType, instance))));

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<BuildService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
