using TodoWeb.Models;

namespace TodoWeb.Services
{
    public interface IUserAuthorizationService
    {
        Task<bool> IsCreatorAsync();
        Task<bool> IsAssigneeAsync();
        Task<UserType> GetCurrentUserTypeAsync();
        Task RedirectToProperDashboardAsync();
    }
}