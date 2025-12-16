using System.Text.Json;
using Microsoft.JSInterop;
using TodoWeb.Models;
using System.IO;

namespace TodoWeb.Services
{
    public class SupabaseTaskSubmissionService : ITaskSubmissionService
    {
        private readonly SupabaseService _supabaseService;
        private readonly IConfiguration _configuration;
        private readonly IJSRuntime _jsRuntime;

        public SupabaseTaskSubmissionService(
            SupabaseService supabaseService,
            IConfiguration configuration,
            IJSRuntime jsRuntime)
        {
            _supabaseService = supabaseService;
            _configuration = configuration;
            _jsRuntime = jsRuntime;
        }

        public async Task<TaskSubmission?> GetSubmissionAsync(string id)
        {
            try
            {
                var submissions = await GetAllSubmissionsAsync();
                return submissions.FirstOrDefault(s => s.Id == id);
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<TaskSubmission>> GetSubmissionsByTaskAsync(string taskId)
        {
            try
            {
                var submissions = await GetAllSubmissionsAsync();
                return submissions.Where(s => s.TaskId == taskId).ToList();
            }
            catch
            {
                return new List<TaskSubmission>();
            }
        }

        public async Task<List<TaskSubmission>> GetSubmissionsByUserAsync(string username)
        {
            try
            {
                var submissions = await GetAllSubmissionsAsync();
                return submissions.Where(s => s.SubmittedBy == username).ToList();
            }
            catch
            {
                return new List<TaskSubmission>();
            }
        }

        public async Task<List<TaskSubmission>> GetRecentSubmissionsAsync(string creatorUsername)
        {
            try
            {
                // Lấy tasks được tạo bởi creator
                var tasksJson = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "tasks");
                if (string.IsNullOrEmpty(tasksJson)) return new List<TaskSubmission>();

                var tasks = JsonSerializer.Deserialize<List<WorkTask>>(tasksJson) ?? new List<WorkTask>();
                var creatorTaskIds = tasks.Where(t => t.CreatedBy == creatorUsername).Select(t => t.Id).ToHashSet();

                // Lấy submissions
                var submissions = await GetAllSubmissionsAsync();
                var recentSubmissions = submissions
                    .Where(s => creatorTaskIds.Contains(s.TaskId))
                    .OrderByDescending(s => s.SubmittedAt)
                    .Take(10)
                    .ToList();

                return recentSubmissions;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetRecentSubmissionsAsync: {ex.Message}");
                return new List<TaskSubmission>();
            }
        }

        public async Task<bool> CreateSubmissionAsync(TaskSubmission submission)
        {
            try
            {
                var submissions = await GetAllSubmissionsAsync();

                // Tạo ID mới nếu chưa có
                if (string.IsNullOrEmpty(submission.Id))
                {
                    submission.Id = Guid.NewGuid().ToString();
                }

                // Gán thời gian nếu chưa có
                if (submission.SubmittedAt == default)
                {
                    submission.SubmittedAt = DateTime.Now;
                }

                submissions.Add(submission);

                var submissionsJson = JsonSerializer.Serialize(submissions);
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "taskSubmissions", submissionsJson);

                return true;
            }
            catch (Exception ex)
            {
                await _jsRuntime.InvokeVoidAsync("console.error", $"Error creating submission: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateSubmissionAsync(TaskSubmission submission)
        {
            try
            {
                var submissions = await GetAllSubmissionsAsync();
                var existingSubmission = submissions.FirstOrDefault(s => s.Id == submission.Id);

                if (existingSubmission != null)
                {
                    submissions.Remove(existingSubmission);
                    submissions.Add(submission);

                    var submissionsJson = JsonSerializer.Serialize(submissions);
                    await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "taskSubmissions", submissionsJson);

                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteSubmissionAsync(string id)
        {
            try
            {
                var submissions = await GetAllSubmissionsAsync();
                var submission = submissions.FirstOrDefault(s => s.Id == id);

                if (submission != null)
                {
                    submissions.Remove(submission);
                    var submissionsJson = JsonSerializer.Serialize(submissions);
                    await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "taskSubmissions", submissionsJson);

                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> MarkSubmissionAsCompletedAsync(string submissionId)
        {
            try
            {
                var submissions = await GetAllSubmissionsAsync();
                var submission = submissions.FirstOrDefault(s => s.Id == submissionId);

                if (submission != null)
                {
                    submission.IsCompleted = true;
                    var submissionsJson = JsonSerializer.Serialize(submissions);
                    await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "taskSubmissions", submissionsJson);

                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<TaskSubmission>> GetSubmissionsAsync(string taskId)
        {
            return await GetSubmissionsByTaskAsync(taskId);
        }

        public async Task<bool> SubmitTaskAsync(TaskSubmission submission)
        {
            return await CreateSubmissionAsync(submission);
        }

        public async Task<bool> MarkAsCompletedAsync(string taskId)
        {
            // Tìm submission của task và đánh dấu hoàn thành
            var submissions = await GetSubmissionsByTaskAsync(taskId);
            if (submissions.Any())
            {
                var lastSubmission = submissions.Last();
                return await MarkSubmissionAsCompletedAsync(lastSubmission.Id);
            }
            return false;
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string taskId)
        {
            try
            {
                // Trong localStorage version, không thực sự upload file
                // Chỉ tạo URL giả lập
                var uniqueFileName = $"{taskId}/{Guid.NewGuid()}_{Path.GetFileName(fileName)}";
                var fakeUrl = $"uploads/{uniqueFileName}";

                // Đọc file stream để lấy kích thước
                using var memoryStream = new MemoryStream();
                await fileStream.CopyToAsync(memoryStream);
                var fileSize = memoryStream.Length;

                return fakeUrl;
            }
            catch (Exception ex)
            {
                await _jsRuntime.InvokeVoidAsync("console.error", $"Error uploading file: {ex.Message}");
                return string.Empty;
            }
        }

        // Lấy tất cả submissions
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