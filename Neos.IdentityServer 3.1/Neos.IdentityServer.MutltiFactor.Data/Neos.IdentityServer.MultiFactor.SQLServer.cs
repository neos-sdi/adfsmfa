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
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using System.DirectoryServices;
using System.Security.Cryptography.X509Certificates;

namespace Neos.IdentityServer.MultiFactor.Data
{
    #region SQL Data Repository
    internal class SQLDataRepositoryService : DataRepositoryService, IDataRepositorySQLConnection, IWebAuthNDataRepositoryService
    {
        public override event KeysDataManagerEvent OnKeyDataEvent;

        /// <summary>
        /// SQLDataRepositoryService constructor
        /// </summary>
        public SQLDataRepositoryService(BaseDataHost host, int deliverywindow = 3000): base(host, deliverywindow)
        {
            if (!(host is SQLServerHost))
                throw new ArgumentException("Invalid Host ! : value but be an SQLServerHost instance");
        }

        private SQLServerHost SQLHost
        {
            get { return ((SQLServerHost)Host); }
        }

        #region DataRepositoryService
        /// <summary>
        /// GetMFAUser method implementation
        /// </summary>
        public override MFAUser GetMFAUser(string upn)
        {
            string request = "SELECT ID, UPN, MAILADDRESS, PHONENUMBER, PIN, ENABLED, METHOD, OVERRIDE FROM REGISTRATIONS WHERE UPN=@UPN";
            SqlConnection con = new SqlConnection(SQLUtils.GetFullConnectionString(SQLHost));
            SqlCommand sql = new SqlCommand(request, con);

            SqlParameter prm = new SqlParameter("@UPN", SqlDbType.VarChar, 256);
            sql.Parameters.Add(prm);
            prm.Value = upn.ToLower();

            MFAUser reg = new MFAUser();
            con.Open();
            try
            {
                SqlDataReader rd = sql.ExecuteReader();
                if (rd.Read())
                {
                    reg.ID = rd.GetInt64(0).ToString();
                    reg.UPN = rd.GetString(1);
                    if (!rd.IsDBNull(2))
                        reg.MailAddress = rd.GetString(2);
                    if (!rd.IsDBNull(3))
                        reg.PhoneNumber = rd.GetString(3);
                    reg.PIN = rd.GetInt32(4);
                    reg.Enabled = rd.GetBoolean(5);
                    reg.PreferredMethod = (PreferredMethod)rd.GetInt32(6);
                    if (!rd.IsDBNull(7))
                        reg.OverrideMethod = rd.GetString(7);
                    else
                        reg.OverrideMethod = string.Empty;
                    reg.IsRegistered = true;
                    return reg;
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
        /// SetMFAUser method implementation
        /// </summary>
        public override MFAUser SetMFAUser(MFAUser reg, bool resetkey = false, bool caninsert = true, bool disableoninsert = false)
        {
            if (!SQLUtils.IsMFAUserRegistered(SQLHost, reg.UPN))
            {
                if (caninsert)
                    return AddMFAUser(reg, resetkey, false);
                else
                    return GetMFAUser(reg.UPN);
            }
            string request;
            if (disableoninsert)
                request = "UPDATE REGISTRATIONS SET MAILADDRESS = @MAILADDRESS, PHONENUMBER = @PHONENUMBER, PIN=@PIN, METHOD=@METHOD, OVERRIDE=@OVERRIDE, WHERE UPN=@UPN";
            else
                request = "UPDATE REGISTRATIONS SET MAILADDRESS = @MAILADDRESS, PHONENUMBER = @PHONENUMBER, PIN=@PIN, METHOD=@METHOD, OVERRIDE=@OVERRIDE, ENABLED=@ENABLED WHERE UPN=@UPN";

            SqlConnection con = new SqlConnection(SQLUtils.GetFullConnectionString(SQLHost));
            SqlCommand sql = new SqlCommand(request, con);

            SqlParameter prm1 = new SqlParameter("@MAILADDRESS", SqlDbType.VarChar, 256);
            sql.Parameters.Add(prm1);
            if (string.IsNullOrEmpty(reg.MailAddress))
                prm1.Value = DBNull.Value;
            else
                prm1.Value = reg.MailAddress;

            SqlParameter prm2 = new SqlParameter("@PHONENUMBER", SqlDbType.VarChar, 50);
            sql.Parameters.Add(prm2);
            if (string.IsNullOrEmpty(reg.PhoneNumber))
                prm2.Value = DBNull.Value;
            else
                prm2.Value = reg.PhoneNumber;

            SqlParameter prm3 = new SqlParameter("@PIN", SqlDbType.Int);
            sql.Parameters.Add(prm3);
            prm3.Value = reg.PIN;

            SqlParameter prm4 = new SqlParameter("@METHOD", SqlDbType.Int);
            sql.Parameters.Add(prm4);
            prm4.Value = reg.PreferredMethod;

            SqlParameter prm5 = new SqlParameter("@OVERRIDE", SqlDbType.VarChar);
            sql.Parameters.Add(prm5);
            if (reg.OverrideMethod == null)
                prm5.Value = string.Empty;
            else
                prm5.Value = reg.OverrideMethod;

            if (!disableoninsert)
            {
                SqlParameter prm6 = new SqlParameter("@ENABLED", SqlDbType.Bit);
                sql.Parameters.Add(prm6);
                prm6.Value = reg.Enabled;
            }

            SqlParameter prm7 = new SqlParameter("@UPN", SqlDbType.VarChar, 256);
            sql.Parameters.Add(prm7);
            prm7.Value = reg.UPN.ToLower();
            con.Open();
            try
            {
                int res = sql.ExecuteNonQuery();
                if (resetkey)
                    this.OnKeyDataEvent(reg.UPN, KeysDataManagerEventKind.add);
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
            return GetMFAUser(reg.UPN);
        }

        /// <summary>
        /// AddMFAUser method implementation
        /// </summary>
        public override MFAUser AddMFAUser(MFAUser reg, bool resetkey = true, bool canupdate = true, bool disableoninsert = false)
        {
            if (SQLUtils.IsMFAUserRegistered(SQLHost, reg.UPN))
            {
                if (canupdate)
                    return SetMFAUser(reg, resetkey, false);
                else
                    return GetMFAUser(reg.UPN);
            }
            string request = "INSERT INTO REGISTRATIONS (UPN, MAILADDRESS, PHONENUMBER, PIN, ENABLED, METHOD, OVERRIDE) VALUES (@UPN, @MAILADDRESS, @PHONENUMBER, @PIN, @ENABLED, @METHOD, @OVERRIDE)";
            SqlConnection con = new SqlConnection(SQLUtils.GetFullConnectionString(SQLHost));
            SqlCommand sql = new SqlCommand(request, con);

            SqlParameter prm1 = new SqlParameter("@UPN", SqlDbType.VarChar, 256);
            sql.Parameters.Add(prm1);
            prm1.Value = reg.UPN.ToLower();

            SqlParameter prm2 = new SqlParameter("@MAILADDRESS", SqlDbType.VarChar, 256);
            sql.Parameters.Add(prm2);
            if (string.IsNullOrEmpty(reg.MailAddress))
                prm2.Value = DBNull.Value;
            else
                prm2.Value = reg.MailAddress;

            SqlParameter prm3 = new SqlParameter("@PHONENUMBER", SqlDbType.VarChar, 50);
            sql.Parameters.Add(prm3);
            if (string.IsNullOrEmpty(reg.PhoneNumber))
                prm3.Value = DBNull.Value;
            else
                prm3.Value = reg.PhoneNumber;

            SqlParameter prm4 = new SqlParameter("@PIN", SqlDbType.Int);
            sql.Parameters.Add(prm4);
            prm4.Value = reg.PIN;

            SqlParameter prm5 = new SqlParameter("@ENABLED", SqlDbType.Bit);
            sql.Parameters.Add(prm5);
            if (disableoninsert)
                prm5.Value = false;
            else
                prm5.Value = reg.Enabled;

            SqlParameter prm6 = new SqlParameter("@METHOD", SqlDbType.Int);
            sql.Parameters.Add(prm6);
            prm6.Value = reg.PreferredMethod;

            SqlParameter prm7 = new SqlParameter("@OVERRIDE", SqlDbType.VarChar);
            sql.Parameters.Add(prm7);
            if (reg.OverrideMethod == null)
                prm7.Value = string.Empty;
            else
                prm7.Value = reg.OverrideMethod;
            con.Open();
            try
            {
                int res = sql.ExecuteNonQuery();
                if (resetkey)
                    this.OnKeyDataEvent(reg.UPN, KeysDataManagerEventKind.add);
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
            return GetMFAUser(reg.UPN);
        }

        /// <summary>
        /// DeleteMFAUser method implementation
        /// </summary>
        public override bool DeleteMFAUser(MFAUser reg, bool dropkey = true)
        {
            if (!SQLUtils.IsMFAUserRegistered(SQLHost, reg.UPN))
                throw new Exception("The user " + reg.UPN.ToLower() + " cannot be deleted ! \r User not found !");

            if (dropkey)
                this.OnKeyDataEvent(reg.UPN, KeysDataManagerEventKind.Remove);

            string request = "DELETE FROM REGISTRATIONS WHERE UPN=@UPN";

            SqlConnection con = new SqlConnection(SQLUtils.GetFullConnectionString(SQLHost));
            SqlCommand sql = new SqlCommand(request, con);

            SqlParameter prm = new SqlParameter("@UPN", SqlDbType.VarChar, 256);
            sql.Parameters.Add(prm);
            prm.Value = reg.UPN.ToLower();
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
            return true;
        }

        /// <summary>
        /// EnableMFAUser method implementation
        /// </summary>
        public override MFAUser EnableMFAUser(MFAUser reg)
        {
            if (!SQLUtils.IsMFAUserRegistered(SQLHost, reg.UPN))
                throw new Exception("The user " + reg.UPN + " cannot be updated ! \r User not found !");

            string request = "UPDATE REGISTRATIONS SET ENABLED=1 WHERE UPN=@UPN";

            SqlConnection con = new SqlConnection(SQLUtils.GetFullConnectionString(SQLHost));
            SqlCommand sql = new SqlCommand(request, con);

            SqlParameter prm5 = new SqlParameter("@UPN", SqlDbType.VarChar, 256);
            sql.Parameters.Add(prm5);
            prm5.Value = reg.UPN.ToLower();
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
            reg.Enabled = true;
            return reg;
        }

        /// <summary>
        /// DisableMFAUser method implementation
        /// </summary>
        public override MFAUser DisableMFAUser(MFAUser reg)
        {
            if (!SQLUtils.IsMFAUserRegistered(SQLHost, reg.UPN))
                throw new Exception("The user " + reg.UPN + " cannot be updated ! \r User not found !");

            string request = "UPDATE REGISTRATIONS SET ENABLED=0 WHERE UPN=@UPN";

            SqlConnection con = new SqlConnection(SQLUtils.GetFullConnectionString(SQLHost));
            SqlCommand sql = new SqlCommand(request, con);

            SqlParameter prm5 = new SqlParameter("@UPN", SqlDbType.VarChar, 256);
            sql.Parameters.Add(prm5);
            prm5.Value = reg.UPN.ToLower();
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
            reg.Enabled = false;
            return reg;
        }

        /// <summary>
        /// GetMFAUsers method implementation
        /// </summary>
        public override MFAUserList GetMFAUsers(DataFilterObject filter, DataOrderObject order, DataPagingObject paging)
        {
            Dictionary<int, string> fliedlsvalues = new Dictionary<int, string> 
            {
                {0, " UPN "} ,
                {1, " MAILADDRESS "},
                {2, " PHONENUMBER "}
            };

            Dictionary<int, string> operatorsvalues = new Dictionary<int, string> 
            { 
                {0, " = @FILTERVALUE "},
                {1, " LIKE @FILTERVALUE +'%' "},
                {2, " LIKE '%'+ @FILTERVALUE +'%' "},
                {3, " <> @FILTERVALUE "},
                {4, " LIKE '%'+ @FILTERVALUE "},
                {5, " NOT LIKE '%' + @FILTERVALUE +'%' "}
            };

            Dictionary<int, string> nulloperatorsvalues = new Dictionary<int, string> 
            { 
                {0, " IS NULL "},
                {1, " IS NULL "},
                {2, " IS NULL "},
                {3, " IS NOT NULL "},
                {4, " IS NOT NULL "},
                {5, " IS NOT NULL "}
            };

            Dictionary<int, string> methodvalues = new Dictionary<int, string> 
            { 
                {0, " METHOD = 0 "},
                {1, " METHOD = 1 "},
                {2, " METHOD = 2 "},
                {3, " METHOD = 3 "},
                {4, " METHOD = 4 "},
                {5, " METHOD = 5 "}
            };

            string request = string.Empty;
            switch (order.Column)
            {
                case DataOrderField.UserName:
                    if (order.Direction == SortDirection.Ascending)
                        request = "SELECT ROW_NUMBER() OVER(ORDER BY UPN) AS NUMBER, ID, UPN, MAILADDRESS, PHONENUMBER, PIN, ENABLED, METHOD, OVERRIDE FROM REGISTRATIONS";
                    else
                        request = "SELECT ROW_NUMBER() OVER(ORDER BY UPN DESC) AS NUMBER, ID, UPN, MAILADDRESS, PHONENUMBER, PIN, ENABLED, METHOD, OVERRIDE FROM REGISTRATIONS";
                    break;
                case DataOrderField.Email:
                    if (order.Direction == SortDirection.Ascending)
                        request = "SELECT ROW_NUMBER() OVER(ORDER BY MAILADDRESS) AS NUMBER, ID, UPN, MAILADDRESS, PHONENUMBER, PIN, ENABLED, METHOD, OVERRIDE FROM REGISTRATIONS";
                    else
                        request = "SELECT ROW_NUMBER() OVER(ORDER BY MAILADDRESS DESC) AS NUMBER, ID, UPN, MAILADDRESS, PHONENUMBER, PIN, ENABLED, METHOD, OVERRIDE FROM REGISTRATIONS";
                    break;
                case DataOrderField.Phone:
                    if (order.Direction == SortDirection.Ascending)
                        request = "SELECT ROW_NUMBER() OVER(ORDER BY PHONENUMBER) AS NUMBER, ID, UPN, MAILADDRESS, PHONENUMBER, PIN, ENABLED, METHOD, OVERRIDE FROM REGISTRATIONS";
                    else
                        request = "SELECT ROW_NUMBER() OVER(ORDER BY PHONENUMBER DESC) AS NUMBER, ID, UPN, MAILADDRESS, PHONENUMBER, PIN, ENABLED, METHOD, OVERRIDE FROM REGISTRATIONS";
                    break;
                default:
                    if (order.Direction == SortDirection.Ascending)
                        request = "SELECT ROW_NUMBER() OVER(ORDER BY ID) AS NUMBER, ID, UPN, MAILADDRESS, PHONENUMBER, PIN, ENABLED, METHOD, OVERRIDE FROM REGISTRATIONS";
                    else
                        request = "SELECT ROW_NUMBER() OVER(ORDER BY ID DESC) AS NUMBER, ID, UPN, MAILADDRESS, PHONENUMBER, PIN, ENABLED, METHOD, OVERRIDE FROM REGISTRATIONS";
                    break;
            }

            bool hasparameter = (string.Empty != filter.FilterValue);
            bool hasmethod = false;
            if (filter.FilterisActive)
            {
                if (hasparameter)
                {
                    request += " WHERE";
                    string strfields = string.Empty;
                    string stroperator = string.Empty;
                    fliedlsvalues.TryGetValue((int)filter.FilterField, out strfields);
                    request += strfields;
                    if (filter.FilterValue != null)
                        operatorsvalues.TryGetValue((int)filter.FilterOperator, out stroperator);
                    else
                        nulloperatorsvalues.TryGetValue((int)filter.FilterOperator, out stroperator);
                    request += stroperator;
                }
                if (filter.FilterMethod != PreferredMethod.None)
                {
                    string strmethod = string.Empty;
                    methodvalues.TryGetValue((int)filter.FilterMethod, out strmethod);
                    if (hasparameter)
                        request += " AND " + strmethod;
                    else
                        request += " WHERE " + strmethod;
                    hasmethod = true;
                }
                if (filter.EnabledOnly)
                {
                    if ((hasparameter) || (hasmethod))
                        request += " AND ENABLED=1";
                    else
                        request += " WHERE ENABLED=1";
                }
            }
            if (paging.IsActive)
            {
                request = "SELECT TOP " + SQLHost.MaxRows.ToString() + " NUMBER, ID, UPN, MAILADDRESS, PHONENUMBER, PIN, ENABLED, METHOD, OVERRIDE FROM (" + request;

                request += ") AS TBL WHERE NUMBER BETWEEN " + ((paging.CurrentPage - 1) * paging.PageSize + 1) + " AND  " + (paging.CurrentPage) * paging.PageSize;
            }

            SqlConnection con = new SqlConnection(SQLUtils.GetFullConnectionString(SQLHost));
            SqlCommand sql = new SqlCommand(request, con);
            if ((hasparameter) && (filter.FilterValue != null))
            {
                SqlParameter prm = new SqlParameter("@FILTERVALUE", SqlDbType.VarChar);
                sql.Parameters.Add(prm);
                prm.Value = filter.FilterValue;
            }

            MFAUserList regs = new MFAUserList();
            con.Open();
            try
            {
                int i = 0;
                SqlDataReader rd = sql.ExecuteReader();
                while (rd.Read())
                {
                    MFAUser reg = new MFAUser
                    {
                        ID = rd.GetInt64(1).ToString(),
                        UPN = rd.GetString(2)
                    };
                    if (!rd.IsDBNull(3))
                        reg.MailAddress = rd.GetString(3);
                    if (!rd.IsDBNull(4))
                        reg.PhoneNumber = rd.GetString(4);
                    reg.PIN = rd.GetInt32(5);
                    reg.Enabled = rd.GetBoolean(6);
                    reg.PreferredMethod = (PreferredMethod)rd.GetInt32(7);
                    if (!rd.IsDBNull(8))
                        reg.OverrideMethod = rd.GetString(8);
                    else
                        reg.OverrideMethod = string.Empty;
                    reg.IsRegistered = true;
                    regs.Add(reg);
                    i++;
                }
                return regs;
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
        /// GetAllMFAUsers method implementation
        /// </summary>
        public override MFAUserList GetMFAUsersAll(DataOrderObject order, bool enabledonly = false)
        {
            string request = string.Empty;
            if (enabledonly)
                request = "SELECT TOP " + SQLHost.MaxRows.ToString() + " ID, UPN, MAILADDRESS, PHONENUMBER, PIN, ENABLED, METHOD, OVERRIDE FROM REGISTRATIONS WHERE ENABLED=1";
            else
                request = "SELECT TOP " + SQLHost.MaxRows.ToString() + " ID, UPN, MAILADDRESS, PHONENUMBER, PIN, ENABLED, METHOD, OVERRIDE FROM REGISTRATIONS";

            switch (order.Column)
            {
                case DataOrderField.UserName:
                    request += " ORDER BY UPN";
                    break;
                case DataOrderField.Email:
                    request += " ORDER BY MAILADDRESS";
                    break;
                case DataOrderField.Phone:
                    request += " ORDER BY PHONENUMBER";
                    break;
                default:
                    request += " ORDER BY ID";
                    break;
            }
            if (order.Direction == SortDirection.Descending)
                request += " DESC";

            SqlConnection con = new SqlConnection(SQLUtils.GetFullConnectionString(SQLHost));
            SqlCommand sql = new SqlCommand(request, con);

            MFAUserList regs = new MFAUserList();
            con.Open();
            try
            {
                SqlDataReader rd = sql.ExecuteReader();
                while (rd.Read())
                {
                    MFAUser reg = new MFAUser
                    {
                        ID = rd.GetInt64(0).ToString(),
                        UPN = rd.GetString(1)
                    };
                    if (!rd.IsDBNull(2))
                        reg.MailAddress = rd.GetString(2);
                    if (!rd.IsDBNull(3))
                        reg.PhoneNumber = rd.GetString(3);
                    reg.PIN = rd.GetInt32(4);
                    reg.Enabled = rd.GetBoolean(5);
                    reg.PreferredMethod = (PreferredMethod)rd.GetInt32(6);
                    if (!rd.IsDBNull(7))
                        reg.OverrideMethod = rd.GetString(7);
                    else
                        reg.OverrideMethod = string.Empty;
                    reg.IsRegistered = true;
                    regs.Add(reg);
                }
                return regs;
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
        /// GetMFAUsersCount method implmentation
        /// </summary>
        public override int GetMFAUsersCount(DataFilterObject filter)
        {
            Dictionary<int, string> fliedlsvalues = new Dictionary<int, string> 
            {
                {0, " UPN "} ,
                {1, " MAILADDRESS "},
                {2, " PHONENUMBER "}
            };

            Dictionary<int, string> operatorsvalues = new Dictionary<int, string> 
            { 
                {0, " = @FILTERVALUE "},
                {1, " LIKE @FILTERVALUE +'%' "},
                {2, " LIKE '%'+ @FILTERVALUE +'%' "},
                {3, " <> @FILTERVALUE "},
                {4, " LIKE '%'+ @FILTERVALUE "},
                {5, " NOT LIKE '%' + @FILTERVALUE +'%' "}
            };

            Dictionary<int, string> nulloperatorsvalues = new Dictionary<int, string> 
            { 
                {0, " IS NULL "},
                {1, " IS NULL "},
                {2, " IS NULL "},
                {3, " IS NOT NULL "},
                {4, " IS NOT NULL "},
                {5, " IS NOT NULL "}
            };

            Dictionary<int, string> methodvalues = new Dictionary<int, string> 
            { 
                {0, " METHOD = 0 "},
                {1, " METHOD = 1 "},
                {2, " METHOD = 2 "},
                {3, " METHOD = 3 "},
                {4, " METHOD = 4 "},
                {5, " METHOD = 5 "}
            };

            string request = "SELECT COUNT(ID) FROM REGISTRATIONS";

            bool hasparameter = (string.Empty != filter.FilterValue);
            bool hasmethod = false;
            if (filter.FilterisActive)
            {
                if (hasparameter)
                {
                    request += " WHERE";
                    string strfields = string.Empty;
                    string stroperator = string.Empty;
                    fliedlsvalues.TryGetValue((int)filter.FilterField, out strfields);
                    request += strfields;
                    if (filter.FilterValue != null)
                        operatorsvalues.TryGetValue((int)filter.FilterOperator, out stroperator);
                    else
                        nulloperatorsvalues.TryGetValue((int)filter.FilterOperator, out stroperator);
                    request += stroperator;
                }
                if (filter.FilterMethod != PreferredMethod.None)
                {
                    string strmethod = string.Empty;
                    methodvalues.TryGetValue((int)filter.FilterMethod, out strmethod);
                    if (hasparameter)
                        request += " AND " + strmethod;
                    else
                        request += " WHERE " + strmethod;
                    hasmethod = true;
                }
                if (filter.EnabledOnly)
                {
                    if ((hasparameter) || (hasmethod))
                        request += " AND ENABLED=1";
                    else
                        request += " WHERE ENABLED=1";
                }
            }

            SqlConnection con = new SqlConnection(SQLUtils.GetFullConnectionString(SQLHost));
            SqlCommand sql = new SqlCommand(request, con);
            if ((hasparameter) && (filter.FilterValue != null))
            {
                SqlParameter prm = new SqlParameter("@FILTERVALUE", SqlDbType.VarChar);
                sql.Parameters.Add(prm);
                prm.Value = filter.FilterValue;
            }
            con.Open();
            try
            {
                return (int)sql.ExecuteScalar();
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
        /// IsMFAUserRegistered method implementation
        /// </summary>
        public override bool IsMFAUserRegistered(string upn)
        {
            return SQLUtils.IsMFAUserRegistered(SQLHost, upn);
        }
        #endregion

        #region IWebAuthNDataRepositoryService
        /// <summary>
        /// GetUser method implementation
        /// </summary>
        public MFAWebAuthNUser GetUser(int challengesize, string username)
        {
            MFAWebAuthNUser result = new MFAWebAuthNUser()
            {
                Id = CheckSumEncoding.EncodeUserID(challengesize, username),
                Name = username,
                DisplayName = username
            };
            return result;
        }

        /// <summary>
        /// GetUsersByCredentialId method implmentation
        /// </summary>
        public List<MFAWebAuthNUser> GetUsersByCredentialId(MFAWebAuthNUser user, byte[] credentialId)
        {
            try
            {
                List<MFAWebAuthNUser> _users = new List<MFAWebAuthNUser>();
                string credsid = HexaEncoding.GetHexStringFromByteArray(credentialId);
                MFAUserCredential cred = GetCredentialByCredentialId(user, credsid);
                if (cred != null)
                    _users.Add(user);
                return _users;
            }
            catch (Exception ex)
            {
                DataLog.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error, 5000);
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// GetCredentialsByUser method implementation
        /// </summary>
        public List<MFAUserCredential> GetCredentialsByUser(MFAWebAuthNUser user)
        {
            List<MFAUserCredential> _lst = new List<MFAUserCredential>();
            try
            {
                string request = "SELECT ID, UPN, CREDENTIALID, PUBLICKEY FROM KEYDESCS WHERE UPN=@UPN";
                SqlConnection con = new SqlConnection(SQLUtils.GetFullConnectionString(SQLHost));
                SqlCommand sql = new SqlCommand(request, con);

                SqlParameter prm = new SqlParameter("@UPN", SqlDbType.VarChar, 256);
                sql.Parameters.Add(prm);
                prm.Value = user.Name;

                WebAuthNPublicKeySerialization ser = new WebAuthNPublicKeySerialization();
                con.Open();
                try
                {
                    SqlDataReader rd = sql.ExecuteReader();
                    while (rd.Read())
                    {
                        long sqlid = rd.GetInt64(0);
                        string username = rd.GetString(1);
                        string credential = rd.GetString(2);
                        string publickey = rd.GetString(3);
                        MFAUserCredential cred = ser.DeserializeCredentials(publickey, username);
                        _lst.Add(cred);
                    }
                }
                finally
                {
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                DataLog.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error, 5000);
                throw new Exception(ex.Message);
            }
            return _lst;       
        }

        /// <summary>
        /// GetCredentialsByUserHandle method implementation
        /// </summary>
        public List<MFAUserCredential> GetCredentialsByUserHandle(MFAWebAuthNUser user, byte[] userHandle)
        {
            try
            {
                List<MFAUserCredential> _lst = GetCredentialsByUser(user);
                return _lst.Where(c => c.UserHandle.SequenceEqual(userHandle)).ToList();
            }
            catch (Exception ex)
            {
                DataLog.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error, 5000);
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// GetCredentialById method implementation
        /// </summary>
        public MFAUserCredential GetCredentialById(MFAWebAuthNUser user, byte[] credentialId)
        {
            try
            {
                string credsid = HexaEncoding.GetHexStringFromByteArray(credentialId);
                MFAUserCredential cred = GetCredentialByCredentialId(user, credsid);
                return cred;
            }
            catch (Exception ex)
            {
                DataLog.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error, 5000);
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// GetCredentialByCredentialId method implementation
        /// </summary>
        public MFAUserCredential GetCredentialByCredentialId(MFAWebAuthNUser user, string credentialid)
        {
            MFAUserCredential result = null;
            try
            {
                string request = "SELECT ID, UPN, CREDENTIALID, PUBLICKEY FROM KEYDESCS WHERE UPN=@UPN AND CREDENTIALID=@CREDENTIALID;";
                SqlConnection con = new SqlConnection(SQLUtils.GetFullConnectionString(SQLHost));
                SqlCommand sql = new SqlCommand(request, con);

                SqlParameter prm1 = new SqlParameter("@UPN", SqlDbType.VarChar, 256);
                sql.Parameters.Add(prm1);
                prm1.Value = user.Name;

                SqlParameter prm2 = new SqlParameter("@CREDENTIALID", SqlDbType.VarChar, 256);
                sql.Parameters.Add(prm2);
                prm2.Value = credentialid;

                WebAuthNPublicKeySerialization ser = new WebAuthNPublicKeySerialization();
                con.Open();
                try
                {
                    SqlDataReader rd = sql.ExecuteReader();
                    if (rd.Read())
                    {
                        long sqlid = rd.GetInt64(0);
                        string username = rd.GetString(1);
                        string credential = rd.GetString(2);
                        string publickey = rd.GetString(3);
                        MFAUserCredential cred = ser.DeserializeCredentials(publickey, username);
                        result = cred;
                    }
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
            catch (Exception ex)
            {
                DataLog.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error, 5000);
                throw new Exception(ex.Message);
            }
            return result;
        }


        /// <summary>
        /// UpdateCounter method implementation
        /// </summary>
        public void UpdateCounter(MFAWebAuthNUser user, byte[] credentialId, uint counter)
        {
            try
            {
                string credsid = HexaEncoding.GetHexStringFromByteArray(credentialId);
                MFAUserCredential cred = GetCredentialByCredentialId(user, credsid);
                if (cred != null)
                {
                    cred.SignatureCounter = counter;
                    SetUserCredential(user, cred);
                }
            }
            catch (Exception ex)
            {
                DataLog.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error, 5000);
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// AddCredentialToUser method implementation
        /// </summary>
        public bool AddUserCredential(MFAWebAuthNUser user, MFAUserCredential credential)
        {
            credential.UserId = user.Id;
            WebAuthNPublicKeySerialization ser = new WebAuthNPublicKeySerialization();
            string value = ser.SerializeCredentials(credential, user.Name);
            string credsid = HexaEncoding.GetHexStringFromByteArray(credential.Descriptor.Id);
            try
            {
                string request = "INSERT INTO KEYDESCS (UPN, CREDENTIALID, PUBLICKEY) VALUES (@UPN, @CREDENTIALID, @PUBLICKEY);";
                SqlConnection con = new SqlConnection(SQLUtils.GetFullConnectionString(SQLHost));
                SqlCommand sql = new SqlCommand(request, con);

                SqlParameter prm1 = new SqlParameter("@UPN", SqlDbType.VarChar, 256);
                sql.Parameters.Add(prm1);
                prm1.Value = user.Name;

                SqlParameter prm2 = new SqlParameter("@CREDENTIALID", SqlDbType.VarChar, 256);
                sql.Parameters.Add(prm2);
                prm2.Value = credsid;

                SqlParameter prm3 = new SqlParameter("@PUBLICKEY", SqlDbType.VarChar, 8000);
                sql.Parameters.Add(prm3);
                prm3.Value = value;

                con.Open();
                try
                {
                    int res = sql.ExecuteNonQuery();
                    if (res == 1)
                        return true;
                    else
                        return false;
                }
                finally
                {
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                DataLog.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error, 5000);
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// SetUserCredential method implementation
        /// </summary>
        public bool SetUserCredential(MFAWebAuthNUser user, MFAUserCredential credential)
        {
            credential.UserId = user.Id;
            WebAuthNPublicKeySerialization ser = new WebAuthNPublicKeySerialization();
            string newcredid = ser.SerializeCredentials(credential, user.Name);
            string credsid = HexaEncoding.GetHexStringFromByteArray(credential.Descriptor.Id);
            try
            {

                string request = "UPDATE KEYDESCS SET PUBLICKEY = @PUBLICKEY WHERE UPN=@UPN AND CREDENTIALID=@CREDENTIALID;";
                SqlConnection con = new SqlConnection(SQLUtils.GetFullConnectionString(SQLHost));
                SqlCommand sql = new SqlCommand(request, con);

                SqlParameter prm1 = new SqlParameter("@UPN", SqlDbType.VarChar, 256);
                sql.Parameters.Add(prm1);
                prm1.Value = user.Name;

                SqlParameter prm2 = new SqlParameter("@CREDENTIALID", SqlDbType.VarChar, 256);
                sql.Parameters.Add(prm2);
                prm2.Value = credsid;

                SqlParameter prm3 = new SqlParameter("@PUBLICKEY", SqlDbType.VarChar, 8000);
                sql.Parameters.Add(prm3);
                prm3.Value = newcredid;

                con.Open();
                try
                {
                    int res = sql.ExecuteNonQuery();
                    if (res == 1)
                        return true;
                    else
                        return false;
                }
                finally
                {
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                DataLog.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error, 5000);
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// RemoveCredentialToUser method implementation
        /// </summary>
        public bool RemoveUserCredential(MFAWebAuthNUser user, string credentialid)
        {
            try
            {
                string request = "DELETE FROM KEYDESCS WHERE UPN=@UPN AND CREDENTIALID=@CREDENTIALID;";
                SqlConnection con = new SqlConnection(SQLUtils.GetFullConnectionString(SQLHost));
                SqlCommand sql = new SqlCommand(request, con);

                SqlParameter prm1 = new SqlParameter("@UPN", SqlDbType.VarChar, 256);
                sql.Parameters.Add(prm1);
                prm1.Value = user.Name;

                SqlParameter prm2 = new SqlParameter("@CREDENTIALID", SqlDbType.VarChar, 256);
                sql.Parameters.Add(prm2);
                prm2.Value = credentialid;

                con.Open();
                try
                {
                    int res = sql.ExecuteNonQuery();
                    if (res == 1)
                        return true;
                    else
                        return false;
                }
                finally
                {
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                DataLog.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error, 5000);
                throw new Exception(ex.Message);
            }
        }
        #endregion

        #region IDataRepositorySQLConnection
        /// <summary>
        /// CheckConnection method implementation
        /// </summary>
        public bool CheckConnection(string connectionstring, string username, string password)
        {
            SqlConnection con;
            if (string.IsNullOrEmpty(connectionstring))
                return false;
            string resconnectionstring = SQLUtils.GetFullConnectionString(SQLHost, connectionstring, username, password);
            if (!resconnectionstring.ToLower().Contains("connection timeout="))
                resconnectionstring += "Connection Timeout=2;";
            con = new SqlConnection(resconnectionstring);
            try
            {
                con.Open();
                return true;
            }
            catch (Exception ex)
            {
                DataLog.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error, 5000);
                return false;
            }
            finally
            {
                con.Close();
            }
        }

        #endregion
    }
    #endregion

    #region SQL Keys Repository
    internal class SQLKeysRepositoryService : KeysRepositoryService, IDataRepositorySQLConnection
    {
        /// <summary>
        /// ADDSKeysRepositoryService constructor
        /// </summary>
        public SQLKeysRepositoryService(BaseDataHost host, int deliverywindow):base(host, deliverywindow)
        {
            if (!(host is SQLServerHost))
                throw new ArgumentException("Invalid Host ! : value but be an SQLServerHost instance");
        }

        private SQLServerHost SQLHost
        {
            get { return (SQLServerHost)Host; }
        }

        #region Key Management
        /// <summary>
        /// GetUserKey method implmentation
        /// </summary>
        public override string GetUserKey(string upn)
        {
            string request = "SELECT SECRETKEY FROM REGISTRATIONS WHERE UPN=@UPN";
            SqlConnection con = new SqlConnection(SQLUtils.GetFullConnectionString(SQLHost));
            SqlCommand sql = new SqlCommand(request, con);

            SqlParameter prm = new SqlParameter("@UPN", SqlDbType.VarChar, 256);
            sql.Parameters.Add(prm);
            prm.Value = upn;

            string ret = string.Empty;
            con.Open();
            try
            {
                SqlDataReader rd = sql.ExecuteReader();
                if (rd.Read())
                {
                    if (!rd.IsDBNull(0))
                        ret = rd.GetString(0);
                    return ret;
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
        public override string NewUserKey(string upn, string secretkey, X509Certificate2 cert = null)
        {
            try
            {
                if (SQLUtils.IsMFAUserRegistered(SQLHost, upn.ToLower()))
                    return DoUpdateUserKey(upn.ToLower(), secretkey);
                else
                    return DoInsertUserKey(upn.ToLower(), secretkey);
            }
            catch (Exception ex)
            {
                DataLog.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error, 5000);
                throw ex;
            }
        }

        /// <summary>
        /// RemoveUserKey method implmentation
        /// </summary>
        public override bool RemoveUserKey(string upn)
        {
            string request1 = "UPDATE REGISTRATIONS SET SECRETKEY = NULL WHERE UPN=@UPN";
            string request2 = "DELETE FROM KEYS WHERE UPN=@UPN AND KIND=1";
            string request3 = "DELETE FROM KEYDESCS WHERE UPN=@UPN";

            SqlConnection con = new SqlConnection(SQLUtils.GetFullConnectionString(SQLHost));
            con.Open();
            SqlTransaction trans = con.BeginTransaction();
            try
            {

                SqlCommand sql1 = new SqlCommand(request1, con, trans);
                SqlCommand sql2 = new SqlCommand(request2, con, trans);
                SqlCommand sql3 = new SqlCommand(request3, con, trans);

                SqlParameter prm1 = new SqlParameter("@UPN", SqlDbType.VarChar, 256);
                sql1.Parameters.Add(prm1);
                prm1.Value = upn.ToLower();

                SqlParameter prm2 = new SqlParameter("@UPN", SqlDbType.VarChar, 256);
                sql2.Parameters.Add(prm2);
                prm2.Value = upn.ToLower();

                SqlParameter prm3 = new SqlParameter("@UPN", SqlDbType.VarChar, 256);
                sql3.Parameters.Add(prm3);
                prm3.Value = upn.ToLower();


                int res1 = sql1.ExecuteNonQuery();
                int res2 = sql2.ExecuteNonQuery();
                int res3 = sql3.ExecuteNonQuery();
                return (res1 == 1);
            }
            catch (Exception ex)
            {
                DataLog.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error, 5000);
                trans.Rollback();
                throw new Exception(ex.Message);
            }
            finally
            {
                trans.Commit();
                con.Close();
            }
        }

        /// <summary>
        /// GetUserCertificate implementation
        /// </summary>
        public override X509Certificate2 GetUserCertificate(string upn, string password)
        {
            return null;
        }

        /// <summary>
        /// CreateCertificate implementation
        /// </summary>
        public override X509Certificate2 CreateCertificate(string upn, string password, int validity)
        {
            return null;
        }

        /// <summary>
        /// HasStoredKey implementation
        /// </summary>
        public override bool HasStoredKey(string upn)
        {
            return false;
        }

        /// <summary>
        /// HasStoredCertificate method implmentation
        /// </summary>
        public override bool HasStoredCertificate(string upn)
        {
            return false;
        }
        #endregion

        #region private methods
        /// <summary>
        /// DoUpdateUserKey method implmentation
        /// </summary>
        private string DoUpdateUserKey(string upn, string secretkey)
        {
            string request = "UPDATE REGISTRATIONS SET SECRETKEY = @SECRETKEY WHERE UPN=@UPN";

            SqlConnection con = new SqlConnection(SQLUtils.GetFullConnectionString(SQLHost));
            SqlCommand sql = new SqlCommand(request, con);

            SqlParameter prm1 = new SqlParameter("@SECRETKEY", SqlDbType.VarChar, 8000);
            sql.Parameters.Add(prm1);
            if (string.IsNullOrEmpty(secretkey))
                prm1.Value = string.Empty;
            else
                prm1.Value = secretkey;

            SqlParameter prm2 = new SqlParameter("@UPN", SqlDbType.VarChar, 256);
            sql.Parameters.Add(prm2);
            prm2.Value = upn.ToLower();
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
        /// DoUpdateUserKey method implmentation
        /// </summary>
        private string DoInsertUserKey(string upn, string secretkey)
        {
            string request = "INSERT INTO REGISTRATIONS (UPN, SECRETKEY, METHOD, OVERRIDE, PIN, ENABLED) VALUES (@UPN, @SECRETKEY, 0, null, 0, 1)";

            SqlConnection con = new SqlConnection(SQLUtils.GetFullConnectionString(SQLHost));
            SqlCommand sql = new SqlCommand(request, con);

            SqlParameter prm1 = new SqlParameter("@SECRETKEY", SqlDbType.VarChar, 8000);
            sql.Parameters.Add(prm1);
            if (string.IsNullOrEmpty(secretkey))
                prm1.Value = string.Empty;
            else
                prm1.Value = secretkey;

            SqlParameter prm2 = new SqlParameter("@UPN", SqlDbType.VarChar, 256);
            sql.Parameters.Add(prm2);
            prm2.Value = upn;
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

        /// <summary>
        /// CheckConnection method implementation
        /// </summary>
        public bool CheckConnection(string connectionstring, string username, string password)
        {
            SqlConnection con;
            if (string.IsNullOrEmpty(connectionstring))
                return false;
            string resconnectionstring = SQLUtils.GetFullConnectionString(SQLHost, connectionstring, username, password);
            if (!resconnectionstring.ToLower().Contains("connection timeout="))
                resconnectionstring += "Connection Timeout=2;";
            con = new SqlConnection(resconnectionstring);
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
    }
    #endregion

    #region SQL Keys2 Repository
    internal class SQLKeys2RepositoryService : KeysRepositoryService, IDataRepositorySQLConnection
    {
        /// <summary>
        /// ADDSKeysRepositoryService constructor
        /// </summary>
        public SQLKeys2RepositoryService(BaseDataHost host, int deliverywindow ): base(host, deliverywindow)
        {
            if (!(host is SQLServerHost))
                throw new ArgumentException("Invalid Host ! : value but be an SQLServerHost instance");
        }

        /// <summary>
        /// SQLHost property
        /// </summary>
        private SQLServerHost SQLHost
        {
            get { return (SQLServerHost)Host; }
        }

        #region Key Management
        /// <summary>
        /// GetUserKey method implmentation
        /// </summary>
        public override string GetUserKey(string upn)
        {
            string request = "SELECT SECRETKEY FROM REGISTRATIONS WHERE UPN=@UPN";
            SqlConnection con = new SqlConnection(SQLUtils.GetFullConnectionString(SQLHost));
            SqlCommand sql = new SqlCommand(request, con);

            SqlParameter prm = new SqlParameter("@UPN", SqlDbType.VarChar, 256);
            sql.Parameters.Add(prm);
            prm.Value = upn;

            string ret = string.Empty;
            con.Open();
            try
            {
                SqlDataReader rd = sql.ExecuteReader();
                if (rd.Read())
                {
                    if (!rd.IsDBNull(0))
                        ret = rd.GetString(0);
                    return ret;
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
        public override string NewUserKey(string upn, string secretkey, X509Certificate2 cert = null)
        {
            try
            {
                if (SQLUtils.IsMFAUserRegistered(SQLHost, upn.ToLower()))
                    DoUpdateUserKey(upn.ToLower(), secretkey);
                else
                    DoInsertUserKey(upn.ToLower(), secretkey);
                if (cert != null)
                {
                    if (HasStoredCertificate(upn.ToLower()))
                        DoUpdateUserCertificate(upn.ToLower(), cert);
                    else
                        DoInsertUserCertificate(upn.ToLower(), cert);
                }
                return secretkey;
            }
            catch (Exception ex)
            {
                DataLog.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error, 5000);
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// RemoveUserKey method implmentation
        /// </summary>
        public override bool RemoveUserKey(string upn)
        {
            string request1 = "UPDATE REGISTRATIONS SET SECRETKEY = NULL WHERE UPN=@UPN";
            string request2 = "DELETE FROM KEYS WHERE UPN=@UPN AND KIND=1";
            string request3 = "DELETE FROM KEYDESCS WHERE UPN=@UPN";

            SqlConnection con = new SqlConnection(SQLUtils.GetFullConnectionString(SQLHost));
            con.Open();
            SqlTransaction trans = con.BeginTransaction();
            try
            {

                SqlCommand sql1 = new SqlCommand(request1, con, trans);
                SqlCommand sql2 = new SqlCommand(request2, con, trans);
                SqlCommand sql3 = new SqlCommand(request3, con, trans);

                SqlParameter prm1 = new SqlParameter("@UPN", SqlDbType.VarChar, 256);
                sql1.Parameters.Add(prm1);
                prm1.Value = upn.ToLower();

                SqlParameter prm2 = new SqlParameter("@UPN", SqlDbType.VarChar, 256);
                sql2.Parameters.Add(prm2);
                prm2.Value = upn.ToLower();

                SqlParameter prm3 = new SqlParameter("@UPN", SqlDbType.VarChar, 256);
                sql3.Parameters.Add(prm3);
                prm3.Value = upn.ToLower();


                int res1 = sql1.ExecuteNonQuery();
                int res2 = sql2.ExecuteNonQuery();
                int res3 = sql3.ExecuteNonQuery();
                return ((res1 == 1) && (res2 == 1));
            }
            catch (Exception ex)
            {
                DataLog.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error, 5000);
                trans.Rollback();
                throw new Exception(ex.Message);
            }
            finally
            {
                trans.Commit();
                con.Close();
            }
        }

        /// <summary>
        /// GetUserCertificate implementation
        /// </summary>
        public override X509Certificate2 GetUserCertificate(string upn, string password)
        {
            string request = "SELECT CERTIFICATE FROM KEYS WHERE UPN=@UPN AND KIND=1";
            SqlConnection con = new SqlConnection(SQLUtils.GetFullConnectionString(SQLHost));
            SqlCommand sql = new SqlCommand(request, con);

            SqlParameter prm = new SqlParameter("@UPN", SqlDbType.VarChar, 256);
            sql.Parameters.Add(prm);
            prm.Value = upn;

            string ret = string.Empty;
            con.Open();
            try
            {
                SqlDataReader rd = sql.ExecuteReader();
                if (rd.Read())
                {
                    string pass = string.Empty;
                    if (!string.IsNullOrEmpty(password))
                        pass = password;
                    if (!rd.IsDBNull(0))
                    {
                        string strcert = rd.GetString(0);
                        X509Certificate2 cert = new X509Certificate2(Convert.FromBase64String(strcert), pass, X509KeyStorageFlags.EphemeralKeySet);
                       // X509Certificate2 cert = new X509Certificate2(Convert.FromBase64String(strcert), pass, X509KeyStorageFlags.Exportable | X509KeyStorageFlags.EphemeralKeySet); 
                        return cert;
                    }
                    else
                        return null;
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
        /// CreateCertificate implementation
        /// </summary>
        public override X509Certificate2 CreateCertificate(string upn, string password, int validity)
        {
            try
            {
                string pass = string.Empty;
                if (!string.IsNullOrEmpty(password))
                    pass = password;
                return Certs.CreateRSAEncryptionCertificateForUser(upn.ToLower(), validity, pass);
            }
            catch (Exception ex)
            {
                DataLog.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error, 5000);
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// HasStoredKey implementation
        /// </summary>
        public override bool HasStoredKey(string upn)
        {
            string request = "SELECT SECRETKEY FROM REGISTRATIONS WHERE UPN=@UPN";
            SqlConnection con = new SqlConnection(SQLUtils.GetFullConnectionString(SQLHost));
            SqlCommand sql = new SqlCommand(request, con);

            SqlParameter prm = new SqlParameter("@UPN", SqlDbType.VarChar, 256);
            sql.Parameters.Add(prm);
            prm.Value = upn;

            string ret = string.Empty;
            con.Open();
            try
            {
                SqlDataReader rd = sql.ExecuteReader();
                if (rd.Read())
                {
                    return (!rd.IsDBNull(0));
                }
                else
                    return false;
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
        /// HasStoredCertificate method implmentation
        /// </summary>
        public override bool HasStoredCertificate(string upn)
        {
            string request = "SELECT CERTIFICATE FROM KEYS WHERE UPN=@UPN AND KIND=1";
            SqlConnection con = new SqlConnection(SQLUtils.GetFullConnectionString(SQLHost));
            SqlCommand sql = new SqlCommand(request, con);

            SqlParameter prm = new SqlParameter("@UPN", SqlDbType.VarChar, 256);
            sql.Parameters.Add(prm);
            prm.Value = upn;

            string ret = string.Empty;
            con.Open();
            try
            {
                SqlDataReader rd = sql.ExecuteReader();
                if (rd.Read())
                {
                    return (!rd.IsDBNull(0));
                }
                else
                    return false;
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

        #region private methods
        /// <summary>
        /// DoUpdateUserKey method implmentation
        /// </summary>
        private void DoUpdateUserKey(string upn, string secretkey)
        {
            string request1 = "UPDATE REGISTRATIONS SET SECRETKEY = @SECRETKEY WHERE UPN=@UPN";

            SqlConnection con = new SqlConnection(SQLUtils.GetFullConnectionString(SQLHost));
            con.Open();
            SqlTransaction trans = con.BeginTransaction();
            try
            {
                SqlCommand sql1 = new SqlCommand(request1, con, trans);

                SqlParameter prm1 = new SqlParameter("@SECRETKEY", SqlDbType.VarChar, 8000);
                sql1.Parameters.Add(prm1);
                prm1.Value = secretkey;              

                SqlParameter prm2 = new SqlParameter("@UPN", SqlDbType.VarChar, 256);
                sql1.Parameters.Add(prm2);
                prm2.Value = upn.ToLower();

                int res1 = sql1.ExecuteNonQuery();
                trans.Commit();
            }
            catch (Exception ex)
            {
                DataLog.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error, 5000);
                trans.Rollback();
                throw new Exception(ex.Message);
            }
            finally
            {
                con.Close();
            }
            return;
        }

        /// <summary>
        /// DoUpdateUserCertificate method implmentation
        /// </summary>
        private void DoUpdateUserCertificate(string upn, X509Certificate2 certificate)
        {
            string request2 = "UPDATE KEYS SET CERTIFICATE = @CERTIFICATE WHERE UPN=@UPN AND KIND=1";

            SqlConnection con = new SqlConnection(SQLUtils.GetFullConnectionString(SQLHost));
            con.Open();
            SqlTransaction trans = con.BeginTransaction();
            try
            {
                SqlCommand sql2 = new SqlCommand(request2, con, trans);

                SqlParameter prm3 = new SqlParameter("@CERTIFICATE", SqlDbType.VarChar, 8000);
                sql2.Parameters.Add(prm3);
                prm3.Value = Convert.ToBase64String(certificate.Export(X509ContentType.Pfx, CheckSumEncoding.CheckSumAsString(upn)));

                SqlParameter prm4 = new SqlParameter("@UPN", SqlDbType.VarChar, 256);
                sql2.Parameters.Add(prm4);
                prm4.Value = upn.ToLower();

                int res2 = sql2.ExecuteNonQuery();
                trans.Commit();
            }
            catch (Exception ex)
            {
                DataLog.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error, 5000);
                trans.Rollback();
                throw new Exception(ex.Message);
            }
            finally
            {
                certificate.Reset();                
                con.Close();
            }
            return;
        }

        /// <summary>
        /// DoUpdateUserKey method implmentation
        /// </summary>
        private void DoInsertUserKey(string upn, string secretkey)
        {
            string request1 = "INSERT INTO REGISTRATIONS (UPN, SECRETKEY, METHOD, OVERRIDE, PIN, ENABLED) VALUES (@UPN, @SECRETKEY, 0, null, 0, 1)";

            SqlConnection con = new SqlConnection(SQLUtils.GetFullConnectionString(SQLHost));
            con.Open();
            SqlTransaction trans = con.BeginTransaction();
            try
            {
                SqlCommand sql1 = new SqlCommand(request1, con, trans);

                SqlParameter prm1 = new SqlParameter("@SECRETKEY", SqlDbType.VarChar, 8000);
                sql1.Parameters.Add(prm1);
                prm1.Value = secretkey;
                
                SqlParameter prm2 = new SqlParameter("@UPN", SqlDbType.VarChar, 256);
                sql1.Parameters.Add(prm2);
                prm2.Value = upn.ToLower();

                int res1 = sql1.ExecuteNonQuery();
                trans.Commit();
            }
            catch (Exception ex)
            {
                DataLog.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error, 5000);
                trans.Rollback();
                throw new Exception(ex.Message);
            }
            finally
            {
                con.Close();
            }
            return;
        }

        /// <summary>
        /// DoInsertUserCertificate method implmentation
        /// </summary>
        private void DoInsertUserCertificate(string upn, X509Certificate2 certificate)
        {
            string request2 = "INSERT INTO KEYS (UPN, CERTIFICATE, KIND) VALUES (@UPN, @CERTIFICATE, 1)";

            SqlConnection con = new SqlConnection(SQLUtils.GetFullConnectionString(SQLHost));
            con.Open();
            SqlTransaction trans = con.BeginTransaction();
            try
            {
                SqlCommand sql2 = new SqlCommand(request2, con, trans);

                SqlParameter prm3 = new SqlParameter("@CERTIFICATE", SqlDbType.VarChar, 8000);
                sql2.Parameters.Add(prm3);
                prm3.Value = Convert.ToBase64String(certificate.Export(X509ContentType.Pfx, CheckSumEncoding.CheckSumAsString(upn)));

                SqlParameter prm4 = new SqlParameter("@UPN", SqlDbType.VarChar, 256);
                sql2.Parameters.Add(prm4);
                prm4.Value = upn.ToLower();

                int res2 = sql2.ExecuteNonQuery();
                trans.Commit();
            }
            catch (Exception ex)
            {
                DataLog.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error, 5000);
                trans.Rollback();
                throw new Exception(ex.Message);
            }
            finally
            {
                certificate.Reset();
                con.Close();
            }
            return;
        }

        #endregion

        /// <summary>
        /// CheckConnection method implementation
        /// </summary>
        public bool CheckConnection(string connectionstring, string username, string password)
        {
            SqlConnection con;
            if (string.IsNullOrEmpty(connectionstring))
                return false;
            string resconnectionstring = SQLUtils.GetFullConnectionString(SQLHost, connectionstring, username, password);
            if (!resconnectionstring.ToLower().Contains("connection timeout="))
                resconnectionstring += "Connection Timeout=2;";
            con = new SqlConnection(resconnectionstring);
            try
            {
                con.Open();
                return true;
            }
            catch (Exception ex)
            {
                DataLog.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error, 5000);
                return false;
            }
            finally
            {
                con.Close();
            }
        }
    }
    #endregion

    #region SQL Utils
    internal static class SQLUtils
    {
        /// <summary>
        /// IsMFAUserRegistered method implementation
        /// </summary>
        internal static bool IsMFAUserRegistered(SQLServerHost SQLHost, string upn)
        {
            string request = "SELECT ID, UPN FROM REGISTRATIONS WHERE UPN=@UPN";

            SqlConnection con = new SqlConnection(SQLUtils.GetFullConnectionString(SQLHost));
            SqlCommand sql = new SqlCommand(request, con);

            SqlParameter prm = new SqlParameter("@UPN", SqlDbType.VarChar);
            sql.Parameters.Add(prm);
            prm.Value = upn;
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
        /// GetFullConnectionString method implementation
        /// </summary>
        /// <returns></returns>
        internal static string GetFullConnectionString(SQLServerHost SQLHost, string connectstr = null, string account = null, string password = null)
        {
            try
            {
                string connectionstring = string.Empty;
                string[] parts = null;
                if (string.IsNullOrEmpty(connectstr))
                    parts = SQLHost.ConnectionString.Split(';');
                else
                    parts = connectstr.Split(';');
                for (int i = 0; i < parts.Length; i++)
                {
                    if (parts[i].ToLower().StartsWith("user id"))
                        parts[i] = string.Empty;
                    else if (parts[i].ToLower().StartsWith("password"))
                        parts[i] = string.Empty;
                    else if (parts[i].ToLower().StartsWith("integrated security"))
                        parts[i] = string.Empty;
                }
                for (int i = 0; i < parts.Length; i++)
                {
                    if (!string.IsNullOrEmpty(parts[i]))
                        connectionstring += parts[i] + ";";
                }

                if (string.IsNullOrEmpty(SQLHost.SQLAccount) || string.IsNullOrEmpty(SQLHost.SQLPassword))
                    connectionstring += "Integrated Security=SSPI;";
                else
                {
                    if (string.IsNullOrEmpty(account))
                        connectionstring += "User ID=" + SQLHost.SQLAccount + ";Password=" + SQLHost.SQLPassword + ";";
                    else
                        connectionstring += "User ID=" + account + ";";
                    if (string.IsNullOrEmpty(password))
                        connectionstring += "Password=" + SQLHost.SQLPassword + ";";
                    else
                        connectionstring += "Password=" + password + ";";
                }
                return connectionstring;
            }
            catch (Exception ex)
            {
                DataLog.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error, 5000);
                throw new Exception(ex.Message);
            }
        }
    }
    #endregion
}
