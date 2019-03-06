using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PrismForcedLoginApp.Core.Helpers;
using PrismForcedLoginApp.Core.Models;
using PrismForcedLoginApp.Core.Services;
using PrismForcedLoginApp.Helpers;
using PrismForcedLoginApp.ViewModels;
using Windows.Storage;

namespace PrismForcedLoginApp.Services
{
    public class UserDataService : IUserDataService
    {
        private const string _userSettingsKey = "IdentityUser";

        private IIdentityService _identityService;       
        private IMicrosoftGraphService _microsoftGraphService;

        public UserDataService(IIdentityService identityService, IMicrosoftGraphService microsoftGraphService)
        {
            _identityService = identityService;
            _microsoftGraphService = microsoftGraphService;
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

            return new UserViewModel()
            {
                Name = userData.DisplayName,
                UserPrincipalName = userData.UserPrincipalName,
                Photo = userPhoto
            };
        }

        public UserViewModel GetDefaultUserData()
        {
            return new UserViewModel()
            {
                Name = _identityService.GetAccountUserName(),
                Photo = ImageHelper.ImageFromAssetsFile("DefaultIcon.png")
            };
        }
    }
}
