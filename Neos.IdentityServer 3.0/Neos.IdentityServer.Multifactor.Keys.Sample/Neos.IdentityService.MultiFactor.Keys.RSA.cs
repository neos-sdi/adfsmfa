using Neos.IdentityServer.MultiFactor;
using Neos.IdentityServer.MultiFactor.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Neos.IdentityServer.Multifactor.Keys
{
    /// <summary>
    /// DBKeyManagerCreator class
    /// </summary>
    internal class DBKeyManagerCreator: ISecretKeyManagerCreator
    {
        /// <summary>
        /// CreateKeyManager method
        /// </summary>
        public ISecretKeyManager CreateInstance(SecretKeyVersion version)
        {
            if (version == SecretKeyVersion.V2)
                return new DBKeyManager.RSADBKeyManagerV2();
            else
                return new DBKeyManager.RSADBKeyManagerV1();
        }
    }

    /// <summary>
    /// DBKeyManager class
    /// </summary>
    internal abstract class DBKeyManager : ISecretKeyManager, IDataRepositorySQLConnection
    {
        private string _connectionstring;
        private string _dataparameters;
        private int _validity;
        private MFAConfig _config = null;
        private KeySizeMode _ksize = KeySizeMode.KeySize1024;
        private KeysRepositoryService _repos = null;
        private int MAX_PROBE_LEN = 0;
        private string _xorsecret = XORUtilities.XORKey;

        /// <summary>
        /// DBKeyManager constructor
        /// limit creation by KeyManager
        /// </summary>
        protected DBKeyManager()
        {
            Trace.TraceInformation("DBKeyManager()");
        }

        /// <summary>
        /// Initialize method implementation
        /// </summary>
        public void Initialize(MFAConfig config)
        {
            _config = config;
            _connectionstring = config.KeysConfig.ExternalKeyManager.ConnectionString;
            _dataparameters = config.KeysConfig.ExternalKeyManager.Parameters.Data;
            _validity = config.KeysConfig.CertificateValidity;
            _ksize = config.KeysConfig.KeySize;
            if (!string.IsNullOrEmpty(config.KeysConfig.XORSecret))
                _xorsecret = config.KeysConfig.XORSecret;
            _repos = new DBKeysRepositoryService(_config.KeysConfig.ExternalKeyManager, _config.DeliveryWindow);
            switch (_ksize)
            {
                case KeySizeMode.KeySize128:
                    MAX_PROBE_LEN = 16;
                    break;
                case KeySizeMode.KeySize256:
                    MAX_PROBE_LEN = 32;
                    break;
                case KeySizeMode.KeySize384:
                    MAX_PROBE_LEN = 48;
                    break;
                case KeySizeMode.KeySize512:
                    MAX_PROBE_LEN = 64;
                    break;
                case KeySizeMode.KeySize1024:
                    MAX_PROBE_LEN = 128;
                    break;
                case KeySizeMode.KeySize2048:
                    MAX_PROBE_LEN = 256;
                    break;
                default:
                    MAX_PROBE_LEN = 128;
                    break;
            }
        }

        /// <summary>
        /// KeysStorage method implementation
        /// </summary>
        public KeysRepositoryService KeysStorage
        {
            get { return _repos; }
        }

        /// <summary>
        /// Prefix property
        /// </summary>
        public string Prefix
        {
            get { return "custom://"; }
        }

        /// <summary>
        /// Prefix property
        /// </summary>
        public string XORSecret
        {
            get { return _xorsecret; }
        }

        /// <summary>
        /// CheckConnection method implementation
        /// </summary>
        public bool CheckConnection(string connectionstring)
        {
            return (KeysStorage as IDataRepositorySQLConnection).CheckConnection(connectionstring);
        }


        #region Storage Methods
        /// <summary>
        /// ReadKey method implementation
        /// </summary>
        public string ReadKey(string upn)
        {
            if (string.IsNullOrEmpty(upn))
                return null;
            string lupn = upn.ToLower();
            return KeysStorage.GetUserKey(lupn);
        }

        /// <summary>
        /// RemoveKey method implementation
        /// </summary>
        public bool RemoveKey(string upn)
        {
            if (string.IsNullOrEmpty(upn))
                return false;
            string lupn = upn.ToLower();
            return KeysStorage.RemoveUserKey(lupn);
        }
        #endregion

        #region Crypting methods

        public abstract string NewKey(string upn);
        public abstract string EncodedKey(string upn);
        public abstract byte[] ProbeKey(string upn);
        public abstract bool ValidateKey(string upn); 

        #endregion

        #region RSAKeyManagerV1
        internal class RSADBKeyManagerV1 : DBKeyManager
        {
            /// <summary>
            /// NewKey method implementation
            /// </summary>
            public override string NewKey(string upn)
            {
                if (string.IsNullOrEmpty(upn))
                    return null;
                string lupn = upn.ToLower();

                byte[] crypted = null;
                using (var prov = new Encryption(_xorsecret))
                {
                    prov.Certificate = KeysStorage.CreateCertificate(lupn, string.Empty, _validity);
                    crypted = prov.Encrypt(lupn);
                    if (crypted == null)
                        return null;
                    string outkey = AddKeyPrefix(System.Convert.ToBase64String(crypted));
                    return KeysStorage.NewUserKey(lupn, outkey, prov.Certificate);
                }
            }

            #region Crypting V1 methods
            /// <summary>
            /// EncodedKey method implementation
            /// </summary>
            public override string EncodedKey(string upn)
            {
                if (string.IsNullOrEmpty(upn))
                    return null;
                string lupn = upn.ToLower();
                string full = StripKeyPrefix(ReadKey(lupn));
                if (string.IsNullOrEmpty(full))
                    return null;
                if (full.Length > MAX_PROBE_LEN)
                    return Base32.Encode(full.Substring(0, MAX_PROBE_LEN));
                else
                    return Base32.Encode(full);
            }

            /// <summary>
            /// ValidateKeyV1 method implmentation
            /// </summary>
            public override bool ValidateKey(string upn)
            {
                if (string.IsNullOrEmpty(upn))
                    return false;
                string lupn = upn.ToLower();
                string key = ReadKey(lupn);
                if (HasKeyPrefix(key))
                {
                    using (var prov = new Encryption(_xorsecret))
                    {
                        string xkey = StripKeyPrefix(key);
                        byte[] crypted = System.Convert.FromBase64CharArray(xkey.ToCharArray(), 0, xkey.Length);
                        if (crypted == null)
                            return false;

                        prov.Certificate = KeysStorage.GetUserCertificate(lupn, string.Empty);
                        byte[] cleared = prov.Decrypt(crypted, lupn);

                        if (cleared == null)
                            return false; // Key corrupted
                        if (prov.CheckSum == null)
                            return false; // Key corrupted
                        if (prov.CheckSum.SequenceEqual(CheckSumEncoding.CheckSum(lupn)))
                            return true;  // OK RSA
                        else
                            return false; // Key corrupted
                    }
                }
                else
                    return false;
            }

            /// <summary>
            /// ProbeKeyV1 method implmentation
            /// </summary>
            public override byte[] ProbeKey(string upn)
            {
                if (string.IsNullOrEmpty(upn))
                    return null;
                string lupn = upn.ToLower();
                string full = StripKeyPrefix(ReadKey(lupn));
                if (string.IsNullOrEmpty(full))
                    return null;
                if (full.Length > MAX_PROBE_LEN)
                {
                    byte[] bytes = new byte[MAX_PROBE_LEN * sizeof(char)];
                    Buffer.BlockCopy(full.ToCharArray(), 0, bytes, 0, bytes.Length);
                    return bytes;
                }
                else
                {
                    byte[] bytes = new byte[full.Length * sizeof(char)];
                    Buffer.BlockCopy(full.ToCharArray(), 0, bytes, 0, bytes.Length);
                    return bytes;
                }
            }

            // <summary>
            /// StripKeyPrefix method implementation
            /// </summary>
            private string StripKeyPrefix(string key)
            {
                if (string.IsNullOrEmpty(key))
                    return null;
                if (key.StartsWith(this.Prefix))
                    key = key.Replace(this.Prefix, "");
                return key;
            }

            /// <summary>
            /// AddKeyPrefix method implementation
            /// </summary>
            private string AddKeyPrefix(string key)
            {
                if (string.IsNullOrEmpty(key))
                    return null;
                if (!key.StartsWith(this.Prefix))
                    key = this.Prefix + key;
                return key;
            }

            /// <summary>
            /// HasKeyPrefix method implementation
            /// </summary>
            private bool HasKeyPrefix(string key)
            {
                if (string.IsNullOrEmpty(key))
                    return false;
                return key.StartsWith(this.Prefix);
            }
            #endregion
        }
        #endregion

        #region RSAKeyManagerV2
        internal class RSADBKeyManagerV2 : DBKeyManager
        {
            /// <summary>
            /// NewKey method implementation
            /// </summary>
            public override string NewKey(string upn)
            {
                if (string.IsNullOrEmpty(upn))
                    return null;
                string lupn = upn.ToLower();
                byte[] crypted = null;
                using (var prov = new RSAEncryption(_xorsecret))
                {
                    string pass = CheckSumEncoding.CheckSumAsString(lupn);
                    prov.Certificate = KeysStorage.CreateCertificate(lupn, pass, _validity);
                    crypted = prov.Encrypt(lupn);
                    if (crypted == null)
                        return null;
                    string outkey = AddStorageInfos(crypted);
                    return KeysStorage.NewUserKey(lupn, outkey, prov.Certificate);
                }
            }

            #region Crypting methods
            /// <summary>
            /// EncodedKey method implementation
            /// </summary>
            public override string EncodedKey(string upn)
            {
                if (string.IsNullOrEmpty(upn))
                    return null;
                string lupn = upn.ToLower();
                string key = ReadKey(lupn);
                if (string.IsNullOrEmpty(key))
                    return null;

                byte[] cleared = null;
                using (var prov = new RSAEncryption(_xorsecret))
                {
                    byte[] crypted = StripStorageInfos(key);
                    if (crypted == null)
                        return null;
                    string pass = CheckSumEncoding.CheckSumAsString(lupn);
                    prov.Certificate = KeysStorage.GetUserCertificate(lupn, pass);
                    cleared = prov.Decrypt(crypted, lupn);
                    if (cleared == null)
                        return null;
                }
                if (cleared.Length > MAX_PROBE_LEN)
                {
                    byte[] buffer = new byte[MAX_PROBE_LEN];
                    Buffer.BlockCopy(cleared, 0, buffer, 0, MAX_PROBE_LEN);
                    return Base32.Encode(buffer);
                }
                else
                    return Base32.Encode(cleared);
            }

            /// <summary>
            /// ValidateKey method implmentation
            /// </summary>
            public override bool ValidateKey(string upn)
            {
                if (string.IsNullOrEmpty(upn))
                    return false;
                string lupn = upn.ToLower();
                string key = ReadKey(lupn);
                if (string.IsNullOrEmpty(key))
                    return false;
                if (HasStorageInfos(key))
                {
                    using (var prov = new RSAEncryption(_xorsecret))
                    {
                        byte[] crypted = StripStorageInfos(key);
                        if (crypted == null)
                            return false;
                        string pass = CheckSumEncoding.CheckSumAsString(lupn);
                        prov.Certificate = KeysStorage.GetUserCertificate(lupn, pass);
                        byte[] cleared = prov.Decrypt(crypted, lupn);

                        if (cleared == null)
                            return false; // Key corrupted
                        if (prov.CheckSum == null)
                            return false; // Key corrupted
                        if (prov.CheckSum.SequenceEqual(CheckSumEncoding.CheckSum(lupn)))
                            return true;  // OK RSA
                        else
                            return false; // Key corrupted
                    }
                }
                else
                    return false;
            }

            /// <summary>
            /// ProbeKey method implmentation
            /// </summary>
            public override byte[] ProbeKey(string upn)
            {
                if (string.IsNullOrEmpty(upn))
                    return null;
                string lupn = upn.ToLower();
                string key = ReadKey(lupn);
                if (string.IsNullOrEmpty(key))
                    return null;

                byte[] probed = null;
                using (var prov = new RSAEncryption(_xorsecret))
                {
                    byte[] crypted = StripStorageInfos(key);
                    if (crypted == null)
                        return null;
                    string pass = CheckSumEncoding.CheckSumAsString(lupn);
                    prov.Certificate = KeysStorage.GetUserCertificate(lupn, pass);
                    probed = prov.Decrypt(crypted, lupn);
                    if (probed == null)
                        return null;
                }
                if (probed.Length > MAX_PROBE_LEN)
                {
                    byte[] buffer = new byte[MAX_PROBE_LEN];
                    Buffer.BlockCopy(probed, 0, buffer, 0, MAX_PROBE_LEN);
                    return buffer;
                }
                else
                    return probed;
            }

            /// <summary>
            /// StripStorageInfos method implementation
            /// </summary>
            private byte[] StripStorageInfos(string key)
            {
                if (string.IsNullOrEmpty(key))
                    return null;
                if (key.StartsWith(this.Prefix))
                    key = key.Replace(this.Prefix, "");
                try
                {
                    return System.Convert.FromBase64CharArray(key.ToCharArray(), 0, key.Length);
                }
                catch (Exception)
                {
                    return null;
                }
            }

            /// <summary>
            /// AddStorageInfos method implementation
            /// </summary>
            private string AddStorageInfos(byte[] key)
            {
                try
                {
                    if (key == null)
                        return null;
                    return this.Prefix + System.Convert.ToBase64String(key);
                }
                catch (Exception)
                {
                    return default(string);
                }
            }

            /// <summary>
            /// HasStorageInfos method implementation
            /// </summary>
            private bool HasStorageInfos(string key)
            {
                if (string.IsNullOrEmpty(key))
                    return false;
                return key.StartsWith(this.Prefix);
            }
            #endregion
        }
        #endregion
    }
}
