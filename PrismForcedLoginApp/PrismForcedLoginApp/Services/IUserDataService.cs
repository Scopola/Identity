using System.Threading.Tasks;
using PrismForcedLoginApp.ViewModels;

namespace PrismForcedLoginApp.Services
{
    public interface IUserDataService
    {
        Task<UserViewModel> GetUserFromCacheAsync();

        Task<UserViewModel> GetUserFromGraphApiAsync();

        UserViewModel GetDefaultUserData();
    }
}
