//******************************************************************************************************************************************************************************************//
// Copyright (c) 2022 @redhook62 (adfsmfa@gmail.com)                                                                                                                                        //                        
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
//                                                                                                                                                                                          //
// https://github.com/neos-sdi/adfsmfa                                                                                                                                                      //
//                                                                                                                                                                                          //
//******************************************************************************************************************************************************************************************//
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Neos.IdentityServer.MultiFactor
{
    #region System encrytion classes
    /// <summary>
    /// SystemBaseEncryption class
    /// </summary>
    public abstract class AbstractSystemBaseEncryption
    {
        public CngProvider KeyStorageProvider { get => CngProvider.MicrosoftSoftwareKeyStorageProvider; }

        public abstract string Encrypt(string data, string description = "");
        public abstract string Decrypt(string data, string description = "");
        public abstract byte[] Encrypt(byte[] data, string description = "");
        public abstract byte[] Decrypt(byte[] data, string description = "");
        /// <summary>
        /// GetHeader method
        /// </summary>
        protected virtual byte[] GetHeader(byte[] data)
        {
            byte[] Header = new byte[4];
            Buffer.BlockCopy(data, 0, Header, 0, 4);
            return Header;
        }
    }

    /// <summary>
    /// SystemBaseEncryption class
    /// </summary>
    public abstract class SystemBaseEncryption: AbstractSystemBaseEncryption
    {
        protected abstract bool IsEncrypted(string data);
        protected abstract bool IsEncrypted(byte[] encrypted);
        protected abstract byte[] AddHeader(byte[] data);
        protected abstract byte[] RemoveHeader(byte[] data);
    }

    /// <summary>
    /// AESEncryption class
    /// </summary>
    public class SystemEncryption : AbstractSystemBaseEncryption, IDisposable
    {
        /// <summary>
        /// Encrypt method implementation
        /// </summary>
        public override string Encrypt(string data, string description = "")
        {
            try
            {
                if (CngKey.Exists(SystemUtilities.SystemKeyName, KeyStorageProvider, CngKeyOpenOptions.MachineKey))
                {
                    using (RSASystemEncryption enc = new RSASystemEncryption())
                    {
                        return enc.Encrypt(data, description);
                    }
                }
                else
                {
                    using (AESSystemEncryption enc = new AESSystemEncryption())
                    {
                        return enc.Encrypt(data, description);
                    }
                }
            }
            catch
            {
                return data;
            }

        }

        /// <summary>
        /// Encrypt method implementation
        /// </summary>
        public override byte[] Encrypt(byte[] data, string description = "")
        {
            try
            {
                if (CngKey.Exists(SystemUtilities.SystemKeyName, KeyStorageProvider, CngKeyOpenOptions.MachineKey))
                {
                    using (RSASystemEncryption enc = new RSASystemEncryption())
                    {
                        return enc.Encrypt(data, description);
                    }
                }
                else
                {
                    using (AESSystemEncryption enc = new AESSystemEncryption())
                    {
                        return enc.Encrypt(data, description);
                    }
                }
            }
            catch
            {
                return data;
            }
        }

        /// <summary>
        /// Decrypt method implementation
        /// </summary>
        public override string Decrypt(string data, string description = "")
        {
            try
            {
                byte[] Hdr = GetHeader(Convert.FromBase64String(data));
                if (Hdr.SequenceEqual(new byte[] { 0x17, 0xD3, 0xF4, 0x2B }))  // RSA
                {
                    using (RSASystemEncryption enc = new RSASystemEncryption())
                    {
                        return enc.Decrypt(data, description);
                    }
                }
                else if (Hdr.SequenceEqual(new byte[] { 0x17, 0xD3, 0xF4, 0x2A }))   // AES256
                {
                    using (AESSystemEncryption enc = new AESSystemEncryption())
                    {
                        return enc.Decrypt(data, description);
                    }
                }
                else
                    return data;
            }
            catch
            {
                return data;
            }
        }

        /// <summary>
        /// Decrypt method implementation
        /// </summary>
        public override byte[] Decrypt(byte[] data, string description = "")
        {
            try
            {
                byte[] Hdr = GetHeader(data);
                if (Hdr.SequenceEqual(new byte[] { 0x17, 0xD3, 0xF4, 0x2B }))  // RSA
                {
                    using (RSASystemEncryption enc = new RSASystemEncryption())
                    {
                        return enc.Decrypt(data);
                    }
                }
                else if (Hdr.SequenceEqual(new byte[] { 0x17, 0xD3, 0xF4, 0x2A }))  // AES256
                {
                    using (AESSystemEncryption enc = new AESSystemEncryption())
                    {
                        return enc.Decrypt(data);
                    }
                }
                else
                    return data;
            }
            catch
            {
                return data;
            }
        }

        /// <summary>
        /// Dispose method implementation
        /// </summary>
        public void Dispose()
        {

        }
    }

    /// <summary>
    /// AESSystemEncryption class
    /// </summary>
    internal class AESSystemEncryption : SystemBaseEncryption, IDisposable
    {
        private readonly byte[] AESIV = { 113, 23, 93, 113, 53, 66, 189, 82, 90, 101, 110, 102, 213, 224, 51, 62 };
        private readonly byte[] AESHdr = { 0x17, 0xD3, 0xF4, 0x2A };
        private byte[] _aeskey = null;

        /// <summary>
        /// AESEncryption constructor
        /// </summary>
        public AESSystemEncryption(byte[] keytouse = null)
        {
            byte[] xkey = null;
            if (keytouse != null)
                xkey = keytouse;
            else
                xkey = SystemUtilities.SystemKey;
            _aeskey = new byte[32];
            Buffer.BlockCopy(xkey, 0, _aeskey, 0, 32);
        }

        /// <summary>
        /// AESKey property implmentation
        /// </summary>
        public byte[] AESKey
        {
            get { return _aeskey; }
            set
            {
                _aeskey = new byte[32];
                Buffer.BlockCopy(value, 0, _aeskey, 0, 32);
            }
        }

        /// <summary>
        /// Encrypt method implementation
        /// </summary>
        public override string Encrypt(string plainText, string description = "")
        {
            if (string.IsNullOrEmpty(plainText))
                return plainText;
            if (IsEncrypted(plainText))
                return plainText;
            try
            {
                byte[] encrypted;
                byte[] unencrypted = Encoding.Unicode.GetBytes(plainText);
                using (AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider())
                {
                    aesAlg.Mode = CipherMode.CBC;
                    aesAlg.KeySize = 256;
                    aesAlg.BlockSize = 128;
                    aesAlg.FeedbackSize = 128;
                    aesAlg.Padding = PaddingMode.PKCS7;
                    aesAlg.Key = AESKey;
                    aesAlg.IV = AESIV;

                    using (ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV))
                    {
                        encrypted = encryptor.TransformFinalBlock(unencrypted, 0, unencrypted.Length);
                    }
                }
                return Convert.ToBase64String(AddHeader(encrypted));
            }
            catch (Exception Ex)
            {
                if (!string.IsNullOrEmpty(description))
                    Log.WriteEntry(string.Format("Error encrypting value for {0} : {1}", description, Ex.Message), System.Diagnostics.EventLogEntryType.Error, 666);
                return plainText;
            }
        }

        /// <summary>
        /// Encrypt method implementation
        /// </summary>
        public override byte[] Encrypt(byte[] unencrypted, string description = "")
        {
            if (unencrypted == null || unencrypted.Length <= 0)
                throw new ArgumentNullException("unencrypted");
            if (IsEncrypted(unencrypted))
                return unencrypted;
            try
            {
                byte[] encrypted;
                using (AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider())
                {
                    aesAlg.Mode = CipherMode.CBC;
                    aesAlg.KeySize = 256;
                    aesAlg.BlockSize = 128;
                    aesAlg.FeedbackSize = 128;
                    aesAlg.Padding = PaddingMode.PKCS7;
                    aesAlg.Key = AESKey;
                    aesAlg.IV = AESIV;
                    using (ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV))
                    {
                        encrypted = encryptor.TransformFinalBlock(unencrypted, 0, unencrypted.Length);
                    }
                }
                return AddHeader(encrypted);
            }
            catch (Exception Ex)
            {
                if (!string.IsNullOrEmpty(description))
                    Log.WriteEntry(string.Format("Error encrypting value for {0} : {1}", description, Ex.Message), System.Diagnostics.EventLogEntryType.Error, 666);
                return unencrypted;
            }
        }

        /// <summary>
        /// Decrypt method implementation
        /// </summary>
        public override string Decrypt(string cipherText, string description = "")
        {
            if (string.IsNullOrEmpty(cipherText))
                return cipherText;
            if (!IsEncrypted(cipherText))
                return cipherText;
            try
            {
                byte[] encrypted = RemoveHeader(Convert.FromBase64String(cipherText));
                byte[] unencrypted = null;
                using (AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider())
                {
                    aesAlg.Mode = CipherMode.CBC;
                    aesAlg.KeySize = 256;
                    aesAlg.BlockSize = 128;
                    aesAlg.FeedbackSize = 128;
                    aesAlg.Padding = PaddingMode.PKCS7;
                    aesAlg.Key = AESKey;
                    aesAlg.IV = AESIV;
                    using (ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV))
                    {
                        unencrypted = decryptor.TransformFinalBlock(encrypted, 0, encrypted.Length);
                    }
                }
                return Encoding.Unicode.GetString(unencrypted);
            }
            catch (Exception Ex)
            {
                if (!string.IsNullOrEmpty(description))
                    Log.WriteEntry(string.Format("Error decrypting value for {0} : {1}", description, Ex.Message), System.Diagnostics.EventLogEntryType.Error, 666);
                return cipherText;
            }
        }

        /// <summary>
        /// Decrypt method implementation
        /// </summary>
        public override byte[] Decrypt(byte[] cipherData, string description = "")
        {
            if (cipherData == null || cipherData.Length <= 0)
                throw new ArgumentNullException("cipherData");
            if (!IsEncrypted(cipherData))
                return cipherData;
            try
            {
                byte[] unencrypted = null;
                byte[] encrypted = RemoveHeader(cipherData);
                using (AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider())
                {
                    aesAlg.Mode = CipherMode.CBC;
                    aesAlg.KeySize = 256;
                    aesAlg.BlockSize = 128;
                    aesAlg.FeedbackSize = 128;
                    aesAlg.Padding = PaddingMode.PKCS7;
                    aesAlg.Key = AESKey;
                    aesAlg.IV = AESIV;
                    using (ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV))
                    {
                        unencrypted = decryptor.TransformFinalBlock(encrypted, 0, encrypted.Length);
                    }
                }
                return unencrypted;
            }
            catch (Exception Ex)
            {
                if (!string.IsNullOrEmpty(description))
                    Log.WriteEntry(string.Format("Error decrypting value for {0} : {1}", description, Ex.Message), System.Diagnostics.EventLogEntryType.Error, 666);
                return cipherData;
            }
        }

        /// <summary>
        /// IsEncrypted method implementation
        /// </summary>
        protected override bool IsEncrypted(string data)
        {
            if (string.IsNullOrEmpty(data))
                return false;
            try
            {
                byte[] encrypted = Convert.FromBase64String(data);
                byte[] ProofHeader = GetHeader(encrypted);
                return ProofHeader.SequenceEqual(AESHdr);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// IsEncrypted method implementation
        /// </summary>
        protected override bool IsEncrypted(byte[] encrypted)
        {
            try
            {
                byte[] ProofHeader = GetHeader(encrypted);
                return ProofHeader.SequenceEqual(AESHdr);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// AddHeader method
        /// </summary>
        protected override byte[] AddHeader(byte[] data)
        {
            byte[] output = new byte[data.Length + 4];
            Buffer.BlockCopy(AESHdr, 0, output, 0, 4);
            Buffer.BlockCopy(data, 0, output, 4, data.Length);
            return output;
        }

        /// <summary>
        /// RemoveHeader method
        /// </summary>
        protected override byte[] RemoveHeader(byte[] data)
        {
            byte[] output = new byte[data.Length - 4];
            Buffer.BlockCopy(data, 4, output, 0, data.Length - 4);
            return output;
        }

        /// <summary>
        /// Dispose method implementation
        /// </summary>
        public void Dispose()
        {

        }
    }

    /// <summary>
    /// RSASystemEncryption class
    /// </summary>
    internal class RSASystemEncryption : SystemBaseEncryption, IDisposable
    {
        private readonly byte[] header = { 0x17, 0xD3, 0xF4, 0x2B };

        /// <summary>
        /// RSASystemEncryption constructor
        /// </summary>
        public RSASystemEncryption():base()
        {
        }

        /// <summary>
        /// Encrypt method implementation
        /// </summary>
        public override string Encrypt(string plainText, string description = "")
        {
            if (string.IsNullOrEmpty(plainText))
                return plainText;
            if (IsEncrypted(plainText))
                return plainText;
            if (!CngKey.Exists(SystemUtilities.SystemKeyName, KeyStorageProvider, CngKeyOpenOptions.MachineKey))
                throw new CryptographicException(string.Format("Error : key {0} doesn't exist...", SystemUtilities.SystemKeyName));
            try
            {
                byte[] encrypted;
                byte[] unencrypted = Encoding.Unicode.GetBytes(plainText);
                CngKey cngkey = CngKey.Open(SystemUtilities.SystemKeyName, KeyStorageProvider, CngKeyOpenOptions.MachineKey);
                using (RSACng rsa = new RSACng(cngkey))
                {
                    encrypted = rsa.Encrypt(unencrypted, RSAEncryptionPadding.OaepSHA256);
                    return Convert.ToBase64String(AddHeader(encrypted));
                }
            }
            catch (Exception Ex)
            {
                if (!string.IsNullOrEmpty(description))
                    Log.WriteEntry(string.Format("Error encrypting value for {0} : {1}", description, Ex.Message), System.Diagnostics.EventLogEntryType.Error, 666);
                return plainText;
            }
        }

        /// <summary>
        /// Encrypt method implementation
        /// </summary>
        public override byte[] Encrypt(byte[] unencrypted, string description = "")
        {
            if (unencrypted == null || unencrypted.Length <= 0)
                throw new ArgumentNullException("unencrypted");
            if (IsEncrypted(unencrypted))
                return unencrypted;
            if (!CngKey.Exists(SystemUtilities.SystemKeyName, KeyStorageProvider, CngKeyOpenOptions.MachineKey))
               throw new CryptographicException(string.Format("Error : key {0} doesn't exist...", SystemUtilities.SystemKeyName));
            try
            {
                byte[] encrypted;
                CngKey cngkey = CngKey.Open(SystemUtilities.SystemKeyName, KeyStorageProvider, CngKeyOpenOptions.MachineKey);
                using (RSACng rsa = new RSACng(cngkey))
                {
                    encrypted = rsa.Encrypt(unencrypted, RSAEncryptionPadding.OaepSHA256);
                    return AddHeader(encrypted);
                }
            }
            catch (Exception Ex)
            {
                if (!string.IsNullOrEmpty(description))
                    Log.WriteEntry(string.Format("Error encrypting value for {0} : {1}", description, Ex.Message), System.Diagnostics.EventLogEntryType.Error, 666);
                return unencrypted;
            }
        }

        /// <summary>
        /// Decrypt method implementation
        /// </summary>
        public override string Decrypt(string cipherText, string description = "")
        {
            if (string.IsNullOrEmpty(cipherText))
                return cipherText;
            if (!IsEncrypted(cipherText))
                return cipherText;
            try
            {
                byte[] encrypted = RemoveHeader(Convert.FromBase64String(cipherText));
                byte[] unencrypted = null;
                CngKey cngkey = CngKey.Open(SystemUtilities.SystemKeyName, KeyStorageProvider, CngKeyOpenOptions.MachineKey);
                using (RSACng aes = new RSACng(cngkey))
                {
                    unencrypted = aes.Decrypt(encrypted, RSAEncryptionPadding.OaepSHA256);
                    return Encoding.Unicode.GetString(unencrypted);
                }
            }
            catch (Exception Ex)
            {
                if (!string.IsNullOrEmpty(description))
                    Log.WriteEntry(string.Format("Error decrypting value for {0} : {1}", description, Ex.Message), System.Diagnostics.EventLogEntryType.Error, 666);
                return cipherText;
            }
        }

        /// <summary>
        /// Decrypt method implementation
        /// </summary>
        public override byte[] Decrypt(byte[] cipherData, string description = "")
        {
            if (cipherData == null || cipherData.Length <= 0)
                throw new ArgumentNullException("cipherData");
            if (!IsEncrypted(cipherData))
                return cipherData;
            if (!CngKey.Exists(SystemUtilities.SystemKeyName, KeyStorageProvider, CngKeyOpenOptions.MachineKey))
                throw new CryptographicException(string.Format("Error : key {0} doesn't exist...", SystemUtilities.SystemKeyName));
            try
            {
                byte[] unencrypted = null;
                byte[] encrypted = RemoveHeader(cipherData);
                CngKey cngkey = CngKey.Open(SystemUtilities.SystemKeyName, KeyStorageProvider, CngKeyOpenOptions.MachineKey);
                using (RSACng aes = new RSACng(cngkey))
                {
                    unencrypted = aes.Decrypt(encrypted, RSAEncryptionPadding.OaepSHA256);
                    return unencrypted;
                }
            }
            catch (Exception Ex)
            {
                if (!string.IsNullOrEmpty(description))
                    Log.WriteEntry(string.Format("Error decrypting value for {0} : {1}", description, Ex.Message), System.Diagnostics.EventLogEntryType.Error, 666);
                return cipherData;
            }
        }

        /// <summary>
        /// IsEncrypted method implementation
        /// </summary>
        protected override bool IsEncrypted(string data)
        {
            if (string.IsNullOrEmpty(data))
                return false;
            try
            {
                byte[] encrypted = Convert.FromBase64String(data);
                byte[] ProofHeader = GetHeader(encrypted);
                return ProofHeader.SequenceEqual(header);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// IsEncrypted method implementation
        /// </summary>
        protected override bool IsEncrypted(byte[] encrypted)
        {
            try
            {
                byte[] ProofHeader = GetHeader(encrypted);
                return ProofHeader.SequenceEqual(header);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// AddHeader method
        /// </summary>
        protected override byte[] AddHeader(byte[] data)
        {
            byte[] output = new byte[data.Length + 4];
            Buffer.BlockCopy(header, 0, output, 0, 4);
            Buffer.BlockCopy(data, 0, output, 4, data.Length);
            return output;
        }

        /// <summary>
        /// RemoveHeader method
        /// </summary>
        protected override byte[] RemoveHeader(byte[] data)
        {
            byte[] output = new byte[data.Length - 4];
            Buffer.BlockCopy(data, 4, output, 0, data.Length - 4);
            return output;
        }

        /// <summary>
        /// Dispose method implementation
        /// </summary>
        public void Dispose()
        {

        }
    }

    #endregion
}
