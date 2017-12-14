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
using PhoneNumbers;
using Neos.IdentityServer.MultiFactor.SMS.Resources;
using System.Globalization;

namespace Neos.IdentityServer.Multifactor.SMS
{
    public class SMSCall: IExternalOTPProvider, IExternalOTPProvider2
    {
        /// <summary>
        /// GetUserCodeWithExternalSystem method implementation for Azure MFA 
        /// </summary>
        public int GetUserCodeWithExternalSystem(string upn, string phonenumber, string smstext, ExternalOTPProvider externalsys, CultureInfo culture)
        {
            azure_strings.Culture = culture;
            String NumberStr = phonenumber;
            int CountryCode = 0;
            ulong NationalNumber = 0;
            string extension = string.Empty;

            PhoneNumberUtil phoneUtil = PhoneNumberUtil.GetInstance();
            PhoneNumber NumberProto = phoneUtil.Parse(NumberStr, culture.TwoLetterISOLanguageName.ToUpper());
            CountryCode = NumberProto.CountryCode;
            NationalNumber = NumberProto.NationalNumber;
            if (NumberProto.HasExtension)
                extension = NumberProto.Extension;

            PhoneFactor.Initialize(externalsys);
            PhoneFactorParams Params = new PhoneFactorParams();
            Params.Username = upn;

            Params.CountryCode = CountryCode.ToString();
            Params.Phone = NationalNumber.ToString();
            Params.Extension = extension;
            Params.ApplicationName = "IdentityServer";
            Params.Sha1Salt = externalsys.Sha1Salt;

            if (externalsys.IsTwoWay)
            {
                Params.SmsText = string.Format(azure_strings.SMSTwoWayMessage, externalsys.Company);
                Params.Mode = PhoneFactor.MODE_SMS_TWO_WAY_OTP;
            }
            else
            {
                Params.SmsText = string.Format(azure_strings.SMSMessage, externalsys.Company);
                Params.Mode = PhoneFactor.MODE_SMS_ONE_WAY_OTP;
            } 

            int callStatus;
            int errorId;
            string otp = string.Empty;
            if (PhoneFactor.Authenticate(Params, out otp, out callStatus, out errorId, externalsys.Timeout))
                if (externalsys.IsTwoWay)
                    return (int)NotificationStatus.ResponseSMSReply;
                else
                    return Convert.ToInt32(otp);
            else
                return (int)NotificationStatus.Error;
        }

        /// <summary>
        /// GetCodeWithExternalSystem method implmentation
        /// </summary>
        public NotificationStatus GetCodeWithExternalSystem(Registration reg, ExternalOTPProvider externalsys, CultureInfo culture, out int otp)
        {
            azure_strings.Culture = culture;
            String NumberStr = reg.PhoneNumber;
            int CountryCode = 0;
            ulong NationalNumber = 0;
            string extension = string.Empty;

            PhoneNumberUtil phoneUtil = PhoneNumberUtil.GetInstance();
            PhoneNumber NumberProto = phoneUtil.Parse(NumberStr, culture.TwoLetterISOLanguageName.ToUpper());
            CountryCode = NumberProto.CountryCode;
            NationalNumber = NumberProto.NationalNumber;
            if (NumberProto.HasExtension)
                extension = NumberProto.Extension;

            PhoneFactor.Initialize(externalsys);
            PhoneFactorParams Params = new PhoneFactorParams();
            Params.Username = reg.UPN;

            Params.CountryCode = CountryCode.ToString();
            Params.Phone = NationalNumber.ToString();
            Params.Extension = extension;
            Params.ApplicationName = "IdentityServer";
            Params.Sha1Salt = externalsys.Sha1Salt;

            if (externalsys.IsTwoWay)
            {
                Params.SmsText = string.Format(azure_strings.SMSTwoWayMessage, externalsys.Company);
                Params.Mode = PhoneFactor.MODE_SMS_TWO_WAY_OTP;
            }
            else
            {
                Params.SmsText = string.Format(azure_strings.SMSMessage, externalsys.Company);
                Params.Mode = PhoneFactor.MODE_SMS_ONE_WAY_OTP;
            }

            int callStatus;
            int errorId;
            string xotp = string.Empty;
            if (PhoneFactor.Authenticate(Params, out xotp, out callStatus, out errorId, externalsys.Timeout))
                if (externalsys.IsTwoWay)
                {
                    otp = Convert.ToInt32(NotificationStatus.ResponseSMSReply);
                    return NotificationStatus.ResponseSMSReply;
                }
                else
                {
                    otp = Convert.ToInt32(xotp);
                    return NotificationStatus.ResponseSMSOTP;
                }
            else
            {
                otp = 0;
                return NotificationStatus.Error;
            }
        }
    }
}
