using TodoWeb.Models;
using Microsoft.JSInterop;
using System.Text.Json;

namespace TodoWeb.Services
{
    public class AuthService : IAuthService
    {
        private readonly IJSRuntime _jsRuntime;
        private User? _currentUser;

        public event Action? OnAuthStateChanged;

        public AuthService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task<bool> SignUpAsync(User user)
        {
            try
            {
                var usersJson = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "users");
                var users = new List<User>();

                if (!string.IsNullOrEmpty(usersJson))
                {
                    users = JsonSerializer.Deserialize<List<User>>(usersJson) ?? new List<User>();
                }

                // Kiểm tra username đã tồn tại
                if (users.Any(u => u.Username == user.Username))
                {
                    return false;
                }

                users.Add(user);
                usersJson = JsonSerializer.Serialize(users);
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "users", usersJson);

                // Tự động đăng nhập sau khi đăng ký
                _currentUser = user;
                _currentUser.IsAuthenticated = true;
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "currentUser", JsonSerializer.Serialize(_currentUser));

                OnAuthStateChanged?.Invoke();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> SignInAsync(string username, string password)
        {
            try
            {
                var usersJson = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "users");
                if (string.IsNullOrEmpty(usersJson))
                    return false;

                var users = JsonSerializer.Deserialize<List<User>>(usersJson);
                var user = users?.FirstOrDefault(u => u.Username == username && u.Password == password);

                if (user != null)
                {
                    _currentUser = user;
                    _currentUser.IsAuthenticated = true;
                    await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "currentUser", JsonSerializer.Serialize(_currentUser));

                    OnAuthStateChanged?.Invoke();
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task SignOutAsync()
        {
            _currentUser = null;
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "currentUser");
            OnAuthStateChanged?.Invoke();
        }

        public async Task<User?> GetCurrentUserAsync()
        {
            if (_currentUser != null)
                return _currentUser;

            try
            {
                var userJson = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "currentUser");
                if (!string.IsNullOrEmpty(userJson))
                {
                    _currentUser = JsonSerializer.Deserialize<User>(userJson);
                }
            }
            catch
            {
                // Ignore errors
            }

            return _currentUser;
        }

        public async Task<bool> IsAuthenticatedAsync()
        {
            var user = await GetCurrentUserAsync();
            return user?.IsAuthenticated == true;
        }
    }
}