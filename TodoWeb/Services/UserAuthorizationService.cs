using TodoWeb.Models;
using Microsoft.JSInterop;
using System.Text.Json;
using Microsoft.AspNetCore.Components;

namespace TodoWeb.Services
{
    public class UserAuthorizationService : IUserAuthorizationService
    {
        private readonly IAuthService _authService;
        private readonly NavigationManager _navigation;
        private readonly IJSRuntime _jsRuntime;

        public UserAuthorizationService(IAuthService authService, NavigationManager navigation, IJSRuntime jsRuntime)
        {
            _authService = authService;
            _navigation = navigation;
            _jsRuntime = jsRuntime;
        }

        public async Task<bool> IsCreatorAsync()
        {
            var user = await _authService.GetCurrentUserAsync();
            return user?.UserType == UserType.Creator;
        }

        public async Task<bool> IsAssigneeAsync()
        {
            var user = await _authService.GetCurrentUserAsync();
            return user?.UserType == UserType.Assignee;
        }

        public async Task<UserType> GetCurrentUserTypeAsync()
        {
            var user = await _authService.GetCurrentUserAsync();
            return user?.UserType ?? UserType.Assignee;
        }

        public async Task RedirectToProperDashboardAsync()
        {
            var userType = await GetCurrentUserTypeAsync();
            if (userType == UserType.Creator)
            {
                _navigation.NavigateTo("/creator/dashboard", true);
            }
            else
            {
                _navigation.NavigateTo("/assignee/dashboard", true);
            }
        }
    }
}