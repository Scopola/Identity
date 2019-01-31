using System;
using System.Threading.Tasks;
using OptionalLoginApp.Core.Helpers;
using OptionalLoginApp.Services;
using Windows.ApplicationModel.Activation;

namespace OptionalLoginApp.Activation
{
    internal class SchemeActivationHandler : ActivationHandler<ProtocolActivatedEventArgs>
    {
        private IdentityService _identityService => Singleton<IdentityService>.Instance;

        // By default, this handler expects URIs of the format 'optionalloginapp:sample?paramName1=paramValue1&paramName2=paramValue2'
        protected override async Task HandleInternalAsync(ProtocolActivatedEventArgs args)
        {            
            var data = new SchemeActivationData(args.Uri);
            NavigationService.Navigate(data.PageType, data.Parameters);            

            await Task.CompletedTask;
        }

        protected override async Task<bool> CanHandleInternal(ProtocolActivatedEventArgs args)
        {            
            var isLoggedIn = await _identityService.IsLoggedInWithDialogAsync();            
            var data = new SchemeActivationData(args.Uri);
            return isLoggedIn && data.IsValid;
        }
    }
}
