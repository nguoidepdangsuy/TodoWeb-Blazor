using System.ComponentModel.DataAnnotations;

namespace TodoWeb.Models
{
    public class WorkTask
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required(ErrorMessage = "Tiêu đề task là bắt buộc")]
        [StringLength(200, ErrorMessage = "Tiêu đề không được vượt quá 200 ký tự")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Người được giao là bắt buộc")]
        public string AssigneeUsername { get; set; } = string.Empty;

        public string Department { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ngày giao là bắt buộc")]
        public DateTime AssignDate { get; set; }

        [Required(ErrorMessage = "Hạn hoàn thành là bắt buộc")]
        public DateTime DueDate { get; set; }

        public bool IsCompleted { get; set; }
        public DateTime? CompletedDate { get; set; }

        [Required(ErrorMessage = "Người tạo task là bắt buộc")]
        public string CreatedBy { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phòng là bắt buộc")]
        public string GroupId { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public string GroupName { get; set; } = string.Empty;
        public string AssigneeName { get; set; } = string.Empty;
        public string CreatorName { get; set; } = string.Empty;
        public bool IsOverdue => !IsCompleted && DueDate.Date < DateTime.Today;
        public bool IsApproachingDeadline => !IsCompleted &&
            (DueDate.Date - DateTime.Today).TotalDays <= 1 &&
            (DueDate.Date - DateTime.Today).TotalDays >= 0;
        public int DaysUntilDue => (int)(DueDate.Date - DateTime.Today).TotalDays;
    }
}