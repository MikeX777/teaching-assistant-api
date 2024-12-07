namespace TaAssistant.Model.Api.Requests
{
    public class SubmitApplicationWithCoursesRequest : SubmitApplicationRequest
    {
        public IEnumerable<CourseApplicationRequest> Courses { get; set; } = [];
    }
}
