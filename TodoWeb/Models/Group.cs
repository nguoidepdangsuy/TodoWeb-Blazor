using System.ComponentModel.DataAnnotations;

namespace TodoWeb.Models
{
    public class Group
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required(ErrorMessage = "Mã phòng là bắt buộc")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Mã phòng phải có 6 ký tự")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên phòng là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên phòng không được vượt quá 100 ký tự")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Người tạo là bắt buộc")]
        public string CreatorUsername { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public List<string> Members { get; set; } = new List<string>();
        public string CreatorName { get; set; } = string.Empty;
        public int MemberCount { get; set; }
        public int TaskCount { get; set; }
    }
}