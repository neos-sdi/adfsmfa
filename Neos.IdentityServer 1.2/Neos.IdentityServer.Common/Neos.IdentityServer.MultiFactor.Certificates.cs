//******************************************************************************************************************************************************************************************//
// Copyright (c) 2017 Neos-Sdi (http://www.neos-sdi.com)                                                                                                                                    //                        
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
//******************************************************************************************************************************************************************************************//
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Neos.IdentityServer.MultiFactor
{
    /// <summary>
    /// X509CertificateData class
    /// </summary>
    public class X509CertificateData
    {
        public byte[] RawData;
        public AsymmetricAlgorithm Key;
    }


    /// <summary>
    /// Encryption class implmentation
    /// </summary>
    public class Encryption<T> : IDisposable
    {
        private X509Certificate2 _cert = null;

		/// <summary>
        /// Encryption constructor
		/// </summary>
        public Encryption(string thumbprint, StoreLocation location)
        {
            _cert = GetCertificate(thumbprint, location);
        }

        /// <summary>
        /// Encrypt method
        /// </summary>
        public string Encrypt(T obj)
        {
            try
            {
                RSACryptoServiceProvider publicKey = (RSACryptoServiceProvider)_cert.PublicKey.Key;
                byte[] plainBytes = SerializeToStream(obj).ToArray();
                byte[] encryptedBytes = publicKey.Encrypt(plainBytes, true);
                return System.Convert.ToBase64String(encryptedBytes);
            }
            catch (Exception ex)
            {
                throw new CryptographicException("Invalid cypher value ! cannot continue !", ex.Message);
            }
        }

        /// <summary>
        /// Decrypt method
        /// </summary>
        public T Decrypt(string encryptedText)
        {
            try
            {
                RSACryptoServiceProvider privateKey = (RSACryptoServiceProvider)_cert.PrivateKey;
                byte[] encryptedBytes = System.Convert.FromBase64CharArray(encryptedText.ToCharArray(), 0, encryptedText.Length);
                byte[] decryptedBytes = privateKey.Decrypt(encryptedBytes, true);
                MemoryStream mem = new MemoryStream(decryptedBytes);
                T decryptedvalue = DeserializeFromStream(mem);
                return decryptedvalue;
            }
            catch (Exception ex)
            {
                throw new CryptographicException("Invalid cypher value ! cannot continue !", ex.Message);
            }
        }

        /// <summary>
        /// SerializeToStream
        /// </summary>
        private MemoryStream SerializeToStream(T objectType)
        {
            MemoryStream stream = new MemoryStream();
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, objectType);
            return stream;
        }

        /// <summary>
        /// DeserializeFromStream
        /// </summary>
        private T DeserializeFromStream(MemoryStream stream)
        {
            IFormatter formatter = new BinaryFormatter();
            stream.Seek(0, SeekOrigin.Begin);
            object objectType = formatter.Deserialize(stream);
            return (T)objectType;
        }

        /// <summary>
        /// GetCertificate method implementation
        /// </summary>
        private X509Certificate2 GetCertificate(string thumprint, StoreLocation location)
        {
            X509Certificate2 data = null;
            X509Store store = new X509Store(location);
            store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
            try
            {
                X509Certificate2Collection collection = (X509Certificate2Collection)store.Certificates;
                X509Certificate2Collection findCollection = (X509Certificate2Collection)collection.Find(X509FindType.FindByThumbprint, thumprint, false);
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
        /// Dispose IDispose method implementation
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose method implementation
        /// </summary>
        public virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
				if (_cert!=null)
					_cert.Reset();
            }
        }

        /// <summary>
        /// GetCertificate method implementation
        /// </summary>
        public X509CertificateData GetCertificateData()
        {
            X509CertificateData data = new X509CertificateData();
            data.RawData = _cert.RawData;
            data.Key = _cert.PrivateKey;
           _cert.Reset();
            return data;
        }
    }
}
