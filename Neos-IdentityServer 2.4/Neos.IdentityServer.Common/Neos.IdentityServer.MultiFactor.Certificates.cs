//******************************************************************************************************************************************************************************************//
// Copyright (c) 2019 Neos-Sdi (http://www.neos-sdi.com)                                                                                                                                    //                        
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
using System.Text;


namespace Neos.IdentityServer.MultiFactor
{
    /// <summary>
    /// BaseEncryption class implmentation
    /// </summary>
    public abstract class BaseEncryption: IDisposable
    {
        private string _xorsecret = string.Empty;
        private X509Certificate2 _cert = null;
        private byte[] _checksum;

        /// <summary>
        /// Constructor
        /// </summary>
        public BaseEncryption(string xorsecret)
        {
            _xorsecret = xorsecret;
        }

        /// <summary>
        /// XORSecret property
        /// </summary>
        public string XORSecret
        {
            get { return _xorsecret; }
            internal set { _xorsecret = value; }
        }

        /// <summary>
        /// CheckSum property
        /// </summary>
        public byte[] CheckSum
        {
            get { return _checksum; }
            internal set { _checksum = value; }
        }

        /// <summary>
        /// Certificate property
        /// </summary>
        public X509Certificate2 Certificate
        {
            get { return _cert; }
            set { _cert = value; }
        }

        public abstract byte[] Encrypt(string username);
        public abstract byte[] Decrypt(byte[] encrypted, string username);
        protected abstract void Dispose(bool disposing);

        /// <summary>
        /// XOREncryptOrDecrypt method
        /// </summary>
        protected byte[] XOREncryptOrDecrypt(byte[] value)
        {
            byte[] xor = new byte[value.Length];

            for (int i = 0; i < value.Length; i++)
            {
                xor[i] = (byte)(value[i] ^ _xorsecret[i % _xorsecret.Length]);
            }
            return xor;
        }

        /// <summary>
        /// Dispose IDispose method implementation
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    /// Encryption class implmentation
    /// </summary>
    public class Encryption: BaseEncryption
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public Encryption(string xorsecret):base(xorsecret)
        {
            Certificate = null;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public Encryption(string xorsecret, string thumbprint): base(xorsecret)
        {
            Certificate = Certs.GetCertificate(thumbprint, StoreLocation.LocalMachine);
        }

        /// <summary>
        /// EncryptV1 method (for compatibility with old versions)
        /// </summary>
        public override byte[] Encrypt(string username)
        {
            try
            {
                if (Certificate == null)
                    throw new Exception("Invalid encryption certificate !");
                byte[] plainBytes = GenerateKey(username);
                byte[] encryptedBytes = null;
                var key = Certificate.GetRSAPublicKey();
                if (key == null)
                    throw new CryptographicException("Invalid public Key !");

                if (key is RSACng)
                    encryptedBytes = ((RSACng)key).Encrypt(plainBytes, RSAEncryptionPadding.OaepSHA256);
                else
                    encryptedBytes = ((RSACryptoServiceProvider)key).Encrypt(plainBytes, true);
                return encryptedBytes;
            }
            catch (Exception ex)
            {
                Log.WriteEntry(string.Format("Crytograpphic Error for user {0} \r {1}", ex.Message, username), System.Diagnostics.EventLogEntryType.Error, 0000);
                return null;
            }
        }

        /// <summary>
        /// Decrypt method
        /// </summary>
        public override byte[] Decrypt(byte[] encryptedBytes, string username)
        {
            try
            {
                if (Certificate == null)
                    throw new Exception("Invalid decryption certificate !");
                byte[] decryptedBytes = null;
                var key = Certificate.GetRSAPrivateKey();
                if (key == null)
                    throw new CryptographicException("Invalid private Key !");

                if (key is RSACng)
                    decryptedBytes = ((RSACng)key).Decrypt(encryptedBytes, RSAEncryptionPadding.OaepSHA256);
                else
                    decryptedBytes = ((RSACryptoServiceProvider)key).Decrypt(encryptedBytes, true);

                MemoryStream mem = new MemoryStream(decryptedBytes);
                string decryptedvalue = DeserializeFromStream(mem);
                int l = Convert.ToInt32(decryptedvalue.Substring(32, 3));

                string outval = decryptedvalue.Substring(35, l);
                byte[] bytes = new byte[outval.Length * sizeof(char)];
                Buffer.BlockCopy(outval.ToCharArray(), 0, bytes, 0, bytes.Length);
                this.CheckSum = Utilities.CheckSum(outval); 
                return bytes;
            }
            catch (System.Security.Cryptography.CryptographicException ce)
            {
                Log.WriteEntry(string.Format("Crytograpphic Error for user {0} \r {1}", ce.Message, username), System.Diagnostics.EventLogEntryType.Error, 0000);
                return null;
            }
            catch (Exception ex)
            {
                Log.WriteEntry(string.Format("Crytograpphic Error for user {0} \r {1}", ex.Message, username), System.Diagnostics.EventLogEntryType.Error, 0000);
                return null;
            }
        }

        /// <summary>
        /// GenerateKey method (for compatibility with old versions)
        /// </summary>
        private byte[] GenerateKey(string username)
        {
            string ptext = Guid.NewGuid().ToString("N") + username.Length.ToString("000") + username + Guid.NewGuid().ToString("N");
            return SerializeToStream(ptext).ToArray();
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
        /// Dispose method implementation
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Certificate != null)
                    Certificate.Reset();
            }
        }
    }

    /// <summary>
    /// RSAEncryption class implmentation
    /// </summary>
    public class RSAEncryption: BaseEncryption
    {
        

        /// <summary>
        /// Constructor
        /// </summary>
        public RSAEncryption(string xorsecret): base(xorsecret)
        {
            Certificate = null;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public RSAEncryption(string xorsecret, string thumbprint): base(xorsecret)
        {
            Certificate = Certs.GetCertificate(thumbprint, StoreLocation.LocalMachine);
        }

        /// <summary>
        /// Encrypt method
        /// </summary>
        public override byte[] Encrypt(string username)
        {
            try
            {
                if (Certificate == null)
                    throw new Exception("Invalid encryption certificate !");
                byte[] plainBytes = GenerateKey(username);
                byte[] encryptedBytes = null;
                var key = Certificate.GetRSAPublicKey();
                if (key == null)
                    throw new CryptographicException("Invalid public Key !");

                if (key is RSACng)
                    encryptedBytes = ((RSACng)key).Encrypt(plainBytes, RSAEncryptionPadding.OaepSHA256);
                else
                    encryptedBytes = ((RSACryptoServiceProvider)key).Encrypt(plainBytes, true);

                return XOREncryptOrDecrypt(encryptedBytes);
            }
            catch (Exception ex)
            {
                Log.WriteEntry(string.Format("Crytograpphic Error for user {0} \r {1}", ex.Message, username), System.Diagnostics.EventLogEntryType.Error, 0000);
                return null;
            }
        }

        /// <summary>
        /// Decrypt method
        /// </summary>
        public override byte[] Decrypt(byte[] encryptedBytes, string username)
        {
            try
            {
                if (Certificate == null)
                    throw new Exception("Invalid decryption certificate !");

                byte[] decryptedBytes = XOREncryptOrDecrypt(encryptedBytes);
                byte[] fulldecryptedBytes = null;

                var key = Certificate.GetRSAPrivateKey();
                if (key == null)
                    throw new CryptographicException("Invalid private Key !");

                if (key is RSACng)
                    fulldecryptedBytes = ((RSACng)key).Decrypt(decryptedBytes, RSAEncryptionPadding.OaepSHA256);
                else
                    fulldecryptedBytes = ((RSACryptoServiceProvider)key).Decrypt(decryptedBytes, true);

                byte[] userbuff = new byte[fulldecryptedBytes.Length - 128];
                Buffer.BlockCopy(fulldecryptedBytes, 128, userbuff, 0, fulldecryptedBytes.Length - 128);
                this.CheckSum = userbuff;

                byte[] decryptedkey = new byte[128];
                Buffer.BlockCopy(fulldecryptedBytes, 0, decryptedkey, 0, 128);
                return decryptedkey;
            }
            catch (System.Security.Cryptography.CryptographicException ce)
            {
                Log.WriteEntry(string.Format("Crytograpphic Error for user {0} \r {1}", ce.Message, username), System.Diagnostics.EventLogEntryType.Error, 0000);
                return null;
            }
            catch (Exception ex)
            {
                Log.WriteEntry(string.Format("Crytograpphic Error for user {0} \r {1}", ex.Message, username), System.Diagnostics.EventLogEntryType.Error, 0000);
                return null;
            }
        }

        /// <summary>
        /// GenerateKey method
        /// </summary>
        private byte[] GenerateKey(string username)
        {
            byte[] text = Utilities.CheckSum(username);

            byte[] buffer = new byte[128 + text.Length];
            RandomNumberGenerator cryptoRandomDataGenerator = new RNGCryptoServiceProvider();
            cryptoRandomDataGenerator.GetBytes(buffer, 0, 128);
            Buffer.BlockCopy(text, 0, buffer, 128, text.Length);
            return buffer;
        }

        /// <summary>
        /// Dispose method implementation
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Certificate != null)
                    Certificate.Reset();
            }
        }
    }

    /// <summary>
    /// RNGXOREncryption class implementation
    /// </summary>
    public class RNGEncryption : BaseEncryption
    {
        KeyGeneratorMode _mode = KeyGeneratorMode.ClientSecret128;

        /// <summary>
        /// Constructor
        /// </summary>
        public RNGEncryption(string xorsecret): base(xorsecret)
        {
            _mode = KeyGeneratorMode.ClientSecret128;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public RNGEncryption(string xorsecret, KeyGeneratorMode mode): base(xorsecret)
        {
            _mode = mode;
        }

        /// <summary>
        /// Encrypt method
        /// </summary>
        public override byte[] Encrypt(string username)
        {
            try
            {
                if (string.IsNullOrEmpty(username))
                    throw new Exception("Invalid encryption context !");
                byte[] plainBytes = GenerateKey(username);
                return XOREncryptOrDecrypt(plainBytes);
            }
            catch (Exception ex)
            {
                Log.WriteEntry(string.Format("Crytograpphic Error for user {0} \r {1}", ex.Message, username), System.Diagnostics.EventLogEntryType.Error, 0000);
                return null;
            }
        }

        /// <summary>
        /// Decrypt method
        /// </summary>
        public override byte[] Decrypt(byte[] encryptedBytes, string username)
        {
            try
            {
                if (encryptedBytes == null)
                    throw new Exception("Invalid decryption context !");

                byte[] decryptedBytes = XOREncryptOrDecrypt(encryptedBytes);
                int size = GetSizeFromMode(_mode);
                byte[] userbuff = new byte[decryptedBytes.Length - size];
                Buffer.BlockCopy(decryptedBytes, size, userbuff, 0, decryptedBytes.Length - size);
                this.CheckSum = userbuff;

                byte[] decryptedkey = new byte[size];
                Buffer.BlockCopy(decryptedBytes, 0, decryptedkey, 0, size);
                return decryptedkey;
            }
            catch (System.Security.Cryptography.CryptographicException ce)
            {
                Log.WriteEntry(string.Format("Crytograpphic Error for user {0} \r {1}", ce.Message, username), System.Diagnostics.EventLogEntryType.Error, 0000);
                return null;
            }
            catch (Exception ex)
            {
                Log.WriteEntry(string.Format("Crytograpphic Error for user {0} \r {1}", ex.Message, username), System.Diagnostics.EventLogEntryType.Error, 0000);
                return null;
            }
        }

        /// <summary>
        /// GetSizeFromMode method implementation
        /// </summary>
        private int GetSizeFromMode(KeyGeneratorMode xmode)
        {
            switch (_mode)
            {
                case KeyGeneratorMode.ClientSecret128:
                    return 16;
                case KeyGeneratorMode.ClientSecret256:
                    return 32;
                case KeyGeneratorMode.ClientSecret384:
                    return 48;
                case KeyGeneratorMode.ClientSecret512:
                    return 64;
                default:
                    return 16;
            }
        }

        /// <summary>
        /// GenerateKey method
        /// </summary>
        private byte[] GenerateKey(string username)
        {
            byte[] text = Utilities.CheckSum(username);

            int size = GetSizeFromMode(_mode);
            byte[] buffer = new byte[size + text.Length];

            RandomNumberGenerator cryptoRandomDataGenerator = new RNGCryptoServiceProvider();
            cryptoRandomDataGenerator.GetBytes(buffer, 0, size);
            Buffer.BlockCopy(text, 0, buffer, size, text.Length);
            return buffer;
        }

        /// <summary>
        /// Dispose method implementation
        /// </summary>
        protected override void Dispose(bool disposing)
        {
        }
    }

    /// <summary>
    /// Certs class implmentation
    /// </summary>
    public class Certs
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
}
