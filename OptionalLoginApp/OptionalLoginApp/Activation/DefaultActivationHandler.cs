using System;
using System.Threading.Tasks;

using OptionalLoginApp.Services;

using Windows.ApplicationModel.Activation;

namespace OptionalLoginApp.Activation
{
    internal class DefaultActivationHandler : ActivationHandler<IActivatedEventArgs>
    {
        private readonly Type _navElement;

        public DefaultActivationHandler(Type navElement)
        {
            _navElement = navElement;
        }

        protected override async Task HandleInternalAsync(IActivatedEventArgs args)
        {
            // When the navigation stack isn't restored, navigate to the first page and configure
            // the new page by passing required information in the navigation parameter
            if (args is LaunchActivatedEventArgs launchArgs)
            {
                // Arguments can be used to control the behavior of the app
                // when it is launched through a secondary tile or through a toast.
                NavigationService.Navigate(_navElement, launchArgs.Arguments);
            }
            else
            {
                NavigationService.Navigate(_navElement);
            }

            await Task.CompletedTask;
        }

        protected override async Task<bool> CanHandleInternal(IActivatedEventArgs args)
        {
            // None of the ActivationHandlers has handled the app activation
            await Task.CompletedTask;
            return NavigationService.Frame.Content == null;
        }
    }
}
