using Microsoft.IdentityServer.Web.Authentication.External;
//******************************************************************************************************************************************************************************************//
// Copyright (c) 2015 Neos-Sdi (http://www.neos-sdi.com)                                                                                                                                    //                        
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
//******************************************************************************************************************************************************************************************//
using System.Collections.Generic;

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
            get { return new string[] { "http://schemas.microsoft.com/ws/2012/12/authmethod/otp" }; }  // Only PIN Code
        }

        /// <summary>
        /// AvailableLcids property implementation
        /// </summary>
        public int[] AvailableLcids
        {
            get { return new int[] { 1033, 1036 }; }
        }

        /// <summary>
        /// Descriptions property implementation
        /// </summary>
        public Dictionary<int, string> Descriptions
        {
            get
            {
                Dictionary<int, string> result = new Dictionary<int, string>();
                result.Add(1036, "Connexion avec un code OTP");
                result.Add(1033, "Connect with an OTP code");

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
                result.Add(1036, "Connexion avec un code OTP");
                result.Add(1033, "Connect with an OTP code");
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
