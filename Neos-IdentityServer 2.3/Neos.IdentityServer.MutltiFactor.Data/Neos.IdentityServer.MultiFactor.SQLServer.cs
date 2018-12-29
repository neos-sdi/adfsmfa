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
using System.Data;
using System.Data.SqlClient;
using System.DirectoryServices;
using System.Security.Cryptography.X509Certificates;

namespace Neos.IdentityServer.MultiFactor.Data
{
    internal class SQLDataRepositoryService : DataRepositoryService, IDataRepositorySQLConnection
    {
        private SQLServerHost _host;
        private string _connectionstring;
        private int _deliverywindow = 300;

        public override event KeysDataManagerEvent OnKeyDataEvent;

        /// <summary>
        /// BaseSQLAdminService constructor
        /// </summary>
        public SQLDataRepositoryService(SQLServerHost host, int deliverywindow = 3000): base()
        {
            _host = host;
            _connectionstring = _host.ConnectionString;
            _deliverywindow = deliverywindow;
        }

        #region DataRepositoryService
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

        /// <summary>
        /// GetUserRegistration method implementation
        /// </summary>
        public override Registration GetUserRegistration(string upn)
        {
            string request = "SELECT ID, UPN, MAILADDRESS, PHONENUMBER, PIN, ENABLED, METHOD, OVERRIDE FROM REGISTRATIONS WHERE UPN=@UPN";
            SqlConnection con = new SqlConnection(_connectionstring);
            SqlCommand sql = new SqlCommand(request, con);

            SqlParameter prm = new SqlParameter("@UPN", SqlDbType.VarChar);
            sql.Parameters.Add(prm);
            prm.Value = upn;

            Registration reg = new Registration();
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
        /// SetUserRegistration method implementation
        /// </summary>
        public override Registration SetUserRegistration(Registration reg, bool resetkey = false, bool caninsert = true, bool disableoninsert = false)
        {
            if (!SQLUtils.HasRegistration(_host, reg.UPN))
                if (caninsert)
                    return AddUserRegistration(reg, resetkey, false);
                else
                    return GetUserRegistration(reg.UPN);
            string request;
            if (disableoninsert)
               request = "UPDATE REGISTRATIONS SET MAILADDRESS = @MAILADDRESS, PHONENUMBER = @PHONENUMBER, PIN=@PIN, METHOD=@METHOD, OVERRIDE=@OVERRIDE, WHERE UPN=@UPN";
            else
                request = "UPDATE REGISTRATIONS SET MAILADDRESS = @MAILADDRESS, PHONENUMBER = @PHONENUMBER, PIN=@PIN, METHOD=@METHOD, OVERRIDE=@OVERRIDE, ENABLED=@ENABLED WHERE UPN=@UPN";

            SqlConnection con = new SqlConnection(_connectionstring);
            SqlCommand sql = new SqlCommand(request, con);

            SqlParameter prm1 = new SqlParameter("@MAILADDRESS", SqlDbType.VarChar);
            sql.Parameters.Add(prm1);
            if (string.IsNullOrEmpty(reg.MailAddress))
                prm1.Value = DBNull.Value;
            else
                prm1.Value = reg.MailAddress;

            SqlParameter prm2 = new SqlParameter("@PHONENUMBER", SqlDbType.VarChar);
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

            SqlParameter prm7 = new SqlParameter("@UPN", SqlDbType.VarChar);
            sql.Parameters.Add(prm7);
            prm7.Value = reg.UPN;
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
            return GetUserRegistration(reg.UPN);
        }

        /// <summary>
        /// AddUserRegistration method implementation
        /// </summary>
        public override Registration AddUserRegistration(Registration reg, bool resetkey = true, bool canupdate = true, bool disableoninsert = false)
        {
            if (SQLUtils.HasRegistration(_host, reg.UPN))
                if (canupdate)
                    return SetUserRegistration(reg, resetkey, false);
                else
                    return GetUserRegistration(reg.UPN);

            string request = "INSERT INTO REGISTRATIONS (UPN, SECRETKEY, MAILADDRESS, PHONENUMBER, PIN, ENABLED, METHOD, OVERRIDE) VALUES (@UPN, @SECRETKEY, @MAILADDRESS, @PHONENUMBER, @PIN, @ENABLED, @METHOD, @OVERRIDE)";

            SqlConnection con = new SqlConnection(_connectionstring);
            SqlCommand sql = new SqlCommand(request, con);

            SqlParameter prm1 = new SqlParameter("@UPN", SqlDbType.VarChar);
            sql.Parameters.Add(prm1);
            prm1.Value = reg.UPN;

            SqlParameter prm1a = new SqlParameter("@SECRETKEY", SqlDbType.VarChar);
            sql.Parameters.Add(prm1a);
            prm1a.Value = Guid.NewGuid().ToString().ToUpper();

            SqlParameter prm2 = new SqlParameter("@MAILADDRESS", SqlDbType.VarChar);
            sql.Parameters.Add(prm2);
            if (string.IsNullOrEmpty(reg.MailAddress))
                prm2.Value = DBNull.Value;
            else
                prm2.Value = reg.MailAddress;

            SqlParameter prm3 = new SqlParameter("@PHONENUMBER", SqlDbType.VarChar);
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
            return GetUserRegistration(reg.UPN);
        }

        /// <summary>
        /// DeleteUserRegistration method implementation
        /// </summary>
        public override bool DeleteUserRegistration(Registration reg, bool dropkey = true)
        {
            if (!SQLUtils.HasRegistration(_host, reg.UPN))
                throw new Exception("The user " + reg.UPN + " cannot be deleted ! \r User not found !");

            if (dropkey)
                this.OnKeyDataEvent(reg.UPN, KeysDataManagerEventKind.Remove);

            string request = "DELETE FROM REGISTRATIONS WHERE UPN=@UPN";

            SqlConnection con = new SqlConnection(_connectionstring);
            SqlCommand sql = new SqlCommand(request, con);

            SqlParameter prm = new SqlParameter("@UPN", SqlDbType.VarChar);
            sql.Parameters.Add(prm);
            prm.Value = reg.UPN;
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
        /// EnableUserRegistration method implementation
        /// </summary>
        public override Registration EnableUserRegistration(Registration reg)
        {
            if (!SQLUtils.HasRegistration(_host, reg.UPN))
                throw new Exception("The user " + reg.UPN + " cannot be updated ! \r User not found !");

            string request = "UPDATE REGISTRATIONS SET ENABLED=1 WHERE UPN=@UPN";

            SqlConnection con = new SqlConnection(_connectionstring);
            SqlCommand sql = new SqlCommand(request, con);

            SqlParameter prm5 = new SqlParameter("@UPN", SqlDbType.VarChar);
            sql.Parameters.Add(prm5);
            prm5.Value = reg.UPN;
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
        /// DisableUserRegistration method implementation
        /// </summary>
        public override Registration DisableUserRegistration(Registration reg)
        {
            if (!SQLUtils.HasRegistration(_host, reg.UPN))
                throw new Exception("The user " + reg.UPN + " cannot be updated ! \r User not found !");

            string request = "UPDATE REGISTRATIONS SET ENABLED=0 WHERE UPN=@UPN";

            SqlConnection con = new SqlConnection(_connectionstring);
            SqlCommand sql = new SqlCommand(request, con);

            SqlParameter prm5 = new SqlParameter("@UPN", SqlDbType.VarChar);
            sql.Parameters.Add(prm5);
            prm5.Value = reg.UPN;
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
        /// GetUserRegistrations method implementation
        /// </summary>
        public override RegistrationList GetUserRegistrations(DataFilterObject filter, DataOrderObject order, DataPagingObject paging)
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
                {4, " METHOD = 4 "}
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
            if (paging.isActive)
            {
                request = "SELECT TOP " + _host.MaxRows.ToString() + " NUMBER, ID, UPN, MAILADDRESS, PHONENUMBER, PIN, ENABLED, METHOD, OVERRIDE FROM (" + request;

                request += ") AS TBL WHERE NUMBER BETWEEN " + ((paging.CurrentPage - 1) * paging.PageSize + 1) + " AND  " + (paging.CurrentPage) * paging.PageSize;
            }

            SqlConnection con = new SqlConnection(_connectionstring);
            SqlCommand sql = new SqlCommand(request, con);
            if ((hasparameter) && (filter.FilterValue != null))
            {
                SqlParameter prm = new SqlParameter("@FILTERVALUE", SqlDbType.VarChar);
                sql.Parameters.Add(prm);
                prm.Value = filter.FilterValue;
            }

            RegistrationList regs = new RegistrationList();
            con.Open();
            try
            {
                int i = 0;
                SqlDataReader rd = sql.ExecuteReader();
                while (rd.Read())
                {
                    Registration reg = new Registration();
                    reg.ID = rd.GetInt64(1).ToString();
                    reg.UPN = rd.GetString(2);
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
        /// GetAllUserRegistrations method implementation
        /// </summary>
        public override RegistrationList GetAllUserRegistrations(DataOrderObject order, bool enabledonly = false)
        {
            string request = string.Empty;
            if (enabledonly)
                request = "SELECT TOP " + _host.MaxRows.ToString() + " ID, UPN, MAILADDRESS, PHONENUMBER, PIN, ENABLED, METHOD, OVERRIDE FROM REGISTRATIONS WHERE ENABLED=1";
            else
                request = "SELECT TOP " + _host.MaxRows.ToString() + " ID, UPN, MAILADDRESS, PHONENUMBER, PIN, ENABLED, METHOD, OVERRIDE FROM REGISTRATIONS";

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

            SqlConnection con = new SqlConnection(_connectionstring);
            SqlCommand sql = new SqlCommand(request, con);

            RegistrationList regs = new RegistrationList();
            con.Open();
            try
            {
                SqlDataReader rd = sql.ExecuteReader();
                while (rd.Read())
                {
                    Registration reg = new Registration();
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
        /// GetUserRegistrationsCount method implmentation
        /// </summary>
        public override int GetUserRegistrationsCount(DataFilterObject filter)
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
                {4, " METHOD = 4 "}
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

            SqlConnection con = new SqlConnection(_connectionstring);
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
        /// GetImportUserRegistrations
        /// </summary>
        public override RegistrationList GetImportUserRegistrations(string domain, string username, string password, string ldappath, DateTime? created, DateTime? modified, string mailattribute, string phoneattribute, PreferredMethod method, bool disableall = false)
        {
            throw new NotImplementedException("Not supported by SQL Provider");
        }

        /// <summary>
        /// HasRegistration method implementation
        /// </summary>
        public override bool HasRegistration(string upn)
        {
            return SQLUtils.HasRegistration(_host, upn);
        }
        #endregion
    }

    internal class SQLKeysRepositoryService : KeysRepositoryService, IDataRepositorySQLConnection
    {
        SQLServerHost _host;
        string _connectionstring;

        /// <summary>
        /// ADDSKeysRepositoryService constructor
        /// </summary>
        public SQLKeysRepositoryService(MFAConfig cfg)
        {
            _host = cfg.Hosts.SQLServerHost;
            _connectionstring = _host.ConnectionString;
        }

        #region Key Management
        /// <summary>
        /// GetUserKey method implmentation
        /// </summary>
        public override string GetUserKey(string upn)
        {
            string request = "SELECT SECRETKEY FROM REGISTRATIONS WHERE UPN=@UPN";
            SqlConnection con = new SqlConnection(_connectionstring);
            SqlCommand sql = new SqlCommand(request, con);

            SqlParameter prm = new SqlParameter("@UPN", SqlDbType.VarChar);
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
        public override string NewUserKey(string upn, string secretkey, string cert = null)
        {
            if (SQLUtils.HasRegistration(_host, upn.ToLower()))
                return DoUpdateUserKey(upn.ToLower(), secretkey);
            else
                return DoInsertUserKey(upn.ToLower(), secretkey);
        }

        /// <summary>
        /// RemoveUserKey method implmentation
        /// </summary>
        public override bool RemoveUserKey(string upn)
        {
            string request = "UPDATE REGISTRATIONS SET SECRETKEY = NULL WHERE UPN=@UPN";

            SqlConnection con = new SqlConnection(_connectionstring);
            SqlCommand sql = new SqlCommand(request, con);

            SqlParameter prm2 = new SqlParameter("@UPN", SqlDbType.VarChar);
            sql.Parameters.Add(prm2);
            prm2.Value = upn.ToLower();
            con.Open();
            try
            {
                int res = sql.ExecuteNonQuery();
                return (res == 1);
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
        /// GetUserCertificate implementation
        /// </summary>
        public override X509Certificate2 GetUserCertificate(string upn)
        {
            return null;
        }

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

            SqlConnection con = new SqlConnection(_connectionstring);
            SqlCommand sql = new SqlCommand(request, con);

            SqlParameter prm1 = new SqlParameter("@SECRETKEY", SqlDbType.VarChar);
            sql.Parameters.Add(prm1);
            prm1.Value = secretkey;

            SqlParameter prm2 = new SqlParameter("@UPN", SqlDbType.VarChar);
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

            SqlConnection con = new SqlConnection(_connectionstring);
            SqlCommand sql = new SqlCommand(request, con);

            SqlParameter prm1 = new SqlParameter("@SECRETKEY", SqlDbType.VarChar);
            sql.Parameters.Add(prm1);
            prm1.Value = secretkey;

            SqlParameter prm2 = new SqlParameter("@UPN", SqlDbType.VarChar);
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
    }

    internal static class SQLUtils
    {
        /// <summary>
        /// HasRegistration method implementation
        /// </summary>
        internal static bool HasRegistration(SQLServerHost host, string upn)
        {
            string request = "SELECT ID, UPN FROM REGISTRATIONS WHERE UPN=@UPN";

            SqlConnection con = new SqlConnection(host.ConnectionString);
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
    }
}
