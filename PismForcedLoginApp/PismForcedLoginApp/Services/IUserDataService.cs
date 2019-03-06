using System.Threading.Tasks;
using PismForcedLoginApp.ViewModels;

namespace PismForcedLoginApp.Services
{
    public interface IUserDataService
    {
        Task<UserViewModel> GetUserFromCacheAsync();

        Task<UserViewModel> GetUserFromGraphApiAsync();

        UserViewModel GetDefaultUserData();
    }
}
