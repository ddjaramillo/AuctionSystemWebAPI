using AuctionSystemWebAPI.DTOs;
using AuctionSystemWebAPI.Models;
using AuctionSystemWebAPI.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace AuctionSystemWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BidItemsController : ControllerBase
    {
        private readonly IBidItemRepository _bidItemRepository;

        private readonly IAuctionRepository _auctionRepository;

        public BidItemsController(IBidItemRepository bidItemRepository, IAuctionRepository auctionRepository)
        {
            _bidItemRepository = bidItemRepository;
            _auctionRepository = auctionRepository;
        }

        [HttpGet("{auctionItemId}")]
        public async Task<IActionResult> GetBidsByAuction(string auctionItemId)
        {
            var bids = await _bidItemRepository.GetBidsByAuctionIdAsync(auctionItemId);
            return Ok(bids);
        }

        //[HttpPost]
        //public async Task<IActionResult> PlaceBid([FromBody] CreateBidItemDto dto)
        //{
        //    if (string.IsNullOrWhiteSpace(dto.AuctionItemId) || dto.BidAmount <= 0)
        //    {
        //        return BadRequest("Invalid bid data.");
        //    }

        //    var bid = new BidItem
        //    {
        //        id = Guid.NewGuid().ToString(),
        //        AuctionItemId = dto.AuctionItemId,
        //        UserId = dto.UserId,
        //        BidAmount = dto.BidAmount,
        //        BidTime = DateTime.UtcNow
        //    };

        //    await _bidRepository.AddAsync(bid);
        //    return Ok(bid);
        //}

        [HttpPost]
        public async Task<IActionResult> PlaceBid([FromBody] CreateBidItemDto dto)
        {
            // Data validation
            if (string.IsNullOrWhiteSpace(dto.AuctionItemId) || string.IsNullOrWhiteSpace(dto.UserId) || dto.BidAmount <= 0)
            {
                return BadRequest("Invalid bid data.");
            }
            // Data: [AuctionItemId;UserId;BidAmount]

            // Step 1: Retrieve and validate AuctionItem
            var auctionItem = await _auctionRepository.GetByIdAsync(dto.AuctionItemId);
            if (auctionItem == null || auctionItem.Status != "Open")
            {
                return BadRequest("Auction is not open for bidding.");
            }

            // Todo: validate user

            // Bid should be higher than the initial bid 
            if (auctionItem.InitialBid >= dto.BidAmount)
            {
                return BadRequest("Offered Bid should be higher than the Minimun Bid for Auction Item.");
            }

            //var higher bid = await _auctionRepository.GetByIdAsync(dto.AuctionItemId);
            // Bid should be higher than the initial bid {optional can be adjusted to current higher bid}
            //if (auctionItem.InitialBid >= dto.BidAmount)




            // Step 2: Create and save the new bid
            var bid = new BidItem
            {
                id = Guid.NewGuid().ToString(),
                AuctionItemId = dto.AuctionItemId,
                UserId = dto.UserId,
                BidAmount = dto.BidAmount,
                BidTime = DateTime.UtcNow
            };
            await _bidItemRepository.AddAsync(bid);


            BidItem? currentHighestBid = null;
            if (!string.IsNullOrEmpty(auctionItem.HighestBidId))
            {
                currentHighestBid = await _bidItemRepository.GetByIdAsync(auctionItem.HighestBidId, auctionItem.id); // Pass both id and partition key
            }

            // Step 3: Update the HighestBidId if the new bid is higher
            if (currentHighestBid == null || dto.BidAmount > currentHighestBid.BidAmount)
            {
                auctionItem.HighestBidId = bid.id;
                await _auctionRepository.UpdateAsync(auctionItem.id, auctionItem);
            }

            return Ok(bid);
        }


    }
}
