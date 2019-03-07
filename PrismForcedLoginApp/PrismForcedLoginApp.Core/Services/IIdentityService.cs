using System;
using System.Threading.Tasks;
using PrismForcedLoginApp.Core.Helpers;

namespace PrismForcedLoginApp.Core.Services
{
    public interface IIdentityService
    {
        event EventHandler LoggedIn;
        event EventHandler LoggedOut;

        void InitializeWithAadAndPersonalMsAccounts();

        void InitializeWithAadMultipleOrgs(bool integratedAuth = false);

        void InitializeWithAadSingleOrg(string tenantId, bool integratedAuth = false);

        bool IsLoggedIn();

        Task<LoginResultType> LoginAsync();

        string GetAccountUserName();

        Task LogoutAsync();

        Task<string> GetAccessTokenAsync();

        Task<bool> SilentLoginAsync();
    }
}
