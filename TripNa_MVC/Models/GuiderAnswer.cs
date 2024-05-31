using System;
using System.Collections.Generic;

namespace TripNa_MVC.Models;

public partial class GuiderAnswer
{
    public int AnswerId { get; set; }

    public int OrderId { get; set; }

    public int GuiderId { get; set; }

    public string? AnswerContent { get; set; }

    public DateTime? AnswerTime { get; set; }

    public virtual Guider Guider { get; set; } = null!;

    public virtual Orderlist Order { get; set; } = null!;
}
