using System;
using System.Collections.Generic;

namespace TripNa_MVC.Models;

public partial class SelectGuider
{
    public int OrderId { get; set; }

    public int MemberId { get; set; }

    public int GuiderId { get; set; }

    public virtual Guider Guider { get; set; } = null!;

    public virtual Member Member { get; set; } = null!;

    public virtual Orderlist Order { get; set; } = null!;
}
