using TodoWeb.Models;

namespace TodoWeb.Services
{
    public interface IGroupService
    {
        Task<List<Group>> GetGroupsAsync();
        Task<Group?> GetGroupAsync(string id);
        Task<Group?> GetGroupByCodeAsync(string code);
        Task<bool> CreateGroupAsync(Group group);
        Task<bool> JoinGroupAsync(string code, string username);
        Task<bool> DeleteGroupAsync(string id);
        Task<List<Group>> GetUserGroupsAsync(string username);
        Task<List<Group>> GetCreatedGroupsAsync(string username);
        Task<bool> RemoveMemberFromGroupAsync(string groupId, string username);
    }
}