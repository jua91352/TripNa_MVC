using System;
using System.Collections.Generic;

namespace TripNa_MVC.Models;

public partial class GuiderRating
{
    public List<Orderlist> Orders { get; set; }
    public List<Guider> Guiders { get; set; }
    public List<Member> Members { get; set; }
    public List<Rating> Ratings { get; set; }
    public string MemberName { get; set; }
    public IEnumerable<GuiderRatingData> Rates { get; set; }
    //public List<G> Rates { get; set; }
    //Rates

}

public class GuiderRatingData
{
    public string GuiderNickname { get; set; }
    public string MemberName { get; set; }
    public string RatingComment { get; set; }
    public int RatingStars { get; set; }

}
