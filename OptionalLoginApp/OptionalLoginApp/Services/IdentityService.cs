using System.Threading.Tasks;
using OptionalLoginApp.Core.Helpers;
using OptionalLoginApp.Core.Services;

namespace OptionalLoginApp.Services
{
    internal class IdentityService : IdentityServiceBase
    {
        private MessageDialogService _messageDialogService => Singleton<MessageDialogService>.Instance;

        internal async Task<bool> IsLoggedInWithDialogAsync()
        {
            if (IsLoggedIn())
            {
                return true;
            }

            var loginSelected = await _messageDialogService.ShowLogInDialogAsync();
            if (loginSelected)
            {
                var loginResult = await InternalLoginAsync();
                return await ShowLoginMessage(loginResult);
            }

            return false;
        }

        internal async Task LoginAsync()
        {
            var loginResult = await InternalLoginAsync();
            await ShowLoginMessage(loginResult);
        }

        private async Task<bool> ShowLoginMessage(LoginResultType loginResult)
        {
            switch (loginResult)
            {
                case LoginResultType.Success:
                    return true;
                case LoginResultType.NoNetworkAvailable:
                    await _messageDialogService.ShowNoNetworkDialogAsync();
                    break;
                case LoginResultType.UnknownError:
                    await _messageDialogService.ShowUnknownErrorDialogAsync();
                    break;
            }

            return false;
        }
    }
}
