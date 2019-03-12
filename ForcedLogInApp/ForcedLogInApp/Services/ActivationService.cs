using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using ForcedLogInApp.Activation;
using ForcedLogInApp.Core.Helpers;
using ForcedLogInApp.Core.Services;
using ForcedLogInApp.Views;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ForcedLogInApp.Services
{
    // For more information on application activation see https://github.com/Microsoft/WindowsTemplateStudio/blob/master/docs/activation.md
    internal class ActivationService
    {
        private readonly App _app;
        private readonly Type _defaultNavItem;
        private Lazy<UIElement> _shell;

        private object _lastActivationArgs;
        private IdentityService _identityService => Singleton<IdentityService>.Instance;

        public ActivationService(App app, Type defaultNavItem, Lazy<UIElement> shell = null)
        {
            _app = app;
            _shell = shell;
            _defaultNavItem = defaultNavItem;
            _identityService.LoggedIn += OnLoggedIn;
        }

        public async Task ActivateAsync(object activationArgs)
        {
            if (IsInteractive(activationArgs))
            {
                // Initialize things like registering background task before the app is loaded
                await InitializeAsync();
                _identityService.InitializeWithAadAndPersonalMsAccounts();
                var silentLoginSuccess = await _identityService.SilentLoginAsync();
                if (!silentLoginSuccess || !_identityService.IsAuthorized())
                {
                    await RedirectLoginPageAsync();
                }

                // Do not repeat app initialization when the Window already has content,
                // just ensure that the window is active
                if (Window.Current.Content == null)
                {
                    // Create a Frame to act as the navigation context and navigate to the first page
                    Window.Current.Content = _shell?.Value ?? new Frame();
                }
            }            

            if (_identityService.IsLoggedIn())
            {
                await HandleActivationAsync(activationArgs);
            }

            _lastActivationArgs = activationArgs;

            if (IsInteractive(activationArgs))
            {
                // Ensure the current window is active
                Window.Current.Activate();

                // Tasks after activation
                await StartupAsync();
            }
        }

        public void SetShell(Lazy<UIElement> shell)
        {
            _shell = shell;
        }

        public async Task RedirectLoginPageAsync()
        {
            var frame = new Frame();
            NavigationService.Frame = frame;
            Window.Current.Content = frame;
            await ThemeSelectorService.SetRequestedThemeAsync();
            NavigationService.Navigate<LogInPage>();
        }

        private async void OnLoggedIn(object sender, EventArgs e)
        {
            Window.Current.Content = _shell?.Value ?? new Frame();
            await ThemeSelectorService.SetRequestedThemeAsync();
            await HandleActivationAsync(_lastActivationArgs);
            _lastActivationArgs = null;
        }

        private async Task InitializeAsync()
        {
            if (Window.Current.Content == null)
            {
                await ThemeSelectorService.InitializeAsync();                
            }
        }

        private async Task HandleActivationAsync(object activationArgs)
        {
            var activationHandler = GetActivationHandlers()
                                                .FirstOrDefault(h => h.CanHandle(activationArgs));

            if (activationHandler != null)
            {
                await activationHandler.HandleAsync(activationArgs);
            }

            if (IsInteractive(activationArgs))
            {
                var defaultHandler = new DefaultLaunchActivationHandler(_defaultNavItem);
                if (defaultHandler.CanHandle(activationArgs))
                {
                    await defaultHandler.HandleAsync(activationArgs);
                }
            }
        }        

        private async Task StartupAsync()
        {
            await ThemeSelectorService.SetRequestedThemeAsync();
            await UserActivityService.AddSampleUserActivity();
        }

        private IEnumerable<ActivationHandler> GetActivationHandlers()
        {
            yield return Singleton<SchemeActivationHandler>.Instance;
        }

        private bool IsInteractive(object args)
        {
            return args is IActivatedEventArgs;
        }
    }
}
