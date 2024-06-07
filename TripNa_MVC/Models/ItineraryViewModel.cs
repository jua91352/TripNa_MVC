namespace TripNa_MVC.Models
{
    public class ItineraryViewModel
    {
        public Itinerary? Itinerary { get; set; }
        public List<Spot>? Spot { get; set; }
        public List<ItineraryDetail>? ItineraryDetail { get; set; }

        public string SpotId { get; set; }
        public string ItineraryDate { get; set; }
        public Byte VisitOrder { get; set; }

    }
}
