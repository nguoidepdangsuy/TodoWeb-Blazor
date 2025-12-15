namespace TodoWeb.Models
{
    public class GroupMember
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string GroupId { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public DateTime JoinedAt { get; set; } = DateTime.Now;

        // Thuộc tính tiện ích
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Avatar { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public bool IsCreator { get; set; }
        public int TaskCount { get; set; }
        public int CompletedTaskCount { get; set; }

        public double CompletionRate => TaskCount > 0 ?
            Math.Round((CompletedTaskCount * 100.0 / TaskCount), 1) : 0;
    }
}