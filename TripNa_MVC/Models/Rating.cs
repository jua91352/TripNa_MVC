using System;
using System.Collections.Generic;

namespace TripNa_MVC.Models;

public partial class Rating
{
    public int RatingId { get; set; }

    public int MemberId { get; set; }

    public int GuiderId { get; set; }

    public int OrderId { get; set; }

    public string? RatingComment { get; set; }

    public byte RatingStars { get; set; }

    public virtual Guider Guider { get; set; } = null!;

    public virtual Member Member { get; set; } = null!;

    public virtual Orderlist Order { get; set; } = null!;
}
