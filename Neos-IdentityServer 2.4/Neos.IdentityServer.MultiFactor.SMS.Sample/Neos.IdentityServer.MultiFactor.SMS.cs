//******************************************************************************************************************************************************************************************//
// Copyright (c) 2019 Neos-Sdi (http://www.neos-sdi.com)                                                                                                                                    //                        
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
//                                                                                                                                                                                          //
// A very small sample using legacy interface to deal with an hardware appliance (8 sim cards : "S100 GATEWAY" see : www.iQSim.com)                                                         //
//                                                                                                                                                                                          //
// You also can develop a full Provider with IExternalProvide interface (right way), see other samples                                                                                      //
//                                                                                                                                                                                          //
// Not Deployed with msi solution (only in source code)                                                                                                                                     //
//                                                                                                                                                                                          //
//*******************************************************************************************************************************************************************************************//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Neos.IdentityServer.MultiFactor;
using System.Globalization;
using Neos.IdentityServer.MultiFactor.SMS.Resources;
using System.Diagnostics;

namespace Neos.IdentityServer.MultiFactor.SMS
{
    public class SMSCall : IExternalOTPProvider
    {
        /// <summary>
        /// GetUserCodeWithExternalSystem method implementation for Azure MFA 
        /// </summary>
        public int GetUserCodeWithExternalSystem(string upn, string phonenumber, string smstext, ExternalOTPProvider externalsys, CultureInfo culture)
        {
            SMS_strings.Culture = culture;
            SMSRuntime.Initialize(externalsys);
            SMSParams Params = new SMSParams();
            try
            {
                int otp = GetRandomOTP();

                Params.IPhost = SMSRuntime.IPhost;
                Params.Password = SMSRuntime.Password;
                Params.SMSText = string.Format(SMS_strings.SMSMessage, externalsys.Company, otp);
                Params.PhoneNumber = phonenumber;

                int errorId;

                if (SMSRuntime.Authenticate(Params, out errorId, externalsys.Timeout))
                    return Convert.ToInt32(otp);
                else
                    return (int)AuthenticationResponseKind.Error;
            }
            catch (Exception ex)
            {
                Log.WriteEntry("SMS SendMessage : \r" + ex.Message, EventLogEntryType.Error, 10000);
                return (int)AuthenticationResponseKind.Error;
            }

        }

        /// <summary>
        /// GetCodeWithExternalSystem method implmentation
        /// </summary>
        public AuthenticationResponseKind GetCodeWithExternalSystem(Registration reg, ExternalOTPProvider externalsys, CultureInfo culture, out int otp)
        {
            SMS_strings.Culture = culture;
            SMSRuntime.Initialize(externalsys);
            SMSParams Params = new SMSParams();

            try
            {
                int zotp = GetRandomOTP();

                Params.IPhost = SMSRuntime.IPhost;
                Params.Password = SMSRuntime.Password;
                Params.SMSText = string.Format(SMS_strings.SMSMessage, externalsys.Company, zotp);
                Params.PhoneNumber = reg.PhoneNumber;

                int errorId;

                if (SMSRuntime.Authenticate(Params, out errorId, externalsys.Timeout))
                {
                    otp = zotp;
                    return AuthenticationResponseKind.SmsOTP;
                }
                else
                {
                    otp = 0;
                    return (int)AuthenticationResponseKind.Error;
                }
            }
            catch (Exception ex)
            {
                Log.WriteEntry("SMS SendMessage : \r" + ex.Message, EventLogEntryType.Error, 10000);
                otp = 0;
                return (int)AuthenticationResponseKind.Error;
            }
        }

        /// <summary>
        /// GetRandomOTP method implementation
        /// </summary>
        private static int GetRandomOTP()
        {
            Random random = new Random();
            return random.Next(1, 999999);
        }

    }
}
