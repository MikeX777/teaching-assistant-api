namespace TaAssistant.Model.Entities
{
    public class UserEntity
    {
        public int UserId { get; set; }
        public string GivenName { get; set; } = string.Empty;
        public string FamilyName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public int UserTypeId { get; set; }
        public bool Pending { get; set; }
    }
}
