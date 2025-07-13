using Infrastructure.Extensions;
using Inventory.Grpc.Repositories;
using Inventory.Grpc.Repositories.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Shared.Configurations;

namespace Inventory.Grpc.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddScoped<IInventoryRepository, InventoryRepository>();

        return services;
    }

    internal static IServiceCollection AddInfrastructureConfigure(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = configuration.GetSection(nameof(MongoDbSettings));
        services.Configure<MongoDbSettings>(settings);
        return services;
    }

    internal static IServiceCollection ConfigureMongoDb(this IServiceCollection services)
    {
        var connectionString = services.GetMongoConnectionString();
        services.AddSingleton<IMongoClient>(new MongoClient(connectionString))
            .AddScoped<IClientSessionHandle>(x =>
            {
                var mongoClient = x.GetService<IMongoClient>();
                ArgumentNullException.ThrowIfNull(mongoClient);

                return mongoClient.StartSession();
            });
        return services;
    }

    private static string GetMongoConnectionString(this IServiceCollection services)
    {
        var settings = services.GetOptions<MongoDbSettings>(nameof(MongoDbSettings));
        ArgumentNullException.ThrowIfNullOrEmpty(settings.ConnectionString, nameof(MongoDbSettings));

        var databaseName = settings.DatabaseName;
        var connectionString = string.Format("{0}/{1}?authSource=admin", 
            settings.ConnectionString, databaseName);
        return connectionString;
    }
}