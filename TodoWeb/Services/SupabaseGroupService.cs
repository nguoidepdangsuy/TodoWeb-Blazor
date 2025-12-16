using System.Text.Json;
using Microsoft.JSInterop;
using TodoWeb.Models;

namespace TodoWeb.Services
{
    public class SupabaseGroupService : IGroupService
    {
        private readonly SupabaseService _supabaseService;
        private readonly IAuthService _authService;
        private readonly IJSRuntime _jsRuntime;

        public SupabaseGroupService(
            SupabaseService supabaseService,
            IAuthService authService,
            IJSRuntime jsRuntime)
        {
            _supabaseService = supabaseService;
            _authService = authService;
            _jsRuntime = jsRuntime;
        }

        public async Task<List<Group>> GetGroupsAsync()
        {
            try
            {
                var groupsJson = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "groups");
                if (!string.IsNullOrEmpty(groupsJson))
                {
                    return JsonSerializer.Deserialize<List<Group>>(groupsJson) ?? new List<Group>();
                }
                return new List<Group>();
            }
            catch (Exception ex)
            {
                await _jsRuntime.InvokeVoidAsync("console.error", $"Error getting groups: {ex.Message}");
                return new List<Group>();
            }
        }

        public async Task<Group?> GetGroupAsync(string id)
        {
            var groups = await GetGroupsAsync();
            return groups.FirstOrDefault(g => g.Id == id);
        }

        public async Task<Group?> GetGroupByCodeAsync(string code)
        {
            var groups = await GetGroupsAsync();
            return groups.FirstOrDefault(g => g.Code == code);
        }

        public async Task<bool> CreateGroupAsync(Group group)
        {
            try
            {
                var groups = await GetGroupsAsync();
                var currentUser = await _authService.GetCurrentUserAsync();

                if (currentUser == null) return false;

                // Gán thông tin người tạo
                group.CreatorUsername = currentUser.Username;
                group.CreatorName = currentUser.FullName ?? currentUser.Username;

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

                // Loại bỏ trùng lặp trong members và thêm creator
                if (group.Members == null)
                    group.Members = new List<string>();

                group.Members = group.Members
                    .Where(m => !string.IsNullOrEmpty(m))
                    .Distinct()
                    .ToList();

                // Thêm creator vào danh sách thành viên nếu chưa có
                if (!group.Members.Contains(group.CreatorUsername))
                {
                    group.Members.Add(group.CreatorUsername);
                }

                groups.Add(group);
                var groupsJson = JsonSerializer.Serialize(groups);
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "groups", groupsJson);

                return true;
            }
            catch (Exception ex)
            {
                await _jsRuntime.InvokeVoidAsync("console.error", $"Error creating group: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> JoinGroupByCodeAsync(string code, string username)
        {
            try
            {
                var groups = await GetGroupsAsync();
                var group = groups.FirstOrDefault(g => g.Code == code);

                if (group != null)
                {
                    // Loại bỏ trùng lặp và thêm thành viên
                    if (group.Members == null)
                        group.Members = new List<string>();

                    var members = group.Members
                        .Where(m => !string.IsNullOrEmpty(m))
                        .Distinct()
                        .ToList();

                    if (!members.Contains(username))
                    {
                        members.Add(username);
                        group.Members = members;
                        group.UpdatedAt = DateTime.Now;

                        var groupsJson = JsonSerializer.Serialize(groups);
                        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "groups", groupsJson);

                        return true;
                    }
                    // Nếu đã là thành viên, vẫn trả về true
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                await _jsRuntime.InvokeVoidAsync("console.error", $"Error joining group: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> JoinGroupAsync(string code, string username)
        {
            return await JoinGroupByCodeAsync(code, username);
        }

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
            catch (Exception ex)
            {
                await _jsRuntime.InvokeVoidAsync("console.error", $"Error deleting group: {ex.Message}");
                return false;
            }
        }

        public async Task<List<Group>> GetUserGroupsAsync(string username)
        {
            var groups = await GetGroupsAsync();
            return groups
                .Where(g => g.Members != null && g.Members.Any(m => m == username))
                .ToList();
        }

        public async Task<List<Group>> GetCreatedGroupsAsync(string username)
        {
            var groups = await GetGroupsAsync();
            return groups.Where(g => g.CreatorUsername == username).ToList();
        }

        public async Task<List<GroupMember>> GetGroupMembersAsync(string groupId)
        {
            try
            {
                var group = await GetGroupAsync(groupId);
                if (group == null) return new List<GroupMember>();

                var members = new List<GroupMember>();
                if (group.Members != null)
                {
                    var uniqueUsernames = group.Members
                        .Where(m => !string.IsNullOrEmpty(m))
                        .Distinct();

                    foreach (var username in uniqueUsernames)
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
                }
                return members;
            }
            catch
            {
                return new List<GroupMember>();
            }
        }

        public async Task<bool> AddMemberToGroupAsync(string groupId, string userId)
        {
            // Chuyển userId thành username (tạm thời)
            var groups = await GetGroupsAsync();
            var group = groups.FirstOrDefault(g => g.Id == groupId);

            if (group != null)
            {
                if (group.Members == null)
                    group.Members = new List<string>();

                // Tìm username từ userId (giả sử userId là username)
                var username = userId;

                if (!group.Members.Contains(username))
                {
                    group.Members.Add(username);
                    group.UpdatedAt = DateTime.Now;

                    var groupsJson = JsonSerializer.Serialize(groups);
                    await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "groups", groupsJson);

                    return true;
                }
                return true;
            }
            return false;
        }

        public async Task<bool> RemoveMemberFromGroupAsync(string groupId, string username)
        {
            try
            {
                var groups = await GetGroupsAsync();
                var group = groups.FirstOrDefault(g => g.Id == groupId);

                if (group != null && group.Members != null && group.Members.Contains(username) && username != group.CreatorUsername)
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

                    // Loại bỏ trùng lặp trong members
                    if (group.Members != null)
                    {
                        existingGroup.Members = group.Members
                            .Where(m => !string.IsNullOrEmpty(m))
                            .Distinct()
                            .ToList();
                    }

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