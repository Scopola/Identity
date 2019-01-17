using System;
using Microsoft.Identity.Client;

namespace ForcedLogInApp.Helpers
{
    public class LoginResult
    {
        public bool HasNetwork { get; set; } = true;

        public AuthenticationResult Result { get; set; }

        public bool Success
        {
            get
            {
                return HasNetwork && Result != null;
            }
        }
    }
}
