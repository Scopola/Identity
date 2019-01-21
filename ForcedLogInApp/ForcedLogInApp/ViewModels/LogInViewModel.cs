using System;
using System.Windows.Input;
using ForcedLogInApp.Helpers;
using ForcedLogInApp.Services;
using ForcedLogInApp.Views;
using Windows.UI.Xaml;

namespace ForcedLogInApp.ViewModels
{
    public class LogInViewModel : Observable
    {
        private ICommand _loginCommand;

        public ICommand LoginCommand => _loginCommand ?? (_loginCommand = new RelayCommand(OnLogin));

        public LogInViewModel()
        {
        }

        private async void OnLogin()
        {
            var loginResult = await Singleton<IdentityService>.Instance.LoginAsync();
        }
    }
}
