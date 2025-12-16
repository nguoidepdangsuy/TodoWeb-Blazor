using System.Text.Json;
using Microsoft.JSInterop;
using TodoWeb.Models;

namespace TodoWeb.Services
{
    public class SupabaseTaskService : ITaskService
    {
        private readonly SupabaseService _supabaseService;
        private readonly IAuthService _authService;
        private readonly IJSRuntime _jsRuntime;

        public SupabaseTaskService(
            SupabaseService supabaseService,
            IAuthService authService,
            IJSRuntime jsRuntime)
        {
            _supabaseService = supabaseService;
            _authService = authService;
            _jsRuntime = jsRuntime;
        }

        public async Task<List<WorkTask>> GetTasksAsync()
        {
            try
            {
                var tasksJson = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "tasks");
                if (!string.IsNullOrEmpty(tasksJson))
                {
                    var tasks = JsonSerializer.Deserialize<List<WorkTask>>(tasksJson) ?? new List<WorkTask>();
                    return tasks.OrderByDescending(t => t.AssignDate).ToList();
                }
                return new List<WorkTask>();
            }
            catch (Exception ex)
            {
                await _jsRuntime.InvokeVoidAsync("console.error", $"Error loading tasks: {ex.Message}");
                return new List<WorkTask>();
            }
        }

        public async Task<WorkTask?> GetTaskAsync(string id)
        {
            var tasks = await GetTasksAsync();
            return tasks.FirstOrDefault(t => t.Id == id);
        }

        public async Task<List<WorkTask>> GetTasksByAssigneeAsync(string username)
        {
            try
            {
                var tasks = await GetTasksAsync();
                return tasks.Where(t => t.AssigneeUsername == username).ToList();
            }
            catch (Exception ex)
            {
                await _jsRuntime.InvokeVoidAsync("console.error", $"Error loading tasks for assignee {username}: {ex.Message}");
                return new List<WorkTask>();
            }
        }

        public async Task<List<WorkTask>> GetTasksByCreatorAsync(string username)
        {
            try
            {
                var tasks = await GetTasksAsync();
                return tasks.Where(t => t.CreatedBy == username).ToList();
            }
            catch (Exception ex)
            {
                await _jsRuntime.InvokeVoidAsync("console.error", $"Error loading tasks for creator {username}: {ex.Message}");
                return new List<WorkTask>();
            }
        }

        public async Task<List<WorkTask>> GetTasksByGroupAsync(string groupId)
        {
            try
            {
                var tasks = await GetTasksAsync();
                return tasks.Where(t => t.GroupId == groupId).ToList();
            }
            catch (Exception ex)
            {
                await _jsRuntime.InvokeVoidAsync("console.error", $"Error loading tasks for group {groupId}: {ex.Message}");
                return new List<WorkTask>();
            }
        }

        public async Task<bool> CreateTaskAsync(WorkTask task)
        {
            try
            {
                var tasks = await GetTasksAsync();
                var currentUser = await _authService.GetCurrentUserAsync();

                // Gán thông tin người tạo nếu chưa có
                if (string.IsNullOrEmpty(task.CreatedBy) && currentUser != null)
                {
                    task.CreatedBy = currentUser.Username;
                }

                // Gán thời gian tạo
                task.CreatedAt = DateTime.Now;
                task.UpdatedAt = DateTime.Now;

                // Tạo ID mới nếu chưa có
                if (string.IsNullOrEmpty(task.Id))
                {
                    task.Id = Guid.NewGuid().ToString();
                }

                tasks.Add(task);

                var tasksJson = JsonSerializer.Serialize(tasks);
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "tasks", tasksJson);

                await _jsRuntime.InvokeVoidAsync("alert", $"Đã giao task '{task.Title}' cho {task.AssigneeUsername}!");

                return true;
            }
            catch (Exception ex)
            {
                await _jsRuntime.InvokeVoidAsync("console.error", $"Error creating task: {ex.Message}");
                await _jsRuntime.InvokeVoidAsync("alert", "Lỗi khi tạo task!");
                return false;
            }
        }

        public async Task<bool> AddTaskAsync(WorkTask task)
        {
            return await CreateTaskAsync(task);
        }

        public async Task<bool> UpdateTaskAsync(WorkTask task)
        {
            try
            {
                var tasks = await GetTasksAsync();
                var existingTask = tasks.FirstOrDefault(t => t.Id == task.Id);

                if (existingTask != null)
                {
                    // Cập nhật các thuộc tính
                    existingTask.Title = task.Title;
                    existingTask.Description = task.Description;
                    existingTask.AssignDate = task.AssignDate;
                    existingTask.DueDate = task.DueDate;
                    existingTask.IsCompleted = task.IsCompleted;
                    existingTask.CompletedDate = task.CompletedDate;
                    existingTask.UpdatedAt = DateTime.Now;

                    // Có thể cập nhật người được giao nếu cần
                    if (!string.IsNullOrEmpty(task.AssigneeUsername))
                    {
                        existingTask.AssigneeUsername = task.AssigneeUsername;
                    }

                    var tasksJson = JsonSerializer.Serialize(tasks);
                    await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "tasks", tasksJson);

                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                await _jsRuntime.InvokeVoidAsync("console.error", $"Error updating task: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteTaskAsync(string id)
        {
            try
            {
                var tasks = await GetTasksAsync();
                var task = tasks.FirstOrDefault(t => t.Id == id);

                if (task != null)
                {
                    tasks.Remove(task);
                    var tasksJson = JsonSerializer.Serialize(tasks);
                    await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "tasks", tasksJson);

                    await _jsRuntime.InvokeVoidAsync("alert", "Đã xóa task thành công!");
                    return true;
                }

                await _jsRuntime.InvokeVoidAsync("console.log", $"Không tìm thấy task để xóa: {id}");
                return false;
            }
            catch (Exception ex)
            {
                await _jsRuntime.InvokeVoidAsync("console.error", $"Lỗi khi xóa task: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ToggleTaskCompletionAsync(string id)
        {
            try
            {
                var tasks = await GetTasksAsync();
                var task = tasks.FirstOrDefault(t => t.Id == id);

                if (task != null)
                {
                    task.IsCompleted = !task.IsCompleted;
                    task.CompletedDate = task.IsCompleted ? DateTime.Now : null;
                    task.UpdatedAt = DateTime.Now;

                    var tasksJson = JsonSerializer.Serialize(tasks);
                    await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "tasks", tasksJson);

                    if (task.IsCompleted)
                    {
                        await _jsRuntime.InvokeVoidAsync("alert", $"Đã đánh dấu hoàn thành task '{task.Title}'!");
                    }
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                await _jsRuntime.InvokeVoidAsync("console.error", $"Error toggling task completion: {ex.Message}");
                return false;
            }
        }

        public async Task<List<string>> GetDepartmentsAsync()
        {
            return await Task.FromResult(new List<string>
            {
                "Kế Toán",
                "Marketing",
                "IT",
                "Nhân Sự",
                "Kinh Doanh",
                "Sản Xuất",
                "Quản Lý",
                "Nhân viên",
                "Kỹ thuật",
                "Thiết kế",
                "Phát triển"
            });
        }

        public async Task<List<WorkTask>> GetOverdueTasksAsync(string username)
        {
            var tasks = await GetTasksAsync();
            var today = DateTime.Today;

            return tasks.Where(t =>
                !t.IsCompleted &&
                (t.DueDate.Date - today).TotalDays < 0 &&
                (t.CreatedBy == username || t.AssigneeUsername == username)
            ).ToList();
        }

        public async Task<List<WorkTask>> GetApproachingDeadlineTasksAsync(string username)
        {
            var tasks = await GetTasksAsync();
            var today = DateTime.Today;

            return tasks.Where(t =>
                !t.IsCompleted &&
                (t.DueDate.Date - today).TotalDays <= 1 &&
                (t.DueDate.Date - today).TotalDays >= 0 &&
                (t.CreatedBy == username || t.AssigneeUsername == username)
            ).ToList();
        }
    }
}