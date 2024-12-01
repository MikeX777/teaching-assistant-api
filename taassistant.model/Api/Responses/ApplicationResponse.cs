namespace TaAssistant.Model.Api.Responses
{
    public class ApplicationResponse
    {
        public int ApplicationId { get; set; }
        public int UserId { get; set; }
        public TermResponse Term { get; set; } = new TermResponse();
        public ApplicationStatusResponse Status { get; set; } = new ApplicationStatusResponse();
        public int Year { get; set; }
        public bool PreviousTA { get; set; }
        public string InstructorNotes { get; set; } = string.Empty;
    }
}
