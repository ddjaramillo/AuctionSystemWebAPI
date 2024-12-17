using AuctionSystemWebAPI.Models;

namespace AuctionSystemWebAPI.Repositories
{
    public interface IUserItemRepository
    {
        Task<UserItem> GetByIdAsync(string id);
        Task AddOrUpdateAsync(UserItem user);
    }
}

