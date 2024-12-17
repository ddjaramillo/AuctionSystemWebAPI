using System.Text.Json.Serialization;

namespace AuctionSystemWebAPI.Models
{
    public class BidItem
    {
        [JsonPropertyName("id")]
#pragma warning disable IDE1006 // Naming Styles
        public required string id { get; set; }                  // Unique ID for the bid
#pragma warning restore IDE1006 // Naming Styles

        public required string AuctionItemId { get; set; }       // Links to the auction item
        public required string UserId { get; set; }              // ID of the bidder
        public decimal BidAmount { get; set; }                  // Amount bid by the user
        public DateTime BidTime { get; set; }                   // Timestamp of the bid
    }
}
