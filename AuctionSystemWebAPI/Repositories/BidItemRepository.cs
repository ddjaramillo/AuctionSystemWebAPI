using Microsoft.Azure.Cosmos;
using AuctionSystemWebAPI.Models;

namespace AuctionSystemWebAPI.Repositories
{
    public class BidItemRepository : IBidItemRepository
    {
        private readonly Container _container;

        public BidItemRepository(CosmosClient cosmosClient, string databaseName, string containerName)
        {
            _container = cosmosClient.GetContainer(databaseName, containerName);
        }

        public async Task<IEnumerable<BidItem>> GetBidsByAuctionIdAsync(string auctionId)
        {
            var query = new QueryDefinition("SELECT * FROM c WHERE c.AuctionItemId = @auctionId")
                .WithParameter("@auctionId", auctionId);

            var bids = new List<BidItem>();
            using (var iterator = _container.GetItemQueryIterator<BidItem>(query))
            {
                while (iterator.HasMoreResults)
                {
                    var response = await iterator.ReadNextAsync();
                    bids.AddRange(response);
                }
            }
            return bids;
        }

        public async Task AddAsync(BidItem bid)
        {
            await _container.CreateItemAsync(bid, new PartitionKey(bid.AuctionItemId));
        }

        public async Task<BidItem> GetByIdAsync(string id, string auctionItemId)
        {
            try
            {
                // Pass both the id and the partition key
                var response = await _container.ReadItemAsync<BidItem>(id, new PartitionKey(auctionItemId));
                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
#pragma warning disable CS8603 // Possible null reference return.
                return null; // Handle not found cases
#pragma warning restore CS8603 // Possible null reference return.
            }
        }
    }
}
