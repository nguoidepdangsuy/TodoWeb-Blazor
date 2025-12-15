using TodoWeb.Models;

namespace TodoWeb.Services
{
    public interface ITaskService
    {
        // Các phương thức mới
        Task<WorkTask?> GetTaskAsync(string id);
        Task<List<WorkTask>> GetTasksByGroupAsync(string groupId);
        Task<List<WorkTask>> GetTasksByAssigneeAsync(string username);
        Task<List<WorkTask>> GetTasksByCreatorAsync(string username);
        Task<List<WorkTask>> GetOverdueTasksAsync(string username);
        Task<List<WorkTask>> GetApproachingDeadlineTasksAsync(string username);
        Task<bool> CreateTaskAsync(WorkTask task);
        Task<bool> UpdateTaskAsync(WorkTask task);
        Task<bool> DeleteTaskAsync(string id);
        Task<bool> ToggleTaskCompletionAsync(string id);
        Task<List<string>> GetDepartmentsAsync();

        // Giữ lại phương thức cũ cho tương thích
        Task<List<WorkTask>> GetTasksAsync(); // Sửa: bỏ context user

        Task<bool> AddTaskAsync(WorkTask task); // Sửa: không gọi CreateTaskAsync trực tiếp
    }
}