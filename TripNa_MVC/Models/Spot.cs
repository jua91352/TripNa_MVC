// Spot.cs
using System;
using System.Collections.Generic;

namespace TripNa_MVC.Models
{
    public partial class Spot
    {
        public int SpotId { get; set; }
        public string SpotName { get; set; } = null!;
        public string SpotCity { get; set; } = null!;
        public string SpotBrief { get; set; } = null!;
        public string SpotIntro { get; set; } = null!;
        public virtual ICollection<FavoriteSpot> FavoriteSpots { get; set; } = new List<FavoriteSpot>();
        public virtual ICollection<ItineraryDetail> ItineraryDetails { get; set; } = new List<ItineraryDetail>();
    }
}
