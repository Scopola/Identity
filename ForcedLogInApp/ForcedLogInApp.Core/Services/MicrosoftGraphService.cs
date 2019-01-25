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
            var httpContent = await GetDataAsync($"{_graphAPIEndpoint}{_apiServiceMe}", accessToken);

            var userData = await httpContent?.ReadAsStringAsync();
            if (!string.IsNullOrEmpty(userData))
            {
                user = await Json.ToObjectAsync<User>(userData);
            }

            return user;
        }

        public async Task<Stream> GetUserPhoto(string accessToken)
        {
            var httpContent = await GetDataAsync($"{_graphAPIEndpoint}{_apiServiceMePhoto}", accessToken);
            return httpContent == null ? null : await httpContent?.ReadAsStreamAsync();
        }

        private async Task<HttpContent> GetDataAsync(string url, string accessToken)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    var request = new HttpRequestMessage(HttpMethod.Get, url);
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                    var response = await httpClient.SendAsync(request);
                    if (response.IsSuccessStatusCode)
                    {
                        return response.Content;
                    }

                    return null;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
