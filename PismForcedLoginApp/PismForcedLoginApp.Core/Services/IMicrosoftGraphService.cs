using System.Threading.Tasks;
using PismForcedLoginApp.Core.Models;

namespace PismForcedLoginApp.Core.Services
{
    public interface IMicrosoftGraphService
    {
        Task<User> GetUserInfoAsync(string accessToken);

        Task<string> GetUserPhoto(string accessToken);
    }
}
