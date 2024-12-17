namespace AuctionSystemWebAPI.DTOs
{
    public class CreateUserItemDto
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public string FullName { get; set; }                                    // User's full name
        public string Email { get; set; }                                       // User's email
        public List<string> AuctionIds { get; set; } = new List<string>();      // Created auctions
        public List<string> WonAuctionIds { get; set; } = new List<string>();   // Won auctions
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    }

}
