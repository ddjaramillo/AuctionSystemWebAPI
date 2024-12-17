using Microsoft.Azure.Cosmos;
using AuctionSystemWebAPI.Models;

namespace AuctionSystemWebAPI.Repositories
{
    public class UserItemRepository : IUserItemRepository
    {
        private readonly Container _container;

        public UserItemRepository(CosmosClient cosmosClient, string databaseName, string containerName)
        {
            _container = cosmosClient.GetContainer(databaseName, containerName);
        }

        public async Task<UserItem> GetByIdAsync(string id)
        {
            var response = await _container.ReadItemAsync<UserItem>(id, new PartitionKey(id));
            return response.Resource;
        }

        public async Task AddOrUpdateAsync(UserItem user)
        {
            await _container.UpsertItemAsync(user, new PartitionKey(user.id));
        }
    }
}

