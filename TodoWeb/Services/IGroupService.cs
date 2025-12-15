using TodoWeb.Models;

namespace TodoWeb.Services
{
    public interface IGroupService
    {
        Task<List<Group>> GetGroupsAsync();
        Task<Group?> GetGroupAsync(string id);
        Task<Group?> GetGroupByCodeAsync(string code);

        // Thêm phương thức còn thiếu
        Task<bool> UpdateGroupAsync(Group group);

        Task<bool> CreateGroupAsync(Group group);
        Task<bool> JoinGroupAsync(string code, string username); 
        Task<bool> DeleteGroupAsync(string id);
        Task<List<Group>> GetUserGroupsAsync(string username);
        Task<List<Group>> GetCreatedGroupsAsync(string username);
        Task<List<GroupMember>> GetGroupMembersAsync(string groupId);
        Task<bool> AddMemberToGroupAsync(string groupId, string username);
        Task<bool> RemoveMemberFromGroupAsync(string groupId, string username);
        Task<bool> JoinGroupByCodeAsync(string code, string username);
    }
}