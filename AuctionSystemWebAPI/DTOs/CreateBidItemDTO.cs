namespace AuctionSystemWebAPI.DTOs
{
    public class CreateBidItemDto
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public string AuctionItemId { get; set; }    // The auction this bid is associated with
        public string UserId { get; set; }          // User placing the bid
        public decimal BidAmount { get; set; }      // Amount of the bid
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    }

}
