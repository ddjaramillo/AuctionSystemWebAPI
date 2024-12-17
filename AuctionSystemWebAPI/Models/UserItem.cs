using System.Text.Json.Serialization;

namespace AuctionSystemWebAPI.Models
{
    public class UserItem
    {
        [JsonPropertyName("id")]
#pragma warning disable IDE1006 // Naming Styles
        public required string id { get; set; }                  // Azure AD B2C User ID
#pragma warning restore IDE1006 // Naming Styles

        public string? FullName { get; set; }
        public required string Email { get; set; }
        public List<string>? AuctionIds { get; set; }           // Auctions created by the user
        public List<string>? WonAuctionIds { get; set; }        // Auctions won by the user
    }
}
