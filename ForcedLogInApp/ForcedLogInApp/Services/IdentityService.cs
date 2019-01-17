using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using ForcedLogInApp.Helpers;
using ForcedLogInApp.Models;
using ForcedLogInApp.Views;
using Microsoft.Identity.Client;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace ForcedLogInApp.Services
{
    internal class IdentityService
    {
        private const string UserSettingsKey = "IdentityUser";
        private const string ClientId = "";
        private const string Endpoint = "https://login.microsoftonline.com";
        private const string GraphAPIEndpoint = "https://graph.microsoft.com/v1.0/";
        private const string ApiServiceMe = "me/";
        private const string ApiServiceMePhoto = "me/photo/$value";

        private IdentityAuthType _authType;
        private string _tenantId;
        private string[] _scopes;
        private bool _integratedAuth = false;
        private PublicClientApplication _client;
        private User _user;
        private BitmapImage _userPhoto;

        internal AuthenticationResult AuthenticationResult { get; private set; }

        internal void Initialize(IdentityAuthType authType, string tenantId = null, bool integratedAuth = false)
        {
            _authType = authType;
            _tenantId = tenantId;
            _scopes = new string[] { "user.read" };
            _integratedAuth = integratedAuth;

            if (_authType == IdentityAuthType.Tenant)
            {
                _client = new PublicClientApplication(ClientId, $"{Endpoint}/{_tenantId}/");
            }
            else
            {
                _client = new PublicClientApplication(ClientId, $"{Endpoint}/{_authType.ToString().ToLower()}/");
            }
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
            else
            {
                await GetUserInfoAsync();
            }
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
                var accounts = await _client.GetAccountsAsync();

                try
                {
                    if (IsIntegratedAuthAvailable())
                    {
                        loginResult.Result = await _client.AcquireTokenByIntegratedWindowsAuthAsync(_scopes);
                    }
                    else
                    {
                        loginResult.Result = await _client.AcquireTokenAsync(_scopes, accounts.FirstOrDefault());
                    }
                }
                catch (MsalServiceException)
                {
                    // Server fails
                }
                catch (Exception ex) when (ex is MsalUiRequiredException || ex is MsalClientException)
                {
                    try
                    {
                        loginResult.Result = await _client.AcquireTokenAsync(_scopes);
                    }
                    catch (MsalException)
                    {
                    }
                }
            }

            AuthenticationResult = loginResult.Result;

            if (loginResult.Success)
            {
                await GetUserInfoAsync();
            }

            return loginResult;
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

        internal async Task<User> GetUserInfoAsync()
        {
            if (_user == null)
            {
                var user = await ApplicationData.Current.LocalFolder.ReadAsync<User>(UserSettingsKey);
                if (user == null || user.Mail != AuthenticationResult.Account.Username)
                {
                    var userJson = await GetStringContentWithToken($"{GraphAPIEndpoint}{ApiServiceMe}", AuthenticationResult.AccessToken);
                    if (!string.IsNullOrEmpty(userJson))
                    {
                        user = await Json.ToObjectAsync<User>(userJson);
                        await ApplicationData.Current.LocalFolder.SaveAsync(UserSettingsKey, user);
                    }
                }

                _user = user;
            }

            return _user;
        }

        internal async Task<ImageSource> GetUserPhoto()
        {
            if (_userPhoto == null)
            {
                var photoStream = await GetStreamContentWithToken($"{GraphAPIEndpoint}{ApiServiceMePhoto}", AuthenticationResult.AccessToken);
                using (var randomAccessStream = photoStream?.AsRandomAccessStream())
                {
                    _userPhoto = new BitmapImage();
                    await _userPhoto.SetSourceAsync(randomAccessStream);
                }
            }

            return _userPhoto;
        }

        private async Task<LoginResult> SilentLoginAsync()
        {
            LoginResult loginResult = new LoginResult();
            try
            {
                if (!NetworkInterface.GetIsNetworkAvailable())
                {
                    loginResult.HasNetwork = false;
                }
                else
                {
                    if (IsIntegratedAuthAvailable())
                    {
                        loginResult.Result = await _client.AcquireTokenByIntegratedWindowsAuthAsync(_scopes);
                    }
                    else
                    {
                        var accounts = await _client.GetAccountsAsync();
                        var account = accounts.FirstOrDefault();
                        if (account != null)
                        {
                            loginResult.Result = await _client.AcquireTokenSilentAsync(_scopes, account);
                        }
                    }
                }
            }
            catch (Exception)
            {
            }

            AuthenticationResult = loginResult.Result;
            return loginResult;
        }

        private bool IsIntegratedAuthAvailable()
        {
            return _integratedAuth && (_authType == IdentityAuthType.Organizations || _authType == IdentityAuthType.Tenant);
        }

        private async Task<string> GetStringContentWithToken(string url, string token)
        {
            var httpContent = await GetHttpContentWithToken(url, token);
            return httpContent != null ? await httpContent.ReadAsStringAsync() : string.Empty;
        }

        private async Task<Stream> GetStreamContentWithToken(string url, string token)
        {
            var httpContent = await GetHttpContentWithToken(url, token);
            return await httpContent?.ReadAsStreamAsync();
        }

        private async Task<HttpContent> GetHttpContentWithToken(string url, string token)
        {
            try
            {
                var httpClient = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var response = await httpClient.SendAsync(request);
                return response.Content;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
