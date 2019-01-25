using System;
using System.Net.NetworkInformation;
using ForcedLogInApp.Core.Helpers;
using ForcedLogInApp.Helpers;
using ForcedLogInApp.Services;

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
            IsBusy = true;
            Status = string.Empty;
            var loginResult = await _identityService.LoginAsync();
            Status = GetStatusMessage(loginResult);
            IsBusy = false;
        }

        private string GetStatusMessage(LoginResultType loginResult)
        {
            switch (loginResult)
            {
                case LoginResultType.NoNetworkAvailable:
                    return "StatusNoNetworkAvailable".GetLocalized();                    
                case LoginResultType.UnknownError:
                    return "StatusLoginFails".GetLocalized();                    
                default:
                    return string.Empty;
            }
        }
    }
}
