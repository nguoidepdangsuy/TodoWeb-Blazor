using TodoWeb.Models;
using Microsoft.JSInterop;
using System.Text.Json;

namespace TodoWeb.Services
{
    public class TaskSubmissionService : ITaskSubmissionService
    {
        private readonly IJSRuntime _jsRuntime;

        public TaskSubmissionService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task<List<TaskSubmission>> GetSubmissionsAsync(string taskId)
        {
            try
            {
                var submissionsJson = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "taskSubmissions");
                if (!string.IsNullOrEmpty(submissionsJson))
                {
                    var allSubmissions = JsonSerializer.Deserialize<List<TaskSubmission>>(submissionsJson) ?? new List<TaskSubmission>();
                    return allSubmissions.Where(s => s.TaskId == taskId).ToList();
                }
            }
            catch
            {
                // Ignore errors
            }
            return new List<TaskSubmission>();
        }

        public async Task<List<TaskSubmission>> GetRecentSubmissionsAsync(string creatorUsername)
        {
            try
            {
                var submissionsJson = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "taskSubmissions");
                if (!string.IsNullOrEmpty(submissionsJson))
                {
                    var allSubmissions = JsonSerializer.Deserialize<List<TaskSubmission>>(submissionsJson) ?? new List<TaskSubmission>();

                    // Lấy tasks của creator để filter submissions
                    var tasksJson = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "tasks");
                    if (!string.IsNullOrEmpty(tasksJson))
                    {
                        var tasks = JsonSerializer.Deserialize<List<WorkTask>>(tasksJson) ?? new List<WorkTask>();
                        var creatorTaskIds = tasks.Where(t => t.CreatedBy == creatorUsername).Select(t => t.Id).ToHashSet();

                        var recentSubmissions = allSubmissions
                            .Where(s => creatorTaskIds.Contains(s.TaskId))
                            .OrderByDescending(s => s.SubmittedAt)
                            .Take(10)
                            .ToList();

                        Console.WriteLine($"Found {recentSubmissions.Count} recent submissions for creator {creatorUsername}");
                        return recentSubmissions;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetRecentSubmissionsAsync: {ex.Message}");
            }

            Console.WriteLine("No submissions found or error occurred");
            return new List<TaskSubmission>();
        }

        public async Task<bool> SubmitTaskAsync(TaskSubmission submission)
        {
            try
            {
                var submissions = await GetAllSubmissionsAsync();
                submissions.Add(submission);
                var submissionsJson = JsonSerializer.Serialize(submissions);
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "taskSubmissions", submissionsJson);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteSubmissionAsync(string submissionId)
        {
            try
            {
                var submissions = await GetAllSubmissionsAsync();
                var submission = submissions.FirstOrDefault(s => s.Id == submissionId);
                if (submission != null)
                {
                    submissions.Remove(submission);
                    var submissionsJson = JsonSerializer.Serialize(submissions);
                    await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "taskSubmissions", submissionsJson);
                    return true;
                }
            }
            catch
            {
                // Ignore errors
            }
            return false;
        }

        public async Task<bool> MarkAsCompletedAsync(string taskId)
        {
            await Task.Delay(100); // Simulate async operation
            return true;
        }

        private async Task<List<TaskSubmission>> GetAllSubmissionsAsync()
        {
            try
            {
                var submissionsJson = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "taskSubmissions");
                if (!string.IsNullOrEmpty(submissionsJson))
                {
                    return JsonSerializer.Deserialize<List<TaskSubmission>>(submissionsJson) ?? new List<TaskSubmission>();
                }
            }
            catch
            {
                // Ignore errors
            }
            return new List<TaskSubmission>();
        }
    }
}