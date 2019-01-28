using System;
using Microsoft.Identity.Client;

namespace ForcedLogInApp.Core.Helpers
{
    public static class AuthenticationResultExtensions
    {
        public static bool IsAccessTokenExpired(this AuthenticationResult authenticationResult)
        {
            var missingTime = authenticationResult.ExpiresOn - DateTime.Now.ToUniversalTime();
            return missingTime.TotalSeconds <= 0;
        }
    }
}
