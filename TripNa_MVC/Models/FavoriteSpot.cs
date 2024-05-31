using System;
using System.Collections.Generic;

namespace TripNa_MVC.Models;

public partial class FavoriteSpot
{
    public int FavoriteSpotId { get; set; }

    public int MemberId { get; set; }

    public int SpotId { get; set; }

    public virtual Member Member { get; set; } = null!;

    public virtual Spot Spot { get; set; } = null!;
}
