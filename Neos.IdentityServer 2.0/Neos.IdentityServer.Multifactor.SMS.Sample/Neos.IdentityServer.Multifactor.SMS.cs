//******************************************************************************************************************************************************************************************//
// Copyright (c) 2017 Neos-Sdi (http://www.neos-sdi.com)                                                                                                                                    //                        
//                                                                                                                                                                                          //
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"),                                       //
// to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software,   //
// and to permit persons to whom the Software is furnished to do so, subject to the following conditions:                                                                                   //
//                                                                                                                                                                                          //
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.                                                           //
//                                                                                                                                                                                          //
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,                                      //
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,                            //
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.                               //
//                                                                                                                                                                                          //
// https://adfsmfa.codeplex.com                                                                                                                                                             //
// https://github.com/neos-sdi/adfsmfa                                                                                                                                                      //
//                                                                                                                                                                                          //
//******************************************************************************************************************************************************************************************//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Neos.IdentityServer.MultiFactor;
using System.Globalization;

namespace Neos.IdentityServer.Multifactor.SMS
{
    public class SMSCall : IExternalOTPProvider, IExternalOTPProvider2
    {
        /// <summary>
        /// GetUserCodeWithExternalSystem demo method
        /// </summary>
        public int GetUserCodeWithExternalSystem(string upn, string phonenumber, string email, ExternalOTPProvider externalsys, CultureInfo culture)
        {
            // Compute and send your TOTP code and return his value if everything goes right
            if (true)
                return 1230;
            else
                return (int)NotificationStatus.Error;  // return error
        }

        /// <summary>
        /// GetCodeWithExternalSystem demo method
        /// </summary>
        public NotificationStatus GetCodeWithExternalSystem(Registration reg, ExternalOTPProvider externalsys, CultureInfo culture, out int otp)
        {
            if (externalsys.IsTwoWay)
            {
                otp = Convert.ToInt32(NotificationStatus.ResponseSMSReply);
                return NotificationStatus.ResponseSMSReply;
            }
            else
            {
                otp = Convert.ToInt32("1230");
                return NotificationStatus.ResponseSMSOTP;
            }
        }
    }
}


