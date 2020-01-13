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
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Neos.IdentityServer.MultiFactor.WebAuthN
{
    /// <summary>
    /// Base class for responses sent by the Authenticator Client
    /// </summary>
    public class AuthenticatorResponse
    {
        protected AuthenticatorResponse(byte[] clientDataJson)
        {
            if (null == clientDataJson)
                throw new VerificationException("clientDataJson cannot be null");
            var stringx = Encoding.UTF8.GetString(clientDataJson);

            AuthenticatorResponse response;
            try
            {
                response = JsonConvert.DeserializeObject<AuthenticatorResponse>(stringx);
            }
            catch (Exception e) when (e is JsonReaderException || e is JsonSerializationException)
            {
                throw new VerificationException("Malformed clientDataJson");
            }

            if (null == response)
                throw new VerificationException("Deserialized authenticator response cannot be null");
            Type = response.Type;
            Challenge = response.Challenge;
            Origin = response.Origin;
            TokenBinding = response.TokenBinding;

        }

        [JsonConstructor]
        private AuthenticatorResponse()
        {

        }

        public string Type { get; set; }

        [JsonConverter(typeof(Base64UrlConverter))]
        public byte[] Challenge { get; set; }
        public string Origin { get; set; }

        public TokenBindingDto TokenBinding { get; set; }

        // todo: add TokenBinding https://www.w3.org/TR/webauthn/#dictdef-tokenbinding

        protected void BaseVerify(string expectedOrigin, byte[] originalChallenge, byte[] requestTokenBindingId)
        {
            if (null == Challenge)
                throw new VerificationException("Challenge cannot be null");

            // verify challenge is same
            if (!Challenge.SequenceEqual(originalChallenge))
                throw new VerificationException("Challenge not equal to original challenge");

            if (Origin != expectedOrigin)
                throw new VerificationException($"Origin {Origin} not equal to original origin {expectedOrigin}");

            if (Type != "webauthn.create" && Type != "webauthn.get")
                throw new VerificationException($"Type not equal to 'webauthn.create' or 'webauthn.get'. Was: '{Type}'");

            if (TokenBinding != null)
            {
                TokenBinding.Verify(requestTokenBindingId);
            }
        }
    }
}
