namespace TripNa_MVC.Models
{
    public class QuestionAnswer
    {
        public string QuestionContent { get; set; }
        public DateTime QuestionTime { get; set; }
        public string AnswerContent { get; set; }
        public DateTime? AnswerTime { get; set; }
    }
}
