using Microsoft.ManagementConsole;
using Neos.IdentityServer.MultiFactor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neos.IdentityServer.MultiFactor.Admin
{
    public partial class MMCRegistration: Registration
    {
        /// <summary>
        /// implicit conversion to ResultNode
        /// </summary>
        public static implicit operator ResultNode(MMCRegistration registration)
        {
            if (registration == null)
                return null;
            else
            {
                ResultNode resultnode = new ResultNode();
                resultnode.DisplayName = registration.UPN;
                resultnode.SubItemDisplayNames.Add(registration.MailAddress);
                resultnode.SubItemDisplayNames.Add(registration.PhoneNumber);
                resultnode.SubItemDisplayNames.Add(registration.CreationDate.ToShortDateString());
                resultnode.SubItemDisplayNames.Add(registration.Enabled.ToString());
                resultnode.SubItemDisplayNames.Add(((int)registration.PreferredMethod).ToString());
                resultnode.SubItemDisplayNames.Add(registration.SecretKey);
                if (registration.Enabled)
                    resultnode.ImageIndex = 0;
                else
                    resultnode.ImageIndex = 1;
                return resultnode;
            }
        }

        /// <summary>
        /// implicit conversion from ResultNode
        /// </summary>
        public static implicit operator MMCRegistration(ResultNode resultnode)
        {
            if (resultnode == null)
                return null;
            else
            {
                MMCRegistration registration = new MMCRegistration();
                registration.UPN = resultnode.DisplayName;
                registration.MailAddress = resultnode.SubItemDisplayNames[1];
                registration.PhoneNumber = resultnode.SubItemDisplayNames[2];
                registration.CreationDate = Convert.ToDateTime(resultnode.SubItemDisplayNames[3]);
                registration.Enabled = bool.Parse(resultnode.SubItemDisplayNames[4]);
                registration.PreferredMethod = (RegistrationPreferredMethod)Convert.ToInt32(resultnode.SubItemDisplayNames[5]);
                registration.SecretKey = resultnode.SubItemDisplayNames[6];
                return registration;
            }
        }
    }
}
