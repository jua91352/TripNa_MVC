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

    }
}
