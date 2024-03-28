using CERTENROLLLib;
using Microsoft.Win32;
using Neos.IdentityServer.MultiFactor.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Threading;
using System.Xml.Serialization;

namespace Neos.IdentityServer.MultiFactor
{
    #region Configuration Reader
    /// <summary>
    /// CFGReaderUtilities class
    /// </summary>
    internal static class CFGReaderUtilities
    {
        private static readonly char sep = Path.DirectorySeparatorChar;
        private static readonly string ConfigReaderCacheFile = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + sep + "MFA" + sep + "Config" + sep + "config.db";

        /// <summary>
        /// ReadConfiguration method implementation
        /// </summary>
        internal static MFAConfig ReadConfiguration()
        {
            MFAConfig config;
            try
            {
                try
                {
                    config = ReadConfigurationFromCache();
                    if (config == null)
                        config = ReadConfigurationFromDatabase();
                }
                catch
                {
                    config = ReadConfigurationFromDatabase();
                }
            }
            catch (Exception ex)
            {
                Log.WriteEntry(string.Format("Error retrieving configuration : {0} ", ex.Message), EventLogEntryType.Error, 2013);
                throw ex;
            }
            if (config==null)
            {
                Log.WriteEntry("Error retrieving configuration : config is NULL", EventLogEntryType.Error, 2013);
                throw new Exception("Error retrieving configuration : config is NULL");
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
                    SPRunSpace?.Close();
                    SPPowerShell?.Dispose();
                }

                using (FileStream stm = new FileStream(pth, FileMode.Open, FileAccess.Read))
                {
                    XmlConfigSerializer xmlserializer = new XmlConfigSerializer(typeof(MFAConfig));
                    using (StreamReader reader = new StreamReader(stm))
                    {
                        config = (MFAConfig)xmlserializer.Deserialize(stm);
                    }
                }
                using (SystemEncryption MSIS = new SystemEncryption())
                {
                    config.KeysConfig.XORSecret = MSIS.Decrypt(config.KeysConfig.XORSecret, "Pass Phrase Encryption");
                    config.Hosts.ActiveDirectoryHost.Password = MSIS.Decrypt(config.Hosts.ActiveDirectoryHost.Password, "ADDS Super Account Password");
                    config.Hosts.SQLServerHost.SQLPassword = MSIS.Decrypt(config.Hosts.SQLServerHost.SQLPassword, "SQL Super Account Password");
                    config.MailProvider.Password = MSIS.Decrypt(config.MailProvider.Password, "Mail Provider Account Password");
                    config.DefaultPin = MSIS.Decrypt(config.DefaultPin, "Default Users Pin");
                    config.AdministrationPin = MSIS.Decrypt(config.AdministrationPin, "Administration Pin");
                };
            }
            catch (Exception ex)
            {
                Log.WriteEntry(string.Format("Error retrieving configuration from ADFS Database : {0} ", ex.Message), EventLogEntryType.Error, 2013);
                throw ex;
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
            try
            {
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
                        }
                    }
                    using (SystemEncryption MSIS = new SystemEncryption())
                    {
                        config.KeysConfig.XORSecret = MSIS.Decrypt(config.KeysConfig.XORSecret, "Pass Phrase Encryption");
                        config.Hosts.ActiveDirectoryHost.Password = MSIS.Decrypt(config.Hosts.ActiveDirectoryHost.Password, "ADDS Super Account Password");
                        config.Hosts.SQLServerHost.SQLPassword = MSIS.Decrypt(config.Hosts.SQLServerHost.SQLPassword, "SQL Super Account Password");
                        config.MailProvider.Password = MSIS.Decrypt(config.MailProvider.Password, "Mail Provider Account Password");
                        config.DefaultPin = MSIS.Decrypt(config.DefaultPin, "Default Users Pin");
                        config.AdministrationPin = MSIS.Decrypt(config.AdministrationPin, "Administration Pin");
                    };
                }
            }
            catch (Exception ex)
            {
                Log.WriteEntry(string.Format("Error retrieving configuration from cache config file : {0} ", ex.Message), EventLogEntryType.Error, 2013);
                throw ex;
            }
            return config;
        }

        #region Privates methods
        /// <summary>
        /// GetCryptedConfig method implementation
        /// </summary>
        internal static byte[] GetCryptedConfig(MFAConfig config)
        {
            using (SystemEncryption MSIS = new SystemEncryption())
            {
                config.KeysConfig.XORSecret = MSIS.Encrypt(config.KeysConfig.XORSecret, "Pass Phrase Encryption");
                config.Hosts.ActiveDirectoryHost.Password = MSIS.Encrypt(config.Hosts.ActiveDirectoryHost.Password, "ADDS Super Account Password");
                config.Hosts.SQLServerHost.SQLPassword = MSIS.Encrypt(config.Hosts.SQLServerHost.SQLPassword, "SQL Super Account Password");
                config.MailProvider.Password = MSIS.Encrypt(config.MailProvider.Password, "Mail Provider Account Password");
                config.DefaultPin = MSIS.Encrypt(config.DefaultPin, "Default Users Pin");
                config.AdministrationPin = MSIS.Encrypt(config.AdministrationPin, "Administration Pin");
            };
            XmlConfigSerializer xmlserializer = new XmlConfigSerializer(typeof(MFAConfig));
            MemoryStream stm = new MemoryStream();
            using (StreamReader reader = new StreamReader(stm))
            {
                xmlserializer.Serialize(stm, config);
                stm.Position = 0;
                byte[] bytes = null;
                using (AESSystemEncryption aes = new AESSystemEncryption())
                {
                    bytes = aes.Encrypt(stm.ToArray());
                }
                return bytes;
            }
        }

        /// <summary>
        /// GetDeCryptedConfig method implementation
        /// </summary>
        internal static MFAConfig GetDeCryptedConfig(byte[] config)
        {
            MFAConfig result = null;
            XmlConfigSerializer xmlserializer = new XmlConfigSerializer(typeof(MFAConfig));
            byte[] byt = null;
            using (AESSystemEncryption aes = new AESSystemEncryption())
            {
                byt = aes.Decrypt(config);
            }

            using (MemoryStream ms = new MemoryStream(byt))
            {
                using (StreamReader reader = new StreamReader(ms))
                {
                    result = (MFAConfig)xmlserializer.Deserialize(ms);
                }
            }
            using (SystemEncryption MSIS = new SystemEncryption())
            {
                result.KeysConfig.XORSecret = MSIS.Decrypt(result.KeysConfig.XORSecret, "Pass Phrase Encryption");
                result.Hosts.ActiveDirectoryHost.Password = MSIS.Decrypt(result.Hosts.ActiveDirectoryHost.Password, "ADDS Super Account Password");
                result.Hosts.SQLServerHost.SQLPassword = MSIS.Decrypt(result.Hosts.SQLServerHost.SQLPassword, "SQL Super Account Password");
                result.MailProvider.Password = MSIS.Decrypt(result.MailProvider.Password, "Mail Provider Account Password");
                result.DefaultPin = MSIS.Decrypt(result.DefaultPin, "Default Users Pin");
                result.AdministrationPin = MSIS.Decrypt(result.AdministrationPin, "Administration Pin");
            };
            return result;
        }
        #endregion
    }
    #endregion

    #region System Utilities
    /// <summary>
    /// SystemUtilities class
    /// </summary>
    internal static class SystemUtilities
    {
        private static readonly char sep = Path.DirectorySeparatorChar;
        internal static string SystemRootDir = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + sep + "MFA" + sep + "Config";
        internal static string SystemCacheFile = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + sep + "MFA" + sep + "Config" + sep + "system.db";
        internal static string PayloadCacheFile = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + sep + "MFA" + sep + "Config" + sep + "blob.db";
        internal static string ThreatCacheFile = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + sep + "MFA" + sep + "Config" + sep + "threatconfig.db";
        internal static string PayloadDownloadBlobUrl = "https://mds.fidoalliance.org/";
        internal static string ThreatDownloadBlobUrl = "https://www.spamhaus.org/drop/drop.txt";
#pragma warning disable IDE0044 // Ajouter un modificateur readonly
        private static object obj = new object();
        private static byte[] _systemkey = null;
#pragma warning restore IDE0044 // Ajouter un modificateur readonly

        /// <summary>
        /// CFGUtilities static constructor
        /// </summary>
        static SystemUtilities()
        {
            if (SystemKey == null)
                _systemkey = ReadConfigurationKey();
        }

        /// <summary>
        /// Property SystemKey implementation
        /// </summary>
        internal static byte[] SystemKey
        {
            get { return _systemkey; }
        }

        /// <summary>
        /// Property SystemKeyName implementation
        /// </summary>
        internal static string SystemKeyName
        {
            get
            {
                string name = string.Empty;
                lock (obj)
                {
                    Guid gd = GetKeyFromRegistry();
                    if (gd == Guid.Empty)
                    {
                        gd = GetKeyFromADDS();
                        SetKeyToRegistry(gd);
                    }
                    name = gd.ToString("N").ToUpper();
                }
                return name;
            }
        }

        /// <summary>
        /// Property BobKeyName implementation
        /// </summary>
        internal static string BobKeyName
        {
            get { return "BOB" + SystemKeyName; }
        }

        /// Property AlicKeyName implementation
        /// </summary>
        internal static string AlicKeyName
        {
            get { return "ALICE" + SystemKeyName; }
        }

        #region Admin Key Reader
        /// <summary>
        /// ReadConfigurationKey method implmentation
        /// </summary>
        private static byte[] ReadConfigurationKey()
        {
            lock (obj)
            {
                Guid gd = GetKeyFromRegistry();
                if (gd == Guid.Empty)
                {
                    gd = GetKeyFromADDS();
                    SetKeyToRegistry(gd);
                }
                byte[] _key = new byte[32];
                byte[] _gd = new byte[16];
                Buffer.BlockCopy(gd.ToByteArray(), 0, _gd, 0, 16);
                Buffer.BlockCopy(_gd, 0, _key, 0, 16);
                Array.Reverse(_gd);
                Buffer.BlockCopy(_gd, 0, _key, 16, 16);
                return _key;
            }
        }

        /// <summary>
        /// GetKeyFromRegistry method implementation
        /// </summary>
        private static Guid GetKeyFromRegistry()
        {
            RegistryKey rk = Registry.LocalMachine.OpenSubKey("Software\\MFA", false);
            object v = rk.GetValue("MFAID");
            if (v != null)
                return new Guid(v.ToString());
            else
                return Guid.Empty;
        }

        /// <summary>
        /// SetKeyToRegistry method implementation
        /// </summary>
        private static void SetKeyToRegistry(Guid gd)
        {
            RegistryKey rk = Registry.LocalMachine.OpenSubKey("Software\\MFA", true);
            rk.SetValue("MFAID", gd.ToString(), RegistryValueKind.String);
        }

        /// <summary>
        /// GetKeyFromADDS method implementation
        /// </summary>
        private static Guid GetKeyFromADDS()
        {
            try
            {
                Domain dom = Domain.GetComputerDomain();
                return dom.GetDirectoryEntry().Guid;
            }
            catch
            {
                return Guid.Empty;
            }
        }

        /// <summary>
        /// GetKeyFromADDS method implementation
        /// </summary>
        public static SecurityIdentifier GetADDSDomainSID()
        {
            // ICI mettre dans Service MFA et Appel svc
            try
            {
                Domain dom = Domain.GetComputerDomain();
                DirectoryEntry de = dom.GetDirectoryEntry();
                byte[] bdomSid = (byte[])de.Properties["objectSid"].Value;
                return new SecurityIdentifier(bdomSid, 0);
            }
            catch
            {
                return null;
            }
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
        private static readonly RegistryVersion _RegistryVersion;

        static Certs()
        {
            _RegistryVersion = new RegistryVersion();
        }

        #region Certicates read
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
                return (findCollection.Count > 0);
            }
            finally
            {
                store.Close();
            }
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
        #endregion

        #region Certificates cleaning
        /// <summary>
        /// DropSelfSignedCertificate method implmentation
        /// </summary>
        internal static bool DropSelfSignedCertificate(X509Certificate2 cert, StoreLocation location = StoreLocation.LocalMachine)
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
        #endregion

        #region Certificates Creation
        /// <summary>
        /// CreateSelfSignedCertificate method implementation
        /// </summary>
        public static X509Certificate2 CreateSelfSignedCertificate(string subjectName, string dnsName, CertificatesKind kind, int years, string pwd = "")
        {
            string strcert;
            if (_RegistryVersion.IsWindows2012R2)
                strcert = InternalCreateSelfSignedCertificate2012R2(subjectName, dnsName, kind, years, pwd);
            else
                strcert = InternalCreateSelfSignedCertificate(subjectName, dnsName, kind, years, pwd);
            X509Certificate2 x509 = new X509Certificate2(Convert.FromBase64String(strcert), pwd, X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet);
            if (DropSelfSignedCertificate(x509, StoreLocation.LocalMachine))
                return x509;
            else
                return null;
        }

        /// <summary>
        /// CreateRSACertificate method implementation
        /// </summary>
        internal static X509Certificate2 CreateRSACertificate(string subjectName, int years, out string cert)
        {
            string strcert;
            if (_RegistryVersion.IsWindows2012R2)
                strcert = InternalCreateRSACertificate2012R2(subjectName, years);
            else
                strcert = InternalCreateRSACertificate(subjectName, years);
            cert = strcert;
            X509Certificate2 x509 = new X509Certificate2(Convert.FromBase64String(strcert), "", X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet);
            if (CleanSelfSignedCertificate(x509, StoreLocation.LocalMachine))
                return x509;
            else
                return null;
        }

        /// <summary>
        /// CreateADFSCertificate method implementation
        /// </summary>
        internal static X509Certificate2 CreateADFSCertificate(string subjectName, ADFSCertificatesKind kind, int years, out string cert)
        {
            string strcert;
            if (_RegistryVersion.IsWindows2012R2)
                strcert = InternalCreateADFSCertificate2012R2(subjectName, kind, years);
            else
                strcert = InternalCreateADFSCertificate(subjectName, kind, years);
            cert = strcert;
            X509Certificate2 x509 = new X509Certificate2(Convert.FromBase64String(strcert), "", X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet);
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
            string strcert;
            if (_RegistryVersion.IsWindows2012R2)
                strcert = InternalCreateSQLCertificate2012R2(subjectName, years);
            else
                strcert = InternalCreateSQLCertificate(subjectName, years);
            cert = strcert;
            X509Certificate2 x509 = new X509Certificate2(Convert.FromBase64String(strcert), "", X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet);
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
            string strcert;
            if (_RegistryVersion.IsWindows2012R2)
                strcert = InternalCreateUserRSACertificate2012R2(subjectName, years, pwd);
            else
                strcert = InternalCreateUserRSACertificate(subjectName, years, pwd);
            X509Certificate2 x509 = new X509Certificate2(Convert.FromBase64String(strcert), pwd, X509KeyStorageFlags.Exportable | X509KeyStorageFlags.EphemeralKeySet);
            if (Certs.DropSelfSignedCertificate(x509, StoreLocation.CurrentUser))
                return x509;
            else
                return null;
        }
        #endregion

        #region Windows 2016 & 2019 && 2022
        /// <summary>
        /// InternalCreateRSACertificate method implementation
        /// </summary>
        [SuppressUnmanagedCodeSecurity]
        private static string InternalCreateSelfSignedCertificate(string subjectName, string dnsName, CertificatesKind kind, int years, string pwd)
        {
            string base64encoded = string.Empty;
            CX500DistinguishedName subject = new CX500DistinguishedName();
            CX500DistinguishedName issuer = new CX500DistinguishedName();
            subject.Encode("CN=" + subjectName, X500NameFlags.XCN_CERT_NAME_STR_NONE);
            issuer.Encode("CN=" + subjectName, X500NameFlags.XCN_CERT_NAME_STR_NONE);

            CX509PrivateKey privateKey = new CX509PrivateKey
            {
                ProviderName = "Microsoft Enhanced Cryptographic Provider v1.0",
                MachineContext = true,
                Length = 2048,
                KeySpec = X509KeySpec.XCN_AT_KEYEXCHANGE, // use is not limited
                ExportPolicy = X509PrivateKeyExportFlags.XCN_NCRYPT_ALLOW_EXPORT_FLAG,
                SecurityDescriptor = "D:(A;;FA;;;SY)(A;;FA;;;BA)",
                KeyUsage = X509PrivateKeyUsageFlags.XCN_NCRYPT_ALLOW_USAGES_NONE
            };
            if (kind.HasFlag(CertificatesKind.Signing) || kind.HasFlag(CertificatesKind.SSL) || kind.HasFlag(CertificatesKind.Client) || kind.HasFlag(CertificatesKind.All))
                privateKey.KeyUsage |= X509PrivateKeyUsageFlags.XCN_NCRYPT_ALLOW_SIGNING_FLAG;
            if (kind.HasFlag(CertificatesKind.Decrypting) || kind.HasFlag(CertificatesKind.SSL) || kind.HasFlag(CertificatesKind.Client) || kind.HasFlag(CertificatesKind.All))
                privateKey.KeyUsage |= X509PrivateKeyUsageFlags.XCN_NCRYPT_ALLOW_DECRYPT_FLAG; 

            try
            {
                privateKey.Create();
                CObjectId hashobj = new CObjectId();
                hashobj.InitializeFromAlgorithmName(ObjectIdGroupId.XCN_CRYPT_HASH_ALG_OID_GROUP_ID,
                                                    ObjectIdPublicKeyFlags.XCN_CRYPT_OID_INFO_PUBKEY_ANY,
                                                    AlgorithmFlags.AlgorithmFlagsNone, "SHA256");

                CERTENROLLLib.X509KeyUsageFlags flg = CERTENROLLLib.X509KeyUsageFlags.XCN_CERT_NO_KEY_USAGE;
                CObjectIds oidlist = new CObjectIds();
                if (kind.HasFlag(CertificatesKind.SSL) || kind.HasFlag(CertificatesKind.All))
                { 
                    CObjectId oid = new CObjectId();
                    oid.InitializeFromValue("1.3.6.1.5.5.7.3.1"); // SSL server  
                    oidlist.Add(oid);
                    flg |= CERTENROLLLib.X509KeyUsageFlags.XCN_CERT_DIGITAL_SIGNATURE_KEY_USAGE | CERTENROLLLib.X509KeyUsageFlags.XCN_CERT_KEY_ENCIPHERMENT_KEY_USAGE;
                }
                if (kind.HasFlag(CertificatesKind.Client) || kind.HasFlag(CertificatesKind.All))
                {
                    CObjectId oid = new CObjectId();
                    oid.InitializeFromValue("1.3.6.1.5.5.7.3.2"); // Client Auth
                    oidlist.Add(oid);
                    flg |= CERTENROLLLib.X509KeyUsageFlags.XCN_CERT_DIGITAL_SIGNATURE_KEY_USAGE | CERTENROLLLib.X509KeyUsageFlags.XCN_CERT_KEY_ENCIPHERMENT_KEY_USAGE;
                }
                if (kind.HasFlag(CertificatesKind.Signing) || kind.HasFlag(CertificatesKind.All))
                {
                    CObjectId oid = new CObjectId();
                    oid.InitializeFromValue("1.3.6.1.5.5.7.3.3"); // Signature
                    oidlist.Add(oid);
                    flg |= CERTENROLLLib.X509KeyUsageFlags.XCN_CERT_DIGITAL_SIGNATURE_KEY_USAGE;
                }
                if (kind.HasFlag(CertificatesKind.Decrypting) || kind.HasFlag(CertificatesKind.All))
                {
                    CObjectId oid = new CObjectId();
                    oid.InitializeFromValue("1.3.6.1.4.1.311.80.1"); // Encryption
                    oidlist.Add(oid);
                    flg |= CERTENROLLLib.X509KeyUsageFlags.XCN_CERT_KEY_ENCIPHERMENT_KEY_USAGE;
                }

                CX509ExtensionEnhancedKeyUsage eku = new CX509ExtensionEnhancedKeyUsage();
                eku.InitializeEncode(oidlist);

                CX509ExtensionKeyUsage ku = new CX509ExtensionKeyUsage();
                ku.InitializeEncode(flg);

                CX509ExtensionBasicConstraints bc = new CX509ExtensionBasicConstraints
                {
                    Critical = true
                };
                bc.InitializeEncode(false, 0);

                CX509ExtensionAlternativeNames san = new CX509ExtensionAlternativeNames();
                if ((!string.IsNullOrEmpty(dnsName)) && (kind.HasFlag(CertificatesKind.SSL) || kind.HasFlag(CertificatesKind.All)))
                {
                    CAlternativeNames dnlist = new CAlternativeNames();
                    CAlternativeName dns = new CAlternativeName();
                    dns.InitializeFromString(AlternativeNameType.XCN_CERT_ALT_NAME_DNS_NAME, dnsName);
                    dnlist.Add(dns);
                    san.InitializeEncode(dnlist);
                }

                // Create the self signing request
                CX509CertificateRequestCertificate certreq = new CX509CertificateRequestCertificate();
                certreq.InitializeFromPrivateKey(X509CertificateEnrollmentContext.ContextMachine, privateKey, "");
                certreq.Subject = subject;
                certreq.Issuer = issuer;
                certreq.NotBefore = DateTime.Now.AddDays(-10);

                certreq.NotAfter = DateTime.Now.AddYears(years);
                certreq.X509Extensions.Add((CX509Extension)eku); // add the EKU
                certreq.X509Extensions.Add((CX509Extension)ku); // add the KU
                certreq.X509Extensions.Add((CX509Extension)bc); // add the BC
                if ((!string.IsNullOrEmpty(dnsName)) && (kind.HasFlag(CertificatesKind.SSL) || kind.HasFlag(CertificatesKind.All)))
                    certreq.X509Extensions.Add((CX509Extension)san); // add the SAN
                certreq.HashAlgorithm = hashobj; // Specify the hashing algorithm
                certreq.Encode(); // encode the certificate

                // Do the final enrollment process
                CX509Enrollment enroll = new CX509Enrollment();
                enroll.InitializeFromRequest(certreq); // load the certificate
                enroll.CertificateFriendlyName = subjectName; // Optional: add a friendly name

                string csr = enroll.CreateRequest(); // Output the request in base64

                // and install it back as the response
                enroll.InstallResponse(InstallResponseRestrictionFlags.AllowUntrustedCertificate, csr, EncodingType.XCN_CRYPT_STRING_BASE64, pwd);

                // output a base64 encoded PKCS#12 so we can import it back to the .Net security classes
                base64encoded = enroll.CreatePFX(pwd, PFXExportOptions.PFXExportEEOnly);
            }
            finally
            {
                privateKey?.Delete(); // Remove Stored elsewhere
            }
            return base64encoded;
        }

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

                CObjectIds oidlist = new CObjectIds();

                CObjectId oid = new CObjectId();
                oid.InitializeFromValue("1.3.6.1.4.1.311.80.1"); // Encryption
                oidlist.Add(oid);

                CObjectId coid = new CObjectId();
                coid.InitializeFromValue("1.3.6.1.5.5.7.3.3"); // Signature
                oidlist.Add(coid);

                CX509ExtensionEnhancedKeyUsage eku = new CX509ExtensionEnhancedKeyUsage();
                eku.InitializeEncode(oidlist);

                CX509ExtensionKeyUsage ku = new CX509ExtensionKeyUsage();
                ku.InitializeEncode(CERTENROLLLib.X509KeyUsageFlags.XCN_CERT_DIGITAL_SIGNATURE_KEY_USAGE | CERTENROLLLib.X509KeyUsageFlags.XCN_CERT_KEY_ENCIPHERMENT_KEY_USAGE);

                CX509ExtensionBasicConstraints bc = new CX509ExtensionBasicConstraints
                {
                    Critical = true
                };
                bc.InitializeEncode(false, 0);

                // Create the self signing request
                CX509CertificateRequestCertificate certreq = new CX509CertificateRequestCertificate();
                certreq.InitializeFromPrivateKey(X509CertificateEnrollmentContext.ContextMachine, privateKey, "");
                certreq.Subject = dn;
                certreq.Issuer = neos;
                certreq.NotBefore = DateTime.Now.AddDays(-10);

                certreq.NotAfter = DateTime.Now.AddYears(years);
                certreq.X509Extensions.Add((CX509Extension)eku); // add the EKU
                certreq.X509Extensions.Add((CX509Extension)ku); // add the KU
                certreq.X509Extensions.Add((CX509Extension)bc); // add the BC
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
            finally
            {
                privateKey?.Delete(); // Remove Stored elsewhere
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

                CObjectIds oidlist = new CObjectIds();

                CObjectId oid = new CObjectId();
                oid.InitializeFromValue("1.3.6.1.4.1.311.80.1"); // Encryption
                oidlist.Add(oid);

                CObjectId coid = new CObjectId();
                coid.InitializeFromValue("1.3.6.1.5.5.7.3.3"); // Signature
                oidlist.Add(coid);

                CX509ExtensionEnhancedKeyUsage eku = new CX509ExtensionEnhancedKeyUsage();
                eku.InitializeEncode(oidlist);

                CX509ExtensionKeyUsage ku = new CX509ExtensionKeyUsage();
                ku.InitializeEncode(CERTENROLLLib.X509KeyUsageFlags.XCN_CERT_DIGITAL_SIGNATURE_KEY_USAGE | CERTENROLLLib.X509KeyUsageFlags.XCN_CERT_KEY_ENCIPHERMENT_KEY_USAGE);

                CX509ExtensionBasicConstraints bc = new CX509ExtensionBasicConstraints
                {
                    Critical = true
                };
                bc.InitializeEncode(false, 0);

                // Create the self signing request
                CX509CertificateRequestCertificate certreq = new CX509CertificateRequestCertificate();
                certreq.InitializeFromPrivateKey(X509CertificateEnrollmentContext.ContextUser, privateKey, "");
                certreq.Subject = dn;
                certreq.Issuer = neos;
                certreq.NotBefore = DateTime.Now.AddDays(-10);

                certreq.NotAfter = DateTime.Now.AddYears(years);
                certreq.X509Extensions.Add((CX509Extension)eku); // add the EKU
                certreq.X509Extensions.Add((CX509Extension)ku); // add the KU
                certreq.X509Extensions.Add((CX509Extension)bc); // add the BC
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
                base64encoded = enroll.CreatePFX(pwd, PFXExportOptions.PFXExportEEOnly);
            }
            finally
            {
                privateKey?.Delete(); // Remove Stored elsewhere
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


                CObjectIds oidlist = new CObjectIds();
                // 2.5.29.37 – Enhanced Key Usage includes
                CObjectId oid = new CObjectId();
                oid.InitializeFromValue("1.3.6.1.5.5.8.2.2"); // IP security IKE intermediate
                oidlist.Add(oid);

                CObjectId coid = new CObjectId();
                coid.InitializeFromValue("1.3.6.1.4.1.311.10.3.11"); // Key Recovery
                oidlist.Add(coid);

                CX509ExtensionEnhancedKeyUsage eku = new CX509ExtensionEnhancedKeyUsage();
                eku.InitializeEncode(oidlist);

                CX509ExtensionKeyUsage ku = new CX509ExtensionKeyUsage();
                ku.InitializeEncode(CERTENROLLLib.X509KeyUsageFlags.XCN_CERT_DIGITAL_SIGNATURE_KEY_USAGE | CERTENROLLLib.X509KeyUsageFlags.XCN_CERT_KEY_ENCIPHERMENT_KEY_USAGE);

                CX509ExtensionBasicConstraints bc = new CX509ExtensionBasicConstraints
                {
                    Critical = true
                };
                bc.InitializeEncode(false, 0);

                // Create the self signing request
                CX509CertificateRequestCertificate certreq = new CX509CertificateRequestCertificate();
                certreq.InitializeFromPrivateKey(X509CertificateEnrollmentContext.ContextMachine, privateKey, "");
                certreq.Subject = dn;
                certreq.Issuer = neos;
                certreq.NotBefore = DateTime.Now.AddDays(-10);

                certreq.NotAfter = DateTime.Now.AddYears(years);
                certreq.X509Extensions.Add((CX509Extension)eku); // add the EKU
                certreq.X509Extensions.Add((CX509Extension)ku); // add the KU
                certreq.X509Extensions.Add((CX509Extension)bc); // add the BC

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
            finally
            {
                privateKey?.Delete(); // Remove Stored elsewhere
            }
            return base64encoded;
        }

        /// <summary>
        /// InternalCreateADFSCertificate method implementation
        /// </summary>
        [SuppressUnmanagedCodeSecurity]
        private static string InternalCreateADFSCertificate(string subjectName, ADFSCertificatesKind kind, int years)
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
                SecurityDescriptor = "D:(A;;FA;;;SY)(A;;FA;;;BA)",
                KeyUsage = X509PrivateKeyUsageFlags.XCN_NCRYPT_ALLOW_USAGES_NONE
            };
            if (kind.HasFlag(ADFSCertificatesKind.Signing))
                privateKey.KeyUsage |= X509PrivateKeyUsageFlags.XCN_NCRYPT_ALLOW_SIGNING_FLAG;
            if (kind.HasFlag(ADFSCertificatesKind.Decrypting))
                privateKey.KeyUsage |= X509PrivateKeyUsageFlags.XCN_NCRYPT_ALLOW_DECRYPT_FLAG;

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

                CERTENROLLLib.X509KeyUsageFlags flg = CERTENROLLLib.X509KeyUsageFlags.XCN_CERT_NO_KEY_USAGE;
                CObjectIds oidlist = new CObjectIds();
                if (kind.HasFlag(ADFSCertificatesKind.Decrypting))
                {
                    CObjectId oid = new CObjectId();
                    oid.InitializeFromValue("1.3.6.1.4.1.311.80.1"); // Encryption
                    oidlist.Add(oid);
                    flg |= CERTENROLLLib.X509KeyUsageFlags.XCN_CERT_KEY_ENCIPHERMENT_KEY_USAGE;
                    dn.Encode("CN=ADFS Encrypt - " + subjectName, X500NameFlags.XCN_CERT_NAME_STR_NONE);
                    neos.Encode("CN=ADFS Encrypt - " + subjectName, X500NameFlags.XCN_CERT_NAME_STR_NONE);
                }
                if (kind.HasFlag(ADFSCertificatesKind.Signing))
                {
                    CObjectId coid = new CObjectId();
                    coid.InitializeFromValue("1.3.6.1.5.5.7.3.3"); // Signature
                    oidlist.Add(coid);
                    flg |= CERTENROLLLib.X509KeyUsageFlags.XCN_CERT_DIGITAL_SIGNATURE_KEY_USAGE;
                    dn.Encode("CN=ADFS Sign - " + subjectName, X500NameFlags.XCN_CERT_NAME_STR_NONE);
                    neos.Encode("CN=ADFS Sign - " + subjectName, X500NameFlags.XCN_CERT_NAME_STR_NONE);
                }

                CX509ExtensionEnhancedKeyUsage eku = new CX509ExtensionEnhancedKeyUsage();
                eku.InitializeEncode(oidlist);

                CX509ExtensionKeyUsage ku = new CX509ExtensionKeyUsage();
                ku.InitializeEncode(flg);

                CX509ExtensionBasicConstraints bc = new CX509ExtensionBasicConstraints
                {
                    Critical = true
                };
                bc.InitializeEncode(false, 0);

                // Create the self signing request
                CX509CertificateRequestCertificate certreq = new CX509CertificateRequestCertificate();
                certreq.InitializeFromPrivateKey(X509CertificateEnrollmentContext.ContextMachine, privateKey, "");
                certreq.Subject = dn;
                certreq.Issuer = neos;
                certreq.NotBefore = DateTime.Now.AddDays(-10);

                certreq.NotAfter = DateTime.Now.AddYears(years);
                certreq.X509Extensions.Add((CX509Extension)eku); // add the EKU
                certreq.X509Extensions.Add((CX509Extension)ku); // add the KU
                certreq.X509Extensions.Add((CX509Extension)bc); // add the BC

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
            finally
            {
                privateKey?.Delete(); // Remove Stored elsewhere
            }
            return base64encoded;
        }
        #endregion

        #region Windows 2012R2
        [SuppressUnmanagedCodeSecurity]
        private static string InternalCreateSelfSignedCertificate2012R2(string subjectName, string dnsName, CertificatesKind kind, int years, string pwd)
        {
            string base64encoded = string.Empty;
            CX500DistinguishedName subject = new CX500DistinguishedName();
            CX500DistinguishedName issuer = new CX500DistinguishedName();
            subject.Encode("CN=" + subjectName, X500NameFlags.XCN_CERT_NAME_STR_NONE);
            issuer.Encode("CN=" + subjectName, X500NameFlags.XCN_CERT_NAME_STR_NONE);

            IX509PrivateKey privateKey = (IX509PrivateKey)Activator.CreateInstance(Type.GetTypeFromProgID("X509Enrollment.CX509PrivateKey"));
            privateKey.ProviderName = "Microsoft Enhanced Cryptographic Provider v1.0";
            privateKey.MachineContext = true;
            privateKey.Length = 2048;
            privateKey.KeySpec = X509KeySpec.XCN_AT_KEYEXCHANGE; // use is not limited
            privateKey.ExportPolicy = X509PrivateKeyExportFlags.XCN_NCRYPT_ALLOW_EXPORT_FLAG;
            privateKey.SecurityDescriptor = "D:(A;;FA;;;SY)(A;;FA;;;BA)";

            privateKey.KeyUsage = X509PrivateKeyUsageFlags.XCN_NCRYPT_ALLOW_USAGES_NONE;
            if (kind.HasFlag(CertificatesKind.Signing) || kind.HasFlag(CertificatesKind.SSL) || kind.HasFlag(CertificatesKind.Client) || kind.HasFlag(CertificatesKind.All))
                privateKey.KeyUsage |= X509PrivateKeyUsageFlags.XCN_NCRYPT_ALLOW_SIGNING_FLAG;
            if (kind.HasFlag(CertificatesKind.Decrypting) || kind.HasFlag(CertificatesKind.SSL) || kind.HasFlag(CertificatesKind.Client) || kind.HasFlag(CertificatesKind.All))
                privateKey.KeyUsage |= X509PrivateKeyUsageFlags.XCN_NCRYPT_ALLOW_DECRYPT_FLAG;

            try
            {
                privateKey.Create();
                CObjectId hashobj = new CObjectId();
                hashobj.InitializeFromAlgorithmName(ObjectIdGroupId.XCN_CRYPT_HASH_ALG_OID_GROUP_ID,
                                                    ObjectIdPublicKeyFlags.XCN_CRYPT_OID_INFO_PUBKEY_ANY,
                                                    AlgorithmFlags.AlgorithmFlagsNone, "SHA256");

                CERTENROLLLib.X509KeyUsageFlags flg = CERTENROLLLib.X509KeyUsageFlags.XCN_CERT_NO_KEY_USAGE;
                CObjectIds oidlist = new CObjectIds();
                if (kind.HasFlag(CertificatesKind.SSL) || kind.HasFlag(CertificatesKind.All))
                {
                    CObjectId oid = new CObjectId();
                    oid.InitializeFromValue("1.3.6.1.5.5.7.3.1"); // SSL server  
                    oidlist.Add(oid);
                    flg |= CERTENROLLLib.X509KeyUsageFlags.XCN_CERT_DIGITAL_SIGNATURE_KEY_USAGE | CERTENROLLLib.X509KeyUsageFlags.XCN_CERT_KEY_ENCIPHERMENT_KEY_USAGE;
                }
                if (kind.HasFlag(CertificatesKind.Client) || kind.HasFlag(CertificatesKind.All))
                {
                    CObjectId oid = new CObjectId();
                    oid.InitializeFromValue("1.3.6.1.5.5.7.3.2"); // Client Auth
                    oidlist.Add(oid);
                    flg |= CERTENROLLLib.X509KeyUsageFlags.XCN_CERT_DIGITAL_SIGNATURE_KEY_USAGE | CERTENROLLLib.X509KeyUsageFlags.XCN_CERT_KEY_ENCIPHERMENT_KEY_USAGE;
                }
                if (kind.HasFlag(CertificatesKind.Signing) || kind.HasFlag(CertificatesKind.All))
                {
                    CObjectId oid = new CObjectId();
                    oid.InitializeFromValue("1.3.6.1.5.5.7.3.3"); // Signature
                    oidlist.Add(oid);
                    flg |= CERTENROLLLib.X509KeyUsageFlags.XCN_CERT_DIGITAL_SIGNATURE_KEY_USAGE;
                }
                if (kind.HasFlag(CertificatesKind.Decrypting) || kind.HasFlag(CertificatesKind.All))
                {
                    CObjectId oid = new CObjectId();
                    oid.InitializeFromValue("1.3.6.1.4.1.311.80.1"); // Encryption
                    oidlist.Add(oid);
                    flg |= CERTENROLLLib.X509KeyUsageFlags.XCN_CERT_KEY_ENCIPHERMENT_KEY_USAGE;
                }

                CX509ExtensionEnhancedKeyUsage eku = new CX509ExtensionEnhancedKeyUsage();
                eku.InitializeEncode(oidlist);

                CX509ExtensionKeyUsage ku = new CX509ExtensionKeyUsage();
                ku.InitializeEncode(flg);

                CX509ExtensionBasicConstraints bc = new CX509ExtensionBasicConstraints
                {
                    Critical = true
                };
                bc.InitializeEncode(false, 0);

                CX509ExtensionAlternativeNames san = new CX509ExtensionAlternativeNames();
                if ((!string.IsNullOrEmpty(dnsName)) && (kind.HasFlag(CertificatesKind.SSL) || kind.HasFlag(CertificatesKind.All)))
                {
                    CAlternativeNames dnlist = new CAlternativeNames();
                    CAlternativeName dns = new CAlternativeName();
                    dns.InitializeFromString(AlternativeNameType.XCN_CERT_ALT_NAME_DNS_NAME, dnsName);
                    dnlist.Add(dns);
                    san.InitializeEncode(dnlist);
                }

                // Create the self signing request
                CX509CertificateRequestCertificate certreq = new CX509CertificateRequestCertificate();
                certreq.InitializeFromPrivateKey(X509CertificateEnrollmentContext.ContextMachine, privateKey, "");
                certreq.Subject = subject;
                certreq.Issuer = issuer;
                certreq.NotBefore = DateTime.Now.AddDays(-10);

                certreq.NotAfter = DateTime.Now.AddYears(years);
                certreq.X509Extensions.Add((CX509Extension)eku); // add the EKU
                certreq.X509Extensions.Add((CX509Extension)ku); // add the KU
                certreq.X509Extensions.Add((CX509Extension)bc); // add the BC
                if ((!string.IsNullOrEmpty(dnsName)) && (kind.HasFlag(CertificatesKind.SSL) || kind.HasFlag(CertificatesKind.All)))
                    certreq.X509Extensions.Add((CX509Extension)san); // add the SAN
                certreq.HashAlgorithm = hashobj; // Specify the hashing algorithm
                certreq.Encode(); // encode the certificate

                // Do the final enrollment process
                CX509Enrollment enroll = new CX509Enrollment();
                enroll.InitializeFromRequest(certreq); // load the certificate
                enroll.CertificateFriendlyName = subjectName; // Optional: add a friendly name

                string csr = enroll.CreateRequest(); // Output the request in base64

                // and install it back as the response
                enroll.InstallResponse(InstallResponseRestrictionFlags.AllowUntrustedCertificate, csr, EncodingType.XCN_CRYPT_STRING_BASE64, pwd);

                // output a base64 encoded PKCS#12 so we can import it back to the .Net security classes
                base64encoded = enroll.CreatePFX(pwd, PFXExportOptions.PFXExportEEOnly);
            }
            finally
            {
                privateKey?.Delete(); // Remove Stored elsewhere
            }
            return base64encoded;
        }

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

                CObjectIds oidlist = new CObjectIds();
                CObjectId oid = new CObjectId();
                oid.InitializeFromValue("1.3.6.1.4.1.311.80.1"); // Encryption
                oidlist.Add(oid);

                CObjectId coid = new CObjectId();
                coid.InitializeFromValue("1.3.6.1.5.5.7.3.3"); // Signature
                oidlist.Add(coid);

                CX509ExtensionEnhancedKeyUsage eku = new CX509ExtensionEnhancedKeyUsage();
                eku.InitializeEncode(oidlist);

                CX509ExtensionKeyUsage ku = new CX509ExtensionKeyUsage();
                ku.InitializeEncode(CERTENROLLLib.X509KeyUsageFlags.XCN_CERT_DIGITAL_SIGNATURE_KEY_USAGE | CERTENROLLLib.X509KeyUsageFlags.XCN_CERT_KEY_ENCIPHERMENT_KEY_USAGE);

                CX509ExtensionBasicConstraints bc = new CX509ExtensionBasicConstraints
                {
                    Critical = true
                };
                bc.InitializeEncode(false, 0);

                // Create the self signing request
                CX509CertificateRequestCertificate certreq = new CX509CertificateRequestCertificate();
                certreq.InitializeFromPrivateKey(X509CertificateEnrollmentContext.ContextMachine, privateKey, "");
                certreq.Subject = dn;
                certreq.Issuer = neos;
                certreq.NotBefore = DateTime.Now.AddDays(-10);
                certreq.NotAfter = DateTime.Now.AddYears(years);

                certreq.X509Extensions.Add((CX509Extension)eku); // add the EKU
                certreq.X509Extensions.Add((CX509Extension)ku); // add the KU
                certreq.X509Extensions.Add((CX509Extension)bc); // add the BC

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
            finally
            {
                privateKey?.Delete(); // Remove Stored elsewhere
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

                CObjectIds oidlist = new CObjectIds();
                CObjectId oid = new CObjectId();
                oid.InitializeFromValue("1.3.6.1.4.1.311.80.1"); // Encryption
                oidlist.Add(oid);

                CObjectId coid = new CObjectId();
                coid.InitializeFromValue("1.3.6.1.5.5.7.3.3"); // Signature
                oidlist.Add(coid);

                CX509ExtensionEnhancedKeyUsage eku = new CX509ExtensionEnhancedKeyUsage();
                eku.InitializeEncode(oidlist);

                CX509ExtensionKeyUsage ku = new CX509ExtensionKeyUsage();
                ku.InitializeEncode(CERTENROLLLib.X509KeyUsageFlags.XCN_CERT_DIGITAL_SIGNATURE_KEY_USAGE | CERTENROLLLib.X509KeyUsageFlags.XCN_CERT_KEY_ENCIPHERMENT_KEY_USAGE);

                CX509ExtensionBasicConstraints bc = new CX509ExtensionBasicConstraints
                {
                    Critical = true
                };
                bc.InitializeEncode(false, 0);

                // Create the self signing request
                CX509CertificateRequestCertificate certreq = new CX509CertificateRequestCertificate();
                certreq.InitializeFromPrivateKey(X509CertificateEnrollmentContext.ContextUser, privateKey, "");
                certreq.Subject = dn;
                certreq.Issuer = neos;
                certreq.NotBefore = DateTime.Now.AddDays(-10);
                certreq.NotAfter = DateTime.Now.AddYears(years);

                certreq.X509Extensions.Add((CX509Extension)eku); // add the EKU
                certreq.X509Extensions.Add((CX509Extension)ku); // add the KU
                certreq.X509Extensions.Add((CX509Extension)bc); // add the BC

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
                base64encoded = enroll.CreatePFX(pwd, PFXExportOptions.PFXExportEEOnly);
            }
            finally
            {
                privateKey?.Delete(); // Remove Stored elsewhere
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
                var oidlist = new CObjectIds();
                CObjectId oid = new CObjectId();
                oid.InitializeFromValue("1.3.6.1.5.5.8.2.2"); // IP security IKE intermediate
                oidlist.Add(oid);

                CObjectId coid = new CObjectId();
                coid.InitializeFromValue("1.3.6.1.4.1.311.10.3.11"); // Key Recovery
                oidlist.Add(coid);

                CX509ExtensionEnhancedKeyUsage eku = new CX509ExtensionEnhancedKeyUsage();
                eku.InitializeEncode(oidlist);

                CX509ExtensionKeyUsage ku = new CX509ExtensionKeyUsage();
                ku.InitializeEncode(CERTENROLLLib.X509KeyUsageFlags.XCN_CERT_DIGITAL_SIGNATURE_KEY_USAGE | CERTENROLLLib.X509KeyUsageFlags.XCN_CERT_KEY_ENCIPHERMENT_KEY_USAGE);

                CX509ExtensionBasicConstraints bc = new CX509ExtensionBasicConstraints
                {
                    Critical = true
                };
                bc.InitializeEncode(false, 0);

                // Create the self signing request
                CX509CertificateRequestCertificate certreq = new CX509CertificateRequestCertificate();
                certreq.InitializeFromPrivateKey(X509CertificateEnrollmentContext.ContextMachine, privateKey, "");
                certreq.Subject = dn;
                certreq.Issuer = neos;
                certreq.NotBefore = DateTime.Now.AddDays(-10);
                certreq.NotAfter = DateTime.Now.AddYears(years);

                certreq.X509Extensions.Add((CX509Extension)eku); // add the EKU
                certreq.X509Extensions.Add((CX509Extension)ku); // add the EKU
                certreq.X509Extensions.Add((CX509Extension)bc); // add the EKU

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
            finally
            {
                privateKey?.Delete(); // Remove Stored elsewhere
            }
            return base64encoded;
        }

        /// <summary>
        /// InternalCreateADFSCertificate method implementation
        /// </summary>
        [SuppressUnmanagedCodeSecurity]
        private static string InternalCreateADFSCertificate2012R2(string subjectName, ADFSCertificatesKind kind, int years)
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

            privateKey.KeyUsage = X509PrivateKeyUsageFlags.XCN_NCRYPT_ALLOW_USAGES_NONE;
            if (kind.HasFlag(ADFSCertificatesKind.Signing)                )
                privateKey.KeyUsage |= X509PrivateKeyUsageFlags.XCN_NCRYPT_ALLOW_SIGNING_FLAG;
            if (kind.HasFlag(ADFSCertificatesKind.Decrypting))
                privateKey.KeyUsage |= X509PrivateKeyUsageFlags.XCN_NCRYPT_ALLOW_DECRYPT_FLAG;

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

                CERTENROLLLib.X509KeyUsageFlags flg = CERTENROLLLib.X509KeyUsageFlags.XCN_CERT_NO_KEY_USAGE;
                CObjectIds oidlist = new CObjectIds();
                if (kind.HasFlag(ADFSCertificatesKind.Decrypting))
                {
                    CObjectId oid = new CObjectId();
                    oid.InitializeFromValue("1.3.6.1.4.1.311.80.1"); // Encryption
                    oidlist.Add(oid);
                    flg |= CERTENROLLLib.X509KeyUsageFlags.XCN_CERT_KEY_ENCIPHERMENT_KEY_USAGE;
                    dn.Encode("CN=ADFS Encrypt - " + subjectName, X500NameFlags.XCN_CERT_NAME_STR_NONE);
                    neos.Encode("CN=ADFS Encrypt - " + subjectName, X500NameFlags.XCN_CERT_NAME_STR_NONE);
                }
                if (kind.HasFlag(ADFSCertificatesKind.Signing))
                {
                    CObjectId coid = new CObjectId();
                    coid.InitializeFromValue("1.3.6.1.5.5.7.3.3"); // Signature
                    oidlist.Add(coid);
                    flg |= CERTENROLLLib.X509KeyUsageFlags.XCN_CERT_DIGITAL_SIGNATURE_KEY_USAGE;
                    dn.Encode("CN=ADFS Sign - " + subjectName, X500NameFlags.XCN_CERT_NAME_STR_NONE);
                    neos.Encode("CN=ADFS Sign - " + subjectName, X500NameFlags.XCN_CERT_NAME_STR_NONE);
                }

                CX509ExtensionEnhancedKeyUsage eku = new CX509ExtensionEnhancedKeyUsage();
                eku.InitializeEncode(oidlist);

                CX509ExtensionKeyUsage ku = new CX509ExtensionKeyUsage();
                ku.InitializeEncode(flg);

                CX509ExtensionBasicConstraints bc = new CX509ExtensionBasicConstraints
                {
                    Critical = true
                };
                bc.InitializeEncode(false, 0);

                // Create the self signing request
                CX509CertificateRequestCertificate certreq = new CX509CertificateRequestCertificate();
                certreq.InitializeFromPrivateKey(X509CertificateEnrollmentContext.ContextMachine, privateKey, "");
                certreq.Subject = dn;
                certreq.Issuer = neos;
                certreq.NotBefore = DateTime.Now.AddDays(-10);
                certreq.NotAfter = DateTime.Now.AddYears(years);

                certreq.X509Extensions.Add((CX509Extension)eku); // add the EKU
                certreq.X509Extensions.Add((CX509Extension)ku); // add the KU
                certreq.X509Extensions.Add((CX509Extension)bc); // add the BC
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
            finally
            {
                privateKey?.Delete();
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
                        if (rsakey is RSACng cng)
                        {
                            cntName = cng.Key.UniqueName;
                        }
                        else if (rsakey is RSACryptoServiceProvider provider)
                        {
                            cntName = provider.CspKeyContainerInfo.UniqueKeyContainerName;
                        }
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
                // Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + sep + "Microsoft" + sep + "Crypto" + sep + "Keys" + sep,
            };
            foreach (string pth in paths)
            {
                DirectoryInfo dir = new DirectoryInfo(pth);

                foreach (FileInfo fi in dir.GetFiles())
                {
                    try
                    {
                        if (fi.Name.ToLower().StartsWith("f686aace6942fb7f7ceb231212eef4a4_"))  // RDP Key do not drop anyway (TSSecKeySet1)
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
                RegistryKey ek = Registry.LocalMachine.OpenSubKey("Software\\MFA", true) ?? Registry.LocalMachine.CreateSubKey("Software\\MFA", true);
                ek.SetValue("PrivateKeysCleanUpEnabled", 1, RegistryValueKind.DWord);
                ek.SetValue("PrivateKeysCleanUpDelay", delay, RegistryValueKind.DWord);
            }
            if (option == 0x02)  // Disable
            {
                RegistryKey dk = Registry.LocalMachine.OpenSubKey("Software\\MFA", true) ?? Registry.LocalMachine.CreateSubKey("Software\\MFA", true);
                dk.SetValue("PrivateKeysCleanUpEnabled", 0, RegistryValueKind.DWord);
            }
        }
        #endregion

        #region MFA RSA Key
        /// <summary>
        /// CreateMFARSACngKey method implmentation
        /// </summary>
        internal static byte[] CreateMFARSACngKey(out string uniquekeyname)
        {
            uniquekeyname = string.Empty;
            CngProvider keyStorageProvider = CngProvider.MicrosoftSoftwareKeyStorageProvider;
            byte[] result;
            try
            {
                CngKeyCreationParameters keyCreationParameters = new CngKeyCreationParameters()
                {
                    ExportPolicy = CngExportPolicies.AllowPlaintextExport,
                    KeyCreationOptions = CngKeyCreationOptions.MachineKey | CngKeyCreationOptions.OverwriteExistingKey,
                    Provider = keyStorageProvider
                };
                CngProperty cngProperty = new CngProperty("Length", System.BitConverter.GetBytes(2048), CngPropertyOptions.None);
                keyCreationParameters.Parameters.Add(cngProperty);
                CngKey key = CngKey.Create(CngAlgorithm.Rsa, SystemUtilities.SystemKeyName, keyCreationParameters);
                result = key.Export(CngKeyBlobFormat.GenericPrivateBlob);
                uniquekeyname = key.UniqueName;
            }
            catch (Exception)
            {
                return null;
            }
            return result;
        }

        /// <summary>
        /// ExportMFARSACngKey method implmentation
        /// </summary>
        internal static byte[] ExportMFARSACngKey(out string uniquekeyname)
        {
            uniquekeyname = string.Empty;
            byte[] result;
            try
            {
                CngKey key = CngKey.Open(SystemUtilities.SystemKeyName, CngProvider.MicrosoftSoftwareKeyStorageProvider, CngKeyOpenOptions.MachineKey);
                result = key.Export(CngKeyBlobFormat.GenericPrivateBlob);
                uniquekeyname = key.UniqueName;
            }
            catch (Exception)
            {
                return null;
            }
            return result;
        }

        /// <summary>
        /// ImportMFARSACngKey method implmentation
        /// </summary>
        internal static bool ImportMFARSACngKey(byte[] blob, out string uniquekeyname)
        {
            uniquekeyname = string.Empty;
            CngProvider keyStorageProvider = CngProvider.MicrosoftSoftwareKeyStorageProvider;
            try
            {
                CngKeyCreationParameters cngKeyParameter = new CngKeyCreationParameters()
                {
                    Provider = keyStorageProvider,                   
                    KeyCreationOptions = CngKeyCreationOptions.MachineKey | CngKeyCreationOptions.OverwriteExistingKey
                };
                CngProperty cngProperty = new CngProperty("Length", System.BitConverter.GetBytes(2048), CngPropertyOptions.None);
                cngKeyParameter.Parameters.Add(cngProperty);
                CngProperty keyBlobProperty = new CngProperty(CngKeyBlobFormat.GenericPrivateBlob.ToString(), blob, CngPropertyOptions.None);
                cngKeyParameter.Parameters.Add(keyBlobProperty);
                CngKey key = CngKey.Create(CngAlgorithm.Rsa, SystemUtilities.SystemKeyName, cngKeyParameter);
                uniquekeyname = key.UniqueName;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// DeleteMFARSACngKey method implementation
        /// </summary>
        internal static bool DeleteMFARSACngKey()
        {
            try
            {
                CngKey cngkey = CngKey.Open(SystemUtilities.SystemKeyName, CngProvider.MicrosoftSoftwareKeyStorageProvider, CngKeyOpenOptions.MachineKey);
                if (cngkey != null)
                {
                    cngkey.Delete();
                    return true;
                }
                else
                    return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// ExistsMFARSACngKey method implementation
        /// </summary>
        internal static bool ExistsMFARSACngKey()
        {
            try
            {
                return CngKey.Exists(SystemUtilities.SystemKeyName, CngProvider.MicrosoftSoftwareKeyStorageProvider, CngKeyOpenOptions.MachineKey);
            }
            catch (Exception)
            {
                return false;
            }
        }
        #endregion

        #region MFA AES Key
        /// <summary>
        /// CreateMFAAESCngKey method implmentation (ECDH)
        /// </summary>
        internal static byte[] CreateMFAAESCngKey(string keyname, out string uniquekeyname)
        {
            uniquekeyname = string.Empty;
            CngProvider keyStorageProvider = CngProvider.MicrosoftSoftwareKeyStorageProvider;
            byte[] result;
            try
            {
                CngKeyCreationParameters keyCreationParameters = new CngKeyCreationParameters()
                {
                    ExportPolicy = CngExportPolicies.AllowPlaintextExport,
                    KeyCreationOptions = CngKeyCreationOptions.MachineKey | CngKeyCreationOptions.OverwriteExistingKey,
                    Provider = keyStorageProvider
                };
                CngProperty cngProperty = new CngProperty("Length", System.BitConverter.GetBytes(256), CngPropertyOptions.None);
                keyCreationParameters.Parameters.Add(cngProperty);
                CngKey key = CngKey.Create(CngAlgorithm.ECDiffieHellmanP256, keyname, keyCreationParameters);
                result = key.Export(CngKeyBlobFormat.GenericPrivateBlob);
                uniquekeyname = key.UniqueName;
            }
            catch (Exception)
            {
                return null;
            }
            return result;
        }

        /// <summary>
        /// ExportMFAAESCngKey method implmentation
        /// </summary>
        internal static byte[] ExportMFAAESCngKey(string keyname, out string uniquekeyname)
        {
            uniquekeyname = string.Empty;
            byte[] result;
            try
            {
                CngKey key = CngKey.Open(keyname, CngProvider.MicrosoftSoftwareKeyStorageProvider, CngKeyOpenOptions.MachineKey);
                result = key.Export(CngKeyBlobFormat.GenericPrivateBlob);
                uniquekeyname = key.UniqueName;
            }
            catch (Exception)
            {
                return null;
            }
            return result;
        }

        /// <summary>
        /// ImportMFAAESCngKey method implmentation
        /// </summary>
        internal static bool ImportMFAAESCngKey(byte[] blob, string keyname, out string uniquekeyname)
        {
            uniquekeyname = string.Empty;
            CngProvider keyStorageProvider = CngProvider.MicrosoftSoftwareKeyStorageProvider;
            try
            {
                CngKeyCreationParameters cngKeyParameter = new CngKeyCreationParameters()
                {
                    Provider = keyStorageProvider,
                    KeyCreationOptions = CngKeyCreationOptions.MachineKey | CngKeyCreationOptions.OverwriteExistingKey
                };
                CngProperty cngProperty = new CngProperty("Length", System.BitConverter.GetBytes(256), CngPropertyOptions.None);
                cngKeyParameter.Parameters.Add(cngProperty);
                CngProperty keyBlobProperty = new CngProperty(CngKeyBlobFormat.GenericPrivateBlob.ToString(), blob, CngPropertyOptions.None);
                cngKeyParameter.Parameters.Add(keyBlobProperty);
                CngKey key = CngKey.Create(CngAlgorithm.ECDiffieHellmanP256, keyname, cngKeyParameter);
                uniquekeyname = key.UniqueName;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// DeleteMFARSACngKey method implementation
        /// </summary>
        internal static bool DeleteMFAAESCngKey(string keyname)
        {
            try
            {
                CngKey cngkey = CngKey.Open(keyname, CngProvider.MicrosoftSoftwareKeyStorageProvider, CngKeyOpenOptions.MachineKey);
                if (cngkey != null)
                {
                    cngkey.Delete();
                    return true;
                }
                else
                    return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// ExistsMFAAESCngKey method implementation
        /// </summary>
        internal static bool ExistsMFAAESCngKey()
        {
            try
            {
                return (CngKey.Exists(SystemUtilities.BobKeyName, CngProvider.MicrosoftSoftwareKeyStorageProvider, CngKeyOpenOptions.MachineKey) &&
                        CngKey.Exists(SystemUtilities.AlicKeyName, CngProvider.MicrosoftSoftwareKeyStorageProvider, CngKeyOpenOptions.MachineKey));
            }
            catch (Exception)
            {
                return false;
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
                RegistryKey rk = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Windows NT\\CurrentVersion", false);

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
        /// IsWindows2022 property implementation
        /// </summary>
        [IgnoreDataMember]
        public bool IsWindows2022
        {
            get { return ((this.CurrentMajorVersionNumber == 10) && (this.CurrentBuild >= 20348)); }
        }

        /// <summary>
        /// IsWindows2019 property implementation
        /// </summary>
        [IgnoreDataMember]
        public bool IsWindows2019
        {
            get { return ((this.CurrentMajorVersionNumber == 10) && ((this.CurrentBuild >= 17763) && (this.CurrentBuild < 20348))); }
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

        /// <summary>
        /// IsADFSBehavior4 property implementation
        /// </summary>
        [IgnoreDataMember]
        public bool IsADFSBehavior4
        {
            get { return IsWindows2019 || IsWindows2022; }
        }
    }

    [DataContract]
    public class ADFSNodeInformation
    {
        /// <summary>
        /// FQDN property implementation
        /// </summary>
        [DataMember]
        public string FQDN { get; set; }

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
