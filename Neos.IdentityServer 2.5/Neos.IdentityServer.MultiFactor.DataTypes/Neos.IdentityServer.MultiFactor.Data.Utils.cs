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
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Neos.IdentityServer.MultiFactor.Data
{
    /// <summary>
    /// Log class
    /// </summary>
    public class DataLog
    {
        private const string EventLogSource = "ADFS MFA DataServices";
        private const string EventLogGroup = "Application";

        /// <summary>
        /// Log constructor
        /// </summary>
        static DataLog()
        {
            if (!EventLog.SourceExists(DataLog.EventLogSource))
                EventLog.CreateEventSource(DataLog.EventLogSource, DataLog.EventLogGroup);
        }

        /// <summary>
        /// WriteEntry method implementation
        /// </summary>
        public static void WriteEntry(string message, EventLogEntryType type, int eventID)
        {
            EventLog.WriteEntry(EventLogSource, message, type, eventID);
        }
    }

    public class CheckSumEncoding
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
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5Cng.Create())
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
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
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
            using (System.Security.Cryptography.SHA1 sha1 = System.Security.Cryptography.SHA1Cng.Create())
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
            using (System.Security.Cryptography.SHA1 sha1 = System.Security.Cryptography.SHA1Cng.Create())
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
            using (System.Security.Cryptography.SHA256 sha256 = System.Security.Cryptography.SHA256Cng.Create())
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
            using (System.Security.Cryptography.SHA256 sha256 = System.Security.Cryptography.SHA256.Create())
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
            using (System.Security.Cryptography.SHA384 sha384 = System.Security.Cryptography.SHA384Cng.Create())
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
            using (System.Security.Cryptography.SHA384 sha384 = System.Security.Cryptography.SHA384Managed.Create())
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
            using (System.Security.Cryptography.SHA512 sha512 = System.Security.Cryptography.SHA512Cng.Create())
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
            using (System.Security.Cryptography.SHA512 sha512 = System.Security.Cryptography.SHA512Managed.Create())
            {
                hash = sha512.ComputeHash(Encoding.UTF8.GetBytes(value));
            }
            return hash;
        }
    }

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

    /// <summary>
    /// ADDSForest class implementation
    /// </summary>
    public class ADDSForest
    {
        public bool IsRoot { get; set; }
        public string ForestDNS { get; set; }
        public List<string> TopLevelNames = new List<string>();
    }

    /// <summary>
    /// ADDSForestUtils class implementation
    /// </summary>
    public class ADDSForestUtils
    {
        private bool _isbinded = false;
        private List<ADDSForest> _forests = new List<ADDSForest>();

        /// <summary>
        /// ADDSForestUtils constructor
        /// </summary>
        public ADDSForestUtils()
        {
            _isbinded = false;
            Bind();
        }

        /// <summary>
        /// Bind method implementation
        /// </summary>
        public void Bind()
        {
            if (_isbinded)
                return;
            _isbinded = true;

            Forests.Clear();
            Forest currentforest = Forest.GetCurrentForest();
            ADDSForest root = new ADDSForest();
            root.IsRoot = true;
            root.ForestDNS = currentforest.Name;
            Forests.Add(root);
            foreach (ForestTrustRelationshipInformation trusts in currentforest.GetAllTrustRelationships())
            {
                ADDSForest sub = new ADDSForest();
                sub.IsRoot = false;
                sub.ForestDNS = trusts.TargetName;
                foreach (TopLevelName t in trusts.TopLevelNames)
                {
                    if (t.Status == TopLevelNameStatus.Enabled)
                        sub.TopLevelNames.Add(t.Name);
                }
                Forests.Add(sub);
            }
        }

        /// <summary>
        /// GetForestDNSForUPN method implementation
        /// </summary>
        public string GetForestDNSForUPN(string upn)
        {
            string result = string.Empty;
            string domtofind = upn.Substring(upn.IndexOf('@') + 1);
            foreach (ADDSForest f in Forests)
            {
                if (f.IsRoot) // Fallback/default Forest
                    result = f.ForestDNS;
                else
                {
                    foreach (string s in f.TopLevelNames)
                    {
                        if (s.ToLower().Equals(domtofind.ToLower()))
                        {
                            result = f.ForestDNS;
                            break;
                        }
                    }
                }
            }
            return result;
        }

        public List<ADDSForest> Forests
        {
            get { return _forests; }
        }
    }
}
