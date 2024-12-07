namespace TaAssistant.Model.Api.Requests
{
    public class CourseApplicationRequest
    {
        public int CourseId { get; set; }
        public int TermId { get; set; }
        public int Year { get; set; }
        public int GradeId { get; set; }
    }
}
