using System;
using System.Collections.Generic;

namespace TripNa_MVC.Models;

public partial class MemberRating
{
    public int RatingId { get; set; }

    public int MemberId { get; set; }

    public int GuiderId { get; set; }

    public int OrderId { get; set; }

    public string? RatingComment { get; set; }

    public byte RatingStars { get; set; }


}
