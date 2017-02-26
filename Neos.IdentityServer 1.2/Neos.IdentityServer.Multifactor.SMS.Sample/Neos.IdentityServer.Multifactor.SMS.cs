using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Neos.IdentityServer.MultiFactor;
using System.Globalization;

namespace Neos.IdentityServer.Multifactor.SMS
{
    public class SMSCall : IExternalOTPProvider
    {
        /// <summary>
        /// GetUserCodeByPhone basic demo 
        /// </summary>
        public int GetUserCodeWithExternalSystem(string upn, string phonenumber, string email, ExternalOTPProvider externalsys, CultureInfo culture)
        {
            // Compute and send your TOTP code and return his value if everything goes right
            if (true)
                return 1230;
            else
                return NotificationStatus.Error;  // return error
        }
    }
}


