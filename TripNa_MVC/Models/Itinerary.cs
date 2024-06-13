using System;
using System.Collections.Generic;

namespace TripNa_MVC.Models;

public partial class Itinerary
{
    public int ItineraryId { get; set; }

    public string ItineraryName { get; set; } = null!;

    public DateTime ItineraryStartDate { get; set; }

    public short ItineraryPeopleNo { get; set; }


    public virtual ICollection<ItineraryDetail> ItineraryDetails { get; set; } = new List<ItineraryDetail>();

    public virtual ICollection<Orderlist> Orderlists { get; set; } = new List<Orderlist>();

}