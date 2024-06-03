using System;
using System.Collections.Generic;

namespace TripNa_MVC.Models;

public partial class ItineraryDetail
{
    public int ItineraryDetailsId { get; set; }

    public int? ItineraryId { get; set; }

    public int? SpotId { get; set; }

    public DateTime ItineraryDate { get; set; }

    public byte? VisitOrder { get; set; }

    public virtual Itinerary? Itinerary { get; set; }

    public virtual Spot? Spot { get; set; }
}
