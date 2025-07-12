using Common.Logging;
using Inventory.Grpc.Extensions;
using Inventory.Grpc.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();
builder.Host.UseSerilog(SeriLogger.Configure);

Log.Information($"Start {builder.Environment.ApplicationName} up");

try
{
// Add services to the container.
    builder.Services.AddGrpc();
    builder.Services.AddInfrastructureServices();
    builder.Services.ConfigureMongoDb();
    var app = builder.Build();

    // Configure the HTTP request pipeline.
    app.MapGrpcService<InventoryService>();
    app.MapGet("/",
        () =>
            "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

    app.Run();
}
catch (Exception ex)
{
    string exceptionType = ex.GetType().Name;
    if (exceptionType.Equals("StopTheHostException", StringComparison.Ordinal))
    {
        throw;
    }

    Log.Fatal(ex, "Unhandled exception");
}
finally
{
    Log.Information("Shutdown Ordering API complete");
    Log.CloseAndFlush();
}