using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

using Microsoft.Identity.Client;

using Windows.Storage;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

using ForcedLogInApp.Helpers;
using ForcedLogInApp.Models;

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

        public MicrosoftGraphService()
        {
        }

        internal async Task<User> GetUserInfoAsync()
        {
            if (_user == null)
            {
                _user = await ApplicationData.Current.LocalFolder.ReadAsync<User>(_userSettingsKey);
                if (_user == null || _user.Mail != GetAuthentication().Account.Username)
                {
                    var userData = await GetStringContentWithToken($"{_graphAPIEndpoint}{_apiServiceMe}", GetAuthentication().AccessToken);
                    if (!string.IsNullOrEmpty(userData))
                    {
                        _user = await Json.ToObjectAsync<User>(userData);
                        await ApplicationData.Current.LocalFolder.SaveAsync(_userSettingsKey, _user);
                    }
                }
            }

            return _user;
        }        

        internal async Task<ImageSource> GetUserPhoto()
        {
            if (_userPhoto == null)
            {
                var photoStream = await GetStreamContentWithToken($"{_graphAPIEndpoint}{_apiServiceMePhoto}", GetAuthentication().AccessToken);
                using (var randomAccessStream = photoStream?.AsRandomAccessStream())
                {
                    _userPhoto = new BitmapImage();
                    await _userPhoto.SetSourceAsync(randomAccessStream);
                }
            }

            return _userPhoto;
        }

        private AuthenticationResult GetAuthentication()
        {
            return Singleton<IdentityService>.Instance.AuthenticationResult;
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
