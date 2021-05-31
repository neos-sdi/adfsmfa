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
// Authors : Asterix & Obelix                                                                                                                                                               //
//                                                                                                                                                                                          //
//******************************************************************************************************************************************************************************************//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Neos.IdentityServer.MultiFactor.Data;
using System.Diagnostics;
using System.Security.Cryptography;

namespace Neos.IdentityServer.MultiFactor.Samples
{
    /// <summary>
    /// CesarKeyManagerCreator class
    /// </summary>
    public class CaesarKeyManagerActivator : ISecretKeyManagerActivator
    {
        /// <summary>
        /// CreateKeyManager method
        /// </summary>
        public ISecretKeyManager CreateInstance(SecretKeyVersion version)
        {
            if (version == SecretKeyVersion.V1)
                return new CaesarKeyManager.CaesarKeyManagerV1();
            else
                return new CaesarKeyManager.CaesarKeyManagerV2();
        }
    }

    /// <summary>
    /// CesarKeyManager class
    /// </summary>
    internal abstract class CaesarKeyManager : ISecretKeyManager
    {
        private int MAX_PROBE_LEN = 0;

        /// <summary>
        /// CesarKeyManager constructor
        /// limit creation by KeyManager
        /// </summary>
        protected CaesarKeyManager()
        {
            Trace.TraceInformation("CesarKeyManager()");
        }

        /// <summary>
        /// KeysStorage property
        /// </summary>
        public KeysRepositoryService KeysStorage { get; private set; } = null;

        /// <summary>
        /// XORSecret property
        /// </summary>
        public string XORSecret { get; private set; } = XORUtilities.XORKey;

        /// <summary>
        /// CustomParameters property 
        /// </summary>
        private string CustomParameters { get; set; }

        /// <summary>
        /// Prefix property
        /// </summary>
        public string Prefix
        {
            get { return "caesar://"; }
        }

        /// <summary>
        /// Initialize method implementation
        /// </summary>
        public void Initialize(KeysRepositoryService keysstorage,  BaseKeysManagerParams parameters)
        {
            CustomKeysManagerParams config = (CustomKeysManagerParams)parameters;
            KeysStorage = keysstorage;
            XORSecret = config.XORSecret;
            CustomParameters = config.CustomParameters;
            MAX_PROBE_LEN = 64;
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

        #region CaesarKeyManagerV1
        internal class CaesarKeyManagerV1 : CaesarKeyManager
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
                using (var prov = new CaesarEncryption1(XORSecret))
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
                using (var prov = new CaesarEncryption1(XORSecret))
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
                    using (var prov = new CaesarEncryption1(XORSecret))
                    {
                        byte[] crypted = StripStorageInfos(key);
                        if (crypted == null)
                            return false;
                        byte[] cleared = prov.GetDecryptedKey(crypted, lupn);

                        if (cleared == null)
                            return false; // Key corrupted
                        else
                            return true;
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
                using (var prov = new CaesarEncryption1(XORSecret))
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

        #region CaesarKeyManagerV2
        internal class CaesarKeyManagerV2 : CaesarKeyManager
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
                using (var prov = new CaesarEncryption2(XORSecret))
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
                using (var prov = new CaesarEncryption2(XORSecret))
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
                    using (var prov = new CaesarEncryption2(XORSecret))
                    {
                        byte[] crypted = StripStorageInfos(key);
                        if (crypted == null)
                            return false;
                        byte[] cleared = prov.GetDecryptedKey(crypted, lupn);

                        if (cleared == null)
                            return false; // Key corrupted
                        else
                            return true;
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
                using (var prov = new CaesarEncryption2(XORSecret))
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

    /// <summary>
    /// CesarEncryption1 class
    /// </summary>
    internal class CaesarEncryption1 : BaseEncryption
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public CaesarEncryption1(string xorsecret) : base(xorsecret)
        {
        }

        /// <summary>
        /// NewEncryptedKey method implementation
        /// </summary>
        public override byte[] NewEncryptedKey(string username)
        {
            try
            {
                if (string.IsNullOrEmpty(username))
                    throw new Exception("Invalid encryption context !");
                byte[] plainBytes = GenerateKey();
                return Caesar(XORUtilities.XOREncryptOrDecrypt(plainBytes, this.XORSecret), +3);
            }
            catch (CryptographicException ce)
            {
                Log.WriteEntry(string.Format("(CesarEncryption1 Encrypt) : Crytographic error for user  {1} \r {0} \r {2}", ce.Message, username, ce.StackTrace), System.Diagnostics.EventLogEntryType.Error, 0000);
                return null;
            }
            catch (Exception ex)
            {
                Log.WriteEntry(string.Format("(CesarEncryption1 Encrypt) : Encryption error for user  {1} \r {0} \r {2}", ex.Message, username, ex.StackTrace), System.Diagnostics.EventLogEntryType.Error, 0000);
                return null;
            }
        }

        /// <summary>
        /// GetDecryptedKey method implementation
        /// </summary>
        public override byte[] GetDecryptedKey(byte[] encryptedBytes, string username)
        {
            try
            {
                if (encryptedBytes == null)
                    throw new Exception("Invalid decryption context !");
                return Caesar(XORUtilities.XOREncryptOrDecrypt(encryptedBytes, this.XORSecret), -3);
            }
            catch (CryptographicException ce)
            {
                Log.WriteEntry(string.Format("(CesarEncryption1 Decrypt) : Crytographic error for user  {1} \r {0} \r {2}", ce.Message, username, ce.StackTrace), System.Diagnostics.EventLogEntryType.Error, 0000);
                return null;
            }
            catch (Exception ex)
            {
                Log.WriteEntry(string.Format("(CesarEncryption1 Decrypt) : Decryption error for user  {1} \r {0} \r {2}", ex.Message, username, ex.StackTrace), System.Diagnostics.EventLogEntryType.Error, 0000);
                return null;
            }
        }

        /// <summary>
        /// GenerateKey method
        /// </summary>
        private byte[] GenerateKey()
        {
            byte[] buffer = new byte[16];
            RandomNumberGenerator cryptoRandomDataGenerator = new RNGCryptoServiceProvider();
            cryptoRandomDataGenerator.GetBytes(buffer, 0, 16);
            return buffer;
        }

        /// <summary>
        /// Caesar method implmentation
        /// </summary>
        protected byte[] Caesar(byte[] key, int shift)
        {
            for (int i = 0; i < key.Length; i++)
            {
                int val = key[i];
                if (shift > 0)
                    val = (val + shift);
                else if (shift < 0)
                    val = (val - shift);
                if (val > 255)
                    val = (val - 255);
                else if (val < 0)
                    val = (val + 255);
                key[i] = (byte)val;
            }
            return key;
        }

        /// <summary>
        /// Dispose method implementation
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            // Nothing
        }
    }

    /// <summary>
    /// CaesarEncryption2 class
    /// </summary>
    internal class CaesarEncryption2 : BaseEncryption
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public CaesarEncryption2(string xorsecret) : base(xorsecret)
        {
        }

        /// <summary>
        /// NewEncryptedKey method implementation
        /// </summary>
        public override byte[] NewEncryptedKey(string username)
        {
            try
            {
                if (string.IsNullOrEmpty(username))
                    throw new Exception("Invalid encryption context !");
                byte[] plainBytes = GenerateKey();
                return Caesar(XORUtilities.XOREncryptOrDecrypt(plainBytes, this.XORSecret), +3);
            }
            catch (CryptographicException ce)
            {
                Log.WriteEntry(string.Format("(CesarEncryption2 Encrypt) : Crytographic error for user  {1} \r {0} \r {2}", ce.Message, username, ce.StackTrace), System.Diagnostics.EventLogEntryType.Error, 0000);
                return null;
            }
            catch (Exception ex)
            {
                Log.WriteEntry(string.Format("(Encryption2 Encrypt) : Encryption error for user  {1} \r {0} \r {2}", ex.Message, username, ex.StackTrace), System.Diagnostics.EventLogEntryType.Error, 0000);
                return null;
            }
        }

        /// <summary>
        /// GetDecryptedKey method implementation
        /// </summary>
        public override byte[] GetDecryptedKey(byte[] encryptedBytes, string username)
        {
            try
            {
                if (encryptedBytes == null)
                    throw new Exception("Invalid decryption context !");
                return Caesar(XORUtilities.XOREncryptOrDecrypt(encryptedBytes, this.XORSecret), -3);
            }
            catch (CryptographicException ce)
            {
                Log.WriteEntry(string.Format("(CesarEncryption2 Decrypt) : Crytographic error for user  {1} \r {0} \r {2}", ce.Message, username, ce.StackTrace), System.Diagnostics.EventLogEntryType.Error, 0000);
                return null;
            }
            catch (Exception ex)
            {
                Log.WriteEntry(string.Format("(CesarEncryption2 Decrypt) : Decryption error for user  {1} \r {0} \r {2}", ex.Message, username, ex.StackTrace), System.Diagnostics.EventLogEntryType.Error, 0000);
                return null;
            }
        }

        /// <summary>
        /// GenerateKey method
        /// </summary>
        private byte[] GenerateKey()
        {
            byte[] buffer = new byte[32];
            RandomNumberGenerator cryptoRandomDataGenerator = new RNGCryptoServiceProvider();
            cryptoRandomDataGenerator.GetBytes(buffer, 0, 32);
            return buffer;
        }

        /// <summary>
        /// Caesar method implmentation
        /// </summary>
        protected byte[] Caesar(byte[] key, int shift)
        {
            for (int i = 0; i < key.Length; i++)
            {
                int val = key[i];
                if (shift > 0)
                    val = (val + shift);
                else if (shift < 0)
                    val = (val - shift);
                if (val > 255)
                    val = (val - 255);
                else if (val < 0)
                    val = (val + 255);
                key[i] = (byte)val;
            }
            return key;
        }

        /// <summary>
        /// Dispose method implementation
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            // Nothing
        }
    }
}
