using System;
using System.Collections.Generic;

namespace TripNa_MVC.Models;

public partial class Coupon
{
    public int CouponId { get; set; }

    public int? MemberId { get; set; }

    public int? ItineraryId { get; set; }

    public string CouponCode { get; set; } = null!;

    public DateOnly CouponDueDate { get; set; }

    public virtual Itinerary? Itinerary { get; set; }

    public virtual Member? Member { get; set; }

    public virtual ICollection<Orderlist> Orderlists { get; set; } = new List<Orderlist>();
}
