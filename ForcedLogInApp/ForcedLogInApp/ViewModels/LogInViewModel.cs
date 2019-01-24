using System;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Windows.Input;
using ForcedLogInApp.Core.Helpers;
using ForcedLogInApp.Helpers;
using ForcedLogInApp.Services;
using ForcedLogInApp.Views;
using Windows.UI.Xaml;

namespace ForcedLogInApp.ViewModels
{
    public class LogInViewModel : Observable
    {
        private IdentityService _identityService => Singleton<IdentityService>.Instance;
        private string _status;
        private bool _isBusy;
        private RelayCommand _loginCommand;                

        public string Status
        {
            get { return _status; }
            set { Set(ref _status, value); }
        }

        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                Set(ref _isBusy, value);
                LoginCommand.OnCanExecuteChanged();
            }
        }

        public RelayCommand LoginCommand => _loginCommand ?? (_loginCommand = new RelayCommand(OnLogin, () => !IsBusy));

        public LogInViewModel()
        {
        }

        private async void OnLogin()
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                Status = "StatusNoNetworkAvailable".GetLocalized();
            }
            else
            {
                IsBusy = true;
                Status = string.Empty;
                await _identityService.LoginAsync();
                if (_identityService.AuthenticationResult == null)
                {
                    Status = "StatusLoginFails".GetLocalized();
                }
                IsBusy = false;
            }
        }
    }
}
