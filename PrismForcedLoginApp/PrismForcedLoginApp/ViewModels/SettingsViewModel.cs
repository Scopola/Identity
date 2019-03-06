using System;
using System.Collections.Generic;
using System.Windows.Input;
using PrismForcedLoginApp.Core.Services;
using PrismForcedLoginApp.Helpers;
using PrismForcedLoginApp.Services;

using Prism.Commands;
using Prism.Windows.Mvvm;
using Prism.Windows.Navigation;

using Windows.ApplicationModel;
using Windows.UI.Xaml;

namespace PrismForcedLoginApp.ViewModels
{
    // TODO WTS: Add other settings as necessary. For help see https://github.com/Microsoft/WindowsTemplateStudio/blob/master/docs/pages/settings.md
    public class SettingsViewModel : ViewModelBase
    {
        private IIdentityService _identityService;
        private UserDataService _userDataService;
        private ElementTheme _elementTheme = ThemeSelectorService.Theme;
        private UserViewModel _user;

        public ElementTheme ElementTheme
        {
            get { return _elementTheme; }

            set { SetProperty(ref _elementTheme, value); }
        }

        private string _versionDescription;

        public string VersionDescription
        {
            get { return _versionDescription; }

            set { SetProperty(ref _versionDescription, value); }
        }

        private ICommand _switchThemeCommand;

        public ICommand SwitchThemeCommand
        {
            get
            {
                if (_switchThemeCommand == null)
                {
                    _switchThemeCommand = new DelegateCommand<object>(
                        async (param) =>
                        {
                            ElementTheme = (ElementTheme)param;
                            await ThemeSelectorService.SetThemeAsync((ElementTheme)param);
                        });
                }

                return _switchThemeCommand;
            }
        }

        public ICommand LogoutCommand { get; }

        public UserViewModel User
        {
            get { return _user; }
            set { SetProperty(ref _user, value); }
        }

        public SettingsViewModel(IIdentityService identityService, UserDataService userDataService)
        {
            _identityService = identityService;
            _userDataService = userDataService;
            LogoutCommand = new DelegateCommand(OnLogout);
        }

        private async void OnLogout()
        {
            await _identityService.LogoutAsync();
        }

        public override async void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            base.OnNavigatedTo(e, viewModelState);

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
    }
}
