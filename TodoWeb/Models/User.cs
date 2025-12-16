using System.ComponentModel.DataAnnotations;

namespace TodoWeb.Models
{
    public class User
    {
        [Required(ErrorMessage = "Tên đăng nhập là bắt buộc")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Tên đăng nhập phải từ 3-50 ký tự")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        public string Password { get; set; } = string.Empty;

        public bool IsAuthenticated { get; set; }
        public UserType? UserType { get; set; }

        // Thông tin bổ sung
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string? Department { get; set; }
        public string? Avatar { get; set; } = "https://via.placeholder.com/100";
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // ID từ Supabase
        public string? Id { get; set; }

        // Thống kê
        public int CreatedGroupCount { get; set; }
        public int JoinedGroupCount { get; set; }
        public int AssignedTaskCount { get; set; }
        public int CompletedTaskCount { get; set; }
    }
}