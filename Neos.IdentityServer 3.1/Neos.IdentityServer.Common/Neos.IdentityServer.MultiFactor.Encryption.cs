//******************************************************************************************************************************************************************************************//
// Copyright (c) 2022 @redhook62 (adfsmfa@gmail.com)                                                                                                                                    //                        
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
using Neos.IdentityServer.MultiFactor.Data;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Neos.IdentityServer.MultiFactor
{
    /// <summary>
    /// BaseEncryption class implmentation
    /// </summary>
    public abstract class BaseEncryption: IDisposable
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public BaseEncryption(string xorsecret)
        {
            XORSecret = xorsecret;
        }

        /// <summary>
        /// XORSecret property
        /// </summary>
        public string XORSecret { get; internal set; } = string.Empty;

        /// <summary>
        /// CheckSum property
        /// </summary>
        public byte[] CheckSum { get; internal set; }

        /// <summary>
        /// Certificate property
        /// </summary>
        public X509Certificate2 Certificate { get; set; } = null;

        public abstract byte[] NewEncryptedKey(string username);
        public abstract byte[] GetDecryptedKey(byte[] encrypted, string username);
        protected abstract void Dispose(bool disposing);

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
        public override byte[] NewEncryptedKey(string username)
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
            catch (CryptographicException ce)
            {
                Log.WriteEntry(string.Format("(Encryption) : Crytographic error for user {1} \r {0} \r {2}", ce.Message, username, ce.StackTrace), System.Diagnostics.EventLogEntryType.Error, 0000);
                return null;
            }
            catch (Exception ex)
            {
                Log.WriteEntry(string.Format("(Encryption) : Encryption error for user {1} \r {0} \r {2}", ex.Message, username, ex.StackTrace), System.Diagnostics.EventLogEntryType.Error, 0000);
                return null;
            }
        }

        /// <summary>
        /// Decrypt method
        /// </summary>
        public override byte[] GetDecryptedKey(byte[] encryptedBytes, string username)
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
                this.CheckSum = CheckSumEncoding.CheckSum(outval); 
                return bytes;
            }
            catch (CryptographicException ce)
            {
                Log.WriteEntry(string.Format("(Encryption) : Crytographic error for user {1} \r {0} \r {2}", ce.Message, username, ce.StackTrace), System.Diagnostics.EventLogEntryType.Error, 0000);
                return null;
            }
            catch (Exception ex)
            {
                Log.WriteEntry(string.Format("(Encryption) : Decryptionc error for user {1} \r {0} \r {2}", ex.Message, username, ex.StackTrace), System.Diagnostics.EventLogEntryType.Error, 0000);
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
        private readonly int datalen = 128;

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
        public override byte[] NewEncryptedKey(string username)
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

                return XORUtilities.XOREncryptOrDecrypt(encryptedBytes, this.XORSecret);
            }
            catch (CryptographicException ce)
            {
                Log.WriteEntry(string.Format("(RSAEncryption Encrypt) : Crytographic error for user  {1} \r {0} \r {2}", ce.Message, username, ce.StackTrace), System.Diagnostics.EventLogEntryType.Error, 0000);
                return null;
            }
            catch (Exception ex)
            {
                Log.WriteEntry(string.Format("(RSAEncryption Encrypt) : Encryption error for user  {1} \r {0} \r {2}", ex.Message, username, ex.StackTrace), System.Diagnostics.EventLogEntryType.Error, 0000);
                return null;
            }
        }

        /// <summary>
        /// Decrypt method
        /// </summary>
        public override byte[] GetDecryptedKey(byte[] encryptedBytes, string username)
        {
            try
            {
                if (Certificate == null)
                    throw new Exception("Invalid decryption certificate !");

                byte[] decryptedBytes = XORUtilities.XOREncryptOrDecrypt(encryptedBytes, this.XORSecret);
                byte[] fulldecryptedBytes = null;

                var key = Certificate.GetRSAPrivateKey();
                if (key == null)
                    throw new CryptographicException("Invalid private Key !");

                if (key is RSACng)
                    fulldecryptedBytes = ((RSACng)key).Decrypt(decryptedBytes, RSAEncryptionPadding.OaepSHA256);
                else
                    fulldecryptedBytes = ((RSACryptoServiceProvider)key).Decrypt(decryptedBytes, true);

                byte[] userbuff = new byte[fulldecryptedBytes.Length - datalen];
                Buffer.BlockCopy(fulldecryptedBytes, datalen, userbuff, 0, fulldecryptedBytes.Length - datalen);
                this.CheckSum = userbuff;

                byte[] decryptedkey = new byte[datalen];
                Buffer.BlockCopy(fulldecryptedBytes, 0, decryptedkey, 0, datalen);
                return decryptedkey;
            }
            catch (CryptographicException ce)
            {
                Log.WriteEntry(string.Format("(RSAEncryption Decrypt) : Crytographic error for user  {1} \r {0} \r {2}", ce.Message, username, ce.StackTrace), System.Diagnostics.EventLogEntryType.Error, 0000);
                return null;
            }
            catch (Exception ex)
            {
                Log.WriteEntry(string.Format("(RSAEncryption Decrypt) : Decryption error for user  {1} \r {0} \r {2}", ex.Message, username, ex.StackTrace), System.Diagnostics.EventLogEntryType.Error, 0000);
                return null;
            }
        }

        /// <summary>
        /// GenerateKey method
        /// </summary>
        private byte[] GenerateKey(string username)
        {
            byte[] text = CheckSumEncoding.CheckSum(username);

            byte[] buffer = new byte[datalen + text.Length];
            RandomNumberGenerator cryptoRandomDataGenerator = new RNGCryptoServiceProvider();
            cryptoRandomDataGenerator.GetBytes(buffer, 0, datalen);
            Buffer.BlockCopy(text, 0, buffer, datalen, text.Length);
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
        readonly KeyGeneratorMode _mode = KeyGeneratorMode.ClientSecret128;

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
        public override byte[] NewEncryptedKey(string username)
        {
            try
            {
                if (string.IsNullOrEmpty(username))
                    throw new Exception("Invalid encryption context !");
                byte[] plainBytes = GenerateKey(username);
                return XORUtilities.XOREncryptOrDecrypt(plainBytes, this.XORSecret);
            }
            catch (CryptographicException ce)
            {
                Log.WriteEntry(string.Format("(RNGEncryption Encrypt) : Crytographic Error for user  {1} \r {0} \r {2}", ce.Message, username, ce.StackTrace), System.Diagnostics.EventLogEntryType.Error, 0000);
                return null;
            }
            catch (Exception ex)
            {
                Log.WriteEntry(string.Format("(RNGEncryption Encrypt) : Encryption error for user  {1} \r {0} \r {2}", ex.Message, username, ex.StackTrace), System.Diagnostics.EventLogEntryType.Error, 0000);
                return null;
            }
        }

        /// <summary>
        /// ExtractAndDecryptKey method
        /// </summary>
        public override byte[] GetDecryptedKey(byte[] encryptedBytes, string username)
        {
            try
            {
                if (encryptedBytes == null)
                    throw new Exception("Invalid decryption context !");

                byte[] decryptedBytes = XORUtilities.XOREncryptOrDecrypt(encryptedBytes, this.XORSecret);
                int size = GetSizeFromMode(_mode);
                byte[] userbuff = new byte[decryptedBytes.Length - size];
                Buffer.BlockCopy(decryptedBytes, size, userbuff, 0, decryptedBytes.Length - size);
                this.CheckSum = userbuff;

                byte[] decryptedkey = new byte[size];
                Buffer.BlockCopy(decryptedBytes, 0, decryptedkey, 0, size);
                return decryptedkey;
            }
            catch (CryptographicException ce)
            {
                Log.WriteEntry(string.Format("(RNGEncryption Decrypt) : Crytographic Error for user {1} \r {0} \r {2}", ce.Message, username, ce.StackTrace), System.Diagnostics.EventLogEntryType.Error, 0000);
                return null;
            }
            catch (Exception ex)
            {
                Log.WriteEntry(string.Format("(RNGEncryption Decrypt) : Decryption Error for user {1} \r {0} \r {2}", ex.Message, username, ex.StackTrace), System.Diagnostics.EventLogEntryType.Error, 0000);
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
            byte[] text = CheckSumEncoding.CheckSum(username);

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
    /// AES128Encryption class
    /// </summary>
    internal class AES128Encryption : BaseEncryption
    {
        private readonly byte[] AESIV = { 193, 213, 93, 174, 155, 166, 179, 82, 90, 181, 110, 102, 213, 124, 51, 162 };
        private readonly byte[] AESKey;

        private readonly int datalen = 128; 

        /// <summary>
        /// Constructor
        /// </summary>
        public AES128Encryption(string xorsecret):base(xorsecret)
        {
            byte[] xkey = SystemUtilities.SystemKey;
            AESKey = new byte[16];
            Buffer.BlockCopy(xkey, 0, AESKey, 0, 16);
            byte[] res = null;
            using (SHA1Managed sha = new SHA1Managed())
            {
                res = sha.ComputeHash(AESKey);
                Buffer.BlockCopy(res, 0, AESKey, 0, 16);
            }

        }

        /// <summary>
        /// NewEncryptedKey method implementation
        /// </summary>
        public override byte[] NewEncryptedKey(string username)
        {
            try
            {
                byte[] plainBytes = GenerateKey(username);
                byte[] encryptedBytes = null;
                using (AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider())
                {
                    aesAlg.BlockSize = 128;
                    aesAlg.KeySize = 128;
                    aesAlg.Key = AESKey;
                    aesAlg.IV = AESIV;
                    aesAlg.Mode = CipherMode.CBC;
                    aesAlg.Padding = PaddingMode.PKCS7;
                    using (ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV))
                    {
                        encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
                    }
                }
                return XORUtilities.XOREncryptOrDecrypt(encryptedBytes, this.XORSecret);
            }
            catch (CryptographicException ce)
            {
                Log.WriteEntry(string.Format("(AES128Encryption Encrypt) : Crytographic error for user  {1} \r {0} \r {2}", ce.Message, username, ce.StackTrace), System.Diagnostics.EventLogEntryType.Error, 0000);
                return null;
            }
            catch (Exception ex)
            {
                Log.WriteEntry(string.Format("(AES128Encryption Encrypt) : Encryption error for user  {1} \r {0} \r {2}", ex.Message, username, ex.StackTrace), System.Diagnostics.EventLogEntryType.Error, 0000);
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
                byte[] fulldecryptedBytes = null;
                byte[] decryptedBytes = XORUtilities.XOREncryptOrDecrypt(encryptedBytes, this.XORSecret);
                using (AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider())
                {
                    aesAlg.BlockSize = 128;
                    aesAlg.KeySize = 128;
                    aesAlg.Key = AESKey;
                    aesAlg.IV = AESIV;
                    aesAlg.Mode = CipherMode.CBC;
                    aesAlg.Padding = PaddingMode.PKCS7;
                    using (ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV))
                    {
                        fulldecryptedBytes = decryptor.TransformFinalBlock(decryptedBytes, 0, decryptedBytes.Length);
                    }
                }
                byte[] userbuff = new byte[fulldecryptedBytes.Length - datalen];
                Buffer.BlockCopy(fulldecryptedBytes, datalen, userbuff, 0, fulldecryptedBytes.Length - datalen);
                this.CheckSum = userbuff;

                byte[] decryptedkey = new byte[datalen];
                Buffer.BlockCopy(fulldecryptedBytes, 0, decryptedkey, 0, datalen);
                return decryptedkey;
            }
            catch (CryptographicException ce)
            {
                Log.WriteEntry(string.Format("(AES128Encryption Decrypt) : Crytographic error for user  {1} \r {0} \r {2}", ce.Message, username, ce.StackTrace), System.Diagnostics.EventLogEntryType.Error, 0000);
                return null;
            }
            catch (Exception ex)
            {
                Log.WriteEntry(string.Format("(AES128Encryption Decrypt) : Decryption error for user  {1} \r {0} \r {2}", ex.Message, username, ex.StackTrace), System.Diagnostics.EventLogEntryType.Error, 0000);
                return null;
            }
        }

        /// <summary>
        /// GenerateKey method
        /// </summary>
        private byte[] GenerateKey(string username)
        {
            byte[] text = CheckSumEncoding.CheckSum(username);

            byte[] buffer = new byte[datalen + text.Length];
            RandomNumberGenerator cryptoRandomDataGenerator = new RNGCryptoServiceProvider();
            cryptoRandomDataGenerator.GetBytes(buffer, 0, datalen);
            Buffer.BlockCopy(text, 0, buffer, datalen, text.Length);
            return buffer;
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
    /// AES256Encryption class
    /// </summary>
    internal class AES256Encryption : BaseEncryption
    {
        private readonly byte[] AESIV = { 123, 123, 193, 113, 253, 66, 189, 182, 90, 101, 110, 102, 213, 224, 151, 62 };
        private readonly byte[] AESKey;

        private readonly int datalen = 128; 

        /// <summary>
        /// AES256Encryption constructor
        /// </summary>
        public AES256Encryption(string xorsecret) : base(xorsecret)
        {
            byte[] xkey = SystemUtilities.SystemKey;
            AESKey = new byte[32];
            Buffer.BlockCopy(xkey, 0, AESKey, 0, 32);
            byte[] res = null;
            using (SHA256Managed sha = new SHA256Managed())
            {
                res = sha.ComputeHash(AESKey);
                Buffer.BlockCopy(res, 0, AESKey, 0, 32);
            }
        }


        /// <summary>
        /// NewEncryptedKey method implementation
        /// </summary>
        public override byte[] NewEncryptedKey(string username)
        {
            try
            {
                byte[] plainBytes = GenerateKey(username);
                byte[] encryptedBytes = null;
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
                        encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
                    }
                }
                return XORUtilities.XOREncryptOrDecrypt(encryptedBytes, this.XORSecret);
            }
            catch (CryptographicException ce)
            {
                Log.WriteEntry(string.Format("(AES256Encryption Encrypt) : Crytographic error for user  {1} \r {0} \r {2}", ce.Message, username, ce.StackTrace), System.Diagnostics.EventLogEntryType.Error, 0000);
                return null;
            }
            catch (Exception ex)
            {
                Log.WriteEntry(string.Format("(AES256Encryption Encrypt) : Encryption error for user  {1} \r {0} \r {2}", ex.Message, username, ex.StackTrace), System.Diagnostics.EventLogEntryType.Error, 0000);
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
                byte[] fulldecryptedBytes = null;
                byte[] decryptedBytes = XORUtilities.XOREncryptOrDecrypt(encryptedBytes, this.XORSecret);
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
                        fulldecryptedBytes = decryptor.TransformFinalBlock(decryptedBytes, 0, decryptedBytes.Length);
                    }
                }
                byte[] userbuff = new byte[fulldecryptedBytes.Length - datalen];
                Buffer.BlockCopy(fulldecryptedBytes, datalen, userbuff, 0, fulldecryptedBytes.Length - datalen);
                this.CheckSum = userbuff;

                byte[] decryptedkey = new byte[datalen];
                Buffer.BlockCopy(fulldecryptedBytes, 0, decryptedkey, 0, datalen);
                return decryptedkey;
            }
            catch (CryptographicException ce)
            {
                Log.WriteEntry(string.Format("(AES256Encryption Decrypt) : Crytographic error for user  {1} \r {0} \r {2}", ce.Message, username, ce.StackTrace), System.Diagnostics.EventLogEntryType.Error, 0000);
                return null;
            }
            catch (Exception ex)
            {
                Log.WriteEntry(string.Format("(AES256Encryption Decrypt) : Decryption error for user  {1} \r {0} \r {2}", ex.Message, username, ex.StackTrace), System.Diagnostics.EventLogEntryType.Error, 0000);
                return null;
            }
        }

        /// <summary>
        /// GenerateKey method
        /// </summary>
        private byte[] GenerateKey(string username)
        {
            byte[] text = CheckSumEncoding.CheckSum(username);

            byte[] buffer = new byte[datalen + text.Length];
            RandomNumberGenerator cryptoRandomDataGenerator = new RNGCryptoServiceProvider();
            cryptoRandomDataGenerator.GetBytes(buffer, 0, datalen);
            Buffer.BlockCopy(text, 0, buffer, datalen, text.Length);
            return buffer;
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
    /// AES256Encryption class
    /// </summary>
    internal class AES256CNGEncryption : BaseEncryption
    {
        private readonly int datalen = 128;
        private byte[] AESIV = { 113, 25, 93, 11, 53, 02, 189, 82, 09, 101, 110, 102, 213, 224, 62, 62 };

        private CngProvider KeyStorageProvider { get => CngProvider.MicrosoftSoftwareKeyStorageProvider; }

        /// <summary>
        /// AES256Encryption constructor
        /// </summary>
        public AES256CNGEncryption(string xorsecret): base(xorsecret)
        {
        }

        /// <summary>
        /// NewEncryptedKey method implementation
        /// </summary>
        public override byte[] NewEncryptedKey(string username)
        {
            try
            {
                byte[] encrypted;
                byte[] unencrypted = GenerateKey(username); 

                using (AesCng aes = new AesCng())
                {
                    aes.BlockSize = 128;
                    aes.KeySize = 256;
                    aes.Key = GetBobSymetricKey();
                    aes.IV = AESIV;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;
                    using (ICryptoTransform decryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                    {
                        encrypted = decryptor.TransformFinalBlock(unencrypted, 0, unencrypted.Length);
                    }
                }
                return XORUtilities.XOREncryptOrDecrypt(encrypted, this.XORSecret);
            }
            catch (CryptographicException ce)
            {
                Log.WriteEntry(string.Format("(AES256CngEncryption Encrypt) : Crytographic error for user  {1} \r {0} \r {2}", ce.Message, username, ce.StackTrace), System.Diagnostics.EventLogEntryType.Error, 0000);
                return null;
            }
            catch (Exception ex)
            {
                Log.WriteEntry(string.Format("(AES256CngEncryption Encrypt) : Encryption error for user  {1} \r {0} \r {2}", ex.Message, username, ex.StackTrace), System.Diagnostics.EventLogEntryType.Error, 0000);
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
                byte[] fulldecryptedBytes = null;
                byte[] decryptedBytes = XORUtilities.XOREncryptOrDecrypt(encryptedBytes, this.XORSecret);
                using (AesCng aes = new AesCng())
                {
                    aes.BlockSize = 128;
                    aes.KeySize = 256;
                    aes.Key = GetAliceSymetricKey();
                    aes.IV = AESIV;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;
                    using (ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                    {
                        fulldecryptedBytes = decryptor.TransformFinalBlock(decryptedBytes, 0, decryptedBytes.Length);
                    }
                }
                byte[] userbuff = new byte[fulldecryptedBytes.Length - datalen];
                Buffer.BlockCopy(fulldecryptedBytes, datalen, userbuff, 0, fulldecryptedBytes.Length - datalen);
                this.CheckSum = userbuff;

                byte[] decryptedkey = new byte[datalen];
                Buffer.BlockCopy(fulldecryptedBytes, 0, decryptedkey, 0, datalen);
                return decryptedkey;

            }
            catch (CryptographicException ce)
            {
                Log.WriteEntry(string.Format("(AES256Encryption Decrypt) : Crytographic error for user  {1} \r {0} \r {2}", ce.Message, username, ce.StackTrace), System.Diagnostics.EventLogEntryType.Error, 0000);
                return null;
            }
            catch (Exception ex)
            {
                Log.WriteEntry(string.Format("(AES256Encryption Decrypt) : Decryption error for user  {1} \r {0} \r {2}", ex.Message, username, ex.StackTrace), System.Diagnostics.EventLogEntryType.Error, 0000);
                return null;
            }
        }

        /// <summary>
        /// GenerateKey method
        /// </summary>
        private byte[] GenerateKey(string username)
        {
            byte[] text = CheckSumEncoding.CheckSum(username);

            byte[] buffer = new byte[datalen + text.Length];
            RandomNumberGenerator cryptoRandomDataGenerator = new RNGCryptoServiceProvider();
            cryptoRandomDataGenerator.GetBytes(buffer, 0, datalen);
            Buffer.BlockCopy(text, 0, buffer, datalen, text.Length);
            return buffer;
        }

        /// <summary>
        /// GetBobSymetricKey method implmentation
        /// </summary>
        private byte[] GetBobSymetricKey()
        {
            CngProvider keyStorageProvider = CngProvider.MicrosoftSoftwareKeyStorageProvider;
            if (!CngKey.Exists(SystemUtilities.BobKeyName, keyStorageProvider, CngKeyOpenOptions.MachineKey))
                throw new Exception("Error: key doesn't exist...");
            if (!CngKey.Exists(SystemUtilities.AlicKeyName, keyStorageProvider, CngKeyOpenOptions.MachineKey))
                throw new Exception("Error: key doesn't exist...");

            using (CngKey alicecngkey = CngKey.Open(SystemUtilities.AlicKeyName, keyStorageProvider, CngKeyOpenOptions.MachineKey))
            {
                byte[] alicePubKeyBlob = alicecngkey.Export(CngKeyBlobFormat.EccPublicBlob);
                using (CngKey alicePubKey = CngKey.Import(alicePubKeyBlob, CngKeyBlobFormat.EccPublicBlob))
                {
                    using (CngKey bobcngkey = CngKey.Open(SystemUtilities.BobKeyName, keyStorageProvider, CngKeyOpenOptions.MachineKey))
                    {
                        using (ECDiffieHellmanCng bobAlgorithm = new ECDiffieHellmanCng(bobcngkey))
                        {
                            return bobAlgorithm.DeriveKeyMaterial(alicePubKey);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// GetAliceSymetricKey method implmentation
        /// </summary>
        private byte[] GetAliceSymetricKey()
        {
            CngProvider keyStorageProvider = CngProvider.MicrosoftSoftwareKeyStorageProvider;
            if (!CngKey.Exists(SystemUtilities.BobKeyName, keyStorageProvider, CngKeyOpenOptions.MachineKey))
                throw new Exception("Error: key doesn't exist...");
            if (!CngKey.Exists(SystemUtilities.AlicKeyName, keyStorageProvider, CngKeyOpenOptions.MachineKey))
                throw new Exception("Error: key doesn't exist...");

            using (CngKey bobcngkey = CngKey.Open(SystemUtilities.BobKeyName, keyStorageProvider, CngKeyOpenOptions.MachineKey))
            {
                byte[] bobPubKeyBlob = bobcngkey.Export(CngKeyBlobFormat.EccPublicBlob);
                using (CngKey bobPubKey = CngKey.Import(bobPubKeyBlob, CngKeyBlobFormat.EccPublicBlob))
                {
                    using (CngKey alicecngkey = CngKey.Open(SystemUtilities.AlicKeyName, keyStorageProvider, CngKeyOpenOptions.MachineKey))
                    {
                        using (ECDiffieHellmanCng bobAlgorithm = new ECDiffieHellmanCng(alicecngkey))
                        {
                            return bobAlgorithm.DeriveKeyMaterial(bobPubKey);
                        }
                    }
                }
            }
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
