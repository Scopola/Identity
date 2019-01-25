using System;
using System.Threading.Tasks;
using ForcedLogInApp.Core.Helpers;
using ForcedLogInApp.Core.Models;
using ForcedLogInApp.Core.Services;
using ForcedLogInApp.Helpers;
using ForcedLogInApp.ViewModels;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;

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
            return await UserFromData(cacheData);
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
                var photoStream = await _microsoftGraphService.GetUserPhoto(accessToken);
                if (photoStream != null)
                {
                    userData.Photo = ImageHelper.StringFromStream(photoStream);
                }

                await ApplicationData.Current.LocalFolder.SaveAsync(_userSettingsKey, userData);
            }

            return await UserFromData(userData);
        }

        private async Task<UserViewModel> UserFromData(User userData)
        {
            if (userData == null)
            {
                return new UserViewModel();
            }

            BitmapImage userPhoto = null;
            if (string.IsNullOrEmpty(userData.Photo))
            {
                userPhoto = ImageHelper.ImageFromAssetsFile("DefaultIcon.png");
            }
            else
            {
                userPhoto = await ImageHelper.ImageFromStringAsync(userData.Photo);
            }

            return new UserViewModel(userData, userPhoto);
        }
    }
}
