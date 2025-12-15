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

        public async Task<List<WorkTask>> GetTasksAsync()
        {
            try
            {
                var tasksJson = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "tasks");

                // DEBUG: Kiểm tra dữ liệu raw từ localStorage
                await _jsRuntime.InvokeVoidAsync("console.log",
                    $"🔍 RAW TASKS JSON FROM LOCALSTORAGE: {tasksJson}");

                if (!string.IsNullOrEmpty(tasksJson))
                {
                    var tasks = JsonSerializer.Deserialize<List<WorkTask>>(tasksJson) ?? new List<WorkTask>();

                    // DEBUG: Log tất cả tasks sau khi deserialize
                    await _jsRuntime.InvokeVoidAsync("console.log",
                        $"🔍 TASKS AFTER DESERIALIZE: {tasks.Count} tasks");
                    foreach (var task in tasks)
                    {
                        await _jsRuntime.InvokeVoidAsync("console.log",
                            $"🔍 Task in storage: '{task.Title}' (ID: {task.Id}), Assignee: '{task.Assignee}'");
                    }

                    return tasks.OrderByDescending(t => t.AssignDate).ToList();
                }

                await _jsRuntime.InvokeVoidAsync("console.log", " NO TASKS FOUND IN LOCALSTORAGE");
                return new List<WorkTask>();
            }
            catch (Exception ex)
            {
                await _jsRuntime.InvokeVoidAsync("console.error", $" Error loading tasks: {ex.Message}");
                return new List<WorkTask>();
            }
        }

        public async Task<WorkTask?> GetTaskAsync(string id)
        {
            var tasks = await GetTasksAsync();
            return tasks.FirstOrDefault(t => t.Id == id);
        }

        public async Task<bool> AddTaskAsync(WorkTask task)
        {
            try
            {
                var tasks = await GetTasksAsync();
                var currentUser = await _authService.GetCurrentUserAsync();
                task.CreatedBy = currentUser?.Username ?? "Unknown";
                task.CreatedAt = DateTime.Now;

                tasks.Add(task);
                var tasksJson = JsonSerializer.Serialize(tasks);
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "tasks", tasksJson);

                await _jsRuntime.InvokeVoidAsync("alert", $"Đã giao task '{task.Title}' cho {task.Assignee}!");
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateTaskAsync(WorkTask task)
        {
            try
            {
                var tasks = await GetTasksAsync();
                var existingTask = tasks.FirstOrDefault(t => t.Id == task.Id);
                if (existingTask != null)
                {
                    tasks.Remove(existingTask);
                    tasks.Add(task);
                    var tasksJson = JsonSerializer.Serialize(tasks);
                    await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "tasks", tasksJson);
                    return true;
                }
            }
            catch
            {
                // Ignore errors
            }
            return false;
        }

        public async Task<bool> DeleteTaskAsync(string id)
        {
            try
            {
                var tasks = await GetTasksAsync();

                // DEBUG: Log trước khi xóa
                await _jsRuntime.InvokeVoidAsync("console.log",
                    $"🗑️ BEFORE DELETE - TOTAL TASKS: {tasks.Count}, DELETING TASK ID: {id}");

                var task = tasks.FirstOrDefault(t => t.Id == id);
                if (task != null)
                {
                    tasks.Remove(task);
                    var tasksJson = JsonSerializer.Serialize(tasks);

                    // DEBUG: Log sau khi xóa trong memory
                    await _jsRuntime.InvokeVoidAsync("console.log",
                        $"🗑️ AFTER DELETE IN MEMORY - REMAINING TASKS: {tasks.Count}");

                    await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "tasks", tasksJson);

                    // DEBUG: Kiểm tra lại localStorage sau khi lưu
                    var verifyJson = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "tasks");
                    var verifyTasks = JsonSerializer.Deserialize<List<WorkTask>>(verifyJson) ?? new List<WorkTask>();
                    await _jsRuntime.InvokeVoidAsync("console.log",
                        $"🗑️ VERIFY LOCALSTORAGE AFTER SAVE - TASKS: {verifyTasks.Count}");

                    await _jsRuntime.InvokeVoidAsync("console.log",
                        $" ĐÃ XÓA TASK: {task.Title} (ID: {task.Id})");

                    return true;
                }

                await _jsRuntime.InvokeVoidAsync("console.log", $" KHÔNG TÌM THẤY TASK ĐỂ XÓA: {id}");
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

                    var tasksJson = JsonSerializer.Serialize(tasks);
                    await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "tasks", tasksJson);

                    if (task.IsCompleted)
                    {
                        await _jsRuntime.InvokeVoidAsync("alert", $"Đã đánh dấu hoàn thành task '{task.Title}'!");
                    }
                    return true;
                }
            }
            catch
            {
                // Ignore errors
            }
            return false;
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
                "Quản Lý"
            });
        }

        public async Task SendDeadlineReminderEmail(WorkTask task)
        {
            try
            {
                var daysUntilDue = (task.DueDate - DateTime.Today).Days;
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

        public async Task CheckAndSendDeadlineReminders()
        {
            try
            {
                var tasks = await GetTasksAsync();
                var today = DateTime.Today;

                foreach (var task in tasks.Where(t => !t.IsCompleted))
                {
                    var daysUntilDue = (task.DueDate - today).Days;
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

        public async Task<List<WorkTask>> GetTasksForNotificationsAsync(string username)
        {
            var tasks = await GetTasksAsync();
            return tasks.Where(t => t.CreatedBy == username).ToList();
        }

        public async Task<List<WorkTask>> GetApproachingDeadlineTasksAsync(string username)
        {
            var tasks = await GetTasksAsync();
            var today = DateTime.Today;
            return tasks.Where(t =>
                !t.IsCompleted &&
                (t.DueDate - today).TotalDays <= 1 &&
                (t.DueDate - today).TotalDays >= 0 &&
                t.CreatedBy == username
            ).ToList();
        }

        public async Task<List<WorkTask>> GetOverdueTasksAsync(string username)
        {
            var tasks = await GetTasksAsync();
            var today = DateTime.Today;
            return tasks.Where(t =>
                !t.IsCompleted &&
                (t.DueDate - today).TotalDays < 0 &&
                t.CreatedBy == username
            ).ToList();
        }
    }
}