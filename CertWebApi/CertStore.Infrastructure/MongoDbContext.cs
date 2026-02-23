using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace CertStore.Infrastructure;

public class MongoDbContext
{
    public IMongoDatabase Database { get; }

    public MongoDbContext(IConfiguration configuration)
    {
        var connectionString = configuration["MongoDb:ConnectionString"]
            ?? throw new InvalidOperationException("MongoDB connection string nincs konfigurálva.");
        var databaseName = configuration["MongoDb:Database"]
            ?? throw new InvalidOperationException("MongoDB adatbázisnév nincs beállítva.");

        var client = new MongoClient(connectionString);
        Database = client.GetDatabase(databaseName);
    }
}
