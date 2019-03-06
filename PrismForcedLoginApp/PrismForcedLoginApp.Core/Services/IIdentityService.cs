using System;
using System.Threading.Tasks;
using PrismForcedLoginApp.Core.Helpers;

namespace PrismForcedLoginApp.Core.Services
{
    public interface IIdentityService
    {
        event EventHandler LoggedIn;
        event EventHandler LoggedOut;

        Task<bool> LoginWithCommonAuthorityAsync();

        Task<bool> LoginWithOrganizationsAuthorityAsync(bool integratedAuth = false);

        Task<bool> LoginWithTenantAuthority(string tenantId, bool integratedAuth = false);

        bool IsLoggedIn();

        Task<LoginResultType> LoginAsync();

        string GetAccountUserName();

        Task LogoutAsync();

        Task<string> GetAccessTokenAsync();        
    }
}
