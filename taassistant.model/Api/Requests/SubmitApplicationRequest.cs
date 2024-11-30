namespace TaAssistant.Model.Api.Requests
{
    public class SubmitApplicationRequest
    {
        public int UserId { get; set; }
        public int TermId { get; set; }
        public int ApplicationStatusId { get; set; }
        public int Year { get; set; }
        public bool PreviousTA { get; set; }
    }
}
