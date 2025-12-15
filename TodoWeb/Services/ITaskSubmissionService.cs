using TodoWeb.Models;

namespace TodoWeb.Services
{
    public interface ITaskSubmissionService
    {
        // Các phương thức mới theo model mới
        Task<TaskSubmission?> GetSubmissionAsync(string id);
        Task<List<TaskSubmission>> GetSubmissionsByTaskAsync(string taskId);
        Task<List<TaskSubmission>> GetSubmissionsByUserAsync(string username);
        Task<List<TaskSubmission>> GetRecentSubmissionsAsync(string creatorUsername);
        Task<bool> CreateSubmissionAsync(TaskSubmission submission);
        Task<bool> UpdateSubmissionAsync(TaskSubmission submission);
        Task<bool> DeleteSubmissionAsync(string id);
        Task<bool> MarkSubmissionAsCompletedAsync(string submissionId);
        Task<List<TaskSubmission>> GetSubmissionsAsync(string taskId) =>
            GetSubmissionsByTaskAsync(taskId);
        Task<bool> SubmitTaskAsync(TaskSubmission submission) =>
            CreateSubmissionAsync(submission);
    }
}