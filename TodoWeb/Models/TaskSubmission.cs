using System.ComponentModel.DataAnnotations;

namespace TodoWeb.Models
{
    public class TaskSubmission
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required(ErrorMessage = "Task ID là bắt buộc")]
        public string TaskId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên file là bắt buộc")]
        [StringLength(255, ErrorMessage = "Tên file không được vượt quá 255 ký tự")]
        public string FileName { get; set; } = string.Empty;

        [Required(ErrorMessage = "URL file là bắt buộc")]
        [Url(ErrorMessage = "URL không hợp lệ")]
        public string FileUrl { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kích thước file là bắt buộc")]
        [Range(1, long.MaxValue, ErrorMessage = "Kích thước file phải lớn hơn 0")]
        public long FileSize { get; set; }

        [Required(ErrorMessage = "Người nộp là bắt buộc")]
        public string SubmittedBy { get; set; } = string.Empty;

        public DateTime SubmittedAt { get; set; } = DateTime.Now;
        public bool IsCompleted { get; set; }

        // Thuộc tính tiện ích
        public string SubmittedByName { get; set; } = string.Empty;
        public string TaskTitle { get; set; } = string.Empty;
        public string GroupId { get; set; } = string.Empty;

        // Định dạng kích thước file
        public string FormattedFileSize
        {
            get
            {
                string[] sizes = { "B", "KB", "MB", "GB" };
                int order = 0;
                double len = FileSize;

                while (len >= 1024 && order < sizes.Length - 1)
                {
                    order++;
                    len = len / 1024;
                }

                return $"{len:0.##} {sizes[order]}";
            }
        }

        // Kiểm tra định dạng file
        public string FileExtension => Path.GetExtension(FileName).ToLower();

        public bool IsImageFile => new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" }.Contains(FileExtension);
        public bool IsDocumentFile => new[] { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx" }.Contains(FileExtension);
        public bool IsArchiveFile => new[] { ".zip", ".rar", ".7z" }.Contains(FileExtension);
    }
}