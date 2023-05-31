namespace WebAPI.Contracts.Responses
{
    public class UserResponse
    {
        public string Name { get; set; } = null!;
        public int Gender { get; set; }
        public DateTime? Birthday { get; set; }
        public bool Admin { get; set; }
        public bool IsActive { get; set; }
    }
}
