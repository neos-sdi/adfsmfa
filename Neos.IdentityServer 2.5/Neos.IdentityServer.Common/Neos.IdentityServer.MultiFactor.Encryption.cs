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
using System.Text;
using Neos.IdentityServer.MultiFactor.Data;
using System.Linq;

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
                this.CheckSum = CheckSumEncoding.CheckSum(outval); 
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

                return XORUtilities.XOREncryptOrDecrypt(encryptedBytes, this.XORSecret);
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

                byte[] decryptedBytes = XORUtilities.XOREncryptOrDecrypt(encryptedBytes, this.XORSecret);
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
            byte[] text = CheckSumEncoding.CheckSum(username);

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
                return XORUtilities.XOREncryptOrDecrypt(plainBytes, this.XORSecret);
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

                byte[] decryptedBytes = XORUtilities.XOREncryptOrDecrypt(encryptedBytes, XORUtilities.XORKey);
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
    /// AESEncryption class
    /// </summary>
    public class AESEncryption : IDisposable
    {
        private byte[] IV = { 113, 23, 93, 174, 155, 66, 179, 82, 90, 101, 110, 102, 213, 124, 51, 62 };
        private byte[] Hdr = { 0x17, 0xD3, 0xF4, 0x29 };
        private byte[] AESKey;

        /// <summary>
        /// AESEncryption constructor
        /// </summary>
        public AESEncryption()
        {
            string basestr = XORUtilities.XORKey.Substring(0, 16);
            string resstr = basestr.PadRight(16, 'x');
            AESKey = Encoding.ASCII.GetBytes(resstr.ToCharArray(), 0, 16);
        }

        /// <summary>
        /// Encrypt method implementation
        /// </summary>
        public string Encrypt(string data)
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
        public string Decrypt(string data)
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
        static string DecryptStringFromBytes(byte[] cipherText, byte[] Key, byte[] IV)
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
                byte[] ProofHeader = GetProofHeader(encrypted);
                UInt16 l = GetLen(encrypted);
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
        private byte[] GetProofHeader(byte[] data)
        {
            byte[] Header = new byte[4];
            Buffer.BlockCopy(data, 0, Header, 0, 4);
            return Header;
        }

        /// <summary>
        /// GetHeader method
        /// </summary>
        private byte[] GetHeader(byte[] data)
        {
            byte[] Header = new byte[5];
            Buffer.BlockCopy(data, 0, Header, 0, 5);
            return Header;
        }

        /// <summary>
        /// GetLen method
        /// </summary>
        private UInt16 GetLen(byte[] data)
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
}
