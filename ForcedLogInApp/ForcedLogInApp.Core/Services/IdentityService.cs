using System;
using System.Configuration;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using ForcedLogInApp.Core.Helpers;
using Microsoft.Identity.Client;

namespace ForcedLogInApp.Core.Services
{
    public class IdentityService
    {
        //// Read more about Microsoft Identity Client here
        //// https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki
        //// https://docs.microsoft.com/azure/active-directory/develop/v2-overview        
        private readonly string[] _scopes = new string[] { "user.read" };

        private bool _integratedAuthAvailable;
        private IPublicClientApplication _client;
        private AuthenticationResult _authenticationResult;

        
        // TODO WTS:
        // The IdentityClientId in App.config is provided to test the project in development environments.
        // Please, follow these steps to create a new one with Azure Active Directory and replace it before going to production.
        // https://docs.microsoft.com/azure/active-directory/develop/quickstart-register-app
        private string _clientId => ConfigurationManager.AppSettings["IdentityClientId"];

        public event EventHandler LoggedIn;
        public event EventHandler LoggedOut;

        private const string _loginEndpoint = "https://login.microsoftonline.com";
        private const string _commonAuthority = "common";
        private const string _organizationsAuthority = "organizations";

        public void InitializeWithAadAndPersonalMsAccounts()
        {
            _integratedAuthAvailable = false;
            _client = new PublicClientApplication(_clientId, $"{_loginEndpoint}/{_commonAuthority}/");
        }

        public void InitializeWithAadMultipleOrgs(bool integratedAuth = false)
        {
            _integratedAuthAvailable = integratedAuth;
            _client = new PublicClientApplication(_clientId, $"{_loginEndpoint}/{_organizationsAuthority}/");
        }

        public void InitializeWithAadSingleOrg(string tenant, bool integratedAuth = false)
        {
            _integratedAuthAvailable = integratedAuth;
            _client = new PublicClientApplication(_clientId, $"{_loginEndpoint}/{tenant}/");
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
                _authenticationResult = await _client.AcquireTokenAsync(_scopes, accounts.FirstOrDefault());
                if (!IsAuthorized())
                {
                    _authenticationResult = null;
                    return LoginResultType.Unauthorized;        
                }

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

        public bool IsAuthorized()
        {
            // TODO WTS: You can also add extra authorization checks here.
            // i.e.: Checks permisions of _authenticationResult.Account.Username in a database.
            return true;
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
            var silentLoginSuccess = await SilentLoginAsync();
            if (silentLoginSuccess)
            {
                return _authenticationResult.AccessToken;
            }
            else
            {
                // SilentLoginAsync failed, reasons for failure might be that users have either signed out or changed their password on another device
                // The session will be closed.
                _authenticationResult = null;
                LoggedOut?.Invoke(this, EventArgs.Empty);
                return string.Empty;
            }
        }

        public async Task<bool> SilentLoginAsync()
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
