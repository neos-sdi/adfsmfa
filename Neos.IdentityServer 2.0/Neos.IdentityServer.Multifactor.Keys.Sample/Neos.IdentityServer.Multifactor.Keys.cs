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
// https://adfsmfa.codeplex.com                                                                                                                                                             //
// https://github.com/neos-sdi/adfsmfa                                                                                                                                                      //
//                                                                                                                                                                                          //
//******************************************************************************************************************************************************************************************//
using Neos.IdentityServer.MultiFactor;
using Neos.IdentityServer.MultiFactor.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Neos.IdentityServer.Multifactor.Keys
{
    /// <summary>
    /// KeyManager Class
    /// </summary>
    public class CustomKeyManager : ISecretKeyManager
    {
        private MFAConfig _cfg;
        private string _connectionstring;
        private int _validity;
        private static Encryption _cryptoRSADataProvider = null;
        private KeySizeMode _ksize = KeySizeMode.KeySize1024;
        private KeysRepositoryService _repos = null;
        private int MAX_PROBE_LEN = 0;

        /// <summary>
        /// CustomKeyManager constructor
        /// limit creation by friends
        /// </summary>
        internal CustomKeyManager()
        {
            Trace.TraceInformation("CustomKeyManager()");   
        } 

        /// <summary>
        /// Initialize method implementation
        /// </summary>
        public void Initialize(MFAConfig config)
        {
            _cfg = config;
            _connectionstring = config.KeysConfig.ExternalKeyManager.Parameters.Data;
            _validity = config.KeysConfig.CertificateValidity;
            _ksize = config.KeysConfig.KeySize;
            _repos = new CustomKeysRepositoryService(_cfg);
            switch (_ksize)
            {
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
        public KeysRepositoryService KeysStorage 
        {
            get{ return _repos; }
        }

        /// <summary>
        /// Prefix property
        /// </summary>
        public string Prefix
        {
            get { return "custom://"; }
        }

        /// <summary>
        /// NewKey method implementation
        /// </summary>
        public string NewKey(string upn)
        {
            if (string.IsNullOrEmpty(upn))
                return null;
            string lupn = upn.ToLower();
            string strcert = string.Empty;
            if (_cryptoRSADataProvider == null)
                _cryptoRSADataProvider = new Encryption();
            _cryptoRSADataProvider.Certificate = CreateCertificate(lupn, _validity, out strcert);
            string crypted = AddKeyPrefix(_cryptoRSADataProvider.Encrypt(lupn));
            return KeysStorage.NewUserKey(lupn, crypted, strcert);
        }

        /// <summary>
        /// ReadKey method implmentation
        /// </summary>
        public string ReadKey(string upn)
        {
            if (string.IsNullOrEmpty(upn))
                return null;
            string lupn = upn.ToLower();
            return KeysStorage.GetUserKey(lupn);
        }

        /// <summary>
        /// EncodedKey method implementation
        /// </summary>
        public string EncodedKey(string upn)
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
        /// ProbeKey method implmentation
        /// </summary>
        public string ProbeKey(string upn)
        {
            if (string.IsNullOrEmpty(upn))
                return null;
            string lupn = upn.ToLower();
            string full = StripKeyPrefix(ReadKey(lupn));
            if (string.IsNullOrEmpty(full))
                return null;
            if (full.Length > MAX_PROBE_LEN)
                return full.Substring(0, MAX_PROBE_LEN);
            else
                return full;
        }

        /// <summary>
        /// RemoveKey method implmentation
        /// </summary>
        public bool RemoveKey(string upn)
        {
            if (string.IsNullOrEmpty(upn))
                return false;
            string lupn = upn.ToLower();
            return KeysStorage.RemoveUserKey(lupn);
        }

        /// <summary>
        /// ValidateKey method implmentation
        /// </summary>
        public bool ValidateKey(string upn)
        {
            if (string.IsNullOrEmpty(upn))
                return false;
            string lupn = upn.ToLower();
            if (!KeysStorage.HasStoredCertificate(upn))
                return false;
            string key = ReadKey(lupn);
            if (HasKeyPrefix(key))
            {
                if (_cryptoRSADataProvider == null)
                    _cryptoRSADataProvider = new Encryption();

                key = StripKeyPrefix(key);
                _cryptoRSADataProvider.Certificate = KeysStorage.GetUserCertificate(lupn);
                string user = _cryptoRSADataProvider.Decrypt(key);
                if (string.IsNullOrEmpty(user))
                    return false;  // Key corrupted
                if (user.ToLower().Equals(lupn))
                    return true;   // OK 
                else
                    return false;  // Key corrupted
            }
            else
                return false;
        }

        /// <summary>
        /// StripKeyPrefix method implementation
        /// </summary>
        public virtual string StripKeyPrefix(string key)
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
        public virtual string AddKeyPrefix(string key)
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
        public virtual bool HasKeyPrefix(string key)
        {
            if (string.IsNullOrEmpty(key))
                return false;
            return key.StartsWith(this.Prefix);
        }

        #region private methods
        /// <summary>
        /// CreateCertificate method implmentation
        /// </summary>
        private X509Certificate2 CreateCertificate(string upn, int validity, out string strcert)
        {
            strcert = Certs.CreateSelfSignedCertificateAsString(upn.ToLower(), validity);
            X509Certificate2 cert = new X509Certificate2(Convert.FromBase64String(strcert), "", X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet);
            if (Certs.RemoveSelfSignedCertificate(cert))
                return cert;
            else
                return null;
        }
        #endregion
    }

    internal class CustomKeysRepositoryService : KeysRepositoryService
    {
        string _connectionstring;

        /// <summary>
        /// ADDSKeysRepositoryService constructor
        /// </summary>
        public CustomKeysRepositoryService(MFAConfig cfg)
        {
            _connectionstring = cfg.KeysConfig.ExternalKeyManager.Parameters.Data;
        }

        #region Key Management
        /// <summary>
        /// GetUserKey method implmentation
        /// </summary>
        public override string GetUserKey(string upn)
        {
            string request = "SELECT SECRETKEY FROM KEYS WHERE UPN=@UPN";
            SqlConnection con = new SqlConnection(_connectionstring);
            SqlCommand sql = new SqlCommand(request, con);

            SqlParameter prm = new SqlParameter("@UPN", SqlDbType.VarChar);
            sql.Parameters.Add(prm);
            prm.Value = upn.ToLower();

            Registration reg = new Registration();
            con.Open();
            try
            {
                SqlDataReader rd = sql.ExecuteReader();
                if (rd.Read())
                {
                    return rd.GetString(0);
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                DataLog.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error, 5000);
                throw new Exception(ex.Message);
            }
            finally
            {
                con.Close();
            }
        }

        /// <summary>
        /// NewUserKey method implmentation
        /// </summary>
        public override string NewUserKey(string upn, string secretkey, string cert)
        {
            if (HasStoredKey(upn.ToLower()))
                return DoUpdateUserKey(upn.ToLower(), secretkey, cert);
            else
                return DoInsertUserKey(upn.ToLower(), secretkey, cert);
        }

        /// <summary>
        /// RemoveUserKey method implmentation
        /// </summary>
        public override bool RemoveUserKey(string upn)
        {
            string request = "DELETE FROM KEYS WHERE UPN=@UPN";

            SqlConnection con = new SqlConnection(_connectionstring);
            SqlCommand sql = new SqlCommand(request, con);

            SqlParameter pupn = new SqlParameter("@UPN", SqlDbType.VarChar);
            sql.Parameters.Add(pupn);
            pupn.Value = upn.ToLower();

            con.Open();
            try
            {
                int res = sql.ExecuteNonQuery();
                return (res > 0);
            }
            catch (Exception ex)
            {
                DataLog.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error, 5000);
                throw new Exception(ex.Message);
            }
            finally
            {
                con.Close();
            }
        }

        /// <summary>
        /// HasStoredKey method implementation
        /// </summary>
        public override bool HasStoredKey(string upn)
        {
            string request = "SELECT ID, UPN FROM KEYS WHERE UPN=@UPN";

            SqlConnection con = new SqlConnection(_connectionstring);
            SqlCommand sql = new SqlCommand(request, con);

            SqlParameter prm = new SqlParameter("@UPN", SqlDbType.VarChar);
            sql.Parameters.Add(prm);
            prm.Value = upn.ToLower();
            con.Open();
            try
            {
                SqlDataReader rd = sql.ExecuteReader();
                return rd.Read();
            }
            catch (Exception ex)
            {
                DataLog.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error, 5000);
                throw new Exception(ex.Message);
            }
            finally
            {
                con.Close();
            }
        }

        /// <summary>
        /// GetUserCertificate method implmentation
        /// </summary>
        public override X509Certificate2 GetUserCertificate(string upn)
        {
            string request = "SELECT CERTIFICATE FROM KEYS WHERE UPN=@UPN";
            SqlConnection con = new SqlConnection(_connectionstring);
            SqlCommand sql = new SqlCommand(request, con);

            SqlParameter prm = new SqlParameter("@UPN", SqlDbType.VarChar);
            sql.Parameters.Add(prm);
            prm.Value = upn.ToLower();

            Registration reg = new Registration();
            con.Open();
            try
            {
                SqlDataReader rd = sql.ExecuteReader();
                if (rd.Read())
                {
                    string strcert = rd.GetString(0);
                    return new X509Certificate2(Convert.FromBase64String(strcert), "", X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet);
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                DataLog.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error, 5000);
                throw new Exception(ex.Message);
            }
            finally
            {
                con.Close();
            }
        }

        /// <summary>
        /// HasStoredCertificate method implementation
        /// </summary>
        public override bool HasStoredCertificate(string upn)
        {
            return true;
        }

        #region private methods
        /// <summary>
        /// InsertStoredKey method implementation
        /// </summary>
        private string DoInsertUserKey(string upn, string secretkey, string certificate)
        {
            string request = "INSERT INTO KEYS (UPN, SECRETKEY, CERTIFICATE) VALUES (@UPN, @SECRETKEY, @CERTIFICATE)";

            SqlConnection con = new SqlConnection(_connectionstring);
            SqlCommand sql = new SqlCommand(request, con);

            SqlParameter pupn = new SqlParameter("@UPN", SqlDbType.VarChar);
            sql.Parameters.Add(pupn);
            pupn.Value = upn.ToLower();

            SqlParameter psecret = new SqlParameter("@SECRETKEY", SqlDbType.VarChar);
            sql.Parameters.Add(psecret);
            psecret.Value = secretkey;

            SqlParameter pcert = new SqlParameter("@CERTIFICATE", SqlDbType.VarChar);
            sql.Parameters.Add(pcert);
            pcert.Value = certificate;

            con.Open();
            try
            {
                int res = sql.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                DataLog.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error, 5000);
                throw new Exception(ex.Message);
            }
            finally
            {
                con.Close();
            }
            return secretkey;
        }

        /// <summary>
        /// UpdateStoredKey method implementation
        /// </summary>
        private string DoUpdateUserKey(string upn, string secretkey, string certificate)
        {
            string request = "UPDATE KEYS SET SECRETKEY = @SECRETKEY, CERTIFICATE = @CERTIFICATE WHERE UPN=@UPN";

            SqlConnection con = new SqlConnection(_connectionstring);
            SqlCommand sql = new SqlCommand(request, con);

            SqlParameter pupn = new SqlParameter("@UPN", SqlDbType.VarChar);
            sql.Parameters.Add(pupn);
            pupn.Value = upn.ToLower();

            SqlParameter psecret = new SqlParameter("@SECRETKEY", SqlDbType.VarChar);
            sql.Parameters.Add(psecret);
            psecret.Value = secretkey;

            SqlParameter pcert = new SqlParameter("@CERTIFICATE", SqlDbType.VarChar);
            sql.Parameters.Add(pcert);
            pcert.Value = certificate;
            con.Open();
            try
            {
                int res = sql.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                DataLog.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error, 5000);
                throw new Exception(ex.Message);
            }
            finally
            {
                con.Close();
            }
            return secretkey;
        }
        #endregion

        #endregion
    }
}
