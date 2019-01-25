using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using ForcedLogInApp.Core.Helpers;
using ForcedLogInApp.Core.Models;

namespace ForcedLogInApp.Core.Services
{
    public class MicrosoftGraphService
    {
        private const string _graphAPIEndpoint = "https://graph.microsoft.com/v1.0/";
        private const string _apiServiceMe = "me/";
        private const string _apiServiceMePhoto = "me/photo/$value";

        public MicrosoftGraphService()
        {
        }

        public async Task<User> GetUserInfoAsync(string accessToken)
        {
            User user = null;
            var userData = await GetStringContentWithTokenAsync($"{_graphAPIEndpoint}{_apiServiceMe}", accessToken);
            if (!string.IsNullOrEmpty(userData))
            {
                user = await Json.ToObjectAsync<User>(userData);
            }

            return user;
        }

        public async Task<Stream> GetUserPhoto(string accessToken)
        {
            return await GetStreamContentWithTokenAsync($"{_graphAPIEndpoint}{_apiServiceMePhoto}", accessToken);
        }


        private async Task<string> GetStringContentWithTokenAsync(string url, string accessToken)
        {
            var httpContent = await GetHttpContentWithTokenAsync(url, accessToken);
            return httpContent != null ? await httpContent.ReadAsStringAsync() : string.Empty;
        }

        private async Task<Stream> GetStreamContentWithTokenAsync(string url, string accessToken)
        {
            var httpContent = await GetHttpContentWithTokenAsync(url, accessToken);
            return await httpContent?.ReadAsStreamAsync();
        }

        private async Task<HttpContent> GetHttpContentWithTokenAsync(string url, string accessToken)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    var request = new HttpRequestMessage(HttpMethod.Get, url);
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                    var response = await httpClient.SendAsync(request);
                    return response.Content;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
