using System;
using System.Threading.Tasks;
using System.Windows.Input;
using OptionalLoginApp.Core.Helpers;
using OptionalLoginApp.Core.Services;
using OptionalLoginApp.Helpers;
using OptionalLoginApp.Services;

using Windows.ApplicationModel;
using Windows.UI.Xaml;

namespace OptionalLoginApp.ViewModels
{
    public class SettingsViewModel : Observable
    {
        private UserDataService UserDataService => Singleton<UserDataService>.Instance;

        private IdentityService IdentityService => Singleton<IdentityService>.Instance;

        private bool _isLoggedIn;
        private bool _isBusy;
        private UserViewModel _user;
        private ElementTheme _elementTheme = ThemeSelectorService.Theme;

        private string _versionDescription;
        private ICommand _switchThemeCommand;
        private RelayCommand _logInCommand;
        private RelayCommand _logOutCommand;

        public ElementTheme ElementTheme
        {
            get { return _elementTheme; }
            set { Set(ref _elementTheme, value); }
        }

        public string VersionDescription
        {
            get { return _versionDescription; }
            set { Set(ref _versionDescription, value); }
        }

        public bool IsLoggedIn
        {
            get { return _isLoggedIn; }
            set { Set(ref _isLoggedIn, value); }
        }

        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                Set(ref _isBusy, value);
                LogInCommand.OnCanExecuteChanged();
                LogOutCommand.OnCanExecuteChanged();
            }
        }

        public ICommand SwitchThemeCommand
        {
            get
            {
                if (_switchThemeCommand == null)
                {
                    _switchThemeCommand = new RelayCommand<ElementTheme>(
                        async (param) =>
                        {
                            ElementTheme = param;
                            await ThemeSelectorService.SetThemeAsync(param);
                        });
                }

                return _switchThemeCommand;
            }
        }

        public UserViewModel User
        {
            get { return _user; }
            set { Set(ref _user, value); }
        }

        public RelayCommand LogInCommand => _logInCommand ?? (_logInCommand = new RelayCommand(OnLogIn, () => !IsBusy));

        public RelayCommand LogOutCommand => _logOutCommand ?? (_logOutCommand = new RelayCommand(OnLogOut, () => !IsBusy));

        public SettingsViewModel()
        {
        }

        public async Task InitializeAsync()
        {
            IdentityService.LoggedIn += OnLoggedIn;
            IdentityService.LoggedOut += OnLoggeOut;
            VersionDescription = GetVersionDescription();
            UserDataService.UserDataUpdated += OnUserDataUpdated;
            IsLoggedIn = IdentityService.IsLoggedIn();
            User = await UserDataService.GetUserAsync();
        }

        public void UnregisterEvents()
        {
            IdentityService.LoggedIn -= OnLoggedIn;
            IdentityService.LoggedOut -= OnLoggeOut;
            UserDataService.UserDataUpdated -= OnUserDataUpdated;
        }

        private void OnUserDataUpdated(object sender, UserViewModel user)
        {
            User = user;
        }

        private void OnLoggedIn(object sender, EventArgs e)
        {
            IsLoggedIn = true;
            IsBusy = false;
        }

        private void OnLoggeOut(object sender, EventArgs e)
        {
            User = null;
            IsLoggedIn = false;
            IsBusy = false;
        }

        private string GetVersionDescription()
        {
            var appName = "AppDisplayName".GetLocalized();
            var package = Package.Current;
            var packageId = package.Id;
            var version = packageId.Version;

            return $"{appName} - {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
        }

        private async void OnLogIn()
        {
            IsBusy = true;
            var loginResult = await IdentityService.LoginAsync();
            if (loginResult != LoginResultType.Success)
            {
                await AuthenticationHelper.ShowLoginErrorAsync(loginResult);
            }
        }

        private async void OnLogOut()
        {
            IsBusy = true;
            await IdentityService.LogoutAsync();
        }
    }
}
