using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Microsoft.Identity.Client;
using PismForcedLoginApp.Core.Configuration;
using PismForcedLoginApp.Core.Helpers;

namespace PismForcedLoginApp.Core.Services
{
    public class IdentityService : IIdentityService
    {
        //// Read more about Microsoft Identity Client here
        //// https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki
        //// https://docs.microsoft.com/azure/active-directory/develop/v2-overview

        private const string _loginEndpoint = "https://login.microsoftonline.com";
        private const string _commonAuthority = "common";
        private const string _organizationsAuthority = "organizations";
        private readonly string[] _scopes = new string[] { "user.read" };

        private bool _integratedAuthAvailable;
        private PublicClientApplication _client;
        private AuthenticationResult _authenticationResult;

        public event EventHandler LoggedIn;
        public event EventHandler LoggedOut;

        public IdentityService()
        {
        }

        public async Task<bool> LoginWithCommonAuthorityAsync()
        {
            // AAD and MSA accounts
            _integratedAuthAvailable = false;
            _client = new PublicClientApplication(AppSecrets.IdentityClientId, $"{_loginEndpoint}/{_commonAuthority}/");
            return await SilentLoginAsync();
        }

        public async Task<bool> LoginWithOrganizationsAuthorityAsync(bool integratedAuth = false)
        {
            // All AAD and Integrated Auth
            _integratedAuthAvailable = integratedAuth;
            _client = new PublicClientApplication(AppSecrets.IdentityClientId, $"{_loginEndpoint}/{_organizationsAuthority}/");
            return await SilentLoginAsync();
        }

        public async Task<bool> LoginWithTenantAuthority(string tenantId, bool integratedAuth = false)
        {
            // Single domain AAD and Integrated Auth
            _integratedAuthAvailable = integratedAuth;
            _client = new PublicClientApplication(AppSecrets.IdentityClientId, $"{_loginEndpoint}/{tenantId}/");
            return await SilentLoginAsync();
        }

        public bool IsLoggedIn() => _authenticationResult != null;

        public async Task<LoginResultType> LoginAsync()
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                return LoginResultType.NoNetworkAvailable;
            }

            try
            {
                var accounts = await _client.GetAccountsAsync();
                var firstAccount = accounts.FirstOrDefault();

                _authenticationResult = firstAccount != null
                    ? await _client.AcquireTokenAsync(_scopes, firstAccount)
                    : await _client.AcquireTokenAsync(_scopes);

                LoggedIn?.Invoke(this, EventArgs.Empty);
                return LoginResultType.Success;
            }
            catch (MsalClientException ex)
            {
                if (ex.ErrorCode == "authentication_canceled")
                {
                    return LoginResultType.CancelledByUser;
                }

                return LoginResultType.UnknownError;
            }
            catch (Exception)
            {
                return LoginResultType.UnknownError;
            }
        }

        public string GetAccountUserName()
        {
            return _authenticationResult?.Account?.Username;
        }

        public async Task LogoutAsync()
        {
            try
            {
                var accounts = await _client.GetAccountsAsync();
                var account = accounts.FirstOrDefault();
                if (account != null)
                {
                    await _client.RemoveAsync(account);
                }

                _authenticationResult = null;
                LoggedOut?.Invoke(this, EventArgs.Empty);
            }
            catch (MsalException)
            {
                // TODO WTS: LogoutAsync can fail please handle exceptions as appropriate to your scenario
                // For more info on MsalExceptions see
                // https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/exceptions
            }
        }

        public async Task<string> GetAccessTokenAsync()
        {
            if (!_authenticationResult.IsAccessTokenExpired())
            {
                return _authenticationResult.AccessToken;
            }

            var loginSuccess = await SilentLoginAsync();
            if (loginSuccess)
            {
                return _authenticationResult.AccessToken;
            }
            else
            {
                // The token has expired and we can't obtain a new one
                // The session will be closed.
                _authenticationResult = null;
                LoggedOut?.Invoke(this, EventArgs.Empty);
                return string.Empty;
            }
        }

        private async Task<bool> SilentLoginAsync()
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                return false;
            }
            try
            {
                if (_integratedAuthAvailable)
                {
                    _authenticationResult = await _client.AcquireTokenByIntegratedWindowsAuthAsync(_scopes);
                }
                else
                {
                    var accounts = await _client.GetAccountsAsync();
                    _authenticationResult = await _client.AcquireTokenSilentAsync(_scopes, accounts.FirstOrDefault());
                }

                return true;
            }
            catch (MsalUiRequiredException)
            {
                // Interactive authentication is required
                return false;
            }
            catch (MsalException)
            {
                // TODO WTS: Silentauth failed, please handle this exception as appropriate to your scenario
                // For more info on MsalExceptions see
                // https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/exceptions
                return false;
            }
        }
    }
}