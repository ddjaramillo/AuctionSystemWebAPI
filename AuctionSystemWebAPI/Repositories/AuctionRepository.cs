using AuctionSystemWebAPI.Data;
using AuctionSystemWebAPI.DTOs;
using AuctionSystemWebAPI.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

namespace AuctionSystemWebAPI.Repositories
{
    public class AuctionRepository : IAuctionRepository
    {
        private readonly Container _container;
        private readonly IBidItemRepository _bidRepository;

        private readonly IConfiguration _configuration;

        public AuctionRepository(CosmosClient cosmosClient, IBidItemRepository bidRepository, IConfiguration configuration, string databaseName, string containerName)
        {
            _container = cosmosClient.GetContainer(databaseName, containerName);
            _bidRepository = bidRepository; // Inject Bid Repository for fetching bids

            _configuration = configuration;
        }

        public async Task<List<AuctionItem>> GetAllAsync()
        {
            var query = _container.GetItemQueryIterator<AuctionItem>();
            var results = new List<AuctionItem>();

            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();
                results.AddRange(response);
            }
            return results;
        }

        public async Task<AuctionItem> GetByIdAsync(string id)
        {
            try
            {
                var response = await _container.ReadItemAsync<AuctionItem>(id, new PartitionKey(id));
                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
#pragma warning disable CS8603 // Possible null reference return.
                return null;
#pragma warning restore CS8603 // Possible null reference return.
            }
        }

        public async Task CreateAsync(AuctionItem item)
        {
            Console.WriteLine($"Creating item with ID: {item.id}");
            item.id ??= Guid.NewGuid().ToString(); // Ensure Id is set
            await _container.CreateItemAsync(item, new PartitionKey(item.id));
        }

        public async Task UpdateAsync(string id, AuctionItem item)
        {
            await _container.UpsertItemAsync(item, new PartitionKey(id));
        }

        public async Task DeleteAsync(string id)
        {
            await _container.DeleteItemAsync<AuctionItem>(id, new PartitionKey(id));
        }

        public async Task CloseAuctionAsync(string id)
        {
            var auction = await GetByIdAsync(id);
            if (auction == null)
                throw new Exception("Auction not found.");

            if (auction.Status == "Closed" || auction.Status == "Completed")
                throw new Exception("Auction is already closed.");


#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            // Fetch the highest bid
            BidItem highestBid = null;
            if (!string.IsNullOrEmpty(auction.HighestBidId))
            {
                highestBid = await _bidRepository.GetByIdAsync(auction.HighestBidId, auction.id);
            }
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
            // Update the auction
            auction.Status = "Closed";
            auction.ClosedDate = DateTime.UtcNow;

            if (highestBid != null)
            {
                auction.WinnerId = highestBid.UserId; // Assign the winner
                auction.Status = "Completed";        // Mark auction as completed if there’s a winner
                auction.ClosedDate = DateTime.UtcNow;
            }
            else
            {
                // Restart auction if no valid bids
                auction.EndDate = DateTime.UtcNow.AddMinutes(5);
                auction.Status = "Open";
            }

            await UpdateAsync(id, auction);
        }


        public async Task<EnrichedAuctionDto> GetEnrichedAuctionByIdAsync(string auctionId)
        {
#pragma warning disable CS8601 // Possible null reference assignment.
            // Step 1: Fetch the auction
            var auction = await GetByIdAsync(auctionId);
#pragma warning disable CS8603 // Possible null reference return.
            if (auction == null) return null;
#pragma warning restore CS8603 // Possible null reference return.

            // Step 2: Fetch the highest bid if it exists
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            BidItem highestBid = null;
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
            if (!string.IsNullOrEmpty(auction.HighestBidId))
            {
                highestBid = await _bidRepository.GetByIdAsync(auction.HighestBidId, auctionId);
            }

            // Step 3: Return the enriched DTO

            return new EnrichedAuctionDto
            {
                id = auction.id,
                Title = auction.Title,
                Description = auction.Description,
                InitialBid = auction.InitialBid,
                Category = auction.Category,
                Status = auction.Status,
                StartDate = auction.StartDate,
                EndDate = auction.EndDate,
                ImageUrl = auction.ImageUrl,
                WinnerId = auction.WinnerId,
                ClosedDate = auction.ClosedDate,
                HighestBid = highestBid
            };
#pragma warning restore CS8601 // Possible null reference assignment.
        }

        public async Task RestartAuctionAsync(string id)
        {
            var auction = await GetByIdAsync(id) ?? throw new Exception("Auction not found.");

            var auctionDuration = _configuration.GetValue<int>("AuctionSettings:DefaultDurationMinutes");

            // Extend EndDate by 5 minutes
            auction.EndDate = DateTime.UtcNow.AddMinutes(auctionDuration);
            auction.Status = "Open";

            await UpdateAsync(id, auction);
        }

        public async Task<IEnumerable<AuctionItem>> GetAuctionsByOwnerAsync(string ownerId)
        {
            var query = _container.GetItemLinqQueryable<AuctionItem>(true)
                                  .Where(a => a.OwnerId == ownerId);

            var iterator = query.ToFeedIterator();
            var results = new List<AuctionItem>();
            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                results.AddRange(response);
            }

            return results;
        }

        public async Task<IEnumerable<AuctionItem>> GetAuctionsWonByUserAsync(string userId)
        {
            var query = _container.GetItemLinqQueryable<AuctionItem>(true)
                                  .Where(a => a.WinnerId == userId);

            var iterator = query.ToFeedIterator();
            var results = new List<AuctionItem>();
            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                results.AddRange(response);
            }

            return results;
        }

    }
}
