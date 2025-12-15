using TodoWeb.Models;

namespace TodoWeb.Services
{
    public interface ITaskSubmissionService
    {
        Task<List<TaskSubmission>> GetSubmissionsAsync(string taskId);
        Task<List<TaskSubmission>> GetRecentSubmissionsAsync(string creatorUsername);
        Task<bool> SubmitTaskAsync(TaskSubmission submission);
        Task<bool> MarkAsCompletedAsync(string taskId);
        Task<bool> DeleteSubmissionAsync(string submissionId);
    }
}