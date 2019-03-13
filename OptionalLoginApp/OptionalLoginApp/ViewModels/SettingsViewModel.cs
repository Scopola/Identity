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
        private UserDataService _userDataService => Singleton<UserDataService>.Instance;
        private IdentityService _identityService => Singleton<IdentityService>.Instance;

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
            _identityService.LoggedIn += OnLoggedIn;
            _identityService.LoggedOut += OnLoggeOut;
            VersionDescription = GetVersionDescription();
            await GetUserDataAsync();
        }

        public void UnregisterEvents()
        {
            _identityService.LoggedIn -= OnLoggedIn;
            _identityService.LoggedOut -= OnLoggeOut;
        }

        private async void OnLoggedIn(object sender, EventArgs e)
        {
            await GetUserDataAsync();
            IsBusy = false;
        }

        private async void OnLoggeOut(object sender, EventArgs e)
        {
            await GetUserDataAsync();
            IsBusy = false;
        }

        private async Task GetUserDataAsync()
        {
            IsLoggedIn = _identityService.IsLoggedIn();
            if (IsLoggedIn)
            {
                User = await _userDataService.GetUserFromCacheAsync();
                User = await _userDataService.GetUserFromGraphApiAsync();
                if (User == null)
                {
                    User = _userDataService.GetDefaultUserData();
                }
            }
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
            var loginResult = await _identityService.LoginAsync();
            if (loginResult != LoginResultType.Success)
            {
                await AuthenticationHelper.ShowLoginErrorAsync(loginResult);
            }
        }

        private async void OnLogOut()
        {
            IsBusy = true;
            await _identityService.LogoutAsync();
        }
    }
}
