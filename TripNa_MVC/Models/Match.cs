namespace TripNa_MVC.Models
{
    public class Match
    {
        public int OrderId { get; set; }

        public int MemberId { get; set; }
        public int? GuiderId { get; set; }
        public int ItineraryId { get; set; }
        public string OrderStatus { get; set; }
        public string OrderMatchStatus { get; set; }
    }
}
