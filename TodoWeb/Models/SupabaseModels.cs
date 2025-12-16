using System;
using System.ComponentModel.DataAnnotations;

namespace TodoWeb.Models
{
    // Tạm thời comment out Supabase attributes
    // [Table("profiles")]
    public class Profile // : BaseModel
    {
        // [PrimaryKey("id", false)]
        public string Id { get; set; } = string.Empty;

        // [Column("username")]
        public string Username { get; set; } = string.Empty;

        // [Column("full_name")]
        public string? FullName { get; set; }

        // [Column("email")]
        public string Email { get; set; } = string.Empty;

        // [Column("phone")]
        public string? Phone { get; set; }

        // [Column("department")]
        public string Department { get; set; } = "Nhân viên";

        // [Column("avatar")]
        public string Avatar { get; set; } = "https://via.placeholder.com/150/4285f4/ffffff?text=U";

        // [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }

    // [Table("groups")]
    public class SupabaseGroup // : BaseModel
    {
        // [PrimaryKey("id", false)]
        public string Id { get; set; } = string.Empty;

        // [Column("code")]
        public string Code { get; set; } = string.Empty;

        // [Column("name")]
        public string Name { get; set; } = string.Empty;

        // [Column("creator_id")]
        public string CreatorId { get; set; } = string.Empty;

        // [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }

    // [Table("group_members")]
    public class SupabaseGroupMember // : BaseModel
    {
        // [PrimaryKey("id", false)]
        public string Id { get; set; } = string.Empty;

        // [Column("group_id")]
        public string GroupId { get; set; } = string.Empty;

        // [Column("user_id")]
        public string UserId { get; set; } = string.Empty;

        // [Column("joined_at")]
        public DateTime JoinedAt { get; set; } = DateTime.Now;
    }

    // [Table("tasks")]
    public class SupabaseTask // : BaseModel
    {
        // [PrimaryKey("id", false)]
        public string Id { get; set; } = string.Empty;

        // [Column("title")]
        public string Title { get; set; } = string.Empty;

        // [Column("assignee_id")]
        public string? AssigneeId { get; set; }

        // [Column("department")]
        public string? Department { get; set; }

        // [Column("description")]
        public string? Description { get; set; }

        // [Column("assign_date")]
        public DateTime AssignDate { get; set; }

        // [Column("due_date")]
        public DateTime DueDate { get; set; }

        // [Column("is_completed")]
        public bool IsCompleted { get; set; } = false;

        // [Column("completed_date")]
        public DateTime? CompletedDate { get; set; }

        // [Column("created_by")]
        public string CreatedBy { get; set; } = string.Empty;

        // [Column("group_id")]
        public string GroupId { get; set; } = string.Empty;

        // [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }

    // [Table("task_submissions")]
    public class SupabaseTaskSubmission // : BaseModel
    {
        // [PrimaryKey("id", false)]
        public string Id { get; set; } = string.Empty;

        // [Column("task_id")]
        public string TaskId { get; set; } = string.Empty;

        // [Column("file_name")]
        public string FileName { get; set; } = string.Empty;

        // [Column("file_url")]
        public string FileUrl { get; set; } = string.Empty;

        // [Column("file_size")]
        public long FileSize { get; set; }

        // [Column("submitted_by")]
        public string SubmittedBy { get; set; } = string.Empty;

        // [Column("submitted_at")]
        public DateTime SubmittedAt { get; set; } = DateTime.Now;

        // [Column("is_completed")]
        public bool IsCompleted { get; set; } = false;
    }
}