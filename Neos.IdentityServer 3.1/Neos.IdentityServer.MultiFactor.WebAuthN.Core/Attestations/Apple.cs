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
// Copyright (c) 2024 redhook (adfsmfa@gmail.com)                                                                                                                                    //                        
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
//                                                                                                                                                             //
// https://github.com/neos-sdi/adfsmfa                                                                                                                                                      //
//                                                                                                                                                                                          //
//******************************************************************************************************************************************************************************************//
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Neos.IdentityServer.MultiFactor.WebAuthN.Library.ASN;
using Neos.IdentityServer.MultiFactor.WebAuthN.Objects;
using Neos.IdentityServer.MultiFactor.WebAuthN.Library.Cbor;

namespace Neos.IdentityServer.MultiFactor.WebAuthN.AttestationFormat
{
    internal sealed class Apple : AttestationVerifier
    {
        public static byte[] GetAppleAttestationExtensionValue(X509ExtensionCollection exts)
        {
            var appleExtension = exts.Cast<X509Extension>().FirstOrDefault(e => e.Oid.Value == "1.2.840.113635.100.8.2");

            if (appleExtension == null || appleExtension.RawData == null || appleExtension.RawData.Length < 0x26)
                throw new VerificationException("Extension with OID 1.2.840.113635.100.8.2 not found on Apple attestation credCert");

            try
            {
                var appleAttestationASN = AsnElt.Decode(appleExtension.RawData);
                appleAttestationASN.CheckConstructed();
                appleAttestationASN.CheckTag(AsnElt.SEQUENCE);
                appleAttestationASN.CheckNumSub(1);

                var sequence = appleAttestationASN.GetSub(0);
                sequence.CheckConstructed();
                sequence.CheckNumSub(1);

                var context = sequence.GetSub(0);
                context.CheckPrimitive();
                context.CheckTag(AsnElt.OCTET_STRING);

                return context.GetOctetString();
            }

            catch (Exception ex)
            {
                throw new VerificationException("Apple attestation extension has invalid data", ex);
            }
        }

        public override (AttestationType, X509Certificate2[]) Verify()
        {
            // 1. Verify that attStmt is valid CBOR conforming to the syntax defined above and perform CBOR decoding on it to extract the contained fields.
            if (null == X5c || CBORType.Array != X5c.Type || X5c.Count < 2 ||
                    null == X5c.Values || 0 == X5c.Values.Count ||
                    CBORType.ByteString != X5c.Values.First().Type ||
                    0 == X5c.Values.First().GetByteString().Length)
                throw new VerificationException("Malformed x5c in Apple attestation");

            // 2. Verify x5c is a valid certificate chain starting from the credCert to the Apple WebAuthn root certificate.
            // This happens in AuthenticatorAttestationResponse.VerifyAsync using metadata from MDS3

            var trustPath = X5c.Values
                .Select(x => new X509Certificate2(x.GetByteString()))
                .ToArray();

            // credCert is the first certificate in the trust path
            var credCert = trustPath[0];

            // 3. Concatenate authenticatorData and clientDataHash to form nonceToHash.
            var nonceToHash = Data;

            // 4. Perform SHA-256 hash of nonceToHash to produce nonce.
            var nonce = CryptoUtils.GetHasher(HashAlgorithmName.SHA256).ComputeHash(nonceToHash);

            // 5. Verify nonce matches the value of the extension with OID ( 1.2.840.113635.100.8.2 ) in credCert.
            var appleExtensionBytes = GetAppleAttestationExtensionValue(credCert.Extensions);

            if (!nonce.SequenceEqual(appleExtensionBytes))
                throw new VerificationException("Mismatch between nonce and credCert attestation extension in Apple attestation");

            // 6. Verify credential public key matches the Subject Public Key of credCert.
            // First, obtain COSE algorithm being used from credential public key
            var coseAlg = CredentialPublicKey[CBORObject.FromObject(COSE.KeyCommonParameter.Alg)].AsInt32();

            // Next, build temporary CredentialPublicKey for comparison from credCert and COSE algorithm
            var cpk = new CredentialPublicKey(credCert, coseAlg);

            // Finally, compare byte sequence of CredentialPublicKey built from credCert with byte sequence of CredentialPublicKey from AttestedCredentialData from authData
            if (!cpk.GetBytes().SequenceEqual(AuthData.AttestedCredentialData.CredentialPublicKey.GetBytes()))
                throw new VerificationException("Credential public key in Apple attestation does not match subject public key of credCert");

            // 7. If successful, return implementation-specific values representing attestation type Anonymous CA and attestation trust path x5c.
            return (AttestationType.Basic, trustPath);
        }
    }
}