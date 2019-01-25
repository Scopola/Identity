using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForcedLogInApp.Helpers
{
    public enum LoginResultType
    {
        Success,
        CancelledByUser,
        NoNetworkAvailable,
        UnknownError
    }
}
