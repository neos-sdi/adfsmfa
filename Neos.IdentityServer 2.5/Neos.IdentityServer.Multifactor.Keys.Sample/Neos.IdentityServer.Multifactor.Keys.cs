//******************************************************************************************************************************************************************************************//
// Copyright (c) 2020 Neos-Sdi (http://www.neos-sdi.com)                                                                                                                                    //                        
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
    /// DBKeysRepositoryService class implementation
    /// </summary>
    internal class DBKeysRepositoryService : KeysRepositoryService, IDataRepositorySQLConnection
    {
        readonly string _connectionstring;
        readonly string _dataparameters;

        /// <summary>
        /// ADDSKeysRepositoryService constructor
        /// </summary>
        public DBKeysRepositoryService(MFAConfig cfg)
        {
            _connectionstring = cfg.KeysConfig.ExternalKeyManager.ConnectionString;
            _dataparameters = cfg.KeysConfig.ExternalKeyManager.Parameters.Data;
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
        public override string NewUserKey(string upn, string secretkey, X509Certificate2 cert)
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
        #endregion

        #region Certs Management
        /// <summary>
        /// GetUserCertificate method implmentation
        /// </summary>
        public override X509Certificate2 GetUserCertificate(string upn, bool generatepassword = false)
        {
            string pass = string.Empty;
            if (generatepassword)
                pass = CheckSumEncoding.CheckSumAsString(upn);
            string request = "SELECT CERTIFICATE FROM KEYS WHERE UPN=@UPN";
            SqlConnection con = new SqlConnection(_connectionstring);
            SqlCommand sql = new SqlCommand(request, con);

            SqlParameter prm = new SqlParameter("@UPN", SqlDbType.VarChar);
            sql.Parameters.Add(prm);
            prm.Value = upn.ToLower();

            con.Open();
            try
            {
                SqlDataReader rd = sql.ExecuteReader();
                if (rd.Read())
                {
                    string strcert = rd.GetString(0);
                    return new X509Certificate2(Convert.FromBase64String(strcert), pass, X509KeyStorageFlags.Exportable | X509KeyStorageFlags.EphemeralKeySet); // | X509KeyStorageFlags.PersistKeySet);
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
        /// CreateCertificate method implmentation
        /// </summary>
        public override X509Certificate2 CreateCertificate(string upn, int validity, bool generatepassword = false)
        {
            string pass = string.Empty;
            string strcert = string.Empty;
            if (generatepassword)
                pass = CheckSumEncoding.CheckSumAsString(upn);
            return Certs.CreateRSAEncryptionCertificateForUser(upn.ToLower(), validity, pass);
        }

        /// <summary>
        /// HasStoredCertificate method implementation
        /// </summary>
        public override bool HasStoredCertificate(string upn)
        {
            return true;
        }
        #endregion

        #region IDataRepositorySQLConnection
        /// <summary>
        /// CheckConnection method implementation
        /// </summary>
        public bool CheckConnection(string connectionstring)
        {
            SqlConnection con;
            if (string.IsNullOrEmpty(connectionstring))
                return false;
            if (!connectionstring.ToLower().Contains("connection timeout="))
                connectionstring += ";Connection Timeout=2";
            con = new SqlConnection(connectionstring);
            try
            {
                con.Open();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                con.Close();
            }
        }
        #endregion

        #region private methods
        /// <summary>
        /// InsertStoredKey method implementation
        /// </summary>
        private string DoInsertUserKey(string upn, string secretkey, X509Certificate2 certificate)
        {
            string request = "INSERT INTO KEYS (UPN, SECRETKEY, CERTIFICATE) VALUES (@UPN, @SECRETKEY, @CERTIFICATE)";

            SqlConnection con = new SqlConnection(_connectionstring);
            SqlCommand sql = new SqlCommand(request, con);

            SqlParameter pupn = new SqlParameter("@UPN", SqlDbType.VarChar);
            sql.Parameters.Add(pupn);
            pupn.Value = upn.ToLower();

            SqlParameter psecret = new SqlParameter("@SECRETKEY", SqlDbType.VarChar, 8000);
            sql.Parameters.Add(psecret);
            psecret.Value = secretkey;

            SqlParameter pcert = new SqlParameter("@CERTIFICATE", SqlDbType.VarChar, 8000);
            sql.Parameters.Add(pcert);
            pcert.Value = Convert.ToBase64String(certificate.Export(X509ContentType.Pfx, CheckSumEncoding.CheckSumAsString(upn)));

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
                certificate.Reset();
                con.Close();
            }
            return secretkey;
        }

        /// <summary>
        /// UpdateStoredKey method implementation
        /// </summary>
        private string DoUpdateUserKey(string upn, string secretkey, X509Certificate2 certificate)
        {
            string request = "UPDATE KEYS SET SECRETKEY = @SECRETKEY, CERTIFICATE = @CERTIFICATE WHERE UPN=@UPN";

            SqlConnection con = new SqlConnection(_connectionstring);
            SqlCommand sql = new SqlCommand(request, con);

            SqlParameter pupn = new SqlParameter("@UPN", SqlDbType.VarChar);
            sql.Parameters.Add(pupn);
            pupn.Value = upn.ToLower();

            SqlParameter psecret = new SqlParameter("@SECRETKEY", SqlDbType.VarChar, 8000);
            sql.Parameters.Add(psecret);
            psecret.Value = secretkey;

            SqlParameter pcert = new SqlParameter("@CERTIFICATE", SqlDbType.VarChar, 8000);
            sql.Parameters.Add(pcert);
            pcert.Value = Convert.ToBase64String(certificate.Export(X509ContentType.Pfx, CheckSumEncoding.CheckSumAsString(upn)));

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
                certificate.Reset();
                con.Close();
            }
            return secretkey;
        }
        #endregion
    }
}
