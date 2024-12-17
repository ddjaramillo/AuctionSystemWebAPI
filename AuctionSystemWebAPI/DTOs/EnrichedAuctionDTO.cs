using AuctionSystemWebAPI.Models;

namespace AuctionSystemWebAPI.DTOs
{
    public class EnrichedAuctionDto
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public string id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal InitialBid { get; set; }
        public string Category { get; set; }
        public string OwnerId { get; set; }
        public string Status { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string ImageUrl { get; set; }
        public string WinnerId { get; set; }
        public DateTime? ClosedDate { get; set; }
        public BidItem HighestBid { get; set; }  // Embedded highest bid
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    }

}
