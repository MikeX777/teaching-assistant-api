namespace TaAssistant.Model.Api.Requests
{
    public class VerifyRequest
    {
        public string Email { get; set; } = string.Empty;
        public string VerificationCode { get; set; } = string.Empty;
    }
}
