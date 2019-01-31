using System;
using System.Threading.Tasks;
using OptionalLoginApp.Helpers;
using Windows.UI.Popups;

namespace OptionalLoginApp.Services
{
    internal class MessageDialogService
    {
        // Authentication Dialog Resources
        private static string _authenticationTitle = "DialogAuthenticationTitle".GetLocalized();
        private static string _activationLogin = "DialogAuthenticationActivationContent".GetLocalized();

        // Common Dialog Resources
        private static string _statusNoNetwork = "DialogNoNetworkAvailableContent".GetLocalized();
        private static string _statusUnknownError = "DialogStatusUnknownErrorContent".GetLocalized();

        // Command labels
        private static string _cancelButtonLabel = "DialogCommandCancel".GetLocalized();
        private static string _logInButtonLabel = "DialogCommandLogIn".GetLocalized();

        // Commands
        private static readonly UICommand _cancelCommand = new UICommand(_cancelButtonLabel);
        private static readonly UICommand _loginCommand = new UICommand(_logInButtonLabel);

        // Common Dialogs
        private readonly MessageDialog _noNetworkDialog
            = new MessageDialog(_statusNoNetwork, _authenticationTitle);

        private readonly MessageDialog _unknownErrorDialog
            = new MessageDialog(_statusUnknownError, _authenticationTitle);

        // Authentication Dialogs
        private readonly MessageDialog _logInDialog
            = new MessageDialog(_activationLogin, _authenticationTitle)
            { Commands = { _loginCommand, _cancelCommand } };

        internal async Task ShowUnknownErrorDialogAsync()
            => await _unknownErrorDialog.ShowAsync();

        internal async Task ShowNoNetworkDialogAsync()
            => await _noNetworkDialog.ShowAsync();
        internal async Task<bool> ShowLogInDialogAsync()
            => await _logInDialog.ShowAsync() == _loginCommand;
    }
}
