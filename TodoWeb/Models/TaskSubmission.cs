using System.ComponentModel.DataAnnotations;

namespace TodoWeb.Models
{
    public class TaskSubmission
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string TaskId { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public DateTime SubmittedAt { get; set; } = DateTime.Now;
        public string SubmittedBy { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
    }
}