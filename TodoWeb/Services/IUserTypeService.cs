using TodoWeb.Models;

namespace TodoWeb.Services
{
    public interface IUserTypeService
    {
        Task SetUserTypeAsync(UserType userType);
        Task<UserType?> GetCurrentUserTypeAsync();
        Task<bool> HasUserTypeAsync();
        Task<bool> IsCreatorAsync();
        Task<bool> IsAssigneeAsync();
    }
}