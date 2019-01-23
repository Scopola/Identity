using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Identity.Client;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

using ForcedLogInApp.Helpers;
using ForcedLogInApp.Views;
using System.Net.NetworkInformation;

namespace ForcedLogInApp.Services
{
    internal class IdentityService
    {
        private const string _clientId = "3b588882-c3f0-4f00-9047-933110e1d425";
        private const string _loginEndpoint = "https://login.microsoftonline.com";
        private const string _commonAuthority = "common";
        private const string _organizationsAuthority = "organizations";
        private readonly string[] _scopes = new string[] { "user.read" };

        private bool _integratedAuthAvailable;
        private PublicClientApplication _client;

        internal AuthenticationResult AuthenticationResult { get; private set; }

        internal event EventHandler LoggedIn;
        internal event EventHandler LoggedOut;
        internal event EventHandler AuthenticationAvailable;

        internal async Task LoginWithCommonAuthorityAsync()
        {
            // AAD and MSA accounts
            _integratedAuthAvailable = false;
            _client = new PublicClientApplication(_clientId, $"{_loginEndpoint}/{_commonAuthority}/");
            await SilentLoginAsync();
        }

        internal async Task LoginWithOrganizationsAuthorityAsync(bool integratedAuth = false)
        {
            // All AAD and Integrated Auth
            _integratedAuthAvailable = integratedAuth;
            _client = new PublicClientApplication(_clientId, $"{_loginEndpoint}/{_organizationsAuthority}/");
            await SilentLoginAsync();
        }

        internal async Task LoginWithTenantAuthority(string tenantId, bool integratedAuth = false)
        {
            // Single domain AAD and Integrated Auth
            _integratedAuthAvailable = integratedAuth;
            _client = new PublicClientApplication(_clientId, $"{_loginEndpoint}/{tenantId}/");
            await SilentLoginAsync();
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
                AuthenticationResult = null;
                LoggedOut?.Invoke(this, EventArgs.Empty);
                SetLoginPage();
            }
        }

        internal bool IsLoggedIn()
        {
            return AuthenticationResult != null;
        }

        internal async Task<string> GetAccessTokenAsync()
        {
            if (AuthenticationResult.IsAccessTokenExpired())
            {
                await SilentLoginAsync();
            }

            return AuthenticationResult.AccessToken;
        }

        internal async Task LoginAsync()
        {
            AuthenticationResult result = null;
            try
            {
                if (_integratedAuthAvailable)
                {
                    result = await _client.AcquireTokenByIntegratedWindowsAuthAsync(_scopes);
                }
                else
                {
                    var accounts = await _client.GetAccountsAsync();
                    result = await _client.AcquireTokenAsync(_scopes, accounts.FirstOrDefault());
                }
            }
            catch (MsalServiceException) { }
            catch (Exception ex) when (ex is MsalUiRequiredException || ex is MsalClientException)
            {
                try
                {
                    result = await _client.AcquireTokenAsync(_scopes);
                }
                catch (MsalException) { }
            }
            finally
            {
                AuthenticationResult = result;
                if (IsLoggedIn())
                {
                    AuthenticationAvailable?.Invoke(this, EventArgs.Empty);
                    LoggedIn?.Invoke(this, EventArgs.Empty);
                }
            }
        }        

        internal async Task SilentLoginAsync()
        {            
            try
            {
                AuthenticationResult result = null;
                if (NetworkInterface.GetIsNetworkAvailable())
                {
                    if (_integratedAuthAvailable)
                    {
                        result = await _client.AcquireTokenByIntegratedWindowsAuthAsync(_scopes);
                    }
                    else
                    {
                        var accounts = await _client.GetAccountsAsync();
                        result = await _client.AcquireTokenSilentAsync(_scopes, accounts.FirstOrDefault());
                    }

                    AuthenticationResult = result;
                    if (IsLoggedIn())
                    {
                        AuthenticationAvailable?.Invoke(this, EventArgs.Empty);
                    }
                }
            }
            catch (Exception)
            {
                SetLoginPage();
            }
        }

        private void SetLoginPage()
        {
            var frame = new Frame();
            frame.Navigate(typeof(LogInPage));
            NavigationService.Frame = frame;
            Window.Current.Content = frame;            
        }
    }
}
