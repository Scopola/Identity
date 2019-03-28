using System;
using System.Collections.Generic;
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

        private UserViewModel _user;

        private IdentityService IdentityService => Singleton<IdentityService>.Instance;

        private MicrosoftGraphService MicrosoftGraphService => Singleton<MicrosoftGraphService>.Instance;

        public event EventHandler<UserViewModel> UserDataUpdated;

        public UserDataService()
        {
        }

        public void Initialize()
        {
            IdentityService.LoggedIn += OnLoggedIn;
            IdentityService.LoggedOut += OnLoggedOut;
        }

        public async Task<UserViewModel> GetUserAsync()
        {
            if (_user == null)
            {
                _user = await GetUserFromCacheAsync();
                if (_user == null)
                {
                    _user = GetDefaultUserData();
                }
            }

            return _user;
        }

        private async void OnLoggedIn(object sender, EventArgs e)
        {
            _user = await GetUserFromGraphApiAsync();
            UserDataUpdated?.Invoke(this, _user);
        }

        private async void OnLoggedOut(object sender, EventArgs e)
        {
            _user = null;
            await ApplicationData.Current.LocalFolder.SaveAsync<User>(_userSettingsKey, null);
        }

        private async Task<UserViewModel> GetUserFromCacheAsync()
        {
            var cacheData = await ApplicationData.Current.LocalFolder.ReadAsync<User>(_userSettingsKey);
            return await GetUserViewModelFromData(cacheData);
        }

        private async Task<UserViewModel> GetUserFromGraphApiAsync()
        {
            var accessToken = await IdentityService.GetAccessTokenAsync();
            if (string.IsNullOrEmpty(accessToken))
            {
                return null;
            }

            var userData = await MicrosoftGraphService.GetUserInfoAsync(accessToken);
            if (userData != null)
            {
                userData.Photo = await MicrosoftGraphService.GetUserPhoto(accessToken);
                await ApplicationData.Current.LocalFolder.SaveAsync(_userSettingsKey, userData);
            }

            return await GetUserViewModelFromData(userData);
        }

        public async Task<IEnumerable<UserViewModel>> GetPeopleFromGraphApiAsync()
        {
            var accessToken = await IdentityService.GetAccessTokenAsync();
            if (string.IsNullOrEmpty(accessToken))
            {
                return null;
            }

            var peopleData = await MicrosoftGraphService.GetPeopleInfoAsync(accessToken);
            if (peopleData != null)
            {
                foreach (var user in peopleData.Value)
                {
                    user.Photo = await MicrosoftGraphService.GetUserPhoto(accessToken);
                }
            }

            return await GetUserViewModelFromData(peopleData.Value);
        }

        private async Task<IEnumerable<UserViewModel>> GetUserViewModelFromData(IEnumerable<User> usersData)
        {
            var people = new List<UserViewModel>();
            foreach (var userData in usersData)
            {
                var user = await GetUserViewModelFromData(userData);
                people.Add(user);
            }

            return people;
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

        private UserViewModel GetDefaultUserData()
        {
            return new UserViewModel()
            {
                Name = IdentityService.GetAccountUserName(),
                Photo = ImageHelper.ImageFromAssetsFile("DefaultIcon.png")
            };
        }
    }
}
