using System;
using System.Collections.Generic;

namespace TripNa_MVC.Models;

public partial class Member
{
    public int MemberId { get; set; }

    public string MemberEmail { get; set; } = null!;

    public string MemberName { get; set; } = null!;

    public DateOnly MemberBirthDate { get; set; }

    public string MemberPhone { get; set; } = null!;

    public string MemberPassword { get; set; } = null!;

    public int? GuiderId { get; set; }

    public virtual ICollection<Coupon> Coupons { get; set; } = new List<Coupon>();

    public virtual ICollection<FavoriteSpot> FavoriteSpots { get; set; } = new List<FavoriteSpot>();

    public virtual Guider? Guider { get; set; }

    public virtual ICollection<MemberQuestion> MemberQuestions { get; set; } = new List<MemberQuestion>();

    public virtual ICollection<Orderlist> Orderlists { get; set; } = new List<Orderlist>();

    public virtual ICollection<Rating> Ratings { get; set; } = new List<Rating>();

    public virtual ICollection<SelectGuider> SelectGuiders { get; set; } = new List<SelectGuider>();
}
