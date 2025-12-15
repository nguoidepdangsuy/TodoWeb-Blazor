using System.ComponentModel.DataAnnotations;

namespace TodoWeb.Models
{
    public class UserProfile
    {
        [Required(ErrorMessage = "Tên đăng nhập là bắt buộc")]
        public string Username { get; set; } = string.Empty;

        public string? FullName { get; set; }

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string Phone { get; set; } = string.Empty;

        public string Department { get; set; } = string.Empty;

        [Url(ErrorMessage = "URL avatar không hợp lệ")]
        public string Avatar { get; set; } = "https://via.placeholder.com/100";

        // Thống kê
        public int CreatedGroupCount { get; set; }
        public int JoinedGroupCount { get; set; }
        public int AssignedTaskCount { get; set; }
        public int CompletedTaskCount { get; set; }

        public double CompletionRate => AssignedTaskCount > 0 ?
            Math.Round((CompletedTaskCount * 100.0 / AssignedTaskCount), 1) : 0;

        public DateTime MemberSince { get; set; } = DateTime.Now;
    }
}