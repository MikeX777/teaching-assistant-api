namespace TaAssistant.Model.Entities
{
    public class CourseEntity
    {
        public int CourseId { get; set; }
        public string Prefix { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public bool RequireTA { get; set; }
    }
}
