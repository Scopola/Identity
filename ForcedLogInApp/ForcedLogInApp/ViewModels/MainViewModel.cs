using System;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;
using ForcedLogInApp.Helpers;
using ForcedLogInApp.Services;

namespace ForcedLogInApp.ViewModels
{
    public class MainViewModel : Observable
    {
        public ObservableCollection<string> AuthenticationResult { get; } = new ObservableCollection<string>();

        private ICommand _showToastCommand;
        private ICommand _updateInfoCommand;
        private ICommand _updateUserInfoCommand;
        private ICommand _silentLoginCommand;
        private IdentityService _identityService => Singleton<IdentityService>.Instance;
        private MicrosoftGraphService _microsoftGraphService => Singleton<MicrosoftGraphService>.Instance;

        public ICommand ShowToastCommand => _showToastCommand ?? (_showToastCommand = new RelayCommand(OnShowToast));

        public ICommand UpdateInfoCommand => _updateInfoCommand ?? (_updateInfoCommand = new RelayCommand(OnUpdateInfo));

        public ICommand UpdateUserInfoCommand => _updateUserInfoCommand ?? (_updateUserInfoCommand = new RelayCommand(OnUpdateUserInfo));        

        public ICommand SilentLoginCommand => _silentLoginCommand ?? (_silentLoginCommand = new RelayCommand(OnSilentLogin));        

        public MainViewModel()
        {
        }

        private void OnShowToast()
        {
            Singleton<ToastNotificationsService>.Instance.ShowToastNotificationSample();
        }

        private void OnUpdateInfo()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Time: {DateTime.Now.ToLongTimeString()}");
            sb.AppendLine($"AccessToken: {_identityService.AuthenticationResult.AccessToken}");
            sb.AppendLine($"ExpiresOn: {_identityService.AuthenticationResult.ExpiresOn}");
            sb.AppendLine($"ExtendedExpiresOn: {_identityService.AuthenticationResult.ExtendedExpiresOn}");
            sb.AppendLine($"IdToken: {_identityService.AuthenticationResult.IdToken}");
            sb.AppendLine($"IsExtendedLifeTimeToken: {_identityService.AuthenticationResult.IsExtendedLifeTimeToken}");
            sb.AppendLine($"IsExtendedLifeTimeToken: {_identityService.AuthenticationResult.AccessToken}");
            sb.AppendLine($"TenantId: {_identityService.AuthenticationResult.TenantId}");
            sb.AppendLine($"UniqueId: {_identityService.AuthenticationResult.UniqueId}");
            AuthenticationResult.Insert(0, sb.ToString());
        }

        private async void OnUpdateUserInfo()
        {
            var user = await _microsoftGraphService.GetUserInfoAsync();
            var sb = new StringBuilder();
            sb.AppendLine($"Time: {DateTime.Now.ToLongTimeString()}");
            sb.AppendLine($"GivenName: {user.GivenName}");
            sb.AppendLine($"Id: {user.Id}");
            AuthenticationResult.Insert(0, sb.ToString());
        }

        private async void OnSilentLogin()
        {
            await _identityService.SilentLoginAsync();
            OnUpdateInfo();
        }
    }
}
