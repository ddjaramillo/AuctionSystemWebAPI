using AuctionSystemWebAPI.DTOs;
using AuctionSystemWebAPI.Models;
using AuctionSystemWebAPI.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Linq;

namespace AuctionSystemWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuctionsController : ControllerBase
    {
        private readonly IAuctionRepository _auctionRepository;
        private readonly IConfiguration _configuration;

        public AuctionsController(IAuctionRepository auctionRepository, IConfiguration configuration)
        {
            _auctionRepository = auctionRepository;
            _configuration = configuration;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAllAuctions()
        {
            var auctions = await _auctionRepository.GetAllAsync();
            return Ok(auctions);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAuctionById(string id)
        {
            var auction = await _auctionRepository.GetByIdAsync(id);
            if (auction == null)
            {
                return NotFound();
            }
            return Ok(auction);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateAuction([FromBody] CreateAuctionDto dto)
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not authenticated.");
            }

            if (dto == null || string.IsNullOrWhiteSpace(dto.Title) || dto.InitialBid <= 0)
            {
                return BadRequest("Invalid auction data.");
            }

            var auctionDuration = _configuration.GetValue<int>("AuctionSettings:DefaultDurationMinutes");

            var auction = new AuctionItem
            {
                id = Guid.NewGuid().ToString(), // Generate a unique id
                Title = dto.Title,
                Description = dto.Description,
                InitialBid = dto.InitialBid,
                HighestBidId = dto.HighestBidId,
                Category = dto.Category,
                OwnerId = userId,
                Status = dto.Status ?? "Open",
                StartDate = dto.StartDate,
                EndDate = DateTime.UtcNow.AddMinutes(auctionDuration),
                ImageUrl = dto.ImageUrl ?? "https://media.istockphoto.com/id/1965292897/vector/unsold-icon.jpg?s=612x612&w=0&k=20&c=VyNxyqXxm7stHG55AT91dSyJvC75YHVYLY74MnCoruM=",
                WinnerId = dto.WinnerId,
                ClosedDate = dto.ClosedDate
            };

            await _auctionRepository.CreateAsync(auction);
            return CreatedAtAction(nameof(GetAllAuctions), new { id = auction.id }, auction);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAuction(string id, [FromBody] CreateAuctionDto dto)
        {
            var existingAuction = await _auctionRepository.GetByIdAsync(id);
            if (existingAuction == null)
            {
                return NotFound();
            }

            existingAuction.Title = dto.Title;
            existingAuction.Description = dto.Description;
            existingAuction.InitialBid = dto.InitialBid;
            existingAuction.HighestBidId = dto.HighestBidId;
            existingAuction.Category = dto.Category;
            existingAuction.Status = dto.Status;
            existingAuction.StartDate = dto.StartDate;
            existingAuction.EndDate = dto.EndDate;
            existingAuction.ImageUrl = dto.ImageUrl;

            await _auctionRepository.UpdateAsync(id, existingAuction);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAuction(string id)
        {
            await _auctionRepository.DeleteAsync(id);
            return NoContent();
        }

//        [HttpGet("enriched/{id}")]
//        public async Task<IActionResult> GetEnrichedAuctionDetails(string id)
//        {
//            // Step 1: Fetch the AuctionItem
//            var auction = await _auctionRepository.GetByIdAsync(id);
//            if (auction == null)
//            {
//                return NotFound("Auction not found.");
//            }

//            // Step 2: Fetch the HighestBid details
//            BidItem? highestBid = null;
//            if (!string.IsNullOrWhiteSpace(auction.HighestBidId))
//            {
//                highestBid = await _bidRepository.GetByIdAsync(auction.HighestBidId, auction.id);
//            }

//            // Step 3: Build the EnrichedAuctionDto
//#pragma warning disable CS8601 // Possible null reference assignment.
//            var enrichedAuction = new EnrichedAuctionDto
//            {
//                id = auction.id,
//                Title = auction.Title,
//                Description = auction.Description,
//                InitialBid = auction.InitialBid,
//                Category = auction.Category,
//                OwnerId = auction.OwnerId,
//                Status = auction.Status,
//                StartDate = auction.StartDate,
//                EndDate = auction.EndDate,
//                ImageUrl = auction.ImageUrl,
//                WinnerId = auction.WinnerId,
//                ClosedDate = auction.ClosedDate,
//                HighestBid = highestBid
//            };
//#pragma warning restore CS8601 // Possible null reference assignment.

//            return Ok(enrichedAuction);
//        }

        [HttpGet("enriched/{id}")]
        public async Task<IActionResult> GetEnrichedAuction(string id)
        {
            var enrichedAuction = await _auctionRepository.GetEnrichedAuctionByIdAsync(id);

            if (enrichedAuction == null)
                return NotFound("Auction not found or no bids placed.");

            return Ok(enrichedAuction);
        }

        [HttpPost("{id}/close")]
        public async Task<IActionResult> CloseAuction(string id)
        {
            try
            {
                await _auctionRepository.CloseAuctionAsync(id);
                return Ok("Auction closed successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{id}/restart")]
        public async Task<IActionResult> RestartAuction(string id)
        {
            try
            {
                await _auctionRepository.RestartAuctionAsync(id);
                return Ok("Auction restarted successfully for another 5 minutes.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /*
        [HttpGet("user/{ownerId}/created")]
        public async Task<IActionResult> GetAuctionsByOwner(string ownerId)
        {
            var auctions = await _auctionRepository.GetAuctionsByOwnerAsync(ownerId);
            return Ok(auctions);
        }

        [HttpGet("user/{userId}/won")]
        public async Task<IActionResult> GetAuctionsWonByUser(string userId)
        {
            var auctions = await _auctionRepository.GetAuctionsWonByUserAsync(userId);
            return Ok(auctions);
        }
        */

        [HttpGet("created")]
        [Authorize]
        public async Task<IActionResult> GetAuctionsByOwner()
        {
            var userId = User.FindFirst("sub")?.Value; // Get the user's ID from the token
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var auctions = await _auctionRepository.GetAuctionsByOwnerAsync(userId);
            return Ok(auctions);
        }

        [HttpGet("won")]
        [Authorize]
        public async Task<IActionResult> GetAuctionsWonByUser()
        {
            var userId = User.FindFirst("sub")?.Value; // Get the user's ID from the token
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var auctions = await _auctionRepository.GetAuctionsWonByUserAsync(userId);
            return Ok(auctions);
        }


    }
}

