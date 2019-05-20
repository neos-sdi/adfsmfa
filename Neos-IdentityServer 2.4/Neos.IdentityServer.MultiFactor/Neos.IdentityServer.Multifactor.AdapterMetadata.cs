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
using System.Collections.Generic;
using Microsoft.IdentityServer.Web.Authentication.External;

namespace Neos.IdentityServer.MultiFactor
{
    public class AuthenticationAdapterMetadata : IAuthenticationAdapterMetadata
    {
        /// <summary>
        /// AuthenticationAdapterMetadata constructor
        /// </summary>
        public AuthenticationAdapterMetadata()
        {

        }

        /// <summary>
        /// AdminName property implementation
        /// </summary>
        public string AdminName
        {
            get { return "Multi Factor Authentication Extension"; }
        }

        /// <summary>
        /// AuthenticationMethods property implementation
        /// </summary>
        public string[] AuthenticationMethods
        {
            get 
            {
                return new string[] 
                { 
                                    "http://schemas.microsoft.com/ws/2012/12/authmethod/none",
                                    "http://schemas.microsoft.com/ws/2012/12/authmethod/otp", 
                                    "http://schemas.microsoft.com/ws/2012/12/authmethod/email",
                                    "http://schemas.microsoft.com/ws/2012/12/authmethod/sms",
                                    "http://schemas.microsoft.com/ws/2012/12/authmethod/smsotp",      
                                    "http://schemas.microsoft.com/ws/2012/12/authmethod/smsreply",
                                    "http://schemas.microsoft.com/ws/2012/12/authmethod/phoneappnotification",
                                    "http://schemas.microsoft.com/ws/2012/12/authmethod/phoneconfirmation",
                                    "http://schemas.microsoft.com/ws/2012/12/authmethod/phoneapplication",
                                    "http://schemas.microsoft.com/ws/2012/12/authmethod/phoneconfirmation",
                                    "http://schemas.microsoft.com/ws/2012/12/authmethod/voicebiometrics",
                                    "http://schemas.microsoft.com/ws/2012/12/authmethod/kba",
                                    "http://schemas.microsoft.com/ws/2012/12/authmethod/windowshello",
                                    "http://schemas.microsoft.com/ws/2012/12/authmethod/fido",
                                    "http://schemas.microsoft.com/ws/2012/12/authmethod/webauthN"
                };
            } 
        }

        /// <summary>
        /// AvailableLcids property implementation
        /// </summary>
        public int[] AvailableLcids
        {
            get { return new int[] { 1025, 1026, 5146, 1029, 1030, 1031, 1032, 2057, 1033, 1034, 2058, 1061, 1035, 3084, 1036, 1037, 1050, 1038, 1040, 1041, 
                                     1042, 1063, 1062, 1043, 1044, 1045, 2070, 1046, 1048, 1049, 1051, 1060, 2074, 1053, 1054, 1055, 1058, 4100, 1028 }; }

        }

        /// <summary>
        /// Descriptions property implementation
        /// </summary>
        public Dictionary<int, string> Descriptions
        {
            get
            {
                Dictionary<int, string> result = new Dictionary<int, string>();
                result.Add(1025, "Neos Multi-Factor Authentication");  // ar
                result.Add(1026, "Neos Multi-Factor Authentication");  // bg
                result.Add(5146, "Neos Multi-Factor Authentication");  // bs
                result.Add(1029, "Neos Multi-Factor Authentication");  // cs
                result.Add(1030, "Neos Multi-Factor Authentication");  // da
                result.Add(1031, "Neos Multi-Factor Authentication");  // de
                result.Add(1032, "Neos Multi-Factor Authentication");  // el
                result.Add(2057, "Neos Multi-Factor Authentication");  // en-GB
                result.Add(1033, "Neos Multi-Factor Authentication");  // en-US
                result.Add(1034, "Neos Multi-Factor Authentication");  // es
                result.Add(2058, "Neos Multi-Factor Authentication");  // es-MX
                result.Add(1061, "Neos Multi-Factor Authentication");  // et
                result.Add(1035, "Neos Multi-Factor Authentication");  // fi
                result.Add(3084, "Neos Multi-Factor Authentication");  // fr-CA
                result.Add(1036, "Neos Multi-Factor Authentication");  // fr-FR
                result.Add(1037, "Neos Multi-Factor Authentication");  // he
                result.Add(1050, "Neos Multi-Factor Authentication");  // hr
                result.Add(1038, "Neos Multi-Factor Authentication");  // hu
                result.Add(1040, "Neos Multi-Factor Authentication");  // it
                result.Add(1041, "Neos Multi-Factor Authentication");  // ja
                result.Add(1042, "Neos Multi-Factor Authentication");  // ko
                result.Add(1063, "Neos Multi-Factor Authentication");  // lt
                result.Add(1062, "Neos Multi-Factor Authentication");  // lv
                result.Add(1043, "Neos Multi-Factor Authentication");  // nl
                result.Add(1044, "Neos Multi-Factor Authentication");  // no
                result.Add(1045, "Neos Multi-Factor Authentication");  // pl
                result.Add(2070, "Neos Multi-Factor Authentication");  // pt
                result.Add(1046, "Neos Multi-Factor Authentication");  // pt-BR
                result.Add(1048, "Neos Multi-Factor Authentication");  // ro
                result.Add(1049, "Neos Multi-Factor Authentication");  // ru
                result.Add(1051, "Neos Multi-Factor Authentication");  // sk
                result.Add(1060, "Neos Multi-Factor Authentication");  // sl
                result.Add(2074, "Neos Multi-Factor Authentication");  // sr-Latn-RS
                result.Add(1053, "Neos Multi-Factor Authentication");  // sv
                result.Add(1054, "Neos Multi-Factor Authentication");  // th
                result.Add(1055, "Neos Multi-Factor Authentication");  // tr
                result.Add(1058, "Neos Multi-Factor Authentication");  // uk
                result.Add(4100, "Neos Multi-Factor Authentication");  // zh-HANS
                result.Add(1028, "Neos Multi-Factor Authentication");  // zh-HANT
                return result;
            }
        }

        /// <summary>
        /// FriendlyNames property implementation
        /// </summary>
        public Dictionary<int, string> FriendlyNames
        {
            get
            {
                Dictionary<int, string> result = new Dictionary<int, string>();
                result.Add(1025, "Neos Multi-Factor Authentication");  // ar
                result.Add(1026, "Neos Multi-Factor Authentication");  // bg
                result.Add(5146, "Neos Multi-Factor Authentication");  // bs
                result.Add(1029, "Neos Multi-Factor Authentication");  // cs
                result.Add(1030, "Neos Multi-Factor Authentication");  // da
                result.Add(1031, "Neos Multi-Factor Authentication");  // de
                result.Add(1032, "Neos Multi-Factor Authentication");  // el
                result.Add(2057, "Neos Multi-Factor Authentication");  // en-GB
                result.Add(1033, "Neos Multi-Factor Authentication");  // en-US
                result.Add(1034, "Neos Multi-Factor Authentication");  // es
                result.Add(2058, "Neos Multi-Factor Authentication");  // es-MX
                result.Add(1061, "Neos Multi-Factor Authentication");  // et
                result.Add(1035, "Neos Multi-Factor Authentication");  // fi
                result.Add(3084, "Neos Multi-Factor Authentication");  // fr-CA
                result.Add(1036, "Neos Multi-Factor Authentication");  // fr-FR
                result.Add(1037, "Neos Multi-Factor Authentication");  // he
                result.Add(1050, "Neos Multi-Factor Authentication");  // hr
                result.Add(1038, "Neos Multi-Factor Authentication");  // hu
                result.Add(1040, "Neos Multi-Factor Authentication");  // it
                result.Add(1041, "Neos Multi-Factor Authentication");  // ja
                result.Add(1042, "Neos Multi-Factor Authentication");  // ko
                result.Add(1063, "Neos Multi-Factor Authentication");  // lt
                result.Add(1062, "Neos Multi-Factor Authentication");  // lv
                result.Add(1043, "Neos Multi-Factor Authentication");  // nl
                result.Add(1044, "Neos Multi-Factor Authentication");  // no
                result.Add(1045, "Neos Multi-Factor Authentication");  // pl
                result.Add(2070, "Neos Multi-Factor Authentication");  // pt
                result.Add(1046, "Neos Multi-Factor Authentication");  // pt-BR
                result.Add(1048, "Neos Multi-Factor Authentication");  // ro
                result.Add(1049, "Neos Multi-Factor Authentication");  // ru
                result.Add(1051, "Neos Multi-Factor Authentication");  // sk
                result.Add(1060, "Neos Multi-Factor Authentication");  // sl
                result.Add(2074, "Neos Multi-Factor Authentication");  // sr-Latn-RS
                result.Add(1053, "Neos Multi-Factor Authentication");  // sv
                result.Add(1054, "Neos Multi-Factor Authentication");  // th
                result.Add(1055, "Neos Multi-Factor Authentication");  // tr
                result.Add(1058, "Neos Multi-Factor Authentication");  // uk
                result.Add(4100, "Neos Multi-Factor Authentication");  // zh-HANS
                result.Add(1028, "Neos Multi-Factor Authentication");  // zh-HANT
                return result;
            }
        }

        /// <summary>
        /// IdentityClaims property implementation
        /// </summary>
        public string[] IdentityClaims
        {
            get { return new string[] { "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/upn" }; }
        }

        /// <summary>
        /// RequiresIdentity property implementation
        /// </summary>
        public bool RequiresIdentity
        {
            get { return true; }
        }

    }
}
