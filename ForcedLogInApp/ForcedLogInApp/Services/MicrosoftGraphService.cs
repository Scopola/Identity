using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using ForcedLogInApp.Core.Helpers;
using ForcedLogInApp.Helpers;
using ForcedLogInApp.Models;
using Windows.Storage;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace ForcedLogInApp.Services
{
    internal class MicrosoftGraphService
    {
        private const string _graphAPIEndpoint = "https://graph.microsoft.com/v1.0/";
        private const string _apiServiceMe = "me/";
        private const string _apiServiceMePhoto = "me/photo/$value";
        private const string _userSettingsKey = "IdentityUser";

        private User _user;
        private BitmapImage _userPhoto;
        private IdentityService _identityService => Singleton<IdentityService>.Instance;

        public MicrosoftGraphService()
        {
        }

        internal void Initialize()
        {
            _identityService.AuthenticationAvailable += OnAuthenticationAvailable;
        }

        private async void OnAuthenticationAvailable(object sender, EventArgs e)
        {
            await GetUserInfoAsync();
            await GetUserPhoto();
        }

        internal async Task<User> GetUserInfoAsync()
        {
            if (_user == null)
            {
                _user = await ApplicationData.Current.LocalFolder.ReadAsync<User>(_userSettingsKey);
                if (_user == null || IsLoggedInWithOtherAccount())
                {
                    var userData = await GetStringContentWithToken($"{_graphAPIEndpoint}{_apiServiceMe}");
                    if (!string.IsNullOrEmpty(userData))
                    {
                        _user = await Json.ToObjectAsync<User>(userData);
                        await ApplicationData.Current.LocalFolder.SaveAsync(_userSettingsKey, _user);
                    }
                }
            }

            return _user;
        }

        private bool IsLoggedInWithOtherAccount()
        {
            return _user.Mail != _identityService.AuthenticationResult.Account.Username;
        }

        internal async Task<ImageSource> GetUserPhoto()
        {
            if (_userPhoto == null)
            {
                var photoStream = await GetStreamContentWithToken($"{_graphAPIEndpoint}{_apiServiceMePhoto}");
                using (var randomAccessStream = photoStream?.AsRandomAccessStream())
                {
                    _userPhoto = new BitmapImage();
                    await _userPhoto.SetSourceAsync(randomAccessStream);
                }
            }

            return _userPhoto;
        }

        private async Task<string> GetStringContentWithToken(string url)
        {
            var httpContent = await GetHttpContentWithToken(url);
            return httpContent != null ? await httpContent.ReadAsStringAsync() : string.Empty;
        }

        private async Task<Stream> GetStreamContentWithToken(string url)
        {
            var httpContent = await GetHttpContentWithToken(url);
            return await httpContent?.ReadAsStreamAsync();
        }

        private async Task<HttpContent> GetHttpContentWithToken(string url)
        {
            try
            {
                var httpClient = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                var token = await _identityService.GetAccessTokenAsync();
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
