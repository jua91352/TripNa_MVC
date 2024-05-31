using System;
using System.Collections.Generic;

namespace TripNa_MVC.Models;

public partial class MemberQuestion
{
    public int QuestionId { get; set; }

    public int OrderId { get; set; }

    public int MemberId { get; set; }

    public string? QuestionContent { get; set; }

    public DateTime? QuestionTime { get; set; }

    public virtual Member Member { get; set; } = null!;

    public virtual Orderlist Order { get; set; } = null!;
}
