namespace TripNa_MVC.Models
{
    public class OrderDetail
    {
        public List<Orderlist> Orders { get; set; }
        public List<Itinerary> Itineraries { get; set; }
        public List<Coupon> Coupons { get; set; }
        public List<Guider> Guiders { get; set; }
        public int?  GuiderID { get; set; }

        public int MemberId { get; set; }
		public List<Member> Members { get; set; }

        public List<Spot> Spots { get; set; }

        public List<ItineraryDetail> ItineraryDetails { get; set; }
        public int OrderId { get; set; }
        public List<MemberQuestion> MemberQuestions { get; set; }
        public List<GuiderAnswer> GuiderAnswers { get; set; }
        public List<QuestionAnswer> Questions { get; set; }

        public List<SelectGuider> SelectGuiders { get; set; }

        public IEnumerable<Order> Order { get; set; }
     

    }

    public class Order
    {
        public string MemberName { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal OrderTotalPrice { get; set; }
        public string OrderStatus { get; set; } = null!;
        public string OrderMatchStatus { get; set; } = null!;

        public short ItineraryPeopleNo { get; set; }
        public DateTime ItineraryStartDate { get; set; }
        public string ItineraryName { get; set; } = null!;

    }




}
