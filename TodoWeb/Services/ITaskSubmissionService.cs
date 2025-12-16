using TodoWeb.Models;
using System.IO;

namespace TodoWeb.Services
{
    public interface ITaskSubmissionService
    {
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
        Task<bool> MarkAsCompletedAsync(string taskId);
        Task<string> UploadFileAsync(Stream fileStream, string fileName, string taskId);
    }
}