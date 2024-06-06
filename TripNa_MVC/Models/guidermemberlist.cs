namespace TripNa_MVC.Models
{
    public class guidermemberlist
    {
        internal Guider Guider;
        internal Member Member;

        public List<guidermemberlist> guidermemberlists { get; set; }
        public List<Guider> Guiders { get; set; }
        public List<Member> Members { get; set; }
        public int MemberId { get; set; }
        public int GuiderId { get; set; }

    }
}
