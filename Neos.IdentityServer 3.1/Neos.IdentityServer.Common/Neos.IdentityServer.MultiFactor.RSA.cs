//******************************************************************************************************************************************************************************************//
// Copyright (c) 2020 @redhook62 (adfsmfa@gmail.com)                                                                                                                                    //                        
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Neos.IdentityServer.MultiFactor.Data;
using System.Diagnostics;

namespace Neos.IdentityServer.MultiFactor.Common
{
    /// <summary>
    /// RSAKeyManagerActivator class
    /// </summary>
    internal class RSAKeyManagerActivator : ISecretKeyManagerActivator
    {
        /// <summary>
        /// CreateKeyManager method
        /// </summary>
        public ISecretKeyManager CreateInstance(SecretKeyVersion version)
        {
            if (version == SecretKeyVersion.V2)
                return new RSAKeyManager.RSAKeyManagerV2();
            else
                return new RSAKeyManager.RSAKeyManagerV1();
        }
    }

    /// <summary>
    /// RSAKeyManager class
    /// </summary>
    internal abstract class RSAKeyManager : ISecretKeyManager
    {
        private int MAX_PROBE_LEN = 0;

        /// <summary>
        /// RSAKeyManager constructor
        /// limit creation by KeyManager
        /// </summary>
        protected RSAKeyManager()
        {
            Trace.TraceInformation("RSAKeyManager()");
        }

        /// <summary>
        /// Initialize method implementation
        /// </summary>
        public void Initialize(KeysRepositoryService keysstorage, BaseKeysManagerParams parameters)
        {
            RSAKeysManagerParams config = (RSAKeysManagerParams)parameters;
            KeysStorage = keysstorage;
            XORSecret = config.XORSecret;
            CertificateThumbprint = config.CertificateThumbprint;
            KeySize = config.KeySizeMode;
            switch (KeySize)
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
        public KeysRepositoryService KeysStorage { get; private set; } = null;

        /// <summary>
        /// XORSecret property
        /// </summary>
        public string XORSecret { get; private set; } = XORUtilities.XORKey;

        /// <summary>
        /// KeySize property
        /// </summary>
        private KeySizeMode KeySize { get; set; } = KeySizeMode.KeySize1024;

        /// <summary>
        /// CertificateThumbprint property
        /// </summary>
        private string CertificateThumbprint { get; set; }

        /// <summary>
        /// Prefix property
        /// </summary>
        public string Prefix
        {
            get { return "rsa://"; }
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
        internal class RSAKeyManagerV1 : RSAKeyManager
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
                using (var prov = new RSAEncryption(XORSecret, CertificateThumbprint))
                {
                    crypted = prov.NewEncryptedKey(lupn);
                    if (crypted == null)
                        return null;
                }
                string outkey = AddKeyPrefix(System.Convert.ToBase64String(crypted));
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
                    using (var prov = new Encryption(XORSecret, CertificateThumbprint))
                    {
                        string xkey = StripKeyPrefix(key);
                        byte[] crypted = System.Convert.FromBase64CharArray(xkey.ToCharArray(), 0, xkey.Length);
                        if (crypted == null)
                            return false;
                        byte[] cleared = prov.GetDecryptedKey(crypted, lupn);

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
        internal class RSAKeyManagerV2 : RSAKeyManager
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
                using (var prov = new RSAEncryption(XORSecret, CertificateThumbprint))
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
                using (var prov = new RSAEncryption(XORSecret, CertificateThumbprint))
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
                    using (var prov = new RSAEncryption(XORSecret, CertificateThumbprint))
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
                using (var prov = new RSAEncryption(XORSecret, CertificateThumbprint))
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
