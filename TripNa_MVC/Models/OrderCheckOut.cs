namespace TripNa_MVC.Models
{
    public class OrderCheckOut
    {
        public string MerchantTradeNo { get; set; }
        public string MerchantTradeDate { get; set; }
        public string TotalAmount { get; set; }
        public string TradeDesc { get; set; }
        public string ItemName { get; set; }
        public string ExpireDate { get; set; }
        public string CustomField1 { get; set; }
        public string CustomField2 { get; set; }
        public string CustomField3 { get; set; }
        public string CustomField4 { get; set; }
        public string ReturnURL { get; set; }
        public string OrderResultURL { get; set; }
        public string PaymentInfoURL { get; set; }
        public string ClientRedirectURL { get; set; }
        public string MerchantID { get; set; }
        public string IgnorePayment { get; set; }
        public string PaymentType { get; set; }
        public string ChoosePayment { get; set; }
        public string EncryptType { get; set; }
        public string CheckMacValue { get; set; }

        public int MemberId { get; set; }
        public int OrderId { get; set; }

        public List<Itinerary> Itineraries { get; set; }
        public List<Spot> Spots { get; set; }
        public List<ItineraryDetail> ItineraryDetails { get; set; }

        public List<Coupon> MemberCoupons { get; set; }
        public List<Orderlist> Orders { get; set; }

    }

   
}
/*
//特店交易編號
{ "MerchantTradeNo",  orderId},
//特店交易時間 yyyy/MM/dd HH:mm:ss
{ "MerchantTradeDate",  DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")},
//交易金額
{ "TotalAmount",  "100"},
//交易描述
{ "TradeDesc",  "無"},
//商品名稱
{ "ItemName",  "測試商品"},
//允許繳費有效天數(付款方式為 ATM 時，需設定此值)
{ "ExpireDate",  "3"},
//自訂名稱欄位1
{ "CustomField1",  ""},
//自訂名稱欄位2
{ "CustomField2",  ""},
//自訂名稱欄位3
{ "CustomField3",  ""},
//自訂名稱欄位4
{ "CustomField4",  ""},
//綠界回傳付款資訊至此URL
{ "ReturnURL",  $"{website}/api/Ecpay/AddPayInfo"},
//使用者於綠界付款完成後，綠界將會轉址至此URL
{ "OrderResultURL", $"{website}/Home/PayInfo/{orderId}"},
//付款方式為 ATM 時，當使用者於綠界操作結束時，綠界回傳 虛擬帳號資訊至 此URL
{ "PaymentInfoURL",  $"{website}/api/Ecpay/AddAccountInfo"},
//付款方式為 ATM 時，當使用者於綠界操作結束時，綠界會轉址至 此URL。
{ "ClientRedirectURL",  $"{website}/Home/AccountInfo/{orderId}"},
//特店編號， 2000132 測試綠界編號
{ "MerchantID",  "2000132"},
//忽略付款方式
{ "IgnorePayment",  "GooglePay#WebATM#CVS#BARCODE"},
//交易類型 固定填入 aio
{ "PaymentType",  "aio"},
//選擇預設付款方式 固定填入 ALL
{ "ChoosePayment",  "ALL"},
//CheckMacValue 加密類型 固定填入 1 (SHA256)
{ "EncryptType",  "1"},
 */