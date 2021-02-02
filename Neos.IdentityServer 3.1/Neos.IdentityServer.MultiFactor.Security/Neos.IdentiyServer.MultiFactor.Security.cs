using CERTENROLLLib;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;
using Neos.IdentityServer.MultiFactor.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.ActiveDirectory;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Permissions;
using System.Security.Principal;
using System.Text;
using System.Xml.Serialization;

namespace Neos.IdentityServer.MultiFactor
{
    #region Configuration Reader
    /// <summary>
    /// CFGReaderUtilities class
    /// </summary>
    internal static class CFGReaderUtilities
    {
        private static char sep = Path.DirectorySeparatorChar;
        private static string ConfigReaderCacheFile = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + sep + "MFA" + sep + "Config" + sep + "config.db";
      
        /// <summary>
        /// ReadConfiguration method implementation
        /// </summary>
        internal static MFAConfig ReadConfiguration()
        {
            MFAConfig config = null;
            try
            {
                config = ReadConfigurationFromCache();
                if (config == null)
                    config = ReadConfigurationFromDatabase();
            }
            catch
            {
                config = null;
            }
            return config;
        }

        /// <summary>
        /// ReadConfigurationFromDatabase method implementation
        /// </summary>
        private static MFAConfig ReadConfigurationFromDatabase()
        {
            MFAConfig config = null;
            Runspace SPRunSpace = null;
            PowerShell SPPowerShell = null;
            string pth = Path.GetTempPath() + Path.GetRandomFileName();
            try
            {
                try
                {
                    SPRunSpace = RunspaceFactory.CreateRunspace();

                    SPPowerShell = PowerShell.Create();
                    SPPowerShell.Runspace = SPRunSpace;
                    SPRunSpace.Open();

                    Pipeline pipeline = SPRunSpace.CreatePipeline();
                    Command exportcmd = new Command("Export-AdfsAuthenticationProviderConfigurationData", false);
                    CommandParameter NParam = new CommandParameter("Name", "MultifactorAuthenticationProvider");
                    exportcmd.Parameters.Add(NParam);
                    CommandParameter PParam = new CommandParameter("FilePath", pth);
                    exportcmd.Parameters.Add(PParam);
                    pipeline.Commands.Add(exportcmd);
                    Collection<PSObject> PSOutput = pipeline.Invoke();
                }
                finally
                {
                    if (SPRunSpace != null)
                        SPRunSpace.Close();
                    if (SPPowerShell != null)
                        SPPowerShell.Dispose();
                }

                FileStream stm = new FileStream(pth, FileMode.Open, FileAccess.Read);
                XmlConfigSerializer xmlserializer = new XmlConfigSerializer(typeof(MFAConfig));
                using (StreamReader reader = new StreamReader(stm))
                {
                    config = (MFAConfig)xmlserializer.Deserialize(stm);
                    using (AESSystemEncryption MSIS = new AESSystemEncryption())
                    {
                        config.KeysConfig.XORSecret = MSIS.Decrypt(config.KeysConfig.XORSecret);
                        config.Hosts.ActiveDirectoryHost.Password = MSIS.Decrypt(config.Hosts.ActiveDirectoryHost.Password);
                        config.MailProvider.Password = MSIS.Decrypt(config.MailProvider.Password);
                    }; 
                }
            }
            finally
            {
                File.Delete(pth);
            }
            return config;
        }

        /// <summary>
        /// ReadConfigurationFromCache method implementation
        /// </summary>
        private static MFAConfig ReadConfigurationFromCache()
        {
            MFAConfig config = null;
            if (!File.Exists(ConfigReaderCacheFile))
                return null;
            XmlConfigSerializer xmlserializer = new XmlConfigSerializer(typeof(MFAConfig));
            using (FileStream fs = new FileStream(ConfigReaderCacheFile, FileMode.Open, FileAccess.Read))
            {
                byte[] bytes = new byte[fs.Length];
                int n = fs.Read(bytes, 0, (int)fs.Length);
                fs.Close();

                byte[] byt = null;
                using (AESSystemEncryption aes = new AESSystemEncryption())
                {
                    byt = aes.Decrypt(bytes);
                }

                using (MemoryStream ms = new MemoryStream(byt))
                {
                    using (StreamReader reader = new StreamReader(ms))
                    {
                        config = (MFAConfig)xmlserializer.Deserialize(ms);
                       /* using (AESSystemEncryption MSIS = new AESSystemEncryption())
                        {
                            config.KeysConfig.XORSecret = MSIS.Decrypt(config.KeysConfig.XORSecret);
                            config.Hosts.ActiveDirectoryHost.Password = MSIS.Decrypt(config.Hosts.ActiveDirectoryHost.Password);
                            config.MailProvider.Password = MSIS.Decrypt(config.MailProvider.Password);
                        }; */
                    }
                }
            }
            return config;
        }
    }
    #endregion

    #region System Utilities
    /// <summary>
    /// CFGUtilities class
    /// </summary>
    internal static class SystemUtilities
    {
        private static char sep = Path.DirectorySeparatorChar;
        internal static string SystemCacheFile = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + sep + "MFA" + sep + "Config" + sep + "system.db";
        internal static byte[] SystemCacheKey = null;

        /// <summary>
        /// CFGUtilities static constructor
        /// </summary>
        static SystemUtilities()
        {
            SystemCacheKey = ReadConfigurationKey();
        }

        internal static byte[] Key
        {
            get { return SystemCacheKey; }
        }      

        #region Admin Key Reader
        /// <summary>
        /// ReadConfigurationKey method implmentation
        /// </summary>
        private static byte[] ReadConfigurationKey()
        {
            if (SystemCacheKey != null)
                return SystemCacheKey;

            Domain dom = Domain.GetComputerDomain();
            Guid gd = dom.GetDirectoryEntry().Guid;

            byte[] _key = new byte[32];
            byte[] _gd = new byte[16];
            Buffer.BlockCopy(gd.ToByteArray(), 0, _gd, 0, 16);
            Buffer.BlockCopy(_gd, 0, _key, 0, 16);
            Array.Reverse(_gd);
            Buffer.BlockCopy(_gd, 0, _key, 16, 16);
            return _key;
        }
        #endregion
    }
    #endregion

    #region XmlConfigSerializer
    /// <summary>
    /// XmlConfigSerializer class
    /// </summary>
    internal class XmlConfigSerializer : XmlSerializer
    {
        public XmlConfigSerializer(Type type) : base(type)
        {
            this.UnknownAttribute += OnUnknownAttribute;
            this.UnknownElement += OnUnknownElement;
            this.UnknownNode += OnUnknownNode;
            this.UnreferencedObject += OnUnreferencedObject;
        }

        /// <summary>
        /// OnUnknownNode method implementation
        /// </summary>
        private void OnUnknownNode(object sender, XmlNodeEventArgs e)
        {
            Log.WriteEntry("Xml Serialization error : Unknow Node : " + e.Name + " Position (" + e.LineNumber.ToString() + ", " + e.LinePosition.ToString() + ")", EventLogEntryType.Error, 700);
        }

        /// <summary>
        /// OnUnknownElement method implementation
        /// </summary>
        private void OnUnknownElement(object sender, XmlElementEventArgs e)
        {
            Log.WriteEntry("Xml Serialization error : Unknow Element : " + e.Element.Name + " at Position (" + e.LineNumber.ToString() + ", " + e.LinePosition.ToString() + ")", EventLogEntryType.Error, 701);
        }

        /// <summary>
        /// OnUnknownAttribute method implementation
        /// </summary>
        private void OnUnknownAttribute(object sender, XmlAttributeEventArgs e)
        {
            Log.WriteEntry("Xml Serialization error : Unknow Attibute : " + e.Attr.Name + " at Position (" + e.LineNumber.ToString() + ", " + e.LinePosition.ToString() + ")", EventLogEntryType.Error, 702);
        }

        /// <summary>
        /// OnUnreferencedObject method implementation
        /// </summary>
        private void OnUnreferencedObject(object sender, UnreferencedObjectEventArgs e)
        {
            Log.WriteEntry("Xml Serialization error : Unknow Object : " + e.UnreferencedId + " of Type (" + e.UnreferencedObject.GetType().ToString() + ")", EventLogEntryType.Error, 703);
        }
    }
    #endregion

    #region Log
    /// <summary>
    /// Log class
    /// </summary>
    public static class Log
    {
        private const string EventLogSource = "ADFS MFA Service";
        private const string EventLogGroup = "Application";

        /// <summary>
        /// Log constructor
        /// </summary>
        static Log()
        {
            if (!EventLog.SourceExists(Log.EventLogSource))
                EventLog.CreateEventSource(Log.EventLogSource, Log.EventLogGroup);
        }

        /// <summary>
        /// WriteEntry method implementation
        /// </summary>
        public static void WriteEntry(string message, EventLogEntryType type, int eventID)
        {
            EventLog.WriteEntry(EventLogSource, message, type, eventID);
        }
    }
    #endregion

    #region Certs
    /// <summary>
    /// Certs class implmentation
    /// </summary>
    public static class Certs
    {
        private static RegistryVersion _RegistryVersion;

        static Certs()
        {
            _RegistryVersion = new RegistryVersion();
        }

        /// <summary>
        /// GetCertificate method implementation
        /// </summary>
        public static bool CertificateExists(string thumbprint, StoreLocation location)
        {
            X509Store store = new X509Store(location);
            store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
            try
            {
                X509Certificate2Collection collection = (X509Certificate2Collection)store.Certificates;
                X509Certificate2Collection findCollection = (X509Certificate2Collection)collection.Find(X509FindType.FindByThumbprint, thumbprint, false);

                foreach (X509Certificate2 x509 in findCollection)
                {
                    return true;
                }
            }
            finally
            {
                store.Close();
            }
            return false;
        }

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
        /// RemoveSelfSignedCertificate method implmentation
        /// </summary>
        internal static bool RemoveSelfSignedCertificate(X509Certificate2 cert, StoreLocation location = StoreLocation.LocalMachine)
        {
            if (cert != null)
            {
                X509Store store1 = new X509Store(StoreName.My, location);
                X509Store store2 = new X509Store(StoreName.CertificateAuthority, location);
                store1.Open(OpenFlags.MaxAllowed);
                store2.Open(OpenFlags.MaxAllowed);
                try
                {
                    X509Certificate2Collection collection1 = (X509Certificate2Collection)store1.Certificates;
                    X509Certificate2Collection findCollection1 = (X509Certificate2Collection)collection1.Find(X509FindType.FindByThumbprint, cert.Thumbprint, false);
                    foreach (X509Certificate2 x509 in findCollection1)
                    {
                        store1.Remove(x509);
                        x509.Reset();
                    }
                    X509Certificate2Collection collection2 = (X509Certificate2Collection)store2.Certificates;
                    X509Certificate2Collection findCollection2 = (X509Certificate2Collection)collection2.Find(X509FindType.FindByThumbprint, cert.Thumbprint, false);
                    foreach (X509Certificate2 x509 in findCollection2)
                    {
                        store2.Remove(x509);
                        x509.Reset();
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
        internal static bool CleanSelfSignedCertificate(X509Certificate2 cert, StoreLocation location = StoreLocation.LocalMachine)
        {
            if (cert != null)
            {
                X509Store store = new X509Store(StoreName.CertificateAuthority, location);
                store.Open(OpenFlags.MaxAllowed);
                try
                {
                    X509Certificate2Collection collection2 = (X509Certificate2Collection)store.Certificates;
                    X509Certificate2Collection findCollection2 = (X509Certificate2Collection)collection2.Find(X509FindType.FindByThumbprint, cert.Thumbprint, false);
                    foreach (X509Certificate2 x509 in findCollection2)
                    {
                        store.Remove(x509);
                        x509.Reset();
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
        /// CreateADFSCertificate method implementation
        /// </summary>
        internal static X509Certificate2 CreateADFSCertificate(string subjectName, bool issigning, int years, out string cert)
        {
            string strcert = string.Empty;
            if (_RegistryVersion.IsWindows2012R2)
                strcert = InternalCreateADFSCertificate2012R2(subjectName, issigning, years);
            else
                strcert = InternalCreateADFSCertificate(subjectName, issigning, years);
            cert = strcert;
            X509Certificate2 x509 = new X509Certificate2(Convert.FromBase64String(strcert), "", X509KeyStorageFlags.Exportable | X509KeyStorageFlags.EphemeralKeySet);
            if (CleanSelfSignedCertificate(x509, StoreLocation.LocalMachine))
                return x509;
            else
                return null;
        }

        /// <summary>
        /// CreateRSACertificate method implementation
        /// </summary>
        internal static X509Certificate2 CreateRSACertificate(string subjectName, int years, out string cert)
        {
            string strcert = string.Empty;
            if (_RegistryVersion.IsWindows2012R2)
                strcert = InternalCreateRSACertificate2012R2(subjectName, years);
            else
                strcert = InternalCreateRSACertificate(subjectName, years);
            cert = strcert;
            X509Certificate2 x509 = new X509Certificate2(Convert.FromBase64String(strcert), "", X509KeyStorageFlags.Exportable | X509KeyStorageFlags.EphemeralKeySet);
            if (CleanSelfSignedCertificate(x509, StoreLocation.LocalMachine))
                return x509;
            else
                return null;
        }

        /// <summary>
        /// CreateRSACertificateForSQLEncryption method implementation
        /// </summary>
        internal static X509Certificate2 CreateRSACertificateForSQLEncryption(string subjectName, int years, out string cert)
        {
            string strcert = string.Empty;
            if (_RegistryVersion.IsWindows2012R2)
                strcert = InternalCreateSQLCertificate2012R2(subjectName, years);
            else
                strcert = InternalCreateSQLCertificate(subjectName, years);
            cert = strcert;
            X509Certificate2 x509 = new X509Certificate2(Convert.FromBase64String(strcert), "", X509KeyStorageFlags.Exportable | X509KeyStorageFlags.EphemeralKeySet);
            if (CleanSelfSignedCertificate(x509, StoreLocation.LocalMachine))
                return x509;
            else
                return null;
        }

        /// <summary>
        /// CreateRSAEncryptionCertificateForUser method implementation
        /// </summary>
        public static X509Certificate2 CreateRSAEncryptionCertificateForUser(string subjectName, int years, string pwd = "")
        {
            string strcert = string.Empty;
            if (_RegistryVersion.IsWindows2012R2)
                strcert = InternalCreateUserRSACertificate2012R2(subjectName, years, pwd);
            else
                strcert = InternalCreateUserRSACertificate(subjectName, years, pwd);
            X509Certificate2 x509 = new X509Certificate2(Convert.FromBase64String(strcert), pwd, X509KeyStorageFlags.Exportable | X509KeyStorageFlags.EphemeralKeySet);
            if (Certs.RemoveSelfSignedCertificate(x509, StoreLocation.CurrentUser))
                return x509;
            else
                return null;
        }

        #region Windows 2016 & 2019
        /// <summary>
        /// InternalCreateRSACertificate method implementation
        /// </summary>
        [SuppressUnmanagedCodeSecurity]
        private static string InternalCreateRSACertificate(string subjectName, int years)
        {
            string base64encoded = string.Empty;
            CX500DistinguishedName dn = new CX500DistinguishedName();
            CX500DistinguishedName neos = new CX500DistinguishedName();
            dn.Encode("CN=" + subjectName + " " + DateTime.UtcNow.ToString("G") + " GMT", X500NameFlags.XCN_CERT_NAME_STR_NONE);
            neos.Encode("CN=MFA RSA Keys Certificate", X500NameFlags.XCN_CERT_NAME_STR_NONE);

            CX509PrivateKey privateKey = new CX509PrivateKey
            {
                ProviderName = "Microsoft RSA SChannel Cryptographic Provider",
                MachineContext = true,
                Length = 2048,
                KeySpec = X509KeySpec.XCN_AT_KEYEXCHANGE, // use is not limited
                ExportPolicy = X509PrivateKeyExportFlags.XCN_NCRYPT_ALLOW_EXPORT_FLAG,
                KeyUsage = X509PrivateKeyUsageFlags.XCN_NCRYPT_ALLOW_DECRYPT_FLAG,
                SecurityDescriptor = "D:(A;;FA;;;SY)(A;;FA;;;BA)"
            };
            if (!string.IsNullOrEmpty(ClientSIDsProxy.ADFSServiceSID))
                privateKey.SecurityDescriptor += "(A;;FA;;;" + ClientSIDsProxy.ADFSServiceSID + ")";
            if (!string.IsNullOrEmpty(ClientSIDsProxy.ADFSAccountSID))
                privateKey.SecurityDescriptor += "(A;;FA;;;" + ClientSIDsProxy.ADFSAccountSID + ")";
            if (!string.IsNullOrEmpty(ClientSIDsProxy.ADFSAdminGroupSID))
                privateKey.SecurityDescriptor += "(A;;FA;;;" + ClientSIDsProxy.ADFSAdminGroupSID + ")";
            try
            {
                privateKey.Create();
                CObjectId hashobj = new CObjectId();
                hashobj.InitializeFromAlgorithmName(ObjectIdGroupId.XCN_CRYPT_HASH_ALG_OID_GROUP_ID,
                                                    ObjectIdPublicKeyFlags.XCN_CRYPT_OID_INFO_PUBKEY_ANY,
                                                    AlgorithmFlags.AlgorithmFlagsNone, "SHA256");

                CObjectId oid = new CObjectId();
                // oid.InitializeFromValue("1.3.6.1.5.5.7.3.1"); // SSL server  
                oid.InitializeFromValue("1.3.6.1.4.1.311.80.1"); // Encryption

                CObjectIds oidlist = new CObjectIds
                {
                    oid
                };

                CObjectId coid = new CObjectId();
                // coid.InitializeFromValue("1.3.6.1.5.5.7.3.2"); // Client auth
                coid.InitializeFromValue("1.3.6.1.5.5.7.3.3"); // Signature
                oidlist.Add(coid);

                CX509ExtensionEnhancedKeyUsage eku = new CX509ExtensionEnhancedKeyUsage();
                eku.InitializeEncode(oidlist);

                // Create the self signing request
                CX509CertificateRequestCertificate certreq = new CX509CertificateRequestCertificate();
                certreq.InitializeFromPrivateKey(X509CertificateEnrollmentContext.ContextMachine, privateKey, "");
                certreq.Subject = dn;
                certreq.Issuer = neos;
                certreq.NotBefore = DateTime.Now.AddDays(-10);

                certreq.NotAfter = DateTime.Now.AddYears(years);
                certreq.X509Extensions.Add((CX509Extension)eku); // add the EKU
                certreq.HashAlgorithm = hashobj; // Specify the hashing algorithm
                certreq.Encode(); // encode the certificate

                // Do the final enrollment process
                CX509Enrollment enroll = new CX509Enrollment();
                enroll.InitializeFromRequest(certreq); // load the certificate
                enroll.CertificateFriendlyName = subjectName; // Optional: add a friendly name

                string csr = enroll.CreateRequest(); // Output the request in base64

                // and install it back as the response
                enroll.InstallResponse(InstallResponseRestrictionFlags.AllowUntrustedCertificate, csr, EncodingType.XCN_CRYPT_STRING_BASE64, "");

                // output a base64 encoded PKCS#12 so we can import it back to the .Net security classes
                base64encoded = enroll.CreatePFX("", PFXExportOptions.PFXExportChainWithRoot);
            }
            catch (Exception ex)
            {
                if (privateKey != null)
                    privateKey.Delete();
                throw ex;
            }
            finally
            {
                // DO nothing, certificate Key is stored in the system
            }
            return base64encoded;
        }

        /// <summary>
        /// InternalCreateUserRSACertificate method implementation
        /// </summary>
        [SuppressUnmanagedCodeSecurity]
        private static string InternalCreateUserRSACertificate(string subjectName, int years, string pwd = "")
        {
            string base64encoded = string.Empty;
            CX500DistinguishedName dn = new CX500DistinguishedName();
            CX500DistinguishedName neos = new CX500DistinguishedName();
            dn.Encode("CN=" + subjectName, X500NameFlags.XCN_CERT_NAME_STR_NONE);
            neos.Encode("CN=MFA RSA Keys Certificate", X500NameFlags.XCN_CERT_NAME_STR_NONE);

            CX509PrivateKey privateKey = new CX509PrivateKey
            {
                ProviderName = "Microsoft RSA SChannel Cryptographic Provider",
                MachineContext = false,
                Length = 2048,
                KeySpec = X509KeySpec.XCN_AT_KEYEXCHANGE, // use is not limited
                ExportPolicy = X509PrivateKeyExportFlags.XCN_NCRYPT_ALLOW_EXPORT_FLAG,
                SecurityDescriptor = "D:(A;;FA;;;SY)(A;;FA;;;BA)"
            };
            if (!string.IsNullOrEmpty(ClientSIDsProxy.ADFSServiceSID))
                privateKey.SecurityDescriptor += "(A;;FA;;;" + ClientSIDsProxy.ADFSServiceSID + ")";
            if (!string.IsNullOrEmpty(ClientSIDsProxy.ADFSAccountSID))
                privateKey.SecurityDescriptor += "(A;;FA;;;" + ClientSIDsProxy.ADFSAccountSID + ")";
            if (!string.IsNullOrEmpty(ClientSIDsProxy.ADFSAdminGroupSID))
                privateKey.SecurityDescriptor += "(A;;FA;;;" + ClientSIDsProxy.ADFSAdminGroupSID + ")";
            try
            {
                privateKey.Create();
                CObjectId hashobj = new CObjectId();
                hashobj.InitializeFromAlgorithmName(ObjectIdGroupId.XCN_CRYPT_HASH_ALG_OID_GROUP_ID,
                                                    ObjectIdPublicKeyFlags.XCN_CRYPT_OID_INFO_PUBKEY_ANY,
                                                    AlgorithmFlags.AlgorithmFlagsNone, "SHA256");

                CObjectId oid = new CObjectId();
                // oid.InitializeFromValue("1.3.6.1.5.5.7.3.1"); // SSL server  
                oid.InitializeFromValue("1.3.6.1.4.1.311.80.1"); // Encryption

                CObjectIds oidlist = new CObjectIds
                {
                    oid
                };

                CObjectId coid = new CObjectId();
                // coid.InitializeFromValue("1.3.6.1.5.5.7.3.2"); // Client auth
                coid.InitializeFromValue("1.3.6.1.5.5.7.3.3"); // Signature
                oidlist.Add(coid);

                CX509ExtensionEnhancedKeyUsage eku = new CX509ExtensionEnhancedKeyUsage();
                eku.InitializeEncode(oidlist);

                // Create the self signing request
                CX509CertificateRequestCertificate certreq = new CX509CertificateRequestCertificate();
                certreq.InitializeFromPrivateKey(X509CertificateEnrollmentContext.ContextUser, privateKey, "");
                certreq.Subject = dn;
                certreq.Issuer = neos;
                certreq.NotBefore = DateTime.Now.AddDays(-10);

                certreq.NotAfter = DateTime.Now.AddYears(years);
                certreq.X509Extensions.Add((CX509Extension)eku); // add the EKU
                certreq.HashAlgorithm = hashobj; // Specify the hashing algorithm
                certreq.Encode(); // encode the certificate

                // Do the final enrollment process
                CX509Enrollment enroll = new CX509Enrollment();
                enroll.InitializeFromRequest(certreq); // load the certificate
                enroll.CertificateFriendlyName = subjectName; // Optional: add a friendly name

                string csr = enroll.CreateRequest(); // Output the request in base64

                // and install it back as the response
                enroll.InstallResponse(InstallResponseRestrictionFlags.AllowUntrustedCertificate, csr, EncodingType.XCN_CRYPT_STRING_BASE64, "");

                // output a base64 encoded PKCS#12 so we can import it back to the .Net security classes
                base64encoded = enroll.CreatePFX(pwd, PFXExportOptions.PFXExportChainWithRoot);
            }
            catch (Exception ex)
            {
                if (privateKey != null)
                    privateKey.Delete();
                throw ex;
            }
            finally
            {
                if (privateKey != null)
                    privateKey.Delete(); // Remove Stored elsewhere
            }
            return base64encoded;
        }

        /// <summary>
        /// InternalCreateSQLCertificate method implementation
        /// </summary>
        [SuppressUnmanagedCodeSecurity]
        private static string InternalCreateSQLCertificate(string subjectName, int years, string pwd = "")
        {
            string base64encoded = string.Empty;
            CX500DistinguishedName dn = new CX500DistinguishedName();
            CX500DistinguishedName neos = new CX500DistinguishedName();
            dn.Encode("CN=" + subjectName + " " + DateTime.UtcNow.ToString("G") + " GMT", X500NameFlags.XCN_CERT_NAME_STR_NONE);
            neos.Encode("CN=Always Encrypted Certificate", X500NameFlags.XCN_CERT_NAME_STR_NONE);

            CX509PrivateKey privateKey = new CX509PrivateKey
            {
                ProviderName = "Microsoft RSA SChannel Cryptographic Provider",
                MachineContext = true,

                Length = 2048,
                KeySpec = X509KeySpec.XCN_AT_KEYEXCHANGE, // use is not limited
                ExportPolicy = X509PrivateKeyExportFlags.XCN_NCRYPT_ALLOW_EXPORT_FLAG,
                SecurityDescriptor = "D:PAI(A;;0xd01f01ff;;;SY)(A;;0xd01f01ff;;;BA)(A;;0xd01f01ff;;;CO)"
            };
            if (!string.IsNullOrEmpty(ClientSIDsProxy.ADFSServiceSID))
                privateKey.SecurityDescriptor += "(A;;FA;;;" + ClientSIDsProxy.ADFSServiceSID + ")";
            if (!string.IsNullOrEmpty(ClientSIDsProxy.ADFSAccountSID))
                privateKey.SecurityDescriptor += "(A;;FA;;;" + ClientSIDsProxy.ADFSAccountSID + ")";
            if (!string.IsNullOrEmpty(ClientSIDsProxy.ADFSAdminGroupSID))
                privateKey.SecurityDescriptor += "(A;;FA;;;" + ClientSIDsProxy.ADFSAdminGroupSID + ")";
            try
            {
                privateKey.Create();
                CObjectId hashobj = new CObjectId();
                hashobj.InitializeFromAlgorithmName(ObjectIdGroupId.XCN_CRYPT_HASH_ALG_OID_GROUP_ID,
                                                    ObjectIdPublicKeyFlags.XCN_CRYPT_OID_INFO_PUBKEY_ANY,
                                                    AlgorithmFlags.AlgorithmFlagsNone, "SHA256");


                // 2.5.29.37 – Enhanced Key Usage includes

                CObjectId oid = new CObjectId();
                oid.InitializeFromValue("1.3.6.1.5.5.8.2.2"); // IP security IKE intermediate
                var oidlist = new CObjectIds
                {
                    oid
                };

                CObjectId coid = new CObjectId();
                coid.InitializeFromValue("1.3.6.1.4.1.311.10.3.11"); // Key Recovery
                oidlist.Add(coid);

                CX509ExtensionEnhancedKeyUsage eku = new CX509ExtensionEnhancedKeyUsage();
                eku.InitializeEncode(oidlist);

                // Create the self signing request
                CX509CertificateRequestCertificate certreq = new CX509CertificateRequestCertificate();
                certreq.InitializeFromPrivateKey(X509CertificateEnrollmentContext.ContextMachine, privateKey, "");
                certreq.Subject = dn;
                certreq.Issuer = neos;
                certreq.NotBefore = DateTime.Now.AddDays(-10);

                certreq.NotAfter = DateTime.Now.AddYears(years);
                certreq.X509Extensions.Add((CX509Extension)eku); // add the EKU
                certreq.HashAlgorithm = hashobj; // Specify the hashing algorithm
                certreq.Encode(); // encode the certificate

                // Do the final enrollment process
                CX509Enrollment enroll = new CX509Enrollment();
                enroll.InitializeFromRequest(certreq); // load the certificate
                enroll.CertificateFriendlyName = subjectName; // Optional: add a friendly name

                string csr = enroll.CreateRequest(); // Output the request in base64

                // and install it back as the response
                enroll.InstallResponse(InstallResponseRestrictionFlags.AllowUntrustedCertificate, csr, EncodingType.XCN_CRYPT_STRING_BASE64, "");

                // output a base64 encoded PKCS#12 so we can import it back to the .Net security classes
                base64encoded = enroll.CreatePFX("", PFXExportOptions.PFXExportChainWithRoot);
            }
            catch (Exception ex)
            {
                if (privateKey != null)
                    privateKey.Delete();
                throw ex;
            }
            finally
            {
                // DO nothing, certificate Key is stored in the system
            }
            return base64encoded;
        }

        /// <summary>
        /// InternalCreateADFSCertificate method implementation
        /// </summary>
        [SuppressUnmanagedCodeSecurity]
        private static string InternalCreateADFSCertificate(string subjectName, bool issigning, int years)
        {
            string base64encoded = string.Empty;
            CX500DistinguishedName dn = new CX500DistinguishedName();
            CX500DistinguishedName neos = new CX500DistinguishedName();

            CX509PrivateKey privateKey = new CX509PrivateKey
            {
                ProviderName = "Microsoft Enhanced Cryptographic Provider v1.0",
                MachineContext = true,
                Length = 2048,
                KeySpec = X509KeySpec.XCN_AT_KEYEXCHANGE, // use is not limited
                ExportPolicy = X509PrivateKeyExportFlags.XCN_NCRYPT_ALLOW_EXPORT_FLAG,
                SecurityDescriptor = "D:(A;;FA;;;SY)(A;;FA;;;BA)"
            };
            if (issigning)
                privateKey.KeyUsage = X509PrivateKeyUsageFlags.XCN_NCRYPT_ALLOW_SIGNING_FLAG;
            else
                privateKey.KeyUsage = X509PrivateKeyUsageFlags.XCN_NCRYPT_ALLOW_DECRYPT_FLAG;
            if (!string.IsNullOrEmpty(ClientSIDsProxy.ADFSServiceSID))
                privateKey.SecurityDescriptor += "(A;;FA;;;" + ClientSIDsProxy.ADFSServiceSID + ")";
            if (!string.IsNullOrEmpty(ClientSIDsProxy.ADFSAccountSID))
                privateKey.SecurityDescriptor += "(A;;FA;;;" + ClientSIDsProxy.ADFSAccountSID + ")";
            if (!string.IsNullOrEmpty(ClientSIDsProxy.ADFSAdminGroupSID))
                privateKey.SecurityDescriptor += "(A;;FA;;;" + ClientSIDsProxy.ADFSAdminGroupSID + ")";
            try
            {
                privateKey.Create();

                CObjectId hashobj = new CObjectId();
                hashobj.InitializeFromAlgorithmName(ObjectIdGroupId.XCN_CRYPT_HASH_ALG_OID_GROUP_ID, ObjectIdPublicKeyFlags.XCN_CRYPT_OID_INFO_PUBKEY_ANY, AlgorithmFlags.AlgorithmFlagsNone, "SHA256");
                CObjectIds oidlist = new CObjectIds();
                if (!issigning)
                {
                    CObjectId oid = new CObjectId();
                    oid.InitializeFromValue("1.3.6.1.4.1.311.80.1"); // Encryption
                    oidlist.Add(oid);
                    dn.Encode("CN=ADFS Encrypt - " + subjectName, X500NameFlags.XCN_CERT_NAME_STR_NONE);
                    neos.Encode("CN=ADFS Encrypt - " + subjectName, X500NameFlags.XCN_CERT_NAME_STR_NONE);
                }
                else
                {
                    CObjectId coid = new CObjectId();
                    coid.InitializeFromValue("1.3.6.1.5.5.7.3.3"); // Signature
                    oidlist.Add(coid);
                    dn.Encode("CN=ADFS Sign - " + subjectName, X500NameFlags.XCN_CERT_NAME_STR_NONE);
                    neos.Encode("CN=ADFS Sign - " + subjectName, X500NameFlags.XCN_CERT_NAME_STR_NONE);
                }

                CX509ExtensionEnhancedKeyUsage eku = new CX509ExtensionEnhancedKeyUsage();
                eku.InitializeEncode(oidlist);

                // Create the self signing request
                CX509CertificateRequestCertificate certreq = new CX509CertificateRequestCertificate();
                certreq.InitializeFromPrivateKey(X509CertificateEnrollmentContext.ContextMachine, privateKey, "");
                certreq.Subject = dn;
                certreq.Issuer = neos;
                certreq.NotBefore = DateTime.Now.AddDays(-10);

                certreq.NotAfter = DateTime.Now.AddYears(years);
                certreq.X509Extensions.Add((CX509Extension)eku); // add the EKU
                certreq.HashAlgorithm = hashobj; // Specify the hashing algorithm
                certreq.Encode(); // encode the certificate

                // Do the final enrollment process
                CX509Enrollment enroll = new CX509Enrollment();
                enroll.InitializeFromRequest(certreq); // load the certificate
                enroll.CertificateFriendlyName = subjectName; // Optional: add a friendly name

                string csr = enroll.CreateRequest(); // Output the request in base64

                // and install it back as the response
                enroll.InstallResponse(InstallResponseRestrictionFlags.AllowUntrustedCertificate, csr, EncodingType.XCN_CRYPT_STRING_BASE64, "");

                // output a base64 encoded PKCS#12 so we can import it back to the .Net security classes
                base64encoded = enroll.CreatePFX("", PFXExportOptions.PFXExportEEOnly);
            }
            catch (Exception ex)
            {
                if (privateKey != null)
                    privateKey.Delete();
                throw ex;
            }
            finally
            {
                // DO nothing, certificate Key is stored in the system
            }
            return base64encoded;
        }
        #endregion

        #region Windows 2012R2
        /// <summary>
        /// InternalCreateRSACertificate2012R2 method implementation
        /// </summary>
        [SuppressUnmanagedCodeSecurity]
        private static string InternalCreateRSACertificate2012R2(string subjectName, int years)
        {
            string base64encoded = string.Empty;
            CX500DistinguishedName dn = new CX500DistinguishedName();
            CX500DistinguishedName neos = new CX500DistinguishedName();
            dn.Encode("CN=" + subjectName + " " + DateTime.UtcNow.ToString("G") + " GMT", X500NameFlags.XCN_CERT_NAME_STR_NONE);
            neos.Encode("CN=MFA RSA Keys Certificate", X500NameFlags.XCN_CERT_NAME_STR_NONE);

            IX509PrivateKey privateKey = (IX509PrivateKey)Activator.CreateInstance(Type.GetTypeFromProgID("X509Enrollment.CX509PrivateKey"));
            privateKey.ProviderName = "Microsoft RSA SChannel Cryptographic Provider";
            privateKey.MachineContext = true;
            privateKey.Length = 2048;
            privateKey.KeySpec = X509KeySpec.XCN_AT_KEYEXCHANGE; // use is not limited
            privateKey.ExportPolicy = X509PrivateKeyExportFlags.XCN_NCRYPT_ALLOW_EXPORT_FLAG;
            privateKey.SecurityDescriptor = "D:(A;;FA;;;SY)(A;;FA;;;BA)";

            if (!string.IsNullOrEmpty(ClientSIDsProxy.ADFSServiceSID))
                privateKey.SecurityDescriptor += "(A;;FA;;;" + ClientSIDsProxy.ADFSServiceSID + ")";
            if (!string.IsNullOrEmpty(ClientSIDsProxy.ADFSAccountSID))
                privateKey.SecurityDescriptor += "(A;;FA;;;" + ClientSIDsProxy.ADFSAccountSID + ")";
            if (!string.IsNullOrEmpty(ClientSIDsProxy.ADFSAdminGroupSID))
                privateKey.SecurityDescriptor += "(A;;FA;;;" + ClientSIDsProxy.ADFSAdminGroupSID + ")";
            try
            {
                privateKey.Create();
                CObjectId hashobj = new CObjectId();
                hashobj.InitializeFromAlgorithmName(ObjectIdGroupId.XCN_CRYPT_HASH_ALG_OID_GROUP_ID,
                                                    ObjectIdPublicKeyFlags.XCN_CRYPT_OID_INFO_PUBKEY_ANY,
                                                    AlgorithmFlags.AlgorithmFlagsNone, "SHA256");

                CObjectId oid = new CObjectId();
                // oid.InitializeFromValue("1.3.6.1.5.5.7.3.1"); // SSL server  
                oid.InitializeFromValue("1.3.6.1.4.1.311.80.1"); // Encryption

                CObjectIds oidlist = new CObjectIds
                {
                    oid
                };

                CObjectId coid = new CObjectId();
                // coid.InitializeFromValue("1.3.6.1.5.5.7.3.2"); // Client auth
                coid.InitializeFromValue("1.3.6.1.5.5.7.3.3"); // Signature
                oidlist.Add(coid);

                CX509ExtensionEnhancedKeyUsage eku = new CX509ExtensionEnhancedKeyUsage();
                eku.InitializeEncode(oidlist);

                // Create the self signing request
                CX509CertificateRequestCertificate certreq = new CX509CertificateRequestCertificate();
                certreq.InitializeFromPrivateKey(X509CertificateEnrollmentContext.ContextMachine, privateKey, "");
                certreq.Subject = dn;
                certreq.Issuer = neos;
                certreq.NotBefore = DateTime.Now.AddDays(-10);

                certreq.NotAfter = DateTime.Now.AddYears(years);
                certreq.X509Extensions.Add((CX509Extension)eku); // add the EKU
                certreq.HashAlgorithm = hashobj; // Specify the hashing algorithm
                certreq.Encode(); // encode the certificate

                // Do the final enrollment process
                CX509Enrollment enroll = new CX509Enrollment();
                enroll.InitializeFromRequest(certreq); // load the certificate
                enroll.CertificateFriendlyName = subjectName; // Optional: add a friendly name

                string csr = enroll.CreateRequest(); // Output the request in base64

                // and install it back as the response
                enroll.InstallResponse(InstallResponseRestrictionFlags.AllowUntrustedCertificate, csr, EncodingType.XCN_CRYPT_STRING_BASE64, "");

                // output a base64 encoded PKCS#12 so we can import it back to the .Net security classes
                base64encoded = enroll.CreatePFX("", PFXExportOptions.PFXExportChainWithRoot);
            }
            catch (Exception ex)
            {
                if (privateKey != null)
                    privateKey.Delete();
                throw ex;
            }
            finally
            {
                // DO nothing, certificate Key is stored in the system
            }
            return base64encoded;
        }

        /// <summary>
        /// InternalCreateUserRSACertificate2012R2 method implementation
        /// </summary>
        [SuppressUnmanagedCodeSecurity]
        private static string InternalCreateUserRSACertificate2012R2(string subjectName, int years, string pwd = "")
        {
            string base64encoded = string.Empty;
            CX500DistinguishedName dn = new CX500DistinguishedName();
            CX500DistinguishedName neos = new CX500DistinguishedName();
            dn.Encode("CN=" + subjectName, X500NameFlags.XCN_CERT_NAME_STR_NONE);
            neos.Encode("CN=MFA RSA Keys Certificate", X500NameFlags.XCN_CERT_NAME_STR_NONE);

            IX509PrivateKey privateKey = (IX509PrivateKey)Activator.CreateInstance(Type.GetTypeFromProgID("X509Enrollment.CX509PrivateKey"));
            privateKey.ProviderName = "Microsoft RSA SChannel Cryptographic Provider";
            privateKey.MachineContext = false;
            privateKey.Length = 2048;
            privateKey.KeySpec = X509KeySpec.XCN_AT_KEYEXCHANGE; // use is not limited
            privateKey.ExportPolicy = X509PrivateKeyExportFlags.XCN_NCRYPT_ALLOW_EXPORT_FLAG;
            privateKey.SecurityDescriptor = "D:(A;;FA;;;SY)(A;;FA;;;BA)";

            if (!string.IsNullOrEmpty(ClientSIDsProxy.ADFSServiceSID))
                privateKey.SecurityDescriptor += "(A;;FA;;;" + ClientSIDsProxy.ADFSServiceSID + ")";
            if (!string.IsNullOrEmpty(ClientSIDsProxy.ADFSAccountSID))
                privateKey.SecurityDescriptor += "(A;;FA;;;" + ClientSIDsProxy.ADFSAccountSID + ")";
            if (!string.IsNullOrEmpty(ClientSIDsProxy.ADFSAdminGroupSID))
                privateKey.SecurityDescriptor += "(A;;FA;;;" + ClientSIDsProxy.ADFSAdminGroupSID + ")";
            try
            {
                privateKey.Create();
                CObjectId hashobj = new CObjectId();
                hashobj.InitializeFromAlgorithmName(ObjectIdGroupId.XCN_CRYPT_HASH_ALG_OID_GROUP_ID,
                                                    ObjectIdPublicKeyFlags.XCN_CRYPT_OID_INFO_PUBKEY_ANY,
                                                    AlgorithmFlags.AlgorithmFlagsNone, "SHA256");

                CObjectId oid = new CObjectId();
                // oid.InitializeFromValue("1.3.6.1.5.5.7.3.1"); // SSL server  
                oid.InitializeFromValue("1.3.6.1.4.1.311.80.1"); // Encryption

                CObjectIds oidlist = new CObjectIds
                {
                    oid
                };

                CObjectId coid = new CObjectId();
                // coid.InitializeFromValue("1.3.6.1.5.5.7.3.2"); // Client auth
                coid.InitializeFromValue("1.3.6.1.5.5.7.3.3"); // Signature
                oidlist.Add(coid);

                CX509ExtensionEnhancedKeyUsage eku = new CX509ExtensionEnhancedKeyUsage();
                eku.InitializeEncode(oidlist);

                // Create the self signing request
                CX509CertificateRequestCertificate certreq = new CX509CertificateRequestCertificate();
                certreq.InitializeFromPrivateKey(X509CertificateEnrollmentContext.ContextUser, privateKey, "");
                certreq.Subject = dn;
                certreq.Issuer = neos;
                certreq.NotBefore = DateTime.Now.AddDays(-10);

                certreq.NotAfter = DateTime.Now.AddYears(years);
                certreq.X509Extensions.Add((CX509Extension)eku); // add the EKU
                certreq.HashAlgorithm = hashobj; // Specify the hashing algorithm
                certreq.Encode(); // encode the certificate

                // Do the final enrollment process
                CX509Enrollment enroll = new CX509Enrollment();
                enroll.InitializeFromRequest(certreq); // load the certificate
                enroll.CertificateFriendlyName = subjectName; // Optional: add a friendly name

                string csr = enroll.CreateRequest(); // Output the request in base64

                // and install it back as the response
                enroll.InstallResponse(InstallResponseRestrictionFlags.AllowUntrustedCertificate, csr, EncodingType.XCN_CRYPT_STRING_BASE64, "");

                // output a base64 encoded PKCS#12 so we can import it back to the .Net security classes
                base64encoded = enroll.CreatePFX(pwd, PFXExportOptions.PFXExportChainWithRoot);
            }
            catch (Exception ex)
            {
                if (privateKey != null)
                    privateKey.Delete();
                throw ex;
            }
            finally
            {
                if (privateKey != null)
                    privateKey.Delete(); // Remove Stored elsewhere
            }
            return base64encoded;
        }

        /// <summary>
        /// InternalCreateSQLCertificate method implementation
        /// </summary>
        [SuppressUnmanagedCodeSecurity]
        private static string InternalCreateSQLCertificate2012R2(string subjectName, int years, string pwd = "")
        {
            string base64encoded = string.Empty;
            CX500DistinguishedName dn = new CX500DistinguishedName();
            CX500DistinguishedName neos = new CX500DistinguishedName();
            dn.Encode("CN=" + subjectName + " " + DateTime.UtcNow.ToString("G") + " GMT", X500NameFlags.XCN_CERT_NAME_STR_NONE);
            neos.Encode("CN=Always Encrypted Certificate", X500NameFlags.XCN_CERT_NAME_STR_NONE);

            IX509PrivateKey privateKey = (IX509PrivateKey)Activator.CreateInstance(Type.GetTypeFromProgID("X509Enrollment.CX509PrivateKey"));
            privateKey.ProviderName = "Microsoft RSA SChannel Cryptographic Provider";
            privateKey.MachineContext = true;
            privateKey.Length = 2048;
            privateKey.KeySpec = X509KeySpec.XCN_AT_KEYEXCHANGE; // use is not limited
            privateKey.ExportPolicy = X509PrivateKeyExportFlags.XCN_NCRYPT_ALLOW_EXPORT_FLAG;
            privateKey.SecurityDescriptor = "D:(A;;FA;;;SY)(A;;FA;;;BA)";

            if (!string.IsNullOrEmpty(ClientSIDsProxy.ADFSServiceSID))
                privateKey.SecurityDescriptor += "(A;;FA;;;" + ClientSIDsProxy.ADFSServiceSID + ")";
            if (!string.IsNullOrEmpty(ClientSIDsProxy.ADFSAccountSID))
                privateKey.SecurityDescriptor += "(A;;FA;;;" + ClientSIDsProxy.ADFSAccountSID + ")";
            if (!string.IsNullOrEmpty(ClientSIDsProxy.ADFSAdminGroupSID))
                privateKey.SecurityDescriptor += "(A;;FA;;;" + ClientSIDsProxy.ADFSAdminGroupSID + ")";
            try
            {
                privateKey.Create();
                CObjectId hashobj = new CObjectId();
                hashobj.InitializeFromAlgorithmName(ObjectIdGroupId.XCN_CRYPT_HASH_ALG_OID_GROUP_ID,
                                                    ObjectIdPublicKeyFlags.XCN_CRYPT_OID_INFO_PUBKEY_ANY,
                                                    AlgorithmFlags.AlgorithmFlagsNone, "SHA256");


                // 2.5.29.37 – Enhanced Key Usage includes

                CObjectId oid = new CObjectId();
                oid.InitializeFromValue("1.3.6.1.5.5.8.2.2"); // IP security IKE intermediate
                var oidlist = new CObjectIds
                {
                    oid
                };

                CObjectId coid = new CObjectId();
                coid.InitializeFromValue("1.3.6.1.4.1.311.10.3.11"); // Key Recovery
                oidlist.Add(coid);

                CX509ExtensionEnhancedKeyUsage eku = new CX509ExtensionEnhancedKeyUsage();
                eku.InitializeEncode(oidlist);

                // Create the self signing request
                CX509CertificateRequestCertificate certreq = new CX509CertificateRequestCertificate();
                certreq.InitializeFromPrivateKey(X509CertificateEnrollmentContext.ContextMachine, privateKey, "");
                certreq.Subject = dn;
                certreq.Issuer = neos;
                certreq.NotBefore = DateTime.Now.AddDays(-10);

                certreq.NotAfter = DateTime.Now.AddYears(years);
                certreq.X509Extensions.Add((CX509Extension)eku); // add the EKU
                certreq.HashAlgorithm = hashobj; // Specify the hashing algorithm
                certreq.Encode(); // encode the certificate

                // Do the final enrollment process
                CX509Enrollment enroll = new CX509Enrollment();
                enroll.InitializeFromRequest(certreq); // load the certificate
                enroll.CertificateFriendlyName = subjectName; // Optional: add a friendly name

                string csr = enroll.CreateRequest(); // Output the request in base64

                // and install it back as the response
                enroll.InstallResponse(InstallResponseRestrictionFlags.AllowUntrustedCertificate, csr, EncodingType.XCN_CRYPT_STRING_BASE64, "");

                // output a base64 encoded PKCS#12 so we can import it back to the .Net security classes
                base64encoded = enroll.CreatePFX("", PFXExportOptions.PFXExportChainWithRoot);
            }
            catch (Exception ex)
            {
                if (privateKey != null)
                    privateKey.Delete();
                throw ex;
            }
            finally
            {
                // DO nothing, certificate Key is stored in the system
            }
            return base64encoded;
        }

        /// <summary>
        /// InternalCreateADFSCertificate method implementation
        /// </summary>
        [SuppressUnmanagedCodeSecurity]
        private static string InternalCreateADFSCertificate2012R2(string subjectName, bool issigning, int years)
        {
            string base64encoded = string.Empty;
            CX500DistinguishedName dn = new CX500DistinguishedName();
            CX500DistinguishedName neos = new CX500DistinguishedName();

            IX509PrivateKey privateKey = (IX509PrivateKey)Activator.CreateInstance(Type.GetTypeFromProgID("X509Enrollment.CX509PrivateKey"));
            privateKey.ProviderName = "Microsoft Enhanced Cryptographic Provider v1.0";
            privateKey.MachineContext = true;
            privateKey.Length = 2048;
            privateKey.KeySpec = X509KeySpec.XCN_AT_KEYEXCHANGE; // use is not limited
            privateKey.ExportPolicy = X509PrivateKeyExportFlags.XCN_NCRYPT_ALLOW_EXPORT_FLAG;
            privateKey.SecurityDescriptor = "D:(A;;FA;;;SY)(A;;FA;;;BA)";

            if (issigning)
                privateKey.KeyUsage = X509PrivateKeyUsageFlags.XCN_NCRYPT_ALLOW_SIGNING_FLAG;
            else
                privateKey.KeyUsage = X509PrivateKeyUsageFlags.XCN_NCRYPT_ALLOW_DECRYPT_FLAG;
            if (!string.IsNullOrEmpty(ClientSIDsProxy.ADFSServiceSID))
                privateKey.SecurityDescriptor += "(A;;FA;;;" + ClientSIDsProxy.ADFSServiceSID + ")";
            if (!string.IsNullOrEmpty(ClientSIDsProxy.ADFSAccountSID))
                privateKey.SecurityDescriptor += "(A;;FA;;;" + ClientSIDsProxy.ADFSAccountSID + ")";
            if (!string.IsNullOrEmpty(ClientSIDsProxy.ADFSAdminGroupSID))
                privateKey.SecurityDescriptor += "(A;;FA;;;" + ClientSIDsProxy.ADFSAdminGroupSID + ")";
            try
            {
                privateKey.Create();

                CObjectId hashobj = new CObjectId();
                hashobj.InitializeFromAlgorithmName(ObjectIdGroupId.XCN_CRYPT_HASH_ALG_OID_GROUP_ID, ObjectIdPublicKeyFlags.XCN_CRYPT_OID_INFO_PUBKEY_ANY, AlgorithmFlags.AlgorithmFlagsNone, "SHA256");
                CObjectIds oidlist = new CObjectIds();
                if (!issigning)
                {
                    CObjectId oid = new CObjectId();
                    oid.InitializeFromValue("1.3.6.1.4.1.311.80.1"); // Encryption
                    oidlist.Add(oid);
                    dn.Encode("CN=ADFS Encrypt - " + subjectName, X500NameFlags.XCN_CERT_NAME_STR_NONE);
                    neos.Encode("CN=ADFS Encrypt - " + subjectName, X500NameFlags.XCN_CERT_NAME_STR_NONE);
                }
                else
                {
                    CObjectId coid = new CObjectId();
                    coid.InitializeFromValue("1.3.6.1.5.5.7.3.3"); // Signature
                    oidlist.Add(coid);
                    dn.Encode("CN=ADFS Sign - " + subjectName, X500NameFlags.XCN_CERT_NAME_STR_NONE);
                    neos.Encode("CN=ADFS Sign - " + subjectName, X500NameFlags.XCN_CERT_NAME_STR_NONE);
                }

                CX509ExtensionEnhancedKeyUsage eku = new CX509ExtensionEnhancedKeyUsage();
                eku.InitializeEncode(oidlist);

                // Create the self signing request
                CX509CertificateRequestCertificate certreq = new CX509CertificateRequestCertificate();
                certreq.InitializeFromPrivateKey(X509CertificateEnrollmentContext.ContextMachine, privateKey, "");
                certreq.Subject = dn;
                certreq.Issuer = neos;
                certreq.NotBefore = DateTime.Now.AddDays(-10);

                certreq.NotAfter = DateTime.Now.AddYears(years);
                certreq.X509Extensions.Add((CX509Extension)eku); // add the EKU
                certreq.HashAlgorithm = hashobj; // Specify the hashing algorithm
                certreq.Encode(); // encode the certificate

                // Do the final enrollment process
                CX509Enrollment enroll = new CX509Enrollment();
                enroll.InitializeFromRequest(certreq); // load the certificate
                enroll.CertificateFriendlyName = subjectName; // Optional: add a friendly name

                string csr = enroll.CreateRequest(); // Output the request in base64

                // and install it back as the response
                enroll.InstallResponse(InstallResponseRestrictionFlags.AllowUntrustedCertificate, csr, EncodingType.XCN_CRYPT_STRING_BASE64, "");

                // output a base64 encoded PKCS#12 so we can import it back to the .Net security classes
                base64encoded = enroll.CreatePFX("", PFXExportOptions.PFXExportEEOnly);
            }
            catch (Exception ex)
            {
                if (privateKey != null)
                    privateKey.Delete();
                throw ex;
            }
            finally
            {
                // DO nothing, certificate Key is stored in the system
            }
            return base64encoded;
        }
        #endregion

        #region Orphaned Keys
        /// <summary>
        /// HasAssociatedCertificate method implementation
        /// </summary>
        private static bool HasAssociatedCertificate(string filename)
        {
            X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            store.Open(OpenFlags.MaxAllowed);
            try
            {
                X509Certificate2Collection collection2 = (X509Certificate2Collection)store.Certificates;
                foreach (X509Certificate2 x509 in collection2)
                {
                    try
                    {
                        string cntName = string.Empty;
                        var rsakey = x509.GetRSAPrivateKey();
                        if (rsakey is RSACng)
                            cntName = ((RSACng)rsakey).Key.UniqueName;
                        else if (rsakey is RSACryptoServiceProvider)
                            cntName = ((RSACryptoServiceProvider)rsakey).CspKeyContainerInfo.UniqueKeyContainerName;
                        if (filename.ToLower().Equals(cntName.ToLower()))
                            return true;
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                store.Close();
            }
        }

        /// <summary>
        /// CleanOrphanedPrivateKeys method implementation
        /// </summary>
        public static int CleanOrphanedPrivateKeys()
        {
            int result = 0;
            char sep = Path.DirectorySeparatorChar;
            List<string> paths = new List<string>()
            {
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + sep + "Microsoft" + sep + "Crypto" + sep + "RSA" + sep + "MachineKeys"+ sep,
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + sep + "Microsoft" + sep + "Crypto" + sep + "Keys" + sep,
            };
            foreach (string pth in paths)
            {
                DirectoryInfo dir = new DirectoryInfo(pth);

                foreach (FileInfo fi in dir.GetFiles())
                {
                    try
                    {
                        if (fi.Name.ToLower().StartsWith("f686aace6942fb7f7ceb231212eef4a4_"))  // RDP Key do not drop anyway
                            continue;
                        if (!HasAssociatedCertificate(fi.Name))
                        {
                            fi.Delete();
                            result++;
                        }
                    }
                    catch { }
                }
            }
            return result;
        }

        /// <summary>
        /// CleanOrphanedPrivateKeysRegistry method implementation
        /// </summary>
        public static void CleanOrphanedPrivateKeysRegistry(byte option, int delay)
        {
            if (option == 0x00)  // None
                return;
            if (option == 0x01)  // Enable
            {
                RegistryKey ek = Registry.LocalMachine.OpenSubKey("Software\\MFA", true);
                if (ek == null)
                {
                    ek = Registry.LocalMachine.CreateSubKey("Software\\MFA", true);
                }
                ek.SetValue("PrivateKeysCleanUpEnabled", 1, RegistryValueKind.DWord);
                ek.SetValue("PrivateKeysCleanUpDelay", delay, RegistryValueKind.DWord);

            }
            if (option == 0x02)  // Disable
            {
                RegistryKey dk = Registry.LocalMachine.OpenSubKey("Software\\MFA", true);
                if (dk == null)
                {
                    dk = Registry.LocalMachine.CreateSubKey("Software\\MFA", true);
                }
                dk.SetValue("PrivateKeysCleanUpEnabled", 0, RegistryValueKind.DWord);
            }
        }
        #endregion   
    }
    #endregion

    #region RegistryVersion
    /// <summary>
    /// RegistryVersion class
    /// </summary>
    [DataContract]
    public class RegistryVersion
    {
        /// <summary>
        /// RegistryVersion constructor
        /// </summary>
        public RegistryVersion(bool loadlocal = true)
        {
            if (loadlocal)
            {
                RegistryKey rk = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Windows NT\\CurrentVersion");

                CurrentVersion = Convert.ToString(rk.GetValue("CurrentVersion"));
                ProductName = Convert.ToString(rk.GetValue("ProductName"));
                InstallationType = Convert.ToString(rk.GetValue("InstallationType"));
                CurrentBuild = Convert.ToInt32(rk.GetValue("CurrentBuild"));
                CurrentMajorVersionNumber = Convert.ToInt32(rk.GetValue("CurrentMajorVersionNumber"));
                CurrentMinorVersionNumber = Convert.ToInt32(rk.GetValue("CurrentMinorVersionNumber"));
            }
        }

        /// <summary>
        /// CurrentVersion property implementation
        /// </summary>
        [DataMember]
        public string CurrentVersion { get; set; }

        /// <summary>
        /// ProductName property implementation
        /// </summary>
        [DataMember]
        public string ProductName { get; set; }

        /// <summary>
        /// InstallationType property implementation
        /// </summary>
        [DataMember]
        public string InstallationType { get; set; }

        /// <summary>
        /// CurrentBuild property implementation
        /// </summary>
        [DataMember]
        public int CurrentBuild { get; set; }

        /// <summary>
        /// CurrentMajorVersionNumber property implementation
        /// </summary>
        [DataMember]
        public int CurrentMajorVersionNumber { get; set; }

        /// <summary>
        /// CurrentMinorVersionNumber property implementation
        /// </summary>
        [DataMember]
        public int CurrentMinorVersionNumber { get; set; }

        /// <summary>
        /// IsWindows2019 property implementation
        /// </summary>
        [IgnoreDataMember]
        public bool IsWindows2019
        {
            get { return ((this.CurrentMajorVersionNumber == 10) && (this.CurrentBuild >= 17763)); }
        }

        /// <summary>
        /// IsWindows2016 property implementation
        /// </summary>
        [IgnoreDataMember]
        public bool IsWindows2016
        {
            get { return ((this.CurrentMajorVersionNumber == 10) && ((this.CurrentBuild >= 14393) && (this.CurrentBuild < 17763))); }
        }

        /// <summary>
        /// IsWindows2012R2 property implementation
        /// </summary>
        [IgnoreDataMember]
        public bool IsWindows2012R2
        {
            get { return ((this.CurrentMajorVersionNumber == 0) && ((this.CurrentBuild >= 9600) && (this.CurrentBuild < 14393))); }
        }
    }

    [DataContract]
    public class ADFSNodeInformation
    {
        /// <summary>
        /// BehaviorLevel property implementation
        /// </summary>
        [DataMember]
        public int BehaviorLevel { get; set; }

        /// <summary>
        /// HeartbeatTmeStamp property implementation
        /// </summary>
        [DataMember]
        public DateTime HeartbeatTmeStamp { get; set; }

        /// <summary>
        /// NodeType property implementation
        /// </summary>
        [DataMember]
        public string NodeType { get; set; }
    }
    #endregion
}
