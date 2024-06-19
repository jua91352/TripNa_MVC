using System;
using System.Collections.Generic;

namespace QAINSERT.Models;

public partial class Qa
{
    public int Qaid { get; set; }

    public int OrderId { get; set; }

    public int MemberId { get; set; }

    public int GuiderId { get; set; }

    public string? QuestionContent { get; set; }

    public DateTime? QuestionTime { get; set; }

    public string? AnswerContent { get; set; }

    public DateTime? AnswerTime { get; set; }
}
