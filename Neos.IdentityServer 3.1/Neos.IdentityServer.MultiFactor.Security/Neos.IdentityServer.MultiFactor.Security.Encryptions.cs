using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Neos.IdentityServer.MultiFactor
{
    #region AES System classes
    /// <summary>
    /// AESSystemBaseEncryption class
    /// </summary>
    public abstract class AESBaseEncryption
    {
        public abstract string Encrypt(string data);
        public abstract string Decrypt(string data);
        public abstract byte[] Encrypt(byte[] data);
        public abstract byte[] Decrypt(byte[] data);
    }


    /// <summary>
    /// AESSystemBaseEncryption class
    /// </summary>
    public abstract class AESSystemBaseEncryption : AESBaseEncryption
    {
        internal abstract byte[] GetHeader(byte[] data);
    }

    /// <summary>
    /// AESEncryption class
    /// </summary>
    public class AESSystemEncryption : AESSystemBaseEncryption, IDisposable
    {
        /// <summary>
        /// Encrypt method implementation
        /// </summary>
        public override string Encrypt(string data)
        {
            try
            {
                using (AES256SystemEncryption enc = new AES256SystemEncryption())
                {
                    return enc.Encrypt(data);
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
        public override byte[] Encrypt(byte[] data)
        {
            try
            {
                using (AES256SystemEncryption enc = new AES256SystemEncryption())
                {
                    return enc.Encrypt(data);
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
        public override string Decrypt(string data)
        {
            try
            {
                byte[] Hdr = GetHeader(Convert.FromBase64String(data));
                if (Hdr.SequenceEqual(new byte[] { 0x17, 0xD3, 0xF4, 0x2A }))
                {
                    using (AES256SystemEncryption enc = new AES256SystemEncryption())
                    {
                        return enc.Decrypt(data);
                    }
                }
                else if (Hdr.SequenceEqual(new byte[] { 0x17, 0xD3, 0xF4, 0x29 })) // For compatibility Only
                {
                    using (AES128SystemEncryption enc = new AES128SystemEncryption())
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
        /// Decrypt method implementation
        /// </summary>
        public override byte[] Decrypt(byte[] data)
        {
            try
            {
                byte[] Hdr = GetHeader(data);
                if (Hdr.SequenceEqual(new byte[] { 0x17, 0xD3, 0xF4, 0x2A }))
                {
                    using (AES256SystemEncryption enc = new AES256SystemEncryption())
                    {
                        return enc.Decrypt(data);
                    }
                }
                else if (Hdr.SequenceEqual(new byte[] { 0x17, 0xD3, 0xF4, 0x29 })) // For compatibilty Only
                {
                    using (AES128SystemEncryption enc = new AES128SystemEncryption())
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
        /// GetHeader method implementation
        /// </summary>
        internal override byte[] GetHeader(byte[] data)
        {
            byte[] Header = new byte[4];
            Buffer.BlockCopy(data, 0, Header, 0, 4);
            return Header;
        }

        /// <summary>
        /// Dispose method implementation
        /// </summary>
        public void Dispose()
        {

        }
    }

    /// <summary>
    /// AES128SystemEncryption class
    /// </summary>
    internal class AES128SystemEncryption : AESSystemBaseEncryption, IDisposable
    {
        private readonly byte[] IV = { 113, 23, 93, 174, 155, 66, 179, 82, 90, 101, 110, 102, 213, 124, 51, 62 };
        private readonly byte[] Hdr = { 0x17, 0xD3, 0xF4, 0x29 };
        private readonly byte[] AESKey;
        private readonly string UtilsKey = "ABCDEFGHIJKLMNOP";

        /// <summary>
        /// AESEncryption constructor
        /// </summary>
        public AES128SystemEncryption()
        {
            string basestr = UtilsKey;
            AESKey = Encoding.ASCII.GetBytes(basestr.ToCharArray(), 0, 16);
        }

        /// <summary>
        /// Encrypt method implementation
        /// </summary>
        public override string Encrypt(string data)
        {
            if (string.IsNullOrEmpty(data))
                return data;
            try
            {
                if (IsEncrypted(data))
                    return data;
                byte[] encrypted = EncryptStringToBytes(data, AESKey, IV);
                return Convert.ToBase64String(AddHeader(encrypted));
            }
            catch
            {
                return data;
            }
        }

        /// <summary>
        /// Decrypt method implementation
        /// </summary>
        public override string Decrypt(string data)
        {
            if (string.IsNullOrEmpty(data))
                return data;
            try
            {
                if (!IsEncrypted(data))
                    return data;
                byte[] encrypted = Convert.FromBase64String(data);
                return DecryptStringFromBytes(RemoveHeader(encrypted), AESKey, IV);
            }
            catch
            {
                return data;
            }
        }

        /// <summary>
        /// Encrypt method implementation
        /// </summary>
        public override byte[] Encrypt(byte[] data)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Decrypt method implementation
        /// </summary>
        public override byte[] Decrypt(byte[] data)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// EncryptStringToBytes method implementation
        /// </summary>
        private byte[] EncryptStringToBytes(string plainText, byte[] Key, byte[] IV)
        {
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");

            byte[] encrypted;
            using (AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }
            return encrypted;
        }

        /// <summary>
        /// DecryptStringFromBytes method implementation
        /// </summary>
        private string DecryptStringFromBytes(byte[] cipherText, byte[] Key, byte[] IV)
        {
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");

            string plaintext = null;
            using (AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
            return plaintext;
        }

        /// <summary>
        /// IsEncrypted method implementation
        /// </summary>
        private bool IsEncrypted(string data)
        {
            if (string.IsNullOrEmpty(data))
                return false;
            try
            {
                byte[] encrypted = Convert.FromBase64String(data);
                byte[] ProofHeader = GetHeader(encrypted);
                UInt16 l = GetHeaderLen(encrypted);
                return ((l == encrypted.Length - 5) && (ProofHeader.SequenceEqual(Hdr)));
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// AddHeader method
        /// </summary>
        private byte[] AddHeader(byte[] data)
        {
            byte[] output = new byte[data.Length + 5];
            Buffer.BlockCopy(Hdr, 0, output, 0, 4);
            output[4] = Convert.ToByte(data.Length);
            Buffer.BlockCopy(data, 0, output, 5, data.Length);
            return output;
        }

        /// <summary>
        /// RemoveHeader method
        /// </summary>
        private byte[] RemoveHeader(byte[] data)
        {
            byte[] output = new byte[data.Length - 5];
            Buffer.BlockCopy(data, 5, output, 0, data.Length - 5);
            return output;
        }

        /// <summary>
        /// GetProofHeader method
        /// </summary>
        internal override byte[] GetHeader(byte[] data)
        {
            byte[] Header = new byte[4];
            Buffer.BlockCopy(data, 0, Header, 0, 4);
            return Header;
        }

        /// <summary>
        /// GetLen method
        /// </summary>
        private UInt16 GetHeaderLen(byte[] data)
        {
            return Convert.ToUInt16(data[4]);
        }

        /// <summary>
        /// Dispose method implementation
        /// </summary>
        public void Dispose()
        {

        }
    }

    /// <summary>
    /// AES256SystemEncryption class
    /// </summary>
    internal class AES256SystemEncryption : AESSystemBaseEncryption, IDisposable
    {
        private readonly byte[] AESIV = { 113, 23, 93, 113, 53, 66, 189, 82, 90, 101, 110, 102, 213, 224, 51, 62 };
        private readonly byte[] AESHdr = { 0x17, 0xD3, 0xF4, 0x2A };
        private readonly byte[] AESKey;

        /// <summary>
        /// AESEncryption constructor
        /// </summary>
        public AES256SystemEncryption()
        {
            byte[] xkey = SystemUtilities.Key;
            AESKey = new byte[32];
            Buffer.BlockCopy(xkey, 0, AESKey, 0, 32);
        }

        /// <summary>
        /// Encrypt method implementation
        /// </summary>
        public override string Encrypt(string plainText)
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
                    aesAlg.BlockSize = 128;
                    aesAlg.KeySize = 256;
                    aesAlg.Key = AESKey;
                    aesAlg.IV = AESIV;
                    aesAlg.Mode = CipherMode.CBC;
                    aesAlg.Padding = PaddingMode.PKCS7;
                    using (ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV))
                    {
                        encrypted = encryptor.TransformFinalBlock(unencrypted, 0, unencrypted.Length);
                    }
                }
                return Convert.ToBase64String(AddHeader(encrypted));
            }
            catch
            {
                return plainText;
            }
        }

        /// <summary>
        /// Encrypt method implementation
        /// </summary>
        public override byte[] Encrypt(byte[] unencrypted)
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
                    aesAlg.BlockSize = 128;
                    aesAlg.KeySize = 256;
                    aesAlg.Key = AESKey;
                    aesAlg.IV = AESIV;
                    aesAlg.Mode = CipherMode.CBC;
                    aesAlg.Padding = PaddingMode.PKCS7;
                    using (ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV))
                    {
                        encrypted = encryptor.TransformFinalBlock(unencrypted, 0, unencrypted.Length);
                    }
                }
                return AddHeader(encrypted);
            }
            catch
            {
                return unencrypted;
            }
        }

        /// <summary>
        /// Decrypt method implementation
        /// </summary>
        public override string Decrypt(string cipherText)
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
                    aesAlg.BlockSize = 128;
                    aesAlg.KeySize = 256;
                    aesAlg.Key = AESKey;
                    aesAlg.IV = AESIV;
                    aesAlg.Mode = CipherMode.CBC;
                    aesAlg.Padding = PaddingMode.PKCS7;
                    using (ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV))
                    {
                        unencrypted = decryptor.TransformFinalBlock(encrypted, 0, encrypted.Length);
                    }
                }
                return Encoding.Unicode.GetString(unencrypted);
            }
            catch
            {
                return cipherText;
            }
        }

        /// <summary>
        /// Decrypt method implementation
        /// </summary>
        public override byte[] Decrypt(byte[] cipherData)
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
                    aesAlg.BlockSize = 128;
                    aesAlg.KeySize = 256;
                    aesAlg.Key = AESKey;
                    aesAlg.IV = AESIV;
                    aesAlg.Mode = CipherMode.CBC;
                    aesAlg.Padding = PaddingMode.PKCS7;
                    using (ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV))
                    {
                        unencrypted = decryptor.TransformFinalBlock(encrypted, 0, encrypted.Length);
                    }
                }
                return unencrypted;
            }
            catch
            {
                return cipherData;
            }
        }

        /// <summary>
        /// IsEncrypted method implementation
        /// </summary>
        private bool IsEncrypted(string data)
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
        private bool IsEncrypted(byte[] encrypted)
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
        private byte[] AddHeader(byte[] data)
        {
            byte[] output = new byte[data.Length + 4];
            Buffer.BlockCopy(AESHdr, 0, output, 0, 4);
            Buffer.BlockCopy(data, 0, output, 4, data.Length);
            return output;
        }

        /// <summary>
        /// RemoveHeader method
        /// </summary>
        private byte[] RemoveHeader(byte[] data)
        {
            byte[] output = new byte[data.Length - 4];
            Buffer.BlockCopy(data, 4, output, 0, data.Length - 4);
            return output;
        }

        /// <summary>
        /// GetHeader method
        /// </summary>
        internal override byte[] GetHeader(byte[] data)
        {
            byte[] Header = new byte[4];
            Buffer.BlockCopy(data, 0, Header, 0, 4);
            return Header;
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
