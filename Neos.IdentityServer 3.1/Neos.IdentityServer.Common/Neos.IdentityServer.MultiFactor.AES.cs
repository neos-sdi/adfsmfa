using Neos.IdentityServer.MultiFactor.Data;
using System;
using System.Diagnostics;
using System.Linq;

namespace Neos.IdentityServer.MultiFactor.Common
{
    /// <summary>
    /// AESKeyManagerActivator class
    /// </summary>
    internal class AESKeyManagerActivator : ISecretKeyManagerActivator
    {
        /// <summary>
        /// CreateKeyManager method
        /// </summary>
        public ISecretKeyManager CreateInstance(SecretKeyVersion version)
        {
            if (version == SecretKeyVersion.V2)
                return new AESKeyManager.AES256KeyManager();
            else
                return new AESKeyManager.AES128KeyManager();
        }
    }

    /// <summary>
    /// AESKeyManager class
    /// </summary>
    internal abstract class AESKeyManager : ISecretKeyManager
    {
        private int MAX_PROBE_LEN = 0;

        /// <summary>
        /// AESKeyManager constructor
        /// limit creation by KeyManager
        /// </summary>
        protected AESKeyManager()
        {
            Trace.TraceInformation("AESKeyManager()");
        }

        /// <summary>
        /// Initialize method implementation
        /// </summary>
        public void Initialize(KeysRepositoryService keysstorage,  BaseKeysManagerParams parameters)
        {
            AESKeysManagerParams config = (AESKeysManagerParams)parameters;
            KeysStorage = keysstorage;
            XORSecret = config.XORSecret;
            KeySize = config.AESKeyGenerator;
            switch (KeySize)
            {
                case AESKeyGeneratorMode.AESSecret512:
                    MAX_PROBE_LEN = 64;
                    break;
                case AESKeyGeneratorMode.AESSecret1024:
                    MAX_PROBE_LEN = 128;
                    break;
                default:
                    MAX_PROBE_LEN = 128;
                    break;
            }
        }

        /// <summary>
        /// KeysStorage method implementation
        /// </summary>
        public KeysRepositoryService KeysStorage { get; private set; } = null;

        /// <summary>
        /// XORSecret property
        /// </summary>
        public string XORSecret { get; private set; } = XORUtilities.XORKey;

        /// <summary>
        /// KSize property
        /// </summary>
        private AESKeyGeneratorMode KeySize { get; set; } = AESKeyGeneratorMode.AESSecret1024;


        /// <summary>
        /// Prefix property
        /// </summary>
        public string Prefix
        {
            get { return "aes://"; }
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

        #region AES128KeyManager
        internal class AES128KeyManager : AESKeyManager
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
                using (var prov = new AES128Encryption(XORSecret, KeySize))
                {
                    crypted = prov.NewEncryptedKey(lupn);
                    if (crypted == null)
                        return null;
                }
                string outkey = AddStorageInfos(crypted);
                return KeysStorage.NewUserKey(lupn, outkey);
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
                string key = ReadKey(lupn);
                if (string.IsNullOrEmpty(key))
                    return null;

                byte[] cleared = null;
                using (var prov = new AES128Encryption(XORSecret, KeySize))
                {
                    byte[] crypted = StripStorageInfos(key);
                    if (crypted == null)
                        return null;

                    cleared = prov.GetDecryptedKey(crypted, lupn);
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
            /// ValidateKeyV1 method implmentation
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
                    using (var prov = new AES128Encryption(XORSecret, KeySize))
                    {
                        byte[] crypted = StripStorageInfos(key);
                        if (crypted == null)
                            return false;
                        byte[] cleared = prov.GetDecryptedKey(crypted, lupn);

                        if (cleared == null)
                            return false; // Key corrupted
                        if (prov.CheckSum == null)
                            return false; // Key corrupted
                        if (prov.CheckSum.SequenceEqual(CheckSumEncoding.CheckSum(lupn)))
                            return true;  // OK 
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
                string key = ReadKey(lupn);
                if (string.IsNullOrEmpty(key))
                    return null;

                byte[] probed = null;
                using (var prov = new AES128Encryption(XORSecret, KeySize))
                {
                    byte[] crypted = StripStorageInfos(key);
                    if (crypted == null)
                        return null;
                    probed = prov.GetDecryptedKey(crypted, lupn);
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

        #region AES256KeyManager
        internal class AES256KeyManager : AESKeyManager
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
                using (var prov = new AES256Encryption(XORSecret, KeySize))
                {
                    crypted = prov.NewEncryptedKey(lupn);
                    if (crypted == null)
                        return null;
                }
                string outkey = AddStorageInfos(crypted);
                return KeysStorage.NewUserKey(lupn, outkey);
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
                using (var prov = new AES256Encryption(XORSecret, KeySize))
                {
                    byte[] crypted = StripStorageInfos(key);
                    if (crypted == null)
                        return null;

                    cleared = prov.GetDecryptedKey(crypted, lupn);
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
                    using (var prov = new AES256Encryption(XORSecret, KeySize))
                    {
                        byte[] crypted = StripStorageInfos(key);
                        if (crypted == null)
                            return false;
                        byte[] cleared = prov.GetDecryptedKey(crypted, lupn);

                        if (cleared == null)
                            return false; // Key corrupted
                        if (prov.CheckSum == null)
                            return false; // Key corrupted
                        if (prov.CheckSum.SequenceEqual(CheckSumEncoding.CheckSum(lupn)))
                            return true;  // OK 
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
                using (var prov = new AES256Encryption(XORSecret, KeySize))
                {
                    byte[] crypted = StripStorageInfos(key);
                    if (crypted == null)
                        return null;
                    probed = prov.GetDecryptedKey(crypted, lupn);
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
