using TodoWeb.Models;
using Microsoft.JSInterop;
using System.Text.Json;

namespace TodoWeb.Services
{
    public class GroupService : IGroupService
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly IAuthService _authService;

        public GroupService(IJSRuntime jsRuntime, IAuthService authService)
        {
            _jsRuntime = jsRuntime;
            _authService = authService;
        }

        // Lấy tất cả groups
        public async Task<List<Group>> GetGroupsAsync()
        {
            try
            {
                var groupsJson = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "groups");
                if (!string.IsNullOrEmpty(groupsJson))
                {
                    return JsonSerializer.Deserialize<List<Group>>(groupsJson) ?? new List<Group>();
                }
            }
            catch
            {
                // Ignore errors
            }
            return new List<Group>();
        }

        // Lấy group theo ID
        public async Task<Group?> GetGroupAsync(string id)
        {
            var groups = await GetGroupsAsync();
            return groups.FirstOrDefault(g => g.Id == id);
        }

        // Lấy group theo code
        public async Task<Group?> GetGroupByCodeAsync(string code)
        {
            var groups = await GetGroupsAsync();
            return groups.FirstOrDefault(g => g.Code == code);
        }

        // Cập nhật group
        public async Task<bool> UpdateGroupAsync(Group group)
        {
            try
            {
                var groups = await GetGroupsAsync();
                var existingGroup = groups.FirstOrDefault(g => g.Id == group.Id);

                if (existingGroup != null)
                {
                    // Cập nhật các thuộc tính
                    existingGroup.Name = group.Name;
                    existingGroup.Code = group.Code;
                    existingGroup.Members = group.Members;
                    existingGroup.UpdatedAt = DateTime.Now;

                    var groupsJson = JsonSerializer.Serialize(groups);
                    await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "groups", groupsJson);

                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        // Tạo group mới
        public async Task<bool> CreateGroupAsync(Group group)
        {
            try
            {
                var groups = await GetGroupsAsync();
                var currentUser = await _authService.GetCurrentUserAsync();

                // Gán thông tin người tạo
                group.CreatorUsername = currentUser?.Username ?? "Unknown";

                // Tạo mã code duy nhất nếu chưa có
                if (string.IsNullOrEmpty(group.Code))
                {
                    group.Code = await GenerateUniqueCodeAsync();
                }

                // Tạo ID mới nếu chưa có
                if (string.IsNullOrEmpty(group.Id))
                {
                    group.Id = Guid.NewGuid().ToString();
                }

                // Gán thời gian
                group.CreatedAt = DateTime.Now;
                group.UpdatedAt = DateTime.Now;

                // Thêm creator vào danh sách thành viên
                group.Members.Add(group.CreatorUsername);

                groups.Add(group);
                var groupsJson = JsonSerializer.Serialize(groups);
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "groups", groupsJson);

                return true;
            }
            catch
            {
                return false;
            }
        }

        // Tham gia group bằng code (JoinGroupByCodeAsync)
        public async Task<bool> JoinGroupByCodeAsync(string code, string username)
        {
            try
            {
                var groups = await GetGroupsAsync();
                var group = groups.FirstOrDefault(g => g.Code == code);

                if (group != null && !group.Members.Contains(username))
                {
                    group.Members.Add(username);
                    group.UpdatedAt = DateTime.Now;

                    var groupsJson = JsonSerializer.Serialize(groups);
                    await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "groups", groupsJson);

                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        // Phương thức tương thích JoinGroupAsync (alias)
        public async Task<bool> JoinGroupAsync(string code, string username)
        {
            return await JoinGroupByCodeAsync(code, username);
        }

        // Xóa group
        public async Task<bool> DeleteGroupAsync(string id)
        {
            try
            {
                var groups = await GetGroupsAsync();
                var group = groups.FirstOrDefault(g => g.Id == id);

                if (group != null)
                {
                    groups.Remove(group);
                    var groupsJson = JsonSerializer.Serialize(groups);
                    await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "groups", groupsJson);

                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        // Lấy groups của user
        public async Task<List<Group>> GetUserGroupsAsync(string username)
        {
            var groups = await GetGroupsAsync();
            return groups.Where(g => g.Members.Contains(username)).ToList();
        }

        // Lấy groups được tạo bởi user
        public async Task<List<Group>> GetCreatedGroupsAsync(string username)
        {
            var groups = await GetGroupsAsync();
            return groups.Where(g => g.CreatorUsername == username).ToList();
        }

        // Lấy thành viên trong group
        public async Task<List<GroupMember>> GetGroupMembersAsync(string groupId)
        {
            try
            {
                var group = await GetGroupAsync(groupId);
                if (group == null) return new List<GroupMember>();

                var members = new List<GroupMember>();
                foreach (var username in group.Members)
                {
                    var member = new GroupMember
                    {
                        Username = username,
                        GroupId = groupId,
                        IsCreator = username == group.CreatorUsername,
                        JoinedAt = group.CreatedAt
                    };
                    members.Add(member);
                }
                return members;
            }
            catch
            {
                return new List<GroupMember>();
            }
        }

        // Thêm thành viên vào group
        public async Task<bool> AddMemberToGroupAsync(string groupId, string username)
        {
            try
            {
                var groups = await GetGroupsAsync();
                var group = groups.FirstOrDefault(g => g.Id == groupId);

                if (group != null && !group.Members.Contains(username))
                {
                    group.Members.Add(username);
                    group.UpdatedAt = DateTime.Now;

                    var groupsJson = JsonSerializer.Serialize(groups);
                    await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "groups", groupsJson);

                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        // Xóa thành viên khỏi group
        public async Task<bool> RemoveMemberFromGroupAsync(string groupId, string username)
        {
            try
            {
                var groups = await GetGroupsAsync();
                var group = groups.FirstOrDefault(g => g.Id == groupId);

                if (group != null && group.Members.Contains(username) && username != group.CreatorUsername)
                {
                    group.Members.Remove(username);
                    group.UpdatedAt = DateTime.Now;

                    var groupsJson = JsonSerializer.Serialize(groups);
                    await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "groups", groupsJson);

                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        // Tạo mã code duy nhất
        private async Task<string> GenerateUniqueCodeAsync()
        {
            var groups = await GetGroupsAsync();
            var existingCodes = groups.Select(g => g.Code).ToHashSet();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();

            string code;
            do
            {
                code = new string(Enumerable.Repeat(chars, 6)
                    .Select(s => s[random.Next(s.Length)]).ToArray());
            } while (existingCodes.Contains(code));

            return code;
        }
    }
}