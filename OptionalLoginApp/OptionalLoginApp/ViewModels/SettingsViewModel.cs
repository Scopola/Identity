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
        private RelayCommand _logInOutCommand;

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
                LogInOutCommand.OnCanExecuteChanged();
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

        public RelayCommand LogInOutCommand => _logInOutCommand ?? (_logInOutCommand = new RelayCommand(OnLogInOut));

        public SettingsViewModel()
        {
        }

        public async Task InitializeAsync()
        {
            _identityService.LoggedIn += OnLoggedIn;
            _identityService.LoggedOut += OnLoggedOut;
            VersionDescription = GetVersionDescription();
            await GetUserDataAsync();
        }

        public void UnregisterEvents()
        {
            _identityService.LoggedIn -= OnLoggedIn;
            _identityService.LoggedOut -= OnLoggedOut;
        }

        private async void OnLoggedIn(object sender, EventArgs e)
            => await GetUserDataAsync();

        private async void OnLoggedOut(object sender, EventArgs e)
            => await GetUserDataAsync();

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

        private async void OnLogInOut()
        {
            IsBusy = true;
            if (IsLoggedIn)
            {
                await _identityService.LogoutAsync();
            }
            else
            {
                await _identityService.LoginAsync();
            }
            IsBusy = false;
            
        }
    }
}
