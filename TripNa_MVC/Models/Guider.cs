using System;
using System.Collections.Generic;

namespace TripNa_MVC.Models;

public partial class Guider
{
    internal Member? Members { get; set; } = null!;
    //internal Rating Ratings { get; set; } = null!;

    public int GuiderId { get; set; }

    public string GuiderNickname { get; set; } = null!;

    public string GuiderGender { get; set; } = null!;

    public string GuiderArea { get; set; } = null!;

    public DateOnly GuiderStartDate { get; set; }

    public string GuiderIntro { get; set; } = null!;

    public virtual ICollection<GuiderAnswer> GuiderAnswers { get; set; } = new List<GuiderAnswer>();

    public virtual ICollection<Orderlist> Orderlists { get; set; } = new List<Orderlist>();

    public virtual ICollection<Rating> Ratings { get; set; } = new List<Rating>();

    public virtual ICollection<SelectGuider> SelectGuiders { get; set; } = new List<SelectGuider>();
    public Rating? Rating { get; internal set; }
}
