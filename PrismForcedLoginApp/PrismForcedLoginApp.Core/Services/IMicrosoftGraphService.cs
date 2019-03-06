using System.Threading.Tasks;
using PrismForcedLoginApp.Core.Models;

namespace PrismForcedLoginApp.Core.Services
{
    public interface IMicrosoftGraphService
    {
        Task<User> GetUserInfoAsync(string accessToken);

        Task<string> GetUserPhoto(string accessToken);
    }
}
