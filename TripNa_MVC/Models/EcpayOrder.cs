using System;
using System.Collections.Generic;

namespace TripNa_MVC.Models;

public partial class EcpayOrder
{
    public string MerchantTradeNo { get; set; } = null!;

    public string? MemberId { get; set; }

    public int? RtnCode { get; set; }

    public string? RtnMsg { get; set; }

    public string? TradeNo { get; set; }

    public int? TradeAmt { get; set; }

    public DateTime? PaymentDate { get; set; }

    public string? PaymentType { get; set; }

    public string? PaymentTypeChargeFee { get; set; }

    public string? TradeDate { get; set; }

    public int? SimulatePaid { get; set; }
}
