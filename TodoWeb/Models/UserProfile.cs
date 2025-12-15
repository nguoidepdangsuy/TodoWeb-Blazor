namespace TodoWeb.Models
{
    public class UserProfile
    {
        public string Username { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Avatar { get; set; } = "https://via.placeholder.com/100";
    }
}