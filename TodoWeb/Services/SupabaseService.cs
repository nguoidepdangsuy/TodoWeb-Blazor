using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.JSInterop;
using TodoWeb.Models;

namespace TodoWeb.Services
{
    public class SupabaseService
    {
        private readonly IConfiguration _configuration;
        private readonly IJSRuntime _jsRuntime;

        public SupabaseService(IConfiguration configuration, IJSRuntime jsRuntime)
        {
            _configuration = configuration;
            _jsRuntime = jsRuntime;
        }

        // Mock client trả về null
        public object? GetClient() => null;

        // Mock storage trả về null
        public object? GetStorage() => null;

        public async Task<object?> GetCurrentUserAsync()
        {
            try
            {
                var userJson = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "currentUser");
                if (!string.IsNullOrEmpty(userJson))
                {
                    return JsonSerializer.Deserialize<object>(userJson);
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task SignOutAsync()
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "currentUser");
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "supabase_session");
        }
    }
}