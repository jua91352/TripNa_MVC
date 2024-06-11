namespace TripNa_MVC.Models
{
    public class ItineraryViewModel
    {
        public Itinerary? Itinerary { get; set; }

        public List<ItineraryDetailViewModel>? ItineraryDetail { get; set; }

        public List<Spot>? Spot { get; set; }
    }

    public class ItineraryDetailViewModel
    {
        public int SpotId { get; set; }
        public DateTime ItineraryDate { get; set; }
        public byte VisitOrder { get; set; }
    }
}
