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
using Neos.IdentityServer.MultiFactor.WebAuthN.Library.Cbor;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Linq;
using Neos.IdentityServer.MultiFactor.WebAuthN.Objects;
using Neos.IdentityServer.MultiFactor.WebAuthN.Library.ASN;

namespace Neos.IdentityServer.MultiFactor.WebAuthN.AttestationFormat
{
    public abstract class AttestationFormat
    {
        public CBORObject attStmt;
        public byte[] authenticatorData;
        public byte[] clientDataHash;

        public AttestationFormat(CBORObject attStmt, byte[] authenticatorData, byte[] clientDataHash)
        {
            this.attStmt = attStmt;
            this.authenticatorData = authenticatorData;
            this.clientDataHash = clientDataHash;
        }

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
            foreach (var ext in exts)
            {
                if (ext.Oid.Value.Equals("1.3.6.1.4.1.45724.1.1.4")) // id-fido-gen-ce-aaguid
                {
                	var decodedAaguid = AsnElt.Decode(ext.RawData);
                    decodedAaguid.CheckTag(AsnElt.OCTET_STRING);
                    decodedAaguid.CheckPrimitive();
                    aaguid = decodedAaguid.GetOctetString(); 
                    
                    //The extension MUST NOT be marked as critical
                    if (true == ext.Critical)
                        throw new VerificationException("extension MUST NOT be marked as critical");
                }
            }
            return aaguid;
        }

        internal static bool IsAttnCertCACert(X509ExtensionCollection exts)
        {
            foreach (var ext in exts)
            {
                if (ext.Oid.Value.Equals("2.5.29.19") && ext is X509BasicConstraintsExtension baseExt)
                {
                    return baseExt.CertificateAuthority;
                }
            }
            return true;
        }

        internal static int U2FTransportsFromAttnCert(X509ExtensionCollection exts)
        {
            var u2ftransports = 0;
            foreach (var ext in exts)
            {
                if (ext.Oid.Value.Equals("1.3.6.1.4.1.45724.2.1.1"))
                {
                	var decodedU2Ftransports = AsnElt.Decode(ext.RawData);
                    decodedU2Ftransports.CheckTag(AsnElt.BIT_STRING);
                    decodedU2Ftransports.CheckPrimitive();
                    u2ftransports = decodedU2Ftransports.GetBitString()[0];
                }
            }
            return u2ftransports;
        }

		internal bool ValidateTrustChain(X509Certificate2[] trustPath, X509Certificate2[] attestationRootCertificates)
        {
            foreach (var attestationRootCert in attestationRootCertificates)
            {
                var chain = new X509Chain();
                chain.ChainPolicy.ExtraStore.Add(attestationRootCert);
                chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
                chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority;
                if (trustPath.Length > 1)
                {
                    foreach (var cert in trustPath.Skip(1).Reverse())
                    {
                        chain.ChainPolicy.ExtraStore.Add(cert);
                    }
                }
                var valid = chain.Build(trustPath[0]);

                // because we are using AllowUnknownCertificateAuthority we have to verify that the root matches ourselves
                var chainRoot = chain.ChainElements[chain.ChainElements.Count - 1].Certificate;
                valid = valid && chainRoot.RawData.SequenceEqual(attestationRootCert.RawData);

                if (true == valid)
                    return true;
            }
            return false;
        }

        public abstract void Verify();
    }
}
