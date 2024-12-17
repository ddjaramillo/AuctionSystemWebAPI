using System.Text.Json.Serialization;

namespace AuctionSystemWebAPI.Models
{
    public class AuctionItem
    {
        [JsonPropertyName("id")]
#pragma warning disable IDE1006 // Naming Styles
        public required string id { get; set; }                     // Partition Key
#pragma warning restore IDE1006 // Naming Styles
        public required string Title { get; set; }                  // Auction item title
        public required string Description { get; set; }            // Auction item description
        public decimal InitialBid { get; set; }                     // Starting bid
        public string? HighestBidId { get; set; }                   // ID of the current highest bid
        public string? Category { get; set; }                        // ID of the current highest bid
        public string? OwnerId { get; set; }                        // ID of the user who created the auction
        public required string Status { get; set; }                 // Open, Closed, Completed
        public DateTime StartDate { get; set; }                     // Auction start date
        public DateTime EndDate { get; set; }                       // Auction end date
        public string? ImageUrl { get; set; }                       // Image URL for the item
        public string? WinnerId { get; set; }                       // UserId of the auction winner (nullable)
        public DateTime? ClosedDate { get; set; }                   // Date when the auction was closed
    }
}
