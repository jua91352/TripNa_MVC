﻿using System;
using System.Collections.Generic;

namespace TripNa_MVC.Models;

public partial class Orderlist
{
    internal Coupon Coupon;
    internal Spot Spots;
    internal ItineraryDetail ItineraryDetail;
    internal MemberQuestion MemberQuestion;
    internal GuiderAnswer GuiderAnswer;
    internal DateTime ItineraryStartDate;
    internal short ItineraryPeopleNo;

    internal Rating Rating { get; set; } = null!;

    public int OrderId { get; set; }

    public int MemberId { get; set; }

    public int? GuiderId { get; set; }
    public int? CouponId { get; set; }

    public int ItineraryId { get; set; }

    public DateTime OrderDate { get; set; }

    public string? OrderNumber { get; set; }

    public string OrderStatus { get; set; } = null!;

    public string OrderMatchStatus { get; set; } = null!;

    public decimal OrderTotalPrice { get; set; }

    public virtual Guider? Guider { get; set; }

    public virtual ICollection<GuiderAnswer> GuiderAnswers { get; set; } = new List<GuiderAnswer>();

    public virtual Itinerary? Itinerary { get; set; } = null!;

    public virtual Member? Member { get; set; } = null!;

    public virtual ICollection<MemberQuestion> MemberQuestions { get; set; } = new List<MemberQuestion>();

    public virtual ICollection<Rating> Ratings { get; set; } = new List<Rating>();

    public virtual ICollection<SelectGuider> SelectGuiders { get; set; } = new List<SelectGuider>();
    //public string? ItineraryName { get; internal set; }
}
