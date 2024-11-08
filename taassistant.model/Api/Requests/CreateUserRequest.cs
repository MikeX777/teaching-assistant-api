namespace TaAssistant.Model.Api.Requests
{
    public class CreateUserRequest
    {
        public string Email { get; set; } = string.Empty;
        public string GivenName { get; set; } = string.Empty;
        public string FamilyName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string PasswordSalt { get; set; } = string.Empty;
        public int UserTypeId { get; set; }
    }
}
