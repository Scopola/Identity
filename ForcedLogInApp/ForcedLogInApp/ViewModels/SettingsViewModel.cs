using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;

using ForcedLogInApp.Helpers;
using ForcedLogInApp.Models;
using ForcedLogInApp.Services;
using ForcedLogInApp.Views;
using Windows.ApplicationModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace ForcedLogInApp.ViewModels
{
    // TODO WTS: Add other settings as necessary. For help see https://github.com/Microsoft/WindowsTemplateStudio/blob/master/docs/pages/settings.md
    public class SettingsViewModel : Observable
    {
        private MicrosoftGraphService _microsoftGraphService => Singleton<MicrosoftGraphService>.Instance;

        private ElementTheme _elementTheme = ThemeSelectorService.Theme;

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

        private User _user;

        public User User
        {
            get { return _user; }

            set { Set(ref _user, value); }
        }

        private ImageSource _userPhoto;

        public ImageSource UserPhoto
        {
            get { return _userPhoto; }

            set { Set(ref _userPhoto, value); }
        }

        public SettingsViewModel()
        {
        }

        public async Task InitializeAsync()
        {
            VersionDescription = GetVersionDescription();
            User = await _microsoftGraphService.GetUserInfoAsync();
            UserPhoto = await _microsoftGraphService.GetUserPhoto();
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
            await Singleton<IdentityService>.Instance.LogoutAsync();
            var frame = new Frame();
            frame.Navigate(typeof(LogInPage));
            NavigationService.Frame = frame;
            Window.Current.Content = frame;
        }
    }
}
