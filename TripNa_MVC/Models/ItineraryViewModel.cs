namespace TripNa_MVC.Models
{
    public class ItineraryViewModel
    {
        public Itinerary? Itinerary { get; set; }
        public List<Spot>? Spot { get; set; }
        public List<ItineraryDetail>? ItineraryDetail { get; set; }

        public int SpotId { get; set; }
        public DateTime ItineraryDate { get; set; }
        public Byte VisitOrder { get; set; }

    }
}
