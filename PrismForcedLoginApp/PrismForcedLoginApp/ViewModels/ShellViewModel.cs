using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using PrismForcedLoginApp.Core.Services;
using PrismForcedLoginApp.Helpers;
using PrismForcedLoginApp.Services;
using PrismForcedLoginApp.Views;

using Prism.Commands;
using Prism.Windows.Mvvm;
using Prism.Windows.Navigation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

using WinUI = Microsoft.UI.Xaml.Controls;
using PrismForcedLoginApp.Core.Helpers;

namespace PrismForcedLoginApp.ViewModels
{
    public class ShellViewModel : ViewModelBase
    {
        private bool _isBusy;
        private bool _isLoggedIn;
        private string _statusMessage;
        private UserViewModel _user;
        private INavigationService _navigationService;
        private IIdentityService _identityService;
        private IUserDataService _userDataService;
        private WinUI.NavigationView _navigationView;
        private bool _isBackEnabled;
        private WinUI.NavigationViewItem _selected;

        public ICommand ItemInvokedCommand { get; }

        public DelegateCommand LoginCommand { get; }

        public ICommand UserProfileCommand { get; }

        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                SetProperty(ref _isBusy, value);
                LoginCommand.RaiseCanExecuteChanged();
            }
        }

        public bool IsLoggedIn
        {
            get { return _isLoggedIn; }
            set { SetProperty(ref _isLoggedIn, value); }
        }

        public string StatusMessage
        {
            get { return _statusMessage; }
            set { SetProperty(ref _statusMessage, value); }
        }

        public UserViewModel User
        {
            get { return _user; }
            set { SetProperty(ref _user, value); }
        }

        public bool IsBackEnabled
        {
            get { return _isBackEnabled; }
            set { SetProperty(ref _isBackEnabled, value); }
        }

        public WinUI.NavigationViewItem Selected
        {
            get { return _selected; }
            set { SetProperty(ref _selected, value); }
        }

        public ShellViewModel(INavigationService navigationServiceInstance, IUserDataService userDataService, IIdentityService identityService)
        {
            _navigationService = navigationServiceInstance;
            _userDataService = userDataService;
            _identityService = identityService;
            ItemInvokedCommand = new DelegateCommand<WinUI.NavigationViewItemInvokedEventArgs>(OnItemInvoked);
            LoginCommand = new DelegateCommand(OnLogin, () => !IsBusy);
            UserProfileCommand = new DelegateCommand(OnUserProfile);
        }

        public async void Initialize(Frame frame, WinUI.NavigationView navigationView)
        {
            _navigationView = navigationView;
            frame.NavigationFailed += (sender, e) =>
            {
                throw e.Exception;
            };
            frame.Navigated += Frame_Navigated;
            _navigationView.BackRequested += OnBackRequested;
            _identityService.LoggedIn += OnLoggedIn;
            _identityService.LoggedOut += OnLoggedOut;
            IsLoggedIn = _identityService.IsLoggedIn();
            await GetUserData();
        }

        private async void OnLoggedIn(object sender, EventArgs e)
        {
            IsLoggedIn = true;
            await GetUserData();
        }

        private void OnLoggedOut(object sender, EventArgs e)
        {
            IsLoggedIn = false;
            _navigationService.Navigate(PageTokens.MainPage, null);
            _navigationService.ClearHistory();
        }

        private async Task GetUserData()
        {
            if (IsLoggedIn)
            {
                User = await _userDataService.GetUserFromCacheAsync();
                User = await _userDataService.GetUserFromGraphApiAsync();
            }
        }

        private void OnItemInvoked(WinUI.NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked)
            {
                _navigationService.Navigate("Settings", null);
                return;
            }

            var item = _navigationView.MenuItems
                            .OfType<WinUI.NavigationViewItem>()
                            .First(menuItem => (string)menuItem.Content == (string)args.InvokedItem);
            var pageKey = item.GetValue(NavHelper.NavigateToProperty) as string;
            _navigationService.Navigate(pageKey, null);
        }

        private async void OnLogin()
        {
            //await _identityService.LoginAsync();
            IsBusy = true;
            StatusMessage = string.Empty;
            var loginResult = await _identityService.LoginAsync();
            StatusMessage = GetStatusMessage(loginResult);
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

        private void OnUserProfile()
        {
            _navigationService.Navigate(PageTokens.SettingsPage, null);
        }

        private void Frame_Navigated(object sender, NavigationEventArgs e)
        {
            IsBackEnabled = _navigationService.CanGoBack();
            if (e.SourcePageType == typeof(SettingsPage))
            {
                Selected = _navigationView.SettingsItem as WinUI.NavigationViewItem;
                return;
            }

            Selected = _navigationView.MenuItems
                            .OfType<WinUI.NavigationViewItem>()
                            .FirstOrDefault(menuItem => IsMenuItemForPageType(menuItem, e.SourcePageType));
        }

        private void OnBackRequested(WinUI.NavigationView sender, WinUI.NavigationViewBackRequestedEventArgs args)
        {
            _navigationService.GoBack();
        }

        private bool IsMenuItemForPageType(WinUI.NavigationViewItem menuItem, Type sourcePageType)
        {
            var sourcePageKey = sourcePageType.Name;
            sourcePageKey = sourcePageKey.Substring(0, sourcePageKey.Length - 4);
            var pageKey = menuItem.GetValue(NavHelper.NavigateToProperty) as string;
            return pageKey == sourcePageKey;
        }
    }
}
