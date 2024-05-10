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

using Neos.IdentityServer.MultiFactor.WebAuthN.Library.ASN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Neos.IdentityServer.MultiFactor.WebAuthN
{
    public static class CryptoUtils
    {
        public static HashAlgorithm GetHasher(HashAlgorithmName hashName)
        {
            switch (hashName.Name)
            {
                case "SHA1":
                    return SHA1.Create();
                case "SHA256":
                case "HS256":
                case "RS256":
                case "ES256":
                case "PS256":
                    return SHA256.Create();
                case "SHA384":
                case "HS384":
                case "RS384":
                case "ES384":
                case "PS384":
                    return SHA384.Create();
                case "SHA512":
                case "HS512":
                case "RS512":
                case "ES512":
                case "PS512":
                    return SHA512.Create();
                default:
                    throw new ArgumentOutOfRangeException(nameof(hashName));
            }
        }

        public static HashAlgorithmName HashAlgFromCOSEAlg(int alg)
        {
            switch ((COSE.Algorithm)alg           )
            {
                case COSE.Algorithm.RS1:
                    return HashAlgorithmName.SHA1;
                case COSE.Algorithm.ES256:
                    return HashAlgorithmName.SHA256;
                case COSE.Algorithm.ES384:
                    return HashAlgorithmName.SHA384;
                case COSE.Algorithm.ES512:
                    return HashAlgorithmName.SHA512;
                case COSE.Algorithm.PS256:
                    return HashAlgorithmName.SHA256;
                case COSE.Algorithm.PS384:
                    return HashAlgorithmName.SHA384;
                case COSE.Algorithm.PS512:
                    return HashAlgorithmName.SHA512;
                case COSE.Algorithm.RS256:
                    return HashAlgorithmName.SHA256;
                case COSE.Algorithm.RS384:
                    return HashAlgorithmName.SHA384;
                case COSE.Algorithm.RS512:
                    return HashAlgorithmName.SHA512;
                case COSE.Algorithm.ES256K:
                    return HashAlgorithmName.SHA256;
                case (COSE.Algorithm)4:
                    return HashAlgorithmName.SHA1;
                case (COSE.Algorithm)11:
                    return HashAlgorithmName.SHA256;
                case (COSE.Algorithm)12:
                    return HashAlgorithmName.SHA384;
                case (COSE.Algorithm)13:
                    return HashAlgorithmName.SHA512;
                case COSE.Algorithm.EdDSA:
                    return HashAlgorithmName.SHA512;
                default:
                    throw new VerificationException("Unrecognized COSE alg value");
            };
        }

        public static bool ValidateTrustChain(X509Certificate2[] trustPath, X509Certificate2[] attestationRootCertificates)
        {
            // Each element of this array represents a PKIX [RFC5280] X.509 certificate that is a valid trust anchor for this authenticator model.
            // Multiple certificates might be used for different batches of the same model.
            // The array does not represent a certificate chain, but only the trust anchor of that chain.
            // A trust anchor can be a root certificate, an intermediate CA certificate or even the attestation certificate itself.

            // Let's check the simplest case first.  If subject and issuer are the same, and the attestation cert is in the list, that's all the validation we need
            if (trustPath.Length == 1 && trustPath[0].Subject.CompareTo(trustPath[0].Issuer) == 0)
            {
                foreach (X509Certificate2 cert in attestationRootCertificates)
                {
                    if (cert.Thumbprint.CompareTo(trustPath[0].Thumbprint) == 0)
                        return true;
                }
                return false;
            }

            // If the attestation cert is not self signed, we will need to build a chain
            var chain = new X509Chain();

            // Put all potential trust anchors into extra store
            chain.ChainPolicy.ExtraStore.AddRange(attestationRootCertificates);

            // We don't know the root here, so allow unknown root, and turn off revocation checking
            chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
            chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority;

            // trustPath[0] is the attestation cert, if there are more in the array than just that, add those to the extra store as well, but skip attestation cert
            if (trustPath.Length > 1)
            {
                foreach (X509Certificate2 cert in trustPath.Skip(1)) // skip attestation cert
                {
                    chain.ChainPolicy.ExtraStore.Add(cert);
                }
            }

            // try to build a chain with what we've got
            if (chain.Build(trustPath[0]))
            {
                // if the chain validates, make sure one of the attestation root certificates is one of the chain elements
                foreach (X509Certificate2 attestationRootCertificate in attestationRootCertificates)
                {
                    // skip the first element, as that is the attestation cert
                    if (chain.ChainElements
                        .Cast<X509ChainElement>()
                        .Skip(1)
                        .Any(x => x.Certificate.Thumbprint == attestationRootCertificate.Thumbprint))
                        return true;
                }
            }
            return false;
          }

       /* public static bool ValidateTrustChain(X509Certificate2[] trustPath, X509Certificate2[] attestationRootCertificates)
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
        }*/

        /*
        public static bool ValidateTrustChain(X509Certificate2[] trustPath, X509Certificate2[] attestationRootCertificates)
        {
            // https://fidoalliance.org/specs/fido-v2.0-id-20180227/fido-metadata-statement-v2.0-id-20180227.html#widl-MetadataStatement-attestationRootCertificates

            // Each element of this array represents a PKIX [RFC5280] X.509 certificate that is a valid trust anchor for this authenticator model.
            // Multiple certificates might be used for different batches of the same model.
            // The array does not represent a certificate chain, but only the trust anchor of that chain.
            // A trust anchor can be a root certificate, an intermediate CA certificate or even the attestation certificate itself.

            // Let's check the simplest case first.  If subject and issuer are the same, and the attestation cert is in the list, that's all the validation we need
            if (trustPath.Length == 1 && trustPath[0].Subject.CompareTo(trustPath[0].Issuer) == 0)
            {
                foreach (X509Certificate2 cert in attestationRootCertificates)
                {
                    if (cert.Thumbprint.CompareTo(trustPath[0].Thumbprint) == 0)
                        return true;
                }
                return false;
            }

            // If the attestation cert is not self signed, we will need to build a chain
            var chain = new X509Chain();

            // Put all potential trust anchors into extra store
            chain.ChainPolicy.ExtraStore.AddRange(attestationRootCertificates);

            // We don't know the root here, so allow unknown root, and turn off revocation checking
            chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
            chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority;

            // trustPath[0] is the attestation cert, if there are more in the array than just that, add those to the extra store as well, but skip attestation cert
            if (trustPath.Length > 1)
            {
                foreach (X509Certificate2 cert in trustPath.Skip(1)) // skip attestation cert
                {
                    chain.ChainPolicy.ExtraStore.Add(cert);
                }
            }

            // try to build a chain with what we've got
            if (chain.Build(trustPath[0]))
            {
                // if that validated, we should have a root for this chain now, add it to the custom trust store
                chain.ChainPolicy.CustomStore.Clear();
                chain.ChainPolicy.CustomStore.Add(chain.ChainElements[chain.ChainElements.Count-1].Certificate);

                // explicitly trust the custom root we just added
                // chain.ChainPolicy.TrustMode = X509ChainTrustMode.CustomRootTrust;

                // if the attestation cert has a CDP extension, go ahead and turn on online revocation checking
                if (!string.IsNullOrEmpty(CDPFromCertificateExts(trustPath[0].Extensions)))
                    chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;

                // don't allow unknown root now that we have a custom root
                chain.ChainPolicy.VerificationFlags = X509VerificationFlags.NoFlag;

                // now, verify chain again with all checks turned on
                if (chain.Build(trustPath[0]))
                {
                    // if the chain validates, make sure one of the attestation root certificates is one of the chain elements
                    foreach (X509Certificate2 attestationRootCertificate in attestationRootCertificates)
                    {
                        // skip the first element, as that is the attestation cert
                        if (chain.ChainElements
                            .Cast<X509ChainElement>()
                            .Skip(1)
                            .Any(x => x.Certificate.Thumbprint == attestationRootCertificate.Thumbprint))
                            return true;
                    }
                }
            }
            return false;
        } */

        public static byte[] SigFromEcDsaSig(byte[] ecDsaSig, int keySize)
        {
            var decoded = AsnElt.Decode(ecDsaSig);
            var r = decoded.Sub[0].GetOctetString();
            var s = decoded.Sub[1].GetOctetString();

            // .NET requires IEEE P-1363 fixed size unsigned big endian values for R and S
            // ASN.1 requires storing positive integer values with any leading 0s removed
            // Convert ASN.1 format to IEEE P-1363 format 
            // determine coefficient size 
            var coefficientSize = (int)Math.Ceiling((decimal)keySize / 8);

            // Create byte array to copy R into 
            var P1363R = new byte[coefficientSize];

            if (0x0 == r[0] && (r[1] & (1 << 7)) != 0)
            {
                r.Skip(1).ToArray().CopyTo(P1363R, coefficientSize - r.Length + 1);
            }
            else
            {
                r.CopyTo(P1363R, coefficientSize - r.Length);
            }

            // Create byte array to copy S into 
            var P1363S = new byte[coefficientSize];

            if (0x0 == s[0] && (s[1] & (1 << 7)) != 0)
            {
                s.Skip(1).ToArray().CopyTo(P1363S, coefficientSize - s.Length + 1);
            }
            else
            {
                s.CopyTo(P1363S, coefficientSize - s.Length);
            }

            // Concatenate R + S coordinates and return the raw signature
            return P1363R.Concat(P1363S).ToArray();
        }

        /// <summary>
        /// Convert PEM formated string into byte array.
        /// </summary>
        /// <param name="pemStr">source string.</param>
        /// <returns>output byte array.</returns>
        public static byte[] PemToBytes(string pemStr)
        {
            const string PemStartStr = "-----BEGIN";
            const string PemEndStr = "-----END";
            byte[] retval = null;
            var lines = pemStr.Split('\n');
            var base64Str = "";
            bool started = false, ended = false;
            var cline = "";
            for (var i = 0; i < lines.Length; i++)
            {
                cline = lines[i].ToUpper();
                if (cline == "")
                    continue;
                if (cline.Length > PemStartStr.Length)
                {
                    if (!started && cline.Substring(0, PemStartStr.Length) == PemStartStr)
                    {
                        started = true;
                        continue;
                    }
                }
                if (cline.Length > PemEndStr.Length)
                {
                    if (cline.Substring(0, PemEndStr.Length) == PemEndStr)
                    {
                        ended = true;
                        break;
                    }
                }
                if (started)
                {
                    base64Str += lines[i];
                }
            }
            if (!(started && ended))
            {
                throw new Exception("'BEGIN'/'END' line is missing.");
            }
            base64Str = base64Str.Replace("\r", "");
            base64Str = base64Str.Replace("\n", "");
            base64Str = base64Str.Replace("\n", " ");
            retval = Convert.FromBase64String(base64Str);
            return retval;
        }

        public static string CDPFromCertificateExts(X509ExtensionCollection exts)
        {
            var cdp = "";
            foreach (var ext in exts)
            {
                if (ext.Oid.Value.Equals("2.5.29.31")) // id-ce-CRLDistributionPoints
                {
                    var asnData = AsnElt.Decode(ext.RawData);
                    cdp = System.Text.Encoding.ASCII.GetString(asnData.Sub[0].Sub[0].Sub[0].Sub[0].GetOctetString());
                }
            }
            return cdp;
        }

        public static bool IsCertInCRL(byte[] crl, X509Certificate2 cert)
        {
            var asnData = AsnElt.Decode(crl);
            if (7 > asnData.Sub[0].Sub.Length)
                return false; // empty CRL

            var revokedCertificates = asnData.Sub[0].Sub[5].Sub;
            var revoked = new List<long>();

            foreach (AsnElt s in revokedCertificates)
            {
                revoked.Add(BitConverter.ToInt64(s.Sub[0].GetOctetString().Reverse().ToArray(), 0));
            }

            return revoked.Contains(BitConverter.ToInt64(cert.GetSerialNumber(), 0));
        }
    }
}
