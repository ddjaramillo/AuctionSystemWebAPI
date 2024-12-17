using AuctionSystemWebAPI.DTOs;
using AuctionSystemWebAPI.Models;

namespace AuctionSystemWebAPI.Repositories
{
    public interface IAuctionRepository
    {
        Task<List<AuctionItem>> GetAllAsync();
        Task<AuctionItem> GetByIdAsync(string id);
        Task CreateAsync(AuctionItem item);
        Task UpdateAsync(string id, AuctionItem existingAuction);
        Task DeleteAsync(string id);
        Task CloseAuctionAsync(string id);
        Task RestartAuctionAsync(string id);
        Task<EnrichedAuctionDto> GetEnrichedAuctionByIdAsync(string auctionId);
        Task<IEnumerable<AuctionItem>> GetAuctionsByOwnerAsync(string ownerId);
        Task<IEnumerable<AuctionItem>> GetAuctionsWonByUserAsync(string userId);

    }
}
