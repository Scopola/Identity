using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
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
            _client = new PublicClientApplication(Consts.IdentityAppId, $"{_loginEndpoint}/{_commonAuthority}/");
            return await SilentLoginAsync();
        }

        internal async Task<bool> LoginWithOrganizationsAuthorityAsync(bool integratedAuth = false)
        {
            // All AAD and Integrated Auth
            _integratedAuthAvailable = integratedAuth;
            _client = new PublicClientApplication(Consts.IdentityAppId, $"{_loginEndpoint}/{_organizationsAuthority}/");
            return await SilentLoginAsync();
        }

        internal async Task<bool> LoginWithTenantAuthority(string tenantId, bool integratedAuth = false)
        {
            // Single domain AAD and Integrated Auth
            _integratedAuthAvailable = integratedAuth;
            _client = new PublicClientApplication(Consts.IdentityAppId, $"{_loginEndpoint}/{tenantId}/");
            return await SilentLoginAsync();
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
            }
            catch (MsalException)
            {
            }
            finally
            {
                _authenticationResult = null;
                LoggedOut?.Invoke(this, EventArgs.Empty);
            }
        }

        internal bool IsLoggedIn()
        {
            return _authenticationResult != null;
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

            _authenticationResult = null;
            LoggedOut?.Invoke(this, EventArgs.Empty);
            return string.Empty;
        }

        internal async Task<LoginResultType> LoginAsync()
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                return LoginResultType.NoNetworkAvailable;
            }

            AuthenticationResult result = null;
            try
            {
                var accounts = await _client.GetAccountsAsync();
                var firstAccount = accounts.FirstOrDefault();
                if (firstAccount != null)
                {
                    result = await _client.AcquireTokenAsync(_scopes, firstAccount);
                }
                else
                {
                    result = await _client.AcquireTokenAsync(_scopes);
                }
                _authenticationResult = result;
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

        internal async Task<bool> SilentLoginAsync()
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                return false;
            }
            try
            {
                AuthenticationResult result = null;
                if (_integratedAuthAvailable)
                {
                    result = await _client.AcquireTokenByIntegratedWindowsAuthAsync(_scopes);
                }
                else
                {
                    var accounts = await _client.GetAccountsAsync();
                    result = await _client.AcquireTokenSilentAsync(_scopes, accounts.FirstOrDefault());
                }

                _authenticationResult = result;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
