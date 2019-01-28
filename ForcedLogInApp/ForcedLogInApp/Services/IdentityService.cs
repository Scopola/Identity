using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using ForcedLogInApp.Configuration;
using ForcedLogInApp.Helpers;
using Microsoft.Identity.Client;

namespace ForcedLogInApp.Services
{
    internal class IdentityService
    {
        private const string _loginEndpoint = "https://login.microsoftonline.com";
        private const string _commonAuthority = "common";
        private const string _organizationsAuthority = "organizations";
        private readonly string[] _scopes = new string[] { "user.read" };

        private bool _integratedAuthAvailable;
        private PublicClientApplication _client;
        private AuthenticationResult _authenticationResult;

        internal event EventHandler LoggedIn;
        internal event EventHandler LoggedOut;

        internal async Task<bool> LoginWithCommonAuthorityAsync()
        {
            // AAD and MSA accounts
            _integratedAuthAvailable = false;
            _client = new PublicClientApplication(AppSecrets.IdentityClientId, $"{_loginEndpoint}/{_commonAuthority}/");
            return await SilentLoginAsync();
        }

        internal async Task<bool> LoginWithOrganizationsAuthorityAsync(bool integratedAuth = false)
        {
            // All AAD and Integrated Auth
            _integratedAuthAvailable = integratedAuth;
            _client = new PublicClientApplication(AppSecrets.IdentityClientId, $"{_loginEndpoint}/{_organizationsAuthority}/");
            return await SilentLoginAsync();
        }

        internal async Task<bool> LoginWithTenantAuthority(string tenantId, bool integratedAuth = false)
        {
            // Single domain AAD and Integrated Auth
            _integratedAuthAvailable = integratedAuth;
            _client = new PublicClientApplication(AppSecrets.IdentityClientId, $"{_loginEndpoint}/{tenantId}/");
            return await SilentLoginAsync();
        }

        internal bool IsLoggedIn() => _authenticationResult != null;

        internal async Task<LoginResultType> LoginAsync()
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

        internal string GetAccountUserName()
        {
            return _authenticationResult?.Account?.Username;
        }

        internal async Task LogoutAsync()
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
            }
        }

        internal async Task<string> GetAccessTokenAsync()
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
            catch (MsalException)
            {
                return false;
            }
        }
    }
}
