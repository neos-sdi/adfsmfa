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
using System;
using System.Security.Cryptography.X509Certificates;
using System.Linq;
using Neos.IdentityServer.MultiFactor.WebAuthN.Objects;
using Neos.IdentityServer.MultiFactor.WebAuthN.Library.ASN;
using Neos.IdentityServer.MultiFactor.WebAuthN.Library.Cbor;

namespace Neos.IdentityServer.MultiFactor.WebAuthN.AttestationFormat
{
    public abstract class AttestationVerifier
    {
        public CBORObject attStmt;
        public byte[] authenticatorData;
        public byte[] clientDataHash;

        internal CBORObject Sig => attStmt["sig"];
        internal CBORObject X5c => attStmt["x5c"];
        internal CBORObject Alg => attStmt["alg"];
        internal CBORObject EcdaaKeyId => attStmt["ecdaaKeyId"];
        internal AuthenticatorData AuthData => new AuthenticatorData(authenticatorData);
        internal CBORObject CredentialPublicKey => AuthData.AttestedCredentialData.CredentialPublicKey.GetCBORObject();
        internal byte[] Data
        {
            get
            {
                byte[] data = new byte[authenticatorData.Length + clientDataHash.Length];
                Buffer.BlockCopy(authenticatorData, 0, data, 0, authenticatorData.Length);
                Buffer.BlockCopy(clientDataHash, 0, data, authenticatorData.Length, clientDataHash.Length);
                return data;
            }
        }
        internal static byte[] AaguidFromAttnCertExts(X509ExtensionCollection exts)
        {
            byte[] aaguid = null;
            var ext = exts.Cast<X509Extension>().FirstOrDefault(e => e.Oid.Value == "1.3.6.1.4.1.45724.1.1.4"); // id-fido-gen-ce-aaguid
            if (null != ext)
            {
                var decodedAaguid = AsnElt.Decode(ext.RawData);
                decodedAaguid.CheckTag(AsnElt.OCTET_STRING);
                decodedAaguid.CheckPrimitive();
                aaguid = decodedAaguid.GetOctetString();

                //The extension MUST NOT be marked as critical
                if (true == ext.Critical)
                    throw new VerificationException("extension MUST NOT be marked as critical");
            }

            return aaguid;
        }
        internal static bool IsAttnCertCACert(X509ExtensionCollection exts)
        {
            var ext = exts.Cast<X509Extension>().FirstOrDefault(e => e.Oid.Value == "2.5.29.19");
            if (null != ext && ext is X509BasicConstraintsExtension baseExt)
            {
                return baseExt.CertificateAuthority;
            }

            return true;
        }
        internal static byte U2FTransportsFromAttnCert(X509ExtensionCollection exts)
        {
            var u2ftransports = new byte();
            var ext = exts.Cast<X509Extension>().FirstOrDefault(e => e.Oid.Value == "1.3.6.1.4.1.45724.2.1.1");
            if (null != ext)
            {
                var decodedU2Ftransports = AsnElt.Decode(ext.RawData);
                decodedU2Ftransports.CheckPrimitive();

                // some certificates seem to have this encoded as an octet string
                // instead of a bit string, attempt to correct
                if (AsnElt.OCTET_STRING == decodedU2Ftransports.TagValue)
                {
                    ext.RawData[0] = AsnElt.BIT_STRING;
                    decodedU2Ftransports = AsnElt.Decode(ext.RawData);
                }
                decodedU2Ftransports.CheckTag(AsnElt.BIT_STRING);
                u2ftransports = decodedU2Ftransports.GetBitString()[0];
            }

            return u2ftransports;
        }
        public virtual (AttestationType, X509Certificate2[]) Verify(CBORObject attStmt, byte[] authenticatorData, byte[] clientDataHash)
        {
            this.attStmt = attStmt;
            this.authenticatorData = authenticatorData;
            this.clientDataHash = clientDataHash;
            return Verify();
        }

        public abstract (AttestationType, X509Certificate2[]) Verify();
    }
}
