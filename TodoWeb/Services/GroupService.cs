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

                group.Creator = currentUser?.Username ?? "Unknown";
                group.Members.Add(group.Creator);

                // Tạo mã code duy nhất
                group.Code = await GenerateUniqueCodeAsync();

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

        public async Task<bool> JoinGroupAsync(string code, string username)
        {
            try
            {
                var groups = await GetGroupsAsync();
                var group = groups.FirstOrDefault(g => g.Code == code);
                if (group != null && !group.Members.Contains(username))
                {
                    group.Members.Add(username);
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
            }
            catch
            {
                // Ignore errors
            }
            return false;
        }

        public async Task<List<Group>> GetUserGroupsAsync(string username)
        {
            var groups = await GetGroupsAsync();
            return groups.Where(g => g.Members.Contains(username)).ToList();
        }

        public async Task<List<Group>> GetCreatedGroupsAsync(string username)
        {
            var groups = await GetGroupsAsync();
            return groups.Where(g => g.Creator == username).ToList();
        }

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
        public async Task<bool> RemoveMemberFromGroupAsync(string groupId, string username)
        {
            try
            {
                // Lấy tất cả nhóm từ localStorage
                var groupsJson = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "groups");
                if (string.IsNullOrEmpty(groupsJson))
                {
                    return false;
                }

                var groups = JsonSerializer.Deserialize<List<Group>>(groupsJson) ?? new List<Group>();

                // Tìm nhóm theo ID
                var group = groups.FirstOrDefault(g => g.Id == groupId);
                if (group == null)
                {
                    return false;
                }

                // Kiểm tra xem user có phải là creator không (không được xóa creator)
                if (group.Creator == username)
                {
                    return false;
                }

                // Xóa thành viên khỏi danh sách
                group.Members = group.Members.Where(m => m != username).ToList();

                // Lưu lại vào localStorage
                var updatedJson = JsonSerializer.Serialize(groups);
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "groups", updatedJson);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing member from group: {ex.Message}");
                return false;
            }
        }
    }
}