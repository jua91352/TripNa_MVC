namespace TripNa_MVC.Models
{
    public class OrderDetail
    {
        public List<Orderlist> Orders { get; set; }
        public List<Itinerary> Itineraries { get; set; }
        public List<Coupon> Coupons { get; set; }
        public List<Guider> Guiders { get; set; }

        public int MemberId { get; set; }
		public List<Member> Members { get; set; }

        public List<Spot> Spots { get; set; }

        public List<ItineraryDetail> ItineraryDetails { get; set; }
        public int OrderId { get; set; }
        public List<MemberQuestion> MemberQuestions { get; set; }
        public List<GuiderAnswer> GuiderAnswers { get; set; }
        public List<QuestionAnswer> Questions { get; set; }

    }
}
