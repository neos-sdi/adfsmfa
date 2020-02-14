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
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using Neos.IdentityServer.MultiFactor.WebAuthN.Library.Cbor;
// using Microsoft.IdentityModel.Logging;

namespace Neos.IdentityServer.MultiFactor.WebAuthN.AttestationFormat
{
    internal class AndroidSafetyNet : AttestationFormat
    {
        private readonly int _driftTolerance;

        public AndroidSafetyNet(CBORObject attStmt, byte[] authenticatorData, byte[] clientDataHash, int driftTolerance) 
            : base(attStmt, authenticatorData, clientDataHash)
        {
            _driftTolerance = driftTolerance;
           // Identity​Model​Event​Source.ShowPII = true;
        }

        public override void Verify()
        {
            // Verify that attStmt is valid CBOR conforming to the syntax defined above and perform 
            // CBOR decoding on it to extract the contained fields
            if ((CBORType.TextString != attStmt["ver"].Type) ||
                (0 == attStmt["ver"].AsString().Length))
                throw new VerificationException("Invalid version in SafetyNet data");

            // Verify that response is a valid SafetyNet response of version ver
            var ver = attStmt["ver"].AsString();

            if ((CBORType.ByteString != attStmt["response"].Type) ||
                (0 == attStmt["response"].GetByteString().Length))
                throw new VerificationException("Invalid response in SafetyNet data");

            byte[] response = attStmt["response"].GetByteString();

            string signedAttestationStatement = Encoding.UTF8.GetString(response);

            JwtSecurityToken jwtToken = new JwtSecurityToken(signedAttestationStatement);
            X509SecurityKey[] keys = (jwtToken.Header["x5c"] as JArray)
                .Values<string>()
                .Select(x => new X509SecurityKey(
                    new X509Certificate2(Convert.FromBase64String(x))))
                .ToArray();
            if ((null == keys) || (0 == keys.Count()))
                throw new VerificationException("SafetyNet attestation missing x5c");
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKeys = keys
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            tokenHandler.ValidateToken(
                signedAttestationStatement,
                validationParameters,
                out var validatedToken);

            if (false == (validatedToken.SigningKey is X509SecurityKey))
                throw new VerificationException("Safetynet signing key invalid");

            var nonce = "";
            var payload = false;
            foreach (var claim in jwtToken.Claims)
            {
                if (("nonce" == claim.Type) && ("http://www.w3.org/2001/XMLSchema#string" == claim.ValueType) && (0 != claim.Value.Length))
                    nonce = claim.Value;
                if (("ctsProfileMatch" == claim.Type) && ("http://www.w3.org/2001/XMLSchema#boolean" == claim.ValueType))
                {
                    payload = bool.Parse(claim.Value);
                }
                if (("timestampMs" == claim.Type) && ("http://www.w3.org/2001/XMLSchema#integer64" == claim.ValueType))
                {
                    var dt = DateTimeHelper.UnixEpoch.AddMilliseconds(double.Parse(claim.Value));
                    var notAfter = DateTime.UtcNow.AddMilliseconds(_driftTolerance);
                    var notBefore = DateTime.UtcNow.AddMinutes(-1).AddMilliseconds(-(_driftTolerance));
                    if ((notAfter < dt) || ((notBefore) > dt))
                    {
                        throw new VerificationException("Android SafetyNet timestampMs must be between one minute ago and now");
                    }
                }
            }

            // Verify that the nonce in the response is identical to the SHA-256 hash of the concatenation of authenticatorData and clientDataHash
            if ("" == nonce)
                throw new VerificationException("Nonce value not found in Android SafetyNet attestation");
            using(var hasher = CryptoUtils.GetHasher(HashAlgorithmName.SHA256))
            {
                var dataHash = hasher.ComputeHash(Data);
                var nonceHash = Convert.FromBase64String(nonce);
                if (false == dataHash.SequenceEqual(nonceHash))
                    throw new VerificationException("Android SafetyNet hash value mismatch");
            }

            // Verify that the attestation certificate is issued to the hostname "attest.android.com"
            var attCert = (validatedToken.SigningKey as X509SecurityKey).Certificate;
            var subject = attCert.GetNameInfo(X509NameType.DnsName, false);
            if (false == ("attest.android.com").Equals(subject))
                throw new VerificationException("Safetynet DnsName is not attest.android.com");

            // Verify that the ctsProfileMatch attribute in the payload of response is true
            if (true != payload)
                throw new VerificationException("Android SafetyNet ctsProfileMatch must be true");
        }
    }
}
