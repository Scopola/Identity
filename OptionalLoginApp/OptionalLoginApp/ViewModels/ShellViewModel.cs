using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using OptionalLoginApp.Core.Helpers;
using OptionalLoginApp.Core.Services;
using OptionalLoginApp.Helpers;
using OptionalLoginApp.Services;
using OptionalLoginApp.Views;
using Windows.System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using WinUI = Microsoft.UI.Xaml.Controls;

namespace OptionalLoginApp.ViewModels
{
    public class ShellViewModel : Observable
    {
        private readonly KeyboardAccelerator _altLeftKeyboardAccelerator = BuildKeyboardAccelerator(VirtualKey.Left, VirtualKeyModifiers.Menu);
        private readonly KeyboardAccelerator _backKeyboardAccelerator = BuildKeyboardAccelerator(VirtualKey.GoBack);

        private bool _isBackEnabled;
        private bool _isLoggedIn;
        private bool _isAuthorized;
        private bool _isBusy;
        private IList<KeyboardAccelerator> _keyboardAccelerators;
        private WinUI.NavigationView _navigationView;
        private WinUI.NavigationViewItem _selected;
        private UserViewModel _user;
        private ICommand _loadedCommand;
        private ICommand _itemInvokedCommand;
        private RelayCommand _userProfileCommand;

        private IdentityService IdentityService => Singleton<IdentityService>.Instance;

        private UserDataService UserDataService => Singleton<UserDataService>.Instance;

        public bool IsBackEnabled
        {
            get { return _isBackEnabled; }
            set { Set(ref _isBackEnabled, value); }
        }

        public bool IsLoggedIn
        {
            get { return _isLoggedIn; }
            set { Set(ref _isLoggedIn, value); }
        }

        public bool IsAuthorized
        {
            get { return _isAuthorized; }
            set { Set(ref _isAuthorized, value); }
        }

        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                Set(ref _isBusy, value);
                UserProfileCommand.OnCanExecuteChanged();
            }
        }

        public WinUI.NavigationViewItem Selected
        {
            get { return _selected; }
            set { Set(ref _selected, value); }
        }

        public UserViewModel User
        {
            get { return _user; }
            set { Set(ref _user, value); }
        }

        public ICommand LoadedCommand => _loadedCommand ?? (_loadedCommand = new RelayCommand(OnLoaded));

        public ICommand ItemInvokedCommand => _itemInvokedCommand ?? (_itemInvokedCommand = new RelayCommand<WinUI.NavigationViewItemInvokedEventArgs>(OnItemInvoked));

        public RelayCommand UserProfileCommand => _userProfileCommand ?? (_userProfileCommand = new RelayCommand(OnUserProfile, () => !IsBusy));

        public ShellViewModel()
        {
        }

        public void Initialize(Frame frame, WinUI.NavigationView navigationView, IList<KeyboardAccelerator> keyboardAccelerators)
        {
            _navigationView = navigationView;
            _keyboardAccelerators = keyboardAccelerators;
            NavigationService.Frame = frame;
            NavigationService.Navigated += Frame_Navigated;
            _navigationView.BackRequested += OnBackRequested;
            IdentityService.LoggedIn += OnLoggedIn;
            IdentityService.LoggedOut += OnLoggedOut;
            UserDataService.UserDataUpdated += OnUserDataUpdated;
        }

        private async void OnLoaded()
        {
            // Keyboard accelerators are added here to avoid showing 'Alt + left' tooltip on the page.
            // More info on tracking issue https://github.com/Microsoft/microsoft-ui-xaml/issues/8
            _keyboardAccelerators.Add(_altLeftKeyboardAccelerator);
            _keyboardAccelerators.Add(_backKeyboardAccelerator);
            IsLoggedIn = IdentityService.IsLoggedIn();
            IsAuthorized = IsLoggedIn && IdentityService.IsAuthorized();
            User = await UserDataService.GetUserAsync();
        }

        private void OnUserDataUpdated(object sender, UserViewModel user)
        {
            User = user;
        }

        private void OnLoggedIn(object sender, EventArgs e)
        {
            IsLoggedIn = true;
            IsAuthorized = IsLoggedIn && IdentityService.IsAuthorized();
            IsBusy = false;
        }

        private void OnLoggedOut(object sender, EventArgs e)
        {
            User = null;
            IsLoggedIn = false;
            foreach (var backStack in NavigationService.Frame.BackStack)
            {
                var isRestricted = Attribute.IsDefined(backStack.SourcePageType, typeof(Restricted));
                if (isRestricted)
                {
                    NavigationService.Frame.BackStack.Remove(backStack);
                }
            }

            var currentPage = NavigationService.Frame.Content as Page;
            var isCurrentPageRestricted = Attribute.IsDefined(currentPage.GetType(), typeof(Restricted));
            if (isCurrentPageRestricted)
            {
                NavigationService.GoBack();
            }            
        }

        private async void OnUserProfile()
        {
            if (IsLoggedIn)
            {
                NavigationService.Navigate<SettingsPage>();
            }
            else
            {
                IsBusy = true;
                var loginResult = await IdentityService.LoginAsync();
                if (loginResult != LoginResultType.Success)
                {
                    await AuthenticationHelper.ShowLoginErrorAsync(loginResult);
                }
            }
        }

        private void OnItemInvoked(WinUI.NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked)
            {
                NavigationService.Navigate(typeof(SettingsPage));
                return;
            }

            var item = _navigationView.MenuItems
                            .OfType<WinUI.NavigationViewItem>()
                            .First(menuItem => (string)menuItem.Content == (string)args.InvokedItem);
            var pageType = item.GetValue(NavHelper.NavigateToProperty) as Type;
            NavigationService.Navigate(pageType);
        }

        private void OnBackRequested(WinUI.NavigationView sender, WinUI.NavigationViewBackRequestedEventArgs args)
        {
            NavigationService.GoBack();
        }

        private void Frame_Navigated(object sender, NavigationEventArgs e)
        {
            IsBackEnabled = NavigationService.CanGoBack;
            if (e.SourcePageType == typeof(SettingsPage))
            {
                Selected = _navigationView.SettingsItem as WinUI.NavigationViewItem;
                return;
            }

            Selected = _navigationView.MenuItems
                            .OfType<WinUI.NavigationViewItem>()
                            .FirstOrDefault(menuItem => IsMenuItemForPageType(menuItem, e.SourcePageType));
        }

        private bool IsMenuItemForPageType(WinUI.NavigationViewItem menuItem, Type sourcePageType)
        {
            var pageType = menuItem.GetValue(NavHelper.NavigateToProperty) as Type;
            return pageType == sourcePageType;
        }

        private static KeyboardAccelerator BuildKeyboardAccelerator(VirtualKey key, VirtualKeyModifiers? modifiers = null)
        {
            var keyboardAccelerator = new KeyboardAccelerator() { Key = key };
            if (modifiers.HasValue)
            {
                keyboardAccelerator.Modifiers = modifiers.Value;
            }

            keyboardAccelerator.Invoked += OnKeyboardAcceleratorInvoked;
            return keyboardAccelerator;
        }

        private static void OnKeyboardAcceleratorInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            var result = NavigationService.GoBack();
            args.Handled = result;
        }
    }
}
