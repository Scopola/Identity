using System.Threading.Tasks;
using System.Windows.Input;
using ForcedLogInApp.Core.Helpers;
using ForcedLogInApp.Core.Services;
using ForcedLogInApp.Helpers;
using ForcedLogInApp.Services;
using Windows.ApplicationModel;
using Windows.UI.Xaml;

namespace ForcedLogInApp.ViewModels
{
    public class SettingsViewModel : Observable
    {
        private UserDataService _userDataService => Singleton<UserDataService>.Instance;
        private IdentityService _identityService => Singleton<IdentityService>.Instance;

        private ElementTheme _elementTheme = ThemeSelectorService.Theme;
        private UserViewModel _user;

        public ElementTheme ElementTheme
        {
            get { return _elementTheme; }

            set { Set(ref _elementTheme, value); }
        }

        private string _versionDescription;

        public string VersionDescription
        {
            get { return _versionDescription; }

            set { Set(ref _versionDescription, value); }
        }

        private ICommand _switchThemeCommand;

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

        private ICommand _logoutCommand;

        public ICommand LogoutCommand => _logoutCommand ?? (_logoutCommand = new RelayCommand(OnLogout));

        public UserViewModel User
        {
            get { return _user; }
            set { Set(ref _user, value); }
        }

        public SettingsViewModel()
        {
        }

        public async Task InitializeAsync()
        {
            VersionDescription = GetVersionDescription();
            User = await _userDataService.GetUserFromCacheAsync();
        }

        private string GetVersionDescription()
        {
            var appName = "AppDisplayName".GetLocalized();
            var package = Package.Current;
            var packageId = package.Id;
            var version = packageId.Version;

            return $"{appName} - {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
        }

        private async void OnLogout()
        {
            await _identityService.LogoutAsync();
        }
    }
}
