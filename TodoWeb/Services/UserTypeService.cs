using TodoWeb.Models;
using Microsoft.JSInterop;
using System.Text.Json;

namespace TodoWeb.Services
{
    public class UserTypeService : IUserTypeService
    {
        private readonly IAuthService _authService;
        private readonly IJSRuntime _jsRuntime;

        public UserTypeService(IAuthService authService, IJSRuntime jsRuntime)
        {
            _authService = authService;
            _jsRuntime = jsRuntime;
        }

        public async Task SetUserTypeAsync(UserType userType)
        {
            var user = await _authService.GetCurrentUserAsync();
            if (user != null)
            {
                user.UserType = userType;
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "currentUser", JsonSerializer.Serialize(user));

                // Cập nhật trong danh sách users
                var usersJson = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "users");
                if (!string.IsNullOrEmpty(usersJson))
                {
                    var users = JsonSerializer.Deserialize<List<User>>(usersJson) ?? new List<User>();
                    var existingUser = users.FirstOrDefault(u => u.Username == user.Username);
                    if (existingUser != null)
                    {
                        existingUser.UserType = userType;
                        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "users", JsonSerializer.Serialize(users));
                    }
                }
            }
        }

        public async Task<UserType?> GetCurrentUserTypeAsync()
        {
            var user = await _authService.GetCurrentUserAsync();
            return user?.UserType;
        }

        public async Task<bool> HasUserTypeAsync()
        {
            var user = await _authService.GetCurrentUserAsync();
            return user?.UserType != null;
        }

        public async Task<bool> IsCreatorAsync()
        {
            var userType = await GetCurrentUserTypeAsync();
            return userType == UserType.Creator;
        }

        public async Task<bool> IsAssigneeAsync()
        {
            var userType = await GetCurrentUserTypeAsync();
            return userType == UserType.Assignee;
        }
    }
}