namespace TaAssistant.Model.Api.Responses
{
    public class UserResponse
    {
        public string GivenName { get; set; } = string.Empty;
        public string FamilyName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int UserTypeId { get; set; }
        public string UserType { get; set; } = string.Empty;
    }
}
