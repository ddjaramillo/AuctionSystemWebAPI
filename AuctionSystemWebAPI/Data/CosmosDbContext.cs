using Microsoft.Azure.Cosmos;

namespace AuctionSystemWebAPI.Data
{
    public class CosmosDbContext
    {
        public Container AuctionsContainer { get; }

        public CosmosDbContext(CosmosClient client, string databaseName, string containerName)
        {
            var database = client.GetDatabase(databaseName);
            AuctionsContainer = database.GetContainer(containerName);
        }

        public static async Task InitializeAsync(CosmosClient client, string databaseName, string containerName)
        {
            var database = await client.CreateDatabaseIfNotExistsAsync(databaseName);
            await database.Database.CreateContainerIfNotExistsAsync(containerName, "/id");
        }
    }
}
