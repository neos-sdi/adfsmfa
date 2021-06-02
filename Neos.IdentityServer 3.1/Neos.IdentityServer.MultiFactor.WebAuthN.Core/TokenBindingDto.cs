//******************************************************************************************************************************************************************************************//
// Copyright (c) 2021 abergs (https://github.com/abergs/fido2-net-lib)                                                                                                                      //                        
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
namespace Neos.IdentityServer.MultiFactor.WebAuthN
{
    public class TokenBindingDto
    {
        /// <summary>
        /// Either "present" or "supported". https://www.w3.org/TR/webauthn/#enumdef-tokenbindingstatus
        /// supported: Indicates the client supports token binding, but it was not negotiated when communicating with the Relying Party.
        /// present: Indicates token binding was used when communicating with the Relying Party. In this case, the id member MUST be present
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// This member MUST be present if status is present, and MUST a base64url encoding of the Token Binding ID that was used when communicating with the Relying Party.
        /// </summary>
        public string Id { get; set; }

        public void Verify(byte[] requestTokenbinding)
        {
            // validation by the FIDO conformance tool (more than spec says)
            switch (Status)
            {
                case "present":
                    if (string.IsNullOrEmpty(Id))
                        throw new VerificationException("TokenBinding status was present but Id is missing");
                    var b64 = Base64Url.Encode(requestTokenbinding);
                    if (Id != b64)
                        throw new VerificationException("Tokenbinding Id does not match");
                    break;
                case "supported":
                case "not-supported":
                    break;
                default:
                    throw new VerificationException("Malformed tokenbinding status field");
            }
        }
    }
}
