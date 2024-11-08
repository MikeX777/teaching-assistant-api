namespace TaAssistant.Model.Entities
{
    public class FullUserEntity : UserEntity
    {
        public string PasswordSalt { get; set; } = string.Empty;
        public string VerificationCode { get; set; } = string.Empty;
        public DateTimeOffset VerificationExpiration { get; set; }
    }
}
