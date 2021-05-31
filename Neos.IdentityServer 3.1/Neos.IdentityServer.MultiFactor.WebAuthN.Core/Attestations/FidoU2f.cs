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
// Copyright (c) 2021 @redhook62 (adfsmfa@gmail.com)                                                                                                                                    //                        
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
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Neos.IdentityServer.MultiFactor.WebAuthN.Objects;
using Neos.IdentityServer.MultiFactor.WebAuthN.Library.Cbor;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Neos.IdentityServer.MultiFactor.WebAuthN.AttestationFormat
{
    internal class FidoU2f : AttestationFormat
    {
        private readonly IMetadataService _metadataService;
        private readonly bool _requireValidAttestationRoot;

        public FidoU2f(CBORObject attStmt, byte[] authenticatorData, byte[] clientDataHash, IMetadataService metadataService, bool requireValidAttestationRoot) : base(attStmt, authenticatorData, clientDataHash)
        {
            _metadataService = metadataService;
            _requireValidAttestationRoot = requireValidAttestationRoot;
        }        

        public override void Verify()
        {
            // verify that aaguid is 16 empty bytes (note: required by fido2 conformance testing, could not find this in spec?)
            if (0 != AuthData.AttestedCredentialData.AaGuid.CompareTo(Guid.Empty))
                throw new VerificationException("Aaguid was not empty parsing fido-u2f atttestation statement");

            // 1. Verify that attStmt is valid CBOR conforming to the syntax defined above and perform CBOR decoding on it to extract the contained fields.
            if (null == X5c || CBORType.Array != X5c.Type || X5c.Count != 1)
                throw new VerificationException("Malformed x5c in fido - u2f attestation");

            // 2a. the attestation certificate attestnCert MUST be the first element in the array
            if (null == X5c.Values || 0 == X5c.Values.Count ||
                CBORType.ByteString != X5c.Values.First().Type ||
                0 == X5c.Values.First().GetByteString().Length)
                throw new VerificationException("Malformed x5c in fido-u2f attestation");

            var cert = new X509Certificate2(X5c.Values.First().GetByteString());

            // TODO : Check why this variable isn't used. Remove it or use it.

            var u2ftransports = U2FTransportsFromAttnCert(cert.Extensions);
			
            // 2b. If certificate public key is not an Elliptic Curve (EC) public key over the P-256 curve, terminate this algorithm and return an appropriate error
            var pubKey = cert.GetECDsaPublicKey();
            var keyParams = pubKey.ExportParameters(false);

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (!keyParams.Curve.Oid.FriendlyName.Equals(ECCurve.NamedCurves.nistP256.Oid.FriendlyName))
                    throw new VerificationException("Attestation certificate public key is not an Elliptic Curve (EC) public key over the P-256 curve");
            }
            else
            {
                if (!keyParams.Curve.Oid.Value.Equals(ECCurve.NamedCurves.nistP256.Oid.Value))
                    throw new VerificationException("Attestation certificate public key is not an Elliptic Curve (EC) public key over the P-256 curve");
            }
            // 3. Extract the claimed rpIdHash from authenticatorData, and the claimed credentialId and credentialPublicKey from authenticatorData
            // see rpIdHash, credentialId, and credentialPublicKey variables

            // 4. Convert the COSE_KEY formatted credentialPublicKey (see Section 7 of [RFC8152]) to CTAP1/U2F public Key format
            var x = CredentialPublicKey[CBORObject.FromObject(COSE.KeyTypeParameter.X)].GetByteString();
            var y = CredentialPublicKey[CBORObject.FromObject(COSE.KeyTypeParameter.Y)].GetByteString();
            var publicKeyU2F = new byte[1] { 0x4 }.Concat(x).Concat(y).ToArray();

            // 5. Let verificationData be the concatenation of (0x00 || rpIdHash || clientDataHash || credentialId || publicKeyU2F)
            var verificationData = new byte[1] { 0x00 };
            verificationData = verificationData
                                .Concat(AuthData.RpIdHash)
                                .Concat(clientDataHash)
                                .Concat(AuthData.AttestedCredentialData.CredentialID)
                                .Concat(publicKeyU2F.ToArray())
                                .ToArray();

            // 6. Verify the sig using verificationData and certificate public key
            if (null == Sig || CBORType.ByteString != Sig.Type || 0 == Sig.GetByteString().Length)
                throw new VerificationException("Invalid fido-u2f attestation signature");

            byte[] ecsig;
            try
            {
                ecsig = CryptoUtils.SigFromEcDsaSig(Sig.GetByteString(), pubKey.KeySize);
            }
            catch (Exception ex)
            {
                throw new VerificationException("Failed to decode fido-u2f attestation signature from ASN.1 encoded form", ex);
            }

            var coseAlg = CredentialPublicKey[CBORObject.FromObject(COSE.KeyCommonParameter.Alg)].AsInt32();
            var hashAlg = CryptoUtils.HashAlgFromCOSEAlg(coseAlg);
			
            if (true != pubKey.VerifyData(verificationData, ecsig, hashAlg))
                throw new VerificationException("Invalid fido-u2f attestation signature");

            if (_requireValidAttestationRoot)
            {
                // 7. Optionally, inspect x5c and consult externally provided knowledge to determine whether attStmt conveys a Basic or AttCA attestation
                var trustPath = X5c.Values
                .Select(z => new X509Certificate2(z.GetByteString()))
                .ToArray();

                var aaguid = AaguidFromAttnCertExts(cert.Extensions);

                if (null != _metadataService && null != aaguid)
                {
                    var guidAaguid = AttestedCredentialData.FromBigEndian(aaguid);
                    var entry = _metadataService.GetEntry(guidAaguid);

                    if (null != entry && null != entry.MetadataStatement)
                    {
                        var attestationRootCertificates = entry.MetadataStatement.AttestationRootCertificates
                            .Select(z => new X509Certificate2(Convert.FromBase64String(z)))
                            .ToArray();

                        if (false == ValidateTrustChain(trustPath, attestationRootCertificates))
                        {
                            throw new VerificationException("Invalid certificate chain in U2F attestation");
                        }
                    }
                }
            }
        }
    }
}
