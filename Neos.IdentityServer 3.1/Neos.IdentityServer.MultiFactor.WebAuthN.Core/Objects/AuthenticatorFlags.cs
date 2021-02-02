//******************************************************************************************************************************************************************************************//
// Copyright (c) 2020 abergs (https://github.com/abergs/fido2-net-lib)                                                                                                                      //                        
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
using System;

namespace Neos.IdentityServer.MultiFactor.WebAuthN.Objects
{
    /// <summary>
    /// Authenticator data flags 
    /// <see cref="https://www.w3.org/TR/webauthn/#flags"/>
    /// </summary>
    [Flags]
    public enum AuthenticatorFlags : byte
    {
        /// <summary>
        /// User Present indicates that the user presence test has completed successfully.
        /// <see cref="https://www.w3.org/TR/webauthn/#up"/>
        /// </summary>
        UP = 0x1,

        /// <summary>
        /// Reserved for future use (RFU1)
        /// </summary>
        RFU1 = 0x2,

        /// <summary>
        /// User Verified indicates that the user verification process has completed successfully.
        /// <see cref="https://www.w3.org/TR/webauthn/#uv"/>
        /// </summary>
        UV = 0x4,

        /// <summary>
        /// Reserved for future use (RFU2)
        /// </summary>
        RFU2 = 0x8,

        /// <summary>
        /// Reserved for future use (RFU3)
        /// </summary>
        RFU3 = 0x10,

        /// <summary>
        /// Reserved for future use (RFU4)
        /// </summary>
        RFU4 = 0x20,

        /// <summary>
        /// Attested credential data included indicates that the authenticator added attested credential data to the authenticator data.
        /// <see cref="https://www.w3.org/TR/webauthn/#attested-credential-data"/>
        /// </summary>
        AT = 0x40,

        /// <summary>
        /// Extension data included indicates that the authenticator added extension data to the authenticator data.
        /// <see cref="https://www.w3.org/TR/webauthn/#authdataextensions"/>
        /// </summary>
        ED = 0x80,
    }
}
