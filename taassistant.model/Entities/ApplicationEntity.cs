namespace TaAssistant.Model.Entities
{
    public class ApplicationEntity
    {
        public int ApplicationId { get; set; }
        public int UserId { get; set; }
        public int TermId { get; set; }
        public int ApplicationStatusId { get; set; }
        public int Year { get; set; }
        public bool PreviousTA { get; set; }
        public string InstructorNotes { get; set; } = string.Empty;
    }
}
