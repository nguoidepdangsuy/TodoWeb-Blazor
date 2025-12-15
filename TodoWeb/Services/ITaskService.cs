using TodoWeb.Models;

namespace TodoWeb.Services
{
    public interface ITaskService
    {
        Task<List<WorkTask>> GetTasksAsync();
        Task<WorkTask?> GetTaskAsync(string id);
        Task<bool> AddTaskAsync(WorkTask task);
        Task<bool> UpdateTaskAsync(WorkTask task);
        Task<bool> DeleteTaskAsync(string id);
        Task<bool> ToggleTaskCompletionAsync(string id);
        Task<List<string>> GetDepartmentsAsync();
        Task CheckAndSendDeadlineReminders();
        Task SendDeadlineReminderEmail(WorkTask task);
        Task<List<WorkTask>> GetTasksForNotificationsAsync(string username);
        Task<List<WorkTask>> GetApproachingDeadlineTasksAsync(string username);
        Task<List<WorkTask>> GetOverdueTasksAsync(string username);
    }
}