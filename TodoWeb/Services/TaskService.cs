using TodoWeb.Models;
using Microsoft.JSInterop;
using System.Text.Json;

namespace TodoWeb.Services
{
    public class TaskService : ITaskService
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly IAuthService _authService;

        public TaskService(IJSRuntime jsRuntime, IAuthService authService)
        {
            _jsRuntime = jsRuntime;
            _authService = authService;
        }

        // Lấy tất cả tasks (tương thích với code cũ)
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

        // Lấy task theo ID
        public async Task<WorkTask?> GetTaskAsync(string id)
        {
            var tasks = await GetTasksAsync();
            return tasks.FirstOrDefault(t => t.Id == id);
        }

        // Lấy tasks theo người được giao
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

        // Lấy tasks theo người tạo
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

        // Lấy tasks theo phòng
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

        // Lấy tasks sắp hết hạn (cho user cụ thể)
        public async Task<List<WorkTask>> GetApproachingDeadlineTasksAsync(string username)
        {
            try
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
            catch (Exception ex)
            {
                await _jsRuntime.InvokeVoidAsync("console.error", $"Error loading approaching deadline tasks: {ex.Message}");
                return new List<WorkTask>();
            }
        }

        // Lấy tasks đã quá hạn (cho user cụ thể)
        public async Task<List<WorkTask>> GetOverdueTasksAsync(string username)
        {
            try
            {
                var tasks = await GetTasksAsync();
                var today = DateTime.Today;

                return tasks.Where(t =>
                    !t.IsCompleted &&
                    (t.DueDate.Date - today).TotalDays < 0 &&
                    (t.CreatedBy == username || t.AssigneeUsername == username)
                ).ToList();
            }
            catch (Exception ex)
            {
                await _jsRuntime.InvokeVoidAsync("console.error", $"Error loading overdue tasks: {ex.Message}");
                return new List<WorkTask>();
            }
        }

        // Tạo task mới
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

        // Phương thức tương thích AddTaskAsync
        public async Task<bool> AddTaskAsync(WorkTask task)
        {
            return await CreateTaskAsync(task);
        }

        // Cập nhật task
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

        // Xóa task
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

        // Chuyển đổi trạng thái hoàn thành
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

        // Lấy danh sách phòng ban
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

        // Gửi thông báo hạn deadline
        public async Task SendDeadlineReminderEmail(WorkTask task)
        {
            try
            {
                var daysUntilDue = (task.DueDate.Date - DateTime.Today).Days;
                var urgencyText = daysUntilDue == 0 ? "HÔM NAY" : $"{daysUntilDue} NGÀY";

                if (daysUntilDue <= 1)
                {
                    await _jsRuntime.InvokeVoidAsync("alert",
                        $"⚠️ Công việc '{task.Title}' sắp hết hạn ({urgencyText})!");
                }
            }
            catch (Exception ex)
            {
                await _jsRuntime.InvokeVoidAsync("console.error", $"Failed to send reminder: {ex.Message}");
            }
        }

        // Kiểm tra và gửi thông báo hạn deadline
        public async Task CheckAndSendDeadlineReminders()
        {
            try
            {
                var tasks = await GetTasksAsync();
                var today = DateTime.Today;

                foreach (var task in tasks.Where(t => !t.IsCompleted))
                {
                    var daysUntilDue = (task.DueDate.Date - today).Days;
                    if (daysUntilDue <= 1 && daysUntilDue >= 0)
                    {
                        await SendDeadlineReminderEmail(task);
                    }
                }
            }
            catch (Exception ex)
            {
                await _jsRuntime.InvokeVoidAsync("console.error", $"Failed to check deadlines: {ex.Message}");
            }
        }

        // Lấy tasks cho notifications (cho creator)
        public async Task<List<WorkTask>> GetTasksForNotificationsAsync(string username)
        {
            var tasks = await GetTasksAsync();
            return tasks.Where(t => t.CreatedBy == username).ToList();
        }
    }
}