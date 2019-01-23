using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Identity.Client;

namespace ForcedLogInApp.Helpers
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
