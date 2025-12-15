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
        Task<List<WorkTask>> GetTasksAsync(); 

        Task<bool> AddTaskAsync(WorkTask task); 
    }
}