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
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.DirectoryServices;
using System.Security.Cryptography.X509Certificates;

namespace Neos.IdentityServer.MultiFactor.Data
{
    internal class SQLDataRepositoryService: DataRepositoryService
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
        /// CheckRepositoryAttribute method implementation
        /// </summary>
        public override bool CheckRepositoryAttribute(string attribute)
        {
            SqlConnection con;
            if (attribute.ToLower().Equals("connectionstring"))
            {
                if (string.IsNullOrEmpty(_connectionstring))
                    return false;
                con = new SqlConnection(_connectionstring);
            }
            else
            {
                if (string.IsNullOrEmpty(attribute))
                    return false;
                con = new SqlConnection(attribute);
            }
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
            string request = "SELECT ID, UPN, MAILADDRESS, PHONENUMBER, ENABLED, CREATIONDATE, METHOD FROM REGISTRATIONS WHERE UPN=@UPN";
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
                    reg.Enabled = rd.GetBoolean(4);
                    reg.CreationDate = rd.GetDateTime(5);
                    reg.PreferredMethod = (PreferredMethod)rd.GetInt32(6);
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
        public override Registration SetUserRegistration(Registration reg, bool resetkey = false)
        {
            if (!SQLUtils.HasRegistration(_host, reg.UPN))
               return AddUserRegistration(reg);

            string request = "UPDATE REGISTRATIONS SET MAILADDRESS = @MAILADDRESS, PHONENUMBER = @PHONENUMBER, METHOD=@METHOD, ENABLED=@ENABLED WHERE UPN=@UPN";

            SqlConnection con = new SqlConnection(_connectionstring);
            SqlCommand sql = new SqlCommand(request, con);

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

            SqlParameter prm4 = new SqlParameter("@METHOD", SqlDbType.Int);
            sql.Parameters.Add(prm4);
            prm4.Value = reg.PreferredMethod;

            SqlParameter prm5 = new SqlParameter("@ENABLED", SqlDbType.Bit);
            sql.Parameters.Add(prm5);
            prm5.Value = reg.Enabled;

            SqlParameter prm6 = new SqlParameter("@UPN", SqlDbType.VarChar);
            sql.Parameters.Add(prm6);
            prm6.Value = reg.UPN;
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
        public override Registration AddUserRegistration(Registration reg, bool newkey = true)
        {
            if (SQLUtils.HasRegistration(_host, reg.UPN))
                return SetUserRegistration(reg, newkey);

            string request = "INSERT INTO REGISTRATIONS (UPN, MAILADDRESS, PHONENUMBER, ENABLED, METHOD) VALUES (@UPN, @MAILADDRESS, @PHONENUMBER, @ENABLED, @METHOD)";

            SqlConnection con = new SqlConnection(_connectionstring);
            SqlCommand sql = new SqlCommand(request, con);

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

            SqlParameter prmb = new SqlParameter("@ENABLED", SqlDbType.Bit);
            sql.Parameters.Add(prmb);
            prmb.Value = reg.Enabled;

            SqlParameter prm4 = new SqlParameter("@METHOD", SqlDbType.Int);
            sql.Parameters.Add(prm4);
            prm4.Value = reg.PreferredMethod;

            SqlParameter prm5 = new SqlParameter("@UPN", SqlDbType.VarChar);
            sql.Parameters.Add(prm5);
            prm5.Value = reg.UPN;
            con.Open();
            try
            {
                int res = sql.ExecuteNonQuery();
                if (newkey)
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

            string request2 = "DELETE FROM NOTIFICATIONS WHERE REGISTRATIONID=@REGISTRATIONID";
            SqlCommand sql2 = new SqlCommand(request2, con);

            SqlParameter prm2 = new SqlParameter("@REGISTRATIONID", SqlDbType.BigInt);
            sql2.Parameters.Add(prm2);
            prm2.Value = reg.ID;
            con.Open();
            try
            {
                int res = sql2.ExecuteNonQuery();
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
        public override RegistrationList GetUserRegistrations(DataFilterObject filter, DataOrderObject order, DataPagingObject paging, int maxrows = 20000)
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
                {3, " METHOD = 3 "}
            };

            string request = string.Empty;
            switch (order.Column)
            {
                case DataOrderField.UserName:
                    if (order.Direction == SortDirection.Ascending)
                        request = "SELECT ROW_NUMBER() OVER(ORDER BY UPN) AS NUMBER, ID, UPN, MAILADDRESS, PHONENUMBER, ENABLED, CREATIONDATE, METHOD FROM REGISTRATIONS";
                    else
                        request = "SELECT ROW_NUMBER() OVER(ORDER BY UPN DESC) AS NUMBER, ID, UPN, MAILADDRESS, PHONENUMBER, ENABLED, CREATIONDATE, METHOD FROM REGISTRATIONS";
                    break;
                case DataOrderField.Email:
                    if (order.Direction == SortDirection.Ascending)
                        request = "SELECT ROW_NUMBER() OVER(ORDER BY MAILADDRESS) AS NUMBER, ID, UPN, MAILADDRESS, PHONENUMBER, ENABLED, CREATIONDATE, METHOD FROM REGISTRATIONS";
                    else
                        request = "SELECT ROW_NUMBER() OVER(ORDER BY MAILADDRESS DESC) AS NUMBER, ID, UPN, MAILADDRESS, PHONENUMBER, ENABLED, CREATIONDATE, METHOD FROM REGISTRATIONS";
                    break;
                case DataOrderField.Phone:
                    if (order.Direction == SortDirection.Ascending)
                        request = "SELECT ROW_NUMBER() OVER(ORDER BY PHONENUMBER) AS NUMBER, ID, UPN, MAILADDRESS, PHONENUMBER, ENABLED, CREATIONDATE, METHOD FROM REGISTRATIONS";
                    else
                        request = "SELECT ROW_NUMBER() OVER(ORDER BY PHONENUMBER DESC) AS NUMBER, ID, UPN, MAILADDRESS, PHONENUMBER, ENABLED, CREATIONDATE, METHOD FROM REGISTRATIONS";
                    break;
                case DataOrderField.CreationDate:
                    if (order.Direction == SortDirection.Ascending)
                        request = "SELECT ROW_NUMBER() OVER(ORDER BY CREATIONDATE) AS NUMBER, ID, UPN, MAILADDRESS, PHONENUMBER, ENABLED, CREATIONDATE, METHOD FROM REGISTRATIONS";
                    else
                        request = "SELECT ROW_NUMBER() OVER(ORDER BY CREATIONDATE DESC) AS NUMBER, ID, UPN, MAILADDRESS, PHONENUMBER, ENABLED, CREATIONDATE, METHOD FROM REGISTRATIONS";
                    break;
                default:
                    if (order.Direction == SortDirection.Ascending)
                        request = "SELECT ROW_NUMBER() OVER(ORDER BY ID) AS NUMBER, ID, UPN, MAILADDRESS, PHONENUMBER, ENABLED, CREATIONDATE, METHOD FROM REGISTRATIONS";
                    else
                        request = "SELECT ROW_NUMBER() OVER(ORDER BY ID DESC) AS NUMBER, ID, UPN, MAILADDRESS, PHONENUMBER, ENABLED, CREATIONDATE, METHOD FROM REGISTRATIONS";
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
                request = "SELECT TOP " + maxrows.ToString() + " NUMBER, ID, UPN, MAILADDRESS, PHONENUMBER, ENABLED, CREATIONDATE, METHOD FROM (" + request;

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
                    reg.Enabled = rd.GetBoolean(5);
                    reg.CreationDate = rd.GetDateTime(6);
                    reg.PreferredMethod = (PreferredMethod)rd.GetInt32(7);
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
        public override RegistrationList GetAllUserRegistrations(DataOrderObject order, int maxrows = 20000, bool enabledonly = false)
        {
            string request = string.Empty;
            if (enabledonly)
                request = "SELECT TOP " + maxrows.ToString() + " ID, UPN, MAILADDRESS, PHONENUMBER, ENABLED, CREATIONDATE, METHOD FROM REGISTRATIONS WHERE ENABLED=1";
            else
                request = "SELECT TOP " + maxrows.ToString() + " ID, UPN, MAILADDRESS, PHONENUMBER, ENABLED, CREATIONDATE, METHOD FROM REGISTRATIONS";

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
                case DataOrderField.CreationDate:
                    request += " ORDER BY ID";
                    break;
                default:
                    request += " ORDER BY CREATIONDATE";
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
                    reg.Enabled = rd.GetBoolean(4);
                    reg.CreationDate = rd.GetDateTime(5);
                    reg.PreferredMethod = (PreferredMethod)rd.GetInt32(6);
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
                {3, " METHOD = 3 "}
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
        public override RegistrationList GetImportUserRegistrations(string ldappath, bool enable)
        {
            throw new NotImplementedException("Not supported by SQL Provider");
        }

        /// <summary>
        /// SetNotification method implementation
        /// </summary>
        public override Notification SetNotification(Registration reg, int otp)
        {
            Notification Notification = new Notification();
            Notification.RegistrationID = reg.ID;
            Notification.OTP = otp;
            Notification.CreationDate = DateTime.UtcNow;
            Notification.ValidityDate = Notification.CreationDate.AddSeconds(_deliverywindow);
            Notification.CheckDate = null;
            if (SQLUtils.HasNotification(_host, Notification.RegistrationID))
                DoUpdateNotification(Notification);
            else
                DoInsertNotification(Notification);
            return Notification;
        }

                /// <summary>
        /// HasRegistration method implementation
        /// </summary>
        public override bool HasRegistration(string upn)
        {
            return SQLUtils.HasRegistration(_host, upn);
        }

        /// <summary>
        /// CheckNotification method implementation
        /// </summary>
        public override  Notification CheckNotification(Registration registration)
        {
            Notification Notification = new Notification();
            Notification.RegistrationID = registration.ID;
            Notification.CheckDate = DateTime.UtcNow;
            return DoCheckNotification(Notification);
        }
        #endregion

        #region Private methods
        /// <summary>
        /// InsertNotification method implementation
        /// </summary>
        /// <param name="regid"></param>
        private bool DoInsertNotification(Notification Notification)
        {
            string request = "INSERT INTO NOTIFICATIONS (REGISTRATIONID, OTP, CREATIONDATE, VALIDITYDATE, CHECKDATE) VALUES (@REGISTRATIONID, @OTP, @CREATEDATE, @VALIDITYDATE, NULL)";
            SqlConnection con = new SqlConnection(_connectionstring);
            SqlCommand sql = new SqlCommand(request, con);

            SqlParameter prmid = new SqlParameter("@REGISTRATIONID", SqlDbType.BigInt);
            sql.Parameters.Add(prmid);
            prmid.Value = Notification.RegistrationID;

            SqlParameter prmotp = new SqlParameter("@OTP", SqlDbType.Int);
            sql.Parameters.Add(prmotp);
            prmotp.Value = Notification.OTP;

            SqlParameter prmdat = new SqlParameter("@CREATEDATE", SqlDbType.DateTime);
            sql.Parameters.Add(prmdat);
            prmdat.Value = Notification.CreationDate;

            SqlParameter prmval = new SqlParameter("@VALIDITYDATE", SqlDbType.DateTime);
            sql.Parameters.Add(prmval);
            prmval.Value = Notification.ValidityDate;

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
        /// UpdateNotification method implementation
        /// </summary>
        private bool DoUpdateNotification(Notification Notification)
        {
            string request = "UPDATE NOTIFICATIONS  SET OTP=@OTP, CREATIONDATE = @CREATEDATE, VALIDITYDATE=@VALIDITYDATE, CHECKDATE = NULL WHERE REGISTRATIONID=@REGISTRATIONID";
            SqlConnection con = new SqlConnection(_connectionstring);
            SqlCommand sql = new SqlCommand(request, con);

            SqlParameter prmid = new SqlParameter("@REGISTRATIONID", SqlDbType.BigInt);
            sql.Parameters.Add(prmid);
            prmid.Value = Notification.RegistrationID;

            SqlParameter prmotp = new SqlParameter("@OTP", SqlDbType.Int);
            sql.Parameters.Add(prmotp);
            prmotp.Value = Notification.OTP;

            SqlParameter prmdat = new SqlParameter("@CREATEDATE", SqlDbType.DateTime);
            sql.Parameters.Add(prmdat);
            prmdat.Value = Notification.CreationDate;

            SqlParameter prmval = new SqlParameter("@VALIDITYDATE", SqlDbType.DateTime);
            sql.Parameters.Add(prmval);
            prmval.Value = Notification.ValidityDate;

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
        /// DoCheckNotification method implementation
        /// </summary>
        private Notification DoCheckNotification(Notification Notification)
        {
            string request = "UPDATE NOTIFICATIONS SET CHECKDATE = @CHECKDATE WHERE REGISTRATIONID=@REGISTRATIONID";
            SqlConnection con = new SqlConnection(_connectionstring);
            SqlCommand sql = new SqlCommand(request, con);

            SqlParameter prmid = new SqlParameter("@REGISTRATIONID", SqlDbType.BigInt);
            sql.Parameters.Add(prmid);
            prmid.Value = Notification.RegistrationID;

            SqlParameter prmdat = new SqlParameter("@CHECKDATE", SqlDbType.DateTime);
            sql.Parameters.Add(prmdat);
            prmdat.Value = Notification.CheckDate;

            con.Open();
            try
            {
                int res = sql.ExecuteNonQuery();
                if (res == 1)
                {
                    string select = "SELECT ID, REGISTRATIONID, OTP, CREATIONDATE, VALIDITYDATE, CHECKDATE FROM NOTIFICATIONS WHERE REGISTRATIONID=@REGISTRATIONID";
                    SqlCommand ssql = new SqlCommand(select, con);

                    SqlParameter prm = new SqlParameter("@REGISTRATIONID", SqlDbType.BigInt);
                    ssql.Parameters.Add(prm);
                    prm.Value = Notification.RegistrationID;
                    // if (con.State!= ConnectionState.Open) con.Open();
                    try
                    {
                        SqlDataReader rd = ssql.ExecuteReader();
                        if (rd.Read())
                        {
                            Notification.ID = rd.GetInt64(0).ToString();
                            Notification.RegistrationID = rd.GetInt64(1).ToString();
                            Notification.OTP = rd.GetInt32(2);
                            Notification.CreationDate = rd.GetDateTime(3);
                            Notification.ValidityDate = rd.GetDateTime(4);
                            if (!rd.IsDBNull(5))
                                Notification.CheckDate = rd.GetDateTime(5);
                            else
                                Notification.CheckDate = null;
                            return Notification;
                        }
                        else
                            return null;
                    }
                    catch (Exception ex)
                    {
                        DataLog.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error, 5000);
                        throw new Exception(ex.Message);
                    }
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
        #endregion
    }

    internal class SQLKeysRepositoryService : KeysRepositoryService
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
            string request = "INSERT INTO REGISTRATIONS (UPN, SECRETKEY, METHOD, ENABLED) VALUES (@UPN, @SECRETKEY, 0, 1)";

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

        /// <summary>
        /// HasNotification method implementation
        /// </summary>
        internal static bool HasNotification(SQLServerHost host, string regid)
        {
            string request = "SELECT REGISTRATIONID FROM NOTIFICATIONS WHERE REGISTRATIONID=@REGISTRATIONID";

            SqlConnection con = new SqlConnection(host.ConnectionString);
            SqlCommand sql = new SqlCommand(request, con);

            SqlParameter prm = new SqlParameter("@REGISTRATIONID", SqlDbType.BigInt);
            sql.Parameters.Add(prm);
            prm.Value = regid;
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
