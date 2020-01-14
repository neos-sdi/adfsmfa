using CERTENROLLLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Neos.IdentityServer.MultiFactor.Data
{
    /// <summary>
    /// Certs class implmentation
    /// </summary>
    public static class Certs
    {
        /// <summary>
        /// GetCertificate method implementation
        /// </summary>
        public static X509Certificate2 GetCertificate(string value, StoreLocation location)
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
            X509Certificate2 x509 = new X509Certificate2(Convert.FromBase64String(cert), "", X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet);
            CleanSelfSignedCertificate(x509);
            return x509;
        }

        /// <summary>
        /// CreateSelfSignedCertificateForSQLEncryption method implementation
        /// </summary>
        public static X509Certificate2 CreateSelfSignedCertificateForSQLEncryption(string subjectName, int years)
        {
            string cert = InternalCreateSelfSignedCertificateForSQL(subjectName, years);
            // instantiate the target class with the PKCS#12 data (and the empty password)
            X509Certificate2 x509 = new X509Certificate2(Convert.FromBase64String(cert), "", X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet);
            CleanSelfSignedCertificate(x509);
            return x509;
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
        /// CleanSelfSignedCertificate method implmentation
        /// </summary>
        public static bool CleanSelfSignedCertificate(X509Certificate2 cert)
        {
            if (cert != null)
            {
                X509Store store = new X509Store(StoreName.CertificateAuthority, StoreLocation.LocalMachine);
                store.Open(OpenFlags.MaxAllowed);
                try
                {
                    X509Certificate2Collection collection2 = (X509Certificate2Collection)store.Certificates;
                    X509Certificate2Collection findCollection2 = (X509Certificate2Collection)collection2.Find(X509FindType.FindByThumbprint, cert.Thumbprint, false);
                    foreach (X509Certificate2 x509 in findCollection2)
                    {
                        store.Remove(x509);
                    }
                    return true;
                }
                catch
                {
                    return false;
                }
                finally
                {
                    store.Close();
                }
            }
            else
                return false;
        }

        /// <summary>
        /// CreateSelfSignedCertificateAsString method implementation
        /// </summary>
        public static string CreateSelfSignedCertificateAsString(string subjectName, int years, string pwd = "")
        {
            return InternalCreateSelfSignedCertificate(subjectName, years, pwd);
        }

        /// <summary>
        /// CreateSelfSignedCertificateForSQLAsString method implementation
        /// </summary>
        public static string CreateSelfSignedCertificateForSQLAsString(string subjectName, int years, string pwd = "")
        {
            return InternalCreateSelfSignedCertificateForSQL(subjectName, years, pwd);
        }

        /// <summary>
        /// InternalCreateSelfSignedCertificate method implementation
        /// </summary>
        [SuppressUnmanagedCodeSecurity]
        private static string InternalCreateSelfSignedCertificate(string subjectName, int years, string pwd = "")
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
            enroll.InstallResponse(InstallResponseRestrictionFlags.AllowUntrustedCertificate, csr, EncodingType.XCN_CRYPT_STRING_BASE64, "");

            // output a base64 encoded PKCS#12 so we can import it back to the .Net security classes
            var base64encoded = enroll.CreatePFX(pwd, PFXExportOptions.PFXExportChainWithRoot);

            return base64encoded;
        }

        /// <summary>
        /// InternalCreateSelfSignedCertificate method implementation
        /// </summary>
        [SuppressUnmanagedCodeSecurity]
        private static string InternalCreateSelfSignedCertificateForSQL(string keyName, int years, string pwd = "")
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
            enroll.InstallResponse(InstallResponseRestrictionFlags.AllowUntrustedCertificate, csr, EncodingType.XCN_CRYPT_STRING_BASE64, "");

            // output a base64 encoded PKCS#12 so we can import it back to the .Net security classes
            var base64encoded = enroll.CreatePFX(pwd, PFXExportOptions.PFXExportChainWithRoot);

            return base64encoded;
        }

    }

    /// <summary>
    /// CheckSumEncoding class implementation
    /// </summary>
    public static class CheckSumEncoding
    {
        /// <summary>
        /// EncodeUserID 
        /// </summary>
        public static byte[] EncodeUserID(int challengesize, string username)
        {
            switch (challengesize)
            {
                case 16:
                    return CheckSum128(username);
                case 20:
                    return CheckSum160(username);
                case 32:
                    return CheckSum256(username);
                case 48:
                    return CheckSum384(username);
                case 64:
                    return CheckSum512(username);
                default:
                    return CheckSum128(username);
            }
        }

        /// <summary>
        /// EncodeByteArray 
        /// </summary>
        public static byte[] EncodeByteArray(int challengesize, byte[] data)
        {
            switch (challengesize)
            {
                case 16:
                    return CheckSum128(data);
                case 20:
                    return CheckSum160(data);
                case 32:
                    return CheckSum256(data);
                case 48:
                    return CheckSum384(data);
                case 64:
                    return CheckSum512(data);
                default:
                    return CheckSum128(data);
            }
        }

        /// <summary>
        /// CheckSum128 method implementation
        /// </summary>
        public static byte[] CheckSum128(byte[] value)
        {
            byte[] hash = null;
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5Cng.Create())
            {
                hash = md5.ComputeHash(value);
            }
            return hash;
        }

        /// <summary>
        /// CheckSum128 method implementation
        /// </summary>
        public static byte[] CheckSum128(string value)
        {
            byte[] hash = null;
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                hash = md5.ComputeHash(Encoding.UTF8.GetBytes(value));
            }
            return hash;
        }

        /// <summary>
        /// CheckSum160 method implementation
        /// </summary>
        public static byte[] CheckSum160(byte[] value)
        {
            byte[] hash = null;
            using (System.Security.Cryptography.SHA1 sha1 = System.Security.Cryptography.SHA1Cng.Create())
            {
                hash = sha1.ComputeHash(value);
            }
            return hash;
        }

        /// <summary>
        /// CheckSum160 method implementation
        /// </summary>
        public static byte[] CheckSum160(string value)
        {
            byte[] hash = null;
            using (System.Security.Cryptography.SHA1 sha1 = System.Security.Cryptography.SHA1Cng.Create())
            {
                hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(value));
            }
            return hash;
        }

        /// <summary>
        /// CheckSum256 method implementation
        /// </summary>
        public static byte[] CheckSum256(byte[] value)
        {
            byte[] hash = null;
            using (System.Security.Cryptography.SHA256 sha256 = System.Security.Cryptography.SHA256Cng.Create())
            {
                hash = sha256.ComputeHash(value);
            }
            return hash;
        }

        /// <summary>
        /// CheckSum256 method implementation
        /// </summary>
        public static byte[] CheckSum256(string value)
        {
            byte[] hash = null;
            using (System.Security.Cryptography.SHA256 sha256 = System.Security.Cryptography.SHA256.Create())
            {
                hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(value));
            }
            return hash;
        }

        /// <summary>
        /// CheckSum384 method implementation
        /// </summary>
        public static byte[] CheckSum384(byte[] value)
        {
            byte[] hash = null;
            using (System.Security.Cryptography.SHA384 sha384 = System.Security.Cryptography.SHA384Cng.Create())
            {
                hash = sha384.ComputeHash(value);
            }
            return hash;
        }

        /// <summary>
        /// CheckSum384 method implementation
        /// </summary>
        public static byte[] CheckSum384(string value)
        {
            byte[] hash = null;
            using (System.Security.Cryptography.SHA384 sha384 = System.Security.Cryptography.SHA384Managed.Create())
            {
                hash = sha384.ComputeHash(Encoding.UTF8.GetBytes(value));
            }
            return hash;
        }

        /// <summary>
        /// CheckSum512 method implementation
        /// </summary>
        public static byte[] CheckSum512(byte[] value)
        {
            byte[] hash = null;
            using (System.Security.Cryptography.SHA512 sha512 = System.Security.Cryptography.SHA512Cng.Create())
            {
                hash = sha512.ComputeHash(value);
            }
            return hash;
        }

        /// <summary>
        /// CheckSum512 method implementation
        /// </summary>
        public static byte[] CheckSum512(string value)
        {
            byte[] hash = null;
            using (System.Security.Cryptography.SHA512 sha512 = System.Security.Cryptography.SHA512Managed.Create())
            {
                hash = sha512.ComputeHash(Encoding.UTF8.GetBytes(value));
            }
            return hash;
        }

        /// <summary>
        /// CheckSum method implementation
        /// </summary>
        public static byte[] CheckSum(string value)
        {
            byte[] hash = null;
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                hash = md5.ComputeHash(Encoding.UTF8.GetBytes(value));
            }
            return hash;
        }

        /// <summary>
        /// CheckSum method implementation
        /// </summary>
        public static string CheckSumAsString(string value)
        {
            string hash = null;
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                hash = BitConverter.ToString(md5.ComputeHash(Encoding.UTF8.GetBytes(value)));
            }
            return hash.Replace("-", String.Empty);
        }

    }

    /// <summary>
    /// HexaEncoding static class
    /// </summary>
    public static class HexaEncoding
    {
        /// <summary>
        /// GetByteArrayFromHexString method
        /// </summary>
        public static byte[] GetByteArrayFromHexString(String value)
        {
            int len = value.Length;
            byte[] bytes = new byte[len / 2];
            for (int i = 0; i < len; i += 2)
                bytes[i / 2] = Convert.ToByte(value.Substring(i, 2), 16);
            return bytes;
        }

        /// <summary>
        /// GetHexStringFromByteArray method
        /// </summary>
        public static string GetHexStringFromByteArray(byte[] data)
        {
            int len = data.Length;
            StringBuilder builder = new StringBuilder(len * 2);
            foreach (byte b in data)
            {
                builder.AppendFormat("{0:x2}", b);
            }
            return builder.ToString().ToUpper();
        }
    }
}
