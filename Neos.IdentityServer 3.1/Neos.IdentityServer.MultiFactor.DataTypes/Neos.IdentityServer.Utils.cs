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
// https://adfsmfa.codeplex.com                                                                                                                                                             //
// https://github.com/neos-sdi/adfsmfa                                                                                                                                                      //
//                                                                                                                                                                                          //
//******************************************************************************************************************************************************************************************//
using System;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;


namespace Neos.IdentityServer.MultiFactor.Data
{


    /// <summary>
    /// KeyMgtOptions
    /// </summary>
    [Flags, DataContract]
    public enum KeyMgtOptions
    {
        [EnumMember]
        AllCerts = 0x0,

        [EnumMember]
        MFACerts = 0x1,

        [EnumMember]
        ADFSCerts = 0x2,

        [EnumMember]
        SSLCerts = 0x4
    }
    #region Encoding
    /// <summary>
    /// CheckSumEncoding class implementation
    /// </summary>
    public static class CheckSumEncoding
    {
        /// <summary>
        /// EncodeUserID 
        /// </summary>
        public static byte[] EncodeUserID(int challengesize, string username)
        {
            switch (challengesize)
            {
                case 16:
                    return CheckSum128(username);
                case 20:
                    return CheckSum160(username);
                case 32:
                    return CheckSum256(username);
                case 48:
                    return CheckSum384(username);
                case 64:
                    return CheckSum512(username);
                default:
                    return CheckSum128(username);
            }
        }

        /// <summary>
        /// EncodeByteArray 
        /// </summary>
        public static byte[] EncodeByteArray(int challengesize, byte[] data)
        {
            switch (challengesize)
            {
                case 16:
                    return CheckSum128(data);
                case 20:
                    return CheckSum160(data);
                case 32:
                    return CheckSum256(data);
                case 48:
                    return CheckSum384(data);
                case 64:
                    return CheckSum512(data);
                default:
                    return CheckSum128(data);
            }
        }

        /// <summary>
        /// CheckSum128 method implementation
        /// </summary>
        public static byte[] CheckSum128(byte[] value)
        {
            byte[] hash = null;
            using (MD5 md5 = MD5Cng.Create())
            {
                hash = md5.ComputeHash(value);
            }
            return hash;
        }

        /// <summary>
        /// CheckSum128 method implementation
        /// </summary>
        public static byte[] CheckSum128(string value)
        {
            byte[] hash = null;
            using (MD5 md5 = MD5.Create())
            {
                hash = md5.ComputeHash(Encoding.UTF8.GetBytes(value));
            }
            return hash;
        }

        /// <summary>
        /// CheckSum160 method implementation
        /// </summary>
        public static byte[] CheckSum160(byte[] value)
        {
            byte[] hash = null;
            using (SHA1 sha1 = SHA1Cng.Create())
            {
                hash = sha1.ComputeHash(value);
            }
            return hash;
        }

        /// <summary>
        /// CheckSum160 method implementation
        /// </summary>
        public static byte[] CheckSum160(string value)
        {
            byte[] hash = null;
            using (SHA1 sha1 = SHA1Cng.Create())
            {
                hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(value));
            }
            return hash;
        }

        /// <summary>
        /// CheckSum256 method implementation
        /// </summary>
        public static byte[] CheckSum256(byte[] value)
        {
            byte[] hash = null;
            using (SHA256 sha256 = SHA256Cng.Create())
            {
                hash = sha256.ComputeHash(value);
            }
            return hash;
        }

        /// <summary>
        /// CheckSum256 method implementation
        /// </summary>
        public static byte[] CheckSum256(string value)
        {
            byte[] hash = null;
            using (SHA256 sha256 = SHA256.Create())
            {
                hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(value));
            }
            return hash;
        }

        /// <summary>
        /// CheckSum384 method implementation
        /// </summary>
        public static byte[] CheckSum384(byte[] value)
        {
            byte[] hash = null;
            using (SHA384 sha384 = SHA384Cng.Create())
            {
                hash = sha384.ComputeHash(value);
            }
            return hash;
        }

        /// <summary>
        /// CheckSum384 method implementation
        /// </summary>
        public static byte[] CheckSum384(string value)
        {
            byte[] hash = null;
            using (SHA384 sha384 = SHA384Managed.Create())
            {
                hash = sha384.ComputeHash(Encoding.UTF8.GetBytes(value));
            }
            return hash;
        }

        /// <summary>
        /// CheckSum512 method implementation
        /// </summary>
        public static byte[] CheckSum512(byte[] value)
        {
            byte[] hash = null;
            using (SHA512 sha512 = SHA512Cng.Create())
            {
                hash = sha512.ComputeHash(value);
            }
            return hash;
        }

        /// <summary>
        /// CheckSum512 method implementation
        /// </summary>
        public static byte[] CheckSum512(string value)
        {
            byte[] hash = null;
            using (SHA512 sha512 = SHA512Managed.Create())
            {
                hash = sha512.ComputeHash(Encoding.UTF8.GetBytes(value));
            }
            return hash;
        }

        /// <summary>
        /// CheckSum method implementation
        /// </summary>
        public static byte[] CheckSum(string value)
        {
            byte[] hash = null;
            using (MD5 md5 = MD5.Create())
            {
                hash = md5.ComputeHash(Encoding.UTF8.GetBytes(value));
            }
            return hash;
        }

        /// <summary>
        /// CheckSum method implementation
        /// </summary>
        public static string CheckSumAsString(string value)
        {
            string hash = null;
            using (MD5 md5 = MD5.Create())
            {
                hash = BitConverter.ToString(md5.ComputeHash(Encoding.UTF8.GetBytes(value)));
            }
            return hash.Replace("-", String.Empty);
        }

    }

    /// <summary>
    /// HexaEncoding static class
    /// </summary>
    public static class HexaEncoding
    {
        /// <summary>
        /// GetByteArrayFromHexString method
        /// </summary>
        public static byte[] GetByteArrayFromHexString(String value)
        {
            int len = value.Length;
            byte[] bytes = new byte[len / 2];
            for (int i = 0; i < len; i += 2)
                bytes[i / 2] = Convert.ToByte(value.Substring(i, 2), 16);
            return bytes;
        }

        /// <summary>
        /// GetHexStringFromByteArray method
        /// </summary>
        public static string GetHexStringFromByteArray(byte[] data)
        {
            int len = data.Length;
            StringBuilder builder = new StringBuilder(len * 2);
            foreach (byte b in data)
            {
                builder.AppendFormat("{0:x2}", b);
            }
            return builder.ToString().ToUpper();
        }
    }
    #endregion
}
