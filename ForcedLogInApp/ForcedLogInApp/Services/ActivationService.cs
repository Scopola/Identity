using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using ForcedLogInApp.Activation;
using ForcedLogInApp.Helpers;
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

        // Start #AddWithdIdentity
        private object _lastActivationArgs;
        private MicrosoftGraphService _microsoftGraphService => Singleton<MicrosoftGraphService>.Instance;
        private IdentityService _identityService => Singleton<IdentityService>.Instance;
        // End

        public ActivationService(App app, Type defaultNavItem, Lazy<UIElement> shell = null)
        {
            _app = app;
            _shell = shell;
            _defaultNavItem = defaultNavItem;
            // Start #AddWithdIdentity
            _identityService.LoggedIn += OnLoggedIn;
            // End
        }

        public async Task ActivateAsync(object activationArgs)
        {
            if (IsInteractive(activationArgs))
            {
                // Initialize things like registering background task before the app is loaded
                await InitializeAsync();

                // Do not repeat app initialization when the Window already has content,
                // just ensure that the window is active
                if (Window.Current.Content == null)
                {
                    // Create a Frame to act as the navigation context and navigate to the first page
                    Window.Current.Content = _shell?.Value ?? new Frame();
                }
            }

            if (IsActivationEnabled())
            {
                await HandleActivationAsync(activationArgs);
            }

            // Start #AddWithdIdentity
            _lastActivationArgs = activationArgs;
            // End

            // Ensure the current window is active
            Window.Current.Activate();

            // Tasks after activation
            await StartupAsync();
        }

        public void SetShell(Lazy<UIElement> shell)
        {
            _shell = shell;
        }

        // Start #AddWithdIdentity
        private async void OnLoggedIn(object sender, EventArgs e)
        {
            Window.Current.Content = _shell?.Value ?? new Frame();
            await HandleActivationAsync(_lastActivationArgs);
            _lastActivationArgs = null;
        }
        // End

        private async Task InitializeAsync()
        {
            if (Window.Current.Content == null)
            {
                await ThemeSelectorService.InitializeAsync();
                // Start #AddWithdIdentity
                _microsoftGraphService.Initialize();
                await _identityService.LoginWithCommonAuthorityAsync();
                // End
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

        private bool IsActivationEnabled()
        {
            // Start#AddWithdIdentity
            if (!_identityService.IsLoggedIn())
            {
                return false;
            }
            // End

            return true;
        }

        private async Task StartupAsync()
        {
            await ThemeSelectorService.SetRequestedThemeAsync();
            // TODO WTS: This is a sample to demonstrate how to add a UserActivity. Please adapt and move this method call to where you consider convenient in your app.
            await UserActivityService.AddSampleUserActivity();
        }

        private IEnumerable<ActivationHandler> GetActivationHandlers()
        {
            yield return Singleton<ToastNotificationsService>.Instance;
            yield return Singleton<SchemeActivationHandler>.Instance;
        }

        private bool IsInteractive(object args)
        {
            return args is IActivatedEventArgs;
        }
    }
}
