using System;
using System.Threading.Tasks;
using ForcedLogInApp.Core.Helpers;
using ForcedLogInApp.Core.Models;
using ForcedLogInApp.Core.Services;
using ForcedLogInApp.Helpers;
using ForcedLogInApp.ViewModels;
using Windows.Storage;

namespace ForcedLogInApp.Services
{
    public class UserDataService
    {
        private const string _userSettingsKey = "IdentityUser";

        private IdentityService _identityService => Singleton<IdentityService>.Instance;
        private MicrosoftGraphService _microsoftGraphService => Singleton<MicrosoftGraphService>.Instance;

        public UserDataService()
        {
            _identityService.LoggedOut += OnLoggedOut;
        }

        private async void OnLoggedOut(object sender, EventArgs e)
        {
            await ApplicationData.Current.LocalFolder.SaveAsync<User>(_userSettingsKey, null);
        }

        public async Task<UserViewModel> GetUserFromCacheAsync()
        {
            var cacheData = await ApplicationData.Current.LocalFolder.ReadAsync<User>(_userSettingsKey);
            return await GetUserViewModelFromData(cacheData);
        }

        public async Task<UserViewModel> GetUserFromGraphApiAsync()
        {
            var accessToken = await _identityService.GetAccessTokenAsync();
            if (string.IsNullOrEmpty(accessToken))
            {
                return null;
            }

            var userData = await _microsoftGraphService.GetUserInfoAsync(accessToken);
            if (userData != null)
            {
                userData.Photo = await _microsoftGraphService.GetUserPhoto(accessToken);
                await ApplicationData.Current.LocalFolder.SaveAsync(_userSettingsKey, userData);
            }

            return await GetUserViewModelFromData(userData);
        }

        private async Task<UserViewModel> GetUserViewModelFromData(User userData)
        {
            if (userData == null)
            {
                return null;
            }

            var userPhoto = string.IsNullOrEmpty(userData.Photo)
                ? ImageHelper.ImageFromAssetsFile("DefaultIcon.png")
                : await ImageHelper.ImageFromStringAsync(userData.Photo);

            return new UserViewModel(userData, userPhoto);
        }
    }
}
