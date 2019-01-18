using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

using Microsoft.Identity.Client;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

using ForcedLogInApp.Helpers;
using ForcedLogInApp.Views;

namespace ForcedLogInApp.Services
{
    internal class IdentityService
    {
        private const string _clientId = "3b588882-c3f0-4f00-9047-933110e1d425";
        private const string _loginEndpoint = "https://login.microsoftonline.com";
        private readonly string[] _scopes = new string[] { "user.read" };

        private bool _integratedAuthAvailable;        
        private PublicClientApplication _client;

        internal AuthenticationResult AuthenticationResult { get; private set; }        

        internal void InitializeWithCommonAuthority()
        {
            // AAD and MSA accounts
            _integratedAuthAvailable = false;
            _client = new PublicClientApplication(_clientId, $"{_loginEndpoint}/common/");
        }

        internal void InitializeWithOrganizationsAuthority(bool integratedAuth = false)
        {
            // All AAD and Integrated Auth
            _integratedAuthAvailable = integratedAuth;
            _client = new PublicClientApplication(_clientId, $"{_loginEndpoint}/organizations/");
        }

        internal void InitializeWithTenantAuthority(string tenantId, bool integratedAuth = false)
        {
            // Single domain AAD and Integrated Auth
            _integratedAuthAvailable = integratedAuth;
            _client = new PublicClientApplication(_clientId, $"{_loginEndpoint}/{tenantId}/");
        }

        internal async Task LaunchLoginAsync()
        {
            var loginResult = await SilentLoginAsync();
            if (!loginResult.Success)
            {
                var frame = new Frame();
                Window.Current.Content = frame;
                NavigationService.Frame = frame;
                NavigationService.Navigate<LogInPage>();
            }
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
            }
        }

        internal bool IsLoggedIn()
        {
            return AuthenticationResult != null;
        }

        internal async Task<LoginResult> LoginAsync()
        {
            var loginResult = new LoginResult();

            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                loginResult.HasNetwork = false;
            }
            else
            {
                try
                {
                    if (_integratedAuthAvailable)
                    {
                        loginResult.Result = await _client.AcquireTokenByIntegratedWindowsAuthAsync(_scopes);
                    }
                    else
                    {
                        var accounts = await _client.GetAccountsAsync();
                        loginResult.Result = await _client.AcquireTokenAsync(_scopes, accounts.FirstOrDefault());
                    }
                }
                catch (MsalServiceException) { }
                catch (Exception ex) when (ex is MsalUiRequiredException || ex is MsalClientException)
                {
                    try
                    {
                        loginResult.Result = await _client.AcquireTokenAsync(_scopes);
                    }
                    catch (MsalException) { }
                }
            }

            await SaveLoginResultAsync(loginResult);
            return loginResult;
        }

        private async Task<LoginResult> SilentLoginAsync()
        {
            var loginResult = new LoginResult();
            try
            {
                if (!NetworkInterface.GetIsNetworkAvailable())
                {
                    loginResult.HasNetwork = false;
                }
                else
                {
                    if (_integratedAuthAvailable)
                    {
                        loginResult.Result = await _client.AcquireTokenByIntegratedWindowsAuthAsync(_scopes);
                    }
                    else
                    {
                        var accounts = await _client.GetAccountsAsync();
                        loginResult.Result = await _client.AcquireTokenSilentAsync(_scopes, accounts.FirstOrDefault());
                    }
                }
            }
            catch (Exception) { }

            await SaveLoginResultAsync(loginResult);
            return loginResult;
        }

        private async Task SaveLoginResultAsync(LoginResult loginResult)
        {
            if (loginResult.Success)
            {
                AuthenticationResult = loginResult.Result;
                await Singleton<MicrosoftGraphService>.Instance.GetUserInfoAsync();
                await Singleton<MicrosoftGraphService>.Instance.GetUserPhoto();
            }
            else
            {
                AuthenticationResult = null;
            }
        }
    }
}
