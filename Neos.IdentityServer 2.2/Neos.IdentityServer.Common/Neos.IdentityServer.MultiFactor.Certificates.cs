//******************************************************************************************************************************************************************************************//
// Copyright (c) 2017 Neos-Sdi (http://www.neos-sdi.com)                                                                                                                                    //                        
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
// https://adfsmfa.codeplex.com                                                                                                                                                             //
// https://github.com/neos-sdi/adfsmfa                                                                                                                                                      //
//                                                                                                                                                                                          //
//******************************************************************************************************************************************************************************************//
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using CERTENROLLLib;
using System.Security;


namespace Neos.IdentityServer.MultiFactor
{
    /// <summary>
    /// Encryption class implmentation
    /// </summary>
    public class Encryption: IDisposable
    {
        private X509Certificate2 _cert = null;

        /// <summary>
        /// Constructor
        /// </summary>
        public Encryption()
        {
            _cert = null;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public Encryption(string thumbprint)
        {
            _cert = GetCertificate(thumbprint, StoreLocation.LocalMachine);
        }

        /// <summary>
        /// Certificate property
        /// </summary>
        public X509Certificate2 Certificate
        {
            get { return _cert; }
            set { _cert = value; }
        }

        /// <summary>
        /// Encrypt method
        /// </summary>
        public string Encrypt(string plainText)
        {
            try
            {
                if (_cert == null)
                    throw new Exception("Invalid encryption certificate !");
                plainText = Guid.NewGuid().ToString("N") + plainText.Length.ToString("000") + plainText + Guid.NewGuid().ToString("N");
                byte[] plainBytes = SerializeToStream(plainText).ToArray();
                byte[] encryptedBytes = null;
                var key = _cert.GetRSAPublicKey();
                if (key == null)
                    throw new CryptographicException("Invalid public Key !");

                if (key is RSACng)
                    encryptedBytes = ((RSACng)key).Encrypt(plainBytes, RSAEncryptionPadding.OaepSHA256);
                else
                    encryptedBytes = ((RSACryptoServiceProvider)key).Encrypt(plainBytes, true);
                return System.Convert.ToBase64String(encryptedBytes);
            }
            catch (Exception ex)
            {
                throw new CryptographicException(ex.Message);
            }
        }

        /// <summary>
        /// Decrypt method
        /// </summary>
        public string Decrypt(string encryptedText)
        {
            try
            {
                if (_cert == null)
                    throw new Exception("Invalid decryption certificate !");
                byte[] encryptedBytes = System.Convert.FromBase64CharArray(encryptedText.ToCharArray(), 0, encryptedText.Length);
                byte[] decryptedBytes = null;
                var key = _cert.GetRSAPrivateKey();
                if (key == null)
                    throw new CryptographicException("Invalid private Key !");

                if (key is RSACng)
                    decryptedBytes = ((RSACng)key).Decrypt(encryptedBytes, RSAEncryptionPadding.OaepSHA256);
                else
                    decryptedBytes = ((RSACryptoServiceProvider)key).Decrypt(encryptedBytes, true);
                
                MemoryStream mem = new MemoryStream(decryptedBytes);
                string decryptedvalue = DeserializeFromStream(mem);
                int l = Convert.ToInt32(decryptedvalue.Substring(32, 3));
 
                return decryptedvalue.Substring(35, l);
            }
            catch (System.Security.Cryptography.CryptographicException)
            {
                return default(string);
            }
            catch (Exception ex)
            {
                throw new CryptographicException(ex.Message);
            }
        }

        /// <summary>
        /// SerializeToStream
        /// </summary>
        private MemoryStream SerializeToStream(string objectType)
        {
            MemoryStream stream = new MemoryStream();
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, objectType);
            return stream;
        }

        /// <summary>
        /// DeserializeFromStream
        /// </summary>
        private string DeserializeFromStream(MemoryStream stream)
        {
            IFormatter formatter = new BinaryFormatter();
            stream.Seek(0, SeekOrigin.Begin);
            object objectType = formatter.Deserialize(stream);
            return (string)objectType;
        }

        /// <summary>
        /// GetCertificate method implementation
        /// </summary>
        private static X509Certificate2 GetCertificate(string thumprint, StoreLocation location)
        {
            X509Certificate2 data = null;
            X509Store store = new X509Store(location);
            store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
            try
            {
                X509Certificate2Collection collection = (X509Certificate2Collection)store.Certificates;
                X509Certificate2Collection findCollection = (X509Certificate2Collection)collection.Find(X509FindType.FindByThumbprint, thumprint, false);

                foreach (X509Certificate2 x509 in findCollection)
                {
                    data = x509;
                    break;
                }
            }
            catch
            {
                data = null;
            }
            finally
            {
                store.Close();
            }
            return data;
        } 

        /// <summary>
        /// Dispose IDispose method implementation
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose method implementation
        /// </summary>
        internal virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
				if (_cert!=null)
					_cert.Reset();
            }
        }
    }

    /// <summary>
    /// Encryption class implmentation
    /// </summary>
    public class Certs
    {
        /// <summary>
        /// GetCertificate method implementation
        /// </summary>
        internal static X509Certificate2 GetCertificate(string value, StoreLocation location)
        {
            X509Certificate2 data = null;
            X509Store store = new X509Store(location);
            store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
            try
            {
                X509Certificate2Collection collection = (X509Certificate2Collection)store.Certificates;
                X509Certificate2Collection findCollection = (X509Certificate2Collection)collection.Find(X509FindType.FindByThumbprint, value, false);

                foreach (X509Certificate2 x509 in findCollection)
                {
                    data = x509;
                    break;
                }
            }
            catch
            {
                data = null;
            }
            finally
            {
                store.Close();
            }
            return data;
        }

        /// <summary>
        /// CreateSelfSignedCertificate method implementation
        /// </summary>
        public static X509Certificate2 CreateSelfSignedCertificate(string subjectName, int years)
        {
            string cert = InternalCreateSelfSignedCertificate(subjectName, years);
            // instantiate the target class with the PKCS#12 data (and the empty password)
            return new X509Certificate2(Convert.FromBase64String(cert), "", X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet);
        }

        /// <summary>
        /// CreateSelfSignedCertificateForSQLEncryption method implementation
        /// </summary>
        public static X509Certificate2 CreateSelfSignedCertificateForSQLEncryption(string subjectName, int years)
        {
            string cert = InternalCreateSelfSignedCertificateForSQL(subjectName, years);
            // instantiate the target class with the PKCS#12 data (and the empty password)
            return new X509Certificate2(Convert.FromBase64String(cert), "", X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet);
        }

        /// <summary>
        /// RemoveSelfSignedCertificate method implmentation
        /// </summary>
        public static bool RemoveSelfSignedCertificate(X509Certificate2 cert)
        {
            if (cert != null)
            {
                X509Store store1 = new X509Store(StoreName.My, StoreLocation.LocalMachine);
                X509Store store2 = new X509Store(StoreName.CertificateAuthority, StoreLocation.LocalMachine);
                store1.Open(OpenFlags.MaxAllowed);
                store2.Open(OpenFlags.MaxAllowed);
                try
                {
                    X509Certificate2Collection collection1 = (X509Certificate2Collection)store1.Certificates;
                    X509Certificate2Collection findCollection1 = (X509Certificate2Collection)collection1.Find(X509FindType.FindByThumbprint, cert.Thumbprint, false);
                    foreach (X509Certificate2 x509 in findCollection1)
                    {
                        store1.Remove(x509);
                    }
                    X509Certificate2Collection collection2 = (X509Certificate2Collection)store2.Certificates;
                    X509Certificate2Collection findCollection2 = (X509Certificate2Collection)collection2.Find(X509FindType.FindByThumbprint, cert.Thumbprint, false);
                    foreach (X509Certificate2 x509 in findCollection2)
                    {
                        store2.Remove(x509);
                    }
                    return true;
                }
                catch
                {
                    return false;
                }
                finally
                {
                    store1.Close();
                    store2.Close();
                }
            }
            else
                return false;
        }

        /// <summary>
        /// CreateSelfSignedCertificateAsString method implementation
        /// </summary>
        public static string CreateSelfSignedCertificateAsString(string subjectName, int years)
        {
            return InternalCreateSelfSignedCertificate(subjectName, years);
        }

        /// <summary>
        /// CreateSelfSignedCertificateForSQLAsString method implementation
        /// </summary>
        public static string CreateSelfSignedCertificateForSQLAsString(string subjectName, int years)
        {
            return InternalCreateSelfSignedCertificateForSQL(subjectName, years);
        }

        /// <summary>
        /// InternalCreateSelfSignedCertificate method implementation
        /// </summary>
        [SuppressUnmanagedCodeSecurity]
        private static string InternalCreateSelfSignedCertificate(string subjectName, int years)
        {

            var dn = new CX500DistinguishedName();
            var neos = new CX500DistinguishedName();
            dn.Encode("CN=" + subjectName, X500NameFlags.XCN_CERT_NAME_STR_NONE);
            neos.Encode("CN=MFA RSA Keys Certificate", X500NameFlags.XCN_CERT_NAME_STR_NONE);

            CX509PrivateKey privateKey = new CX509PrivateKey();
            privateKey.ProviderName = "Microsoft RSA SChannel Cryptographic Provider";
            privateKey.MachineContext = true;
            privateKey.Length = 2048;
            privateKey.KeySpec = X509KeySpec.XCN_AT_KEYEXCHANGE; // use is not limited
            privateKey.ExportPolicy = X509PrivateKeyExportFlags.XCN_NCRYPT_ALLOW_PLAINTEXT_EXPORT_FLAG | X509PrivateKeyExportFlags.XCN_NCRYPT_ALLOW_EXPORT_FLAG | X509PrivateKeyExportFlags.XCN_NCRYPT_ALLOW_ARCHIVING_FLAG | X509PrivateKeyExportFlags.XCN_NCRYPT_ALLOW_PLAINTEXT_ARCHIVING_FLAG;
            privateKey.SecurityDescriptor = "D:PAI(A;;0xd01f01ff;;;SY)(A;;0xd01f01ff;;;BA)(A;;0x80120089;;;NS)";
            privateKey.Create();

            var hashobj = new CObjectId();
            hashobj.InitializeFromAlgorithmName(ObjectIdGroupId.XCN_CRYPT_HASH_ALG_OID_GROUP_ID,
                                                ObjectIdPublicKeyFlags.XCN_CRYPT_OID_INFO_PUBKEY_ANY,
                                                AlgorithmFlags.AlgorithmFlagsNone, "SHA256");

            var oid = new CObjectId();
            oid.InitializeFromValue("1.3.6.1.5.5.7.3.1"); // SSL server
            var oidlist = new CObjectIds();
            oidlist.Add(oid);

            var coid = new CObjectId();
            coid.InitializeFromValue("1.3.6.1.5.5.7.3.2"); // Client auth
            oidlist.Add(coid);

            var eku = new CX509ExtensionEnhancedKeyUsage();
            eku.InitializeEncode(oidlist);

            // Create the self signing request
            var cert = new CX509CertificateRequestCertificate();
            cert.InitializeFromPrivateKey(X509CertificateEnrollmentContext.ContextAdministratorForceMachine, privateKey, "");
            cert.Subject = dn;
            cert.Issuer = neos;
            cert.NotBefore = DateTime.Now.AddDays(-10);

            cert.NotAfter = DateTime.Now.AddYears(years);
            cert.X509Extensions.Add((CX509Extension)eku); // add the EKU
            cert.HashAlgorithm = hashobj; // Specify the hashing algorithm
            cert.Encode(); // encode the certificate

            // Do the final enrollment process
            var enroll = new CX509Enrollment();
            enroll.InitializeFromRequest(cert); // load the certificate
            enroll.CertificateFriendlyName = subjectName; // Optional: add a friendly name

            string csr = enroll.CreateRequest(); // Output the request in base64

            // and install it back as the response
            enroll.InstallResponse(InstallResponseRestrictionFlags.AllowUntrustedCertificate, csr, EncodingType.XCN_CRYPT_STRING_BASE64, ""); // no password

            // output a base64 encoded PKCS#12 so we can import it back to the .Net security classes
            var base64encoded = enroll.CreatePFX("", PFXExportOptions.PFXExportChainWithRoot);

            return base64encoded;           
        }

        /// <summary>
        /// InternalCreateSelfSignedCertificate method implementation
        /// </summary>
        [SuppressUnmanagedCodeSecurity]
        private static string InternalCreateSelfSignedCertificateForSQL(string keyName, int years)
        {

            var dn = new CX500DistinguishedName();
            var neos = new CX500DistinguishedName();
            dn.Encode("CN=" + keyName, X500NameFlags.XCN_CERT_NAME_STR_NONE);
            neos.Encode("CN=Always Encrypted Certificate", X500NameFlags.XCN_CERT_NAME_STR_NONE);

            CX509PrivateKey privateKey = new CX509PrivateKey();
            privateKey.ProviderName = "Microsoft RSA SChannel Cryptographic Provider";
            privateKey.MachineContext = true;
            privateKey.Length = 2048;
            privateKey.KeySpec = X509KeySpec.XCN_AT_KEYEXCHANGE; // use is not limited
            privateKey.ExportPolicy = X509PrivateKeyExportFlags.XCN_NCRYPT_ALLOW_PLAINTEXT_EXPORT_FLAG | X509PrivateKeyExportFlags.XCN_NCRYPT_ALLOW_EXPORT_FLAG | X509PrivateKeyExportFlags.XCN_NCRYPT_ALLOW_ARCHIVING_FLAG | X509PrivateKeyExportFlags.XCN_NCRYPT_ALLOW_PLAINTEXT_ARCHIVING_FLAG;
            privateKey.SecurityDescriptor = "D:PAI(A;;0xd01f01ff;;;SY)(A;;0xd01f01ff;;;BA)(A;;0x80120089;;;NS)";
            privateKey.Create();

            var hashobj = new CObjectId();
            hashobj.InitializeFromAlgorithmName(ObjectIdGroupId.XCN_CRYPT_HASH_ALG_OID_GROUP_ID,
                                                ObjectIdPublicKeyFlags.XCN_CRYPT_OID_INFO_PUBKEY_ANY,
                                                AlgorithmFlags.AlgorithmFlagsNone, "SHA256");


           // 2.5.29.37 – Enhanced Key Usage includes

            var oid = new CObjectId();
            oid.InitializeFromValue("1.3.6.1.5.5.8.2.2"); // IP security IKE intermediate
            var oidlist = new CObjectIds();
            oidlist.Add(oid);

            var coid = new CObjectId();
            coid.InitializeFromValue("1.3.6.1.4.1.311.10.3.11"); // Key Recovery
            oidlist.Add(coid);

            var eku = new CX509ExtensionEnhancedKeyUsage();
            eku.InitializeEncode(oidlist);

            // Create the self signing request
            var cert = new CX509CertificateRequestCertificate();
            cert.InitializeFromPrivateKey(X509CertificateEnrollmentContext.ContextAdministratorForceMachine, privateKey, "");
            cert.Subject = dn;
            cert.Issuer = neos;
            cert.NotBefore = DateTime.Now.AddDays(-10);

            cert.NotAfter = DateTime.Now.AddYears(years);
            cert.X509Extensions.Add((CX509Extension)eku); // add the EKU
            cert.HashAlgorithm = hashobj; // Specify the hashing algorithm
            cert.Encode(); // encode the certificate

            // Do the final enrollment process
            var enroll = new CX509Enrollment();
            enroll.InitializeFromRequest(cert); // load the certificate
            enroll.CertificateFriendlyName = keyName; // Optional: add a friendly name

            string csr = enroll.CreateRequest(); // Output the request in base64

            // and install it back as the response
            enroll.InstallResponse(InstallResponseRestrictionFlags.AllowUntrustedCertificate, csr, EncodingType.XCN_CRYPT_STRING_BASE64, ""); // no password

            // output a base64 encoded PKCS#12 so we can import it back to the .Net security classes
            var base64encoded = enroll.CreatePFX("", PFXExportOptions.PFXExportChainWithRoot);

            return base64encoded;
        }

    }
}
