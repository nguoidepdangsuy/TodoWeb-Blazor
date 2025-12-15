using TodoWeb.Models;

namespace TodoWeb.Services
{
    public interface IAuthService
    {
        Task<bool> SignUpAsync(User user);
        Task<bool> SignInAsync(string username, string password);
        Task SignOutAsync();
        Task<User?> GetCurrentUserAsync();
        Task<bool> IsAuthenticatedAsync();
        event Action? OnAuthStateChanged;
    }
}