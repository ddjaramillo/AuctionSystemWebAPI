using AuctionSystemWebAPI.Models;

namespace AuctionSystemWebAPI.Repositories
{
    public interface IBidItemRepository
    {
        Task<IEnumerable<BidItem>> GetBidsByAuctionIdAsync(string auctionId);
        Task AddAsync(BidItem bid);
        Task<BidItem> GetByIdAsync(string id, string auctionId);
    }
}

